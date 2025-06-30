using Box.V2;
using Box.V2.Config;
using Box.V2.JWTAuth;
using Box.V2.Models;
using MedicalBookingService.Server.Data;
using Microsoft.Extensions.Configuration;

namespace MedicalBookingService.Shared.Services
{
    public class FileService
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;
        private readonly string _folderId;
        private readonly BoxJWTAuth _boxJwtAuth;
        private readonly string _enterpriseId;

        public FileService(IConfiguration config, ApplicationDbContext context)
        {
            _config = config;
            _context = context;
            _folderId = config["boxAppSettings:FolderId"]!;

             // Initialize BoxJWTAuth using boxAppSettings from appsettings.json
            // This is done once in the constructor, which is correct.
            var boxConfig = new BoxConfigBuilder(
                _config["boxAppSettings:clientID"],
                _config["boxAppSettings:clientSecret"],
                _config["boxAppSettings:enterpriseID"],
                _config["boxAppSettings:appAuth:privateKey"],
                _config["boxAppSettings:appAuth:passphrase"],
                _config["boxAppSettings:appAuth:publicKeyID"]
            ).Build();
            _boxJwtAuth = new BoxJWTAuth(boxConfig);
            _enterpriseId = _config["boxAppSettings:enterpriseID"];
        }

        private async Task<BoxClient> GetBoxClientAsync()
        {
            // The 'await' keyword MUST be used in an 'async' method.
            // 'await' unwraps the Task<string> and gives you the string token.
            string adminToken = await _boxJwtAuth.AdminTokenAsync();
            // The AdminClient method accepts the string token directly.
            return _boxJwtAuth.AdminClient(adminToken);
        }

        public async Task<string> UploadAsync(string fileName, Stream fileStream, string contentType)
        {
            // Await the asynchronous client creation.
            var client = await GetBoxClientAsync();
            string[] folders = fileName.Split('/');

            // The rest of your logic is correct, just needs 'await' and 'client' variable.
            var patientsFolder = await GetOrCreateFolderAsync(client, folders[0], _folderId);
            var userFolder = await GetOrCreateFolderAsync(client, folders[1], patientsFolder.Id);

            var request = new BoxFileRequest
            {
                Name = folders[2],
                Parent = new BoxRequestEntity { Id = userFolder.Id }
            };

            var file = await client.FilesManager.UploadAsync(request, fileStream);
            return file.Id;
        }

        public async Task<string> GetPreviewUrlAsync(string fileId)
        {
            var client = await GetBoxClientAsync();
            var sharedLinkRequest = new BoxSharedLinkRequest
            {
                Access = BoxSharedLinkAccessType.open // "People with the link"
            };

            var file = await client.FilesManager.CreateSharedLinkAsync(fileId, sharedLinkRequest);
            return file.SharedLink?.Url ?? throw new Exception("Failed to generate shared link.");
        }

        public async Task<Stream?> DownloadAsync(string fileId)
        {
            var client = await GetBoxClientAsync();
            return await client.FilesManager.DownloadAsync(fileId);
        }

        public async Task DeleteAsync(string fileId)
        {
            var client = await GetBoxClientAsync();
            await client.FilesManager.DeleteAsync(fileId);
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
    }
}