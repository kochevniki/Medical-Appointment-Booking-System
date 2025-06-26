using Box.V2;
using Box.V2.Auth;
using Box.V2.Config;
using Box.V2.Models;
using MedicalBookingService.Server.Data;
using MedicalBookingService.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MedicalBookingService.Shared.Services
{
    public class FileService
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;
        private readonly string _folderId;

        public FileService(IConfiguration config, ApplicationDbContext context)
        {
            _config = config;
            _context = context;
            _folderId = config["Box:FolderId"]!;
        }

        public async Task<string> UploadAsync(string fileName, Stream fileStream, string contentType)
        {
            return await ExecuteWithValidToken(async client =>
            {
                string[] folders = fileName.Split('/');
                var patientsFolder = await GetOrCreateFolderAsync(client, folders[0], _folderId);
                var userFolder = await GetOrCreateFolderAsync(client, folders[1], patientsFolder.Id);

                var request = new BoxFileRequest
                {
                    Name = folders[2],
                    Parent = new BoxRequestEntity { Id = userFolder.Id }
                };

                var file = await client.FilesManager.UploadAsync(request, fileStream);
                return file.Id;
            });
        }

        public async Task<Stream?> DownloadAsync(string fileId)
        {
            return await ExecuteWithValidToken(client => client.FilesManager.DownloadAsync(fileId));
        }

        public async Task DeleteAsync(string fileId)
        {
            await ExecuteWithValidToken(async client =>
            {
                await client.FilesManager.DeleteAsync(fileId);
                return true; // dummy return value for consistency
            });
        }

        private async Task<BoxFolder> GetOrCreateFolderAsync(BoxClient client, string folderName, string parentId)
        {
            var items = await client.FoldersManager.GetFolderItemsAsync(parentId, 1000);
            var folder = items.Entries
                .OfType<BoxFolder>()
                .FirstOrDefault(f => f.Name.Equals(folderName, StringComparison.OrdinalIgnoreCase));

            if (folder != null)
                return folder;

            var request = new BoxFolderRequest
            {
                Name = folderName,
                Parent = new BoxRequestEntity { Id = parentId }
            };

            return await client.FoldersManager.CreateAsync(request);
        }

        private async Task<BoxClient> CreateBoxClientAsync(OAuthSession session)
        {
            var config = new BoxConfig(
                _config["Box:ClientId"]!,
                _config["Box:ClientSecret"]!,
                new Uri(_config["Box:RedirectUri"]!)
            );
            return new BoxClient(config, session);
        }

        private async Task<T> ExecuteWithValidToken<T>(Func<BoxClient, Task<T>> apiCall)
        {
            var tokenEntry = await _context.BoxTokens.FirstOrDefaultAsync();
            if (tokenEntry == null)
                throw new InvalidOperationException("Box tokens not found in database.");

            var session = new OAuthSession(
                tokenEntry.AccessToken,
                tokenEntry.RefreshToken,
                3600,
                "bearer"
            );

            var client = await CreateBoxClientAsync(session);

            try
            {
                var result = await apiCall(client);

                // Optionally: Check if client.Auth.Session has changed and update DB
                if (client.Auth.Session.AccessToken != tokenEntry.AccessToken ||
                    client.Auth.Session.RefreshToken != tokenEntry.RefreshToken)
                {
                    tokenEntry.AccessToken = client.Auth.Session.AccessToken;
                    tokenEntry.RefreshToken = client.Auth.Session.RefreshToken;
                    tokenEntry.UpdatedAt = DateTime.UtcNow;
                    _context.BoxTokens.Update(tokenEntry);
                    await _context.SaveChangesAsync();
                }

                return result;
            }
            catch (Box.V2.Exceptions.BoxSessionInvalidatedException)
            {
                // Refresh token
                var newSession = await client.Auth.RefreshAccessTokenAsync(tokenEntry.RefreshToken);

                // Update DB
                tokenEntry.AccessToken = newSession.AccessToken;
                tokenEntry.RefreshToken = newSession.RefreshToken;
                tokenEntry.UpdatedAt = DateTime.UtcNow;

                _context.BoxTokens.Update(tokenEntry);
                await _context.SaveChangesAsync();

                // Rebuild client with fresh tokens
                var newClient = await CreateBoxClientAsync(newSession);

                // Retry the API call once
                return await apiCall(newClient);
            }
        }
    }
}
