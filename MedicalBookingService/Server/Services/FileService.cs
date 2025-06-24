using Box.V2;
using Box.V2.Auth;
using Box.V2.Config;
using Box.V2.JWTAuth;
using Box.V2.Models;
using Microsoft.Extensions.Configuration;

namespace MedicalBookingService.Shared.Services
{
    public class FileService
    {
        private readonly BoxClient _boxClient;
        private readonly string _folderId;

        public FileService(IConfiguration config)
        {
            var boxConfig = new BoxConfig(
                config["Box:ClientId"],
                config["Box:ClientSecret"],
                new Uri(config["Box:RedirectUri"]));

            var session = new OAuthSession(config["Box:AccessToken"], null, 3600, "bearer");
            _boxClient = new BoxClient(boxConfig, session);

            _folderId = config["Box:FolderId"]; // Box folder where files will be stored
        }

        public async Task<string> UploadAsync(string fileName, Stream fileStream, string contentType)
        {
            string[] folders = fileName.Split('/');
            // Step 1: Ensure "patients" folder exists
            var patientsFolder = await GetOrCreateFolderAsync(folders[0], _folderId);

            // Step 2: Ensure userId folder exists under "patients"
            var userFolder = await GetOrCreateFolderAsync(folders[1], patientsFolder.Id);

            var request = new BoxFileRequest
            {
                Name = folders[2],
                Parent = new BoxRequestEntity { Id = userFolder.Id }
            };

            var file = await _boxClient.FilesManager.UploadAsync(request, fileStream);
            return file.Id; // or file.SharedLink?.Url if you create a shared link
        }

        public async Task<Stream?> DownloadAsync(string fileId)
        {
            var stream = await _boxClient.FilesManager.DownloadAsync(fileId);
            return stream;
        }

        public async Task DeleteAsync(string fileId)
        {
            await _boxClient.FilesManager.DeleteAsync(fileId);
        }

        private async Task<BoxFolder> GetOrCreateFolderAsync(string folderName, string parentId)
        {
            // Check if folder exists
            var items = await _boxClient.FoldersManager.GetFolderItemsAsync(parentId, 1000);
            var folder = items.Entries
                .OfType<BoxFolder>()
                .FirstOrDefault(f => f.Name.Equals(folderName, StringComparison.OrdinalIgnoreCase));

            if (folder != null)
                return folder;

            // Create folder
            var request = new BoxFolderRequest
            {
                Name = folderName,
                Parent = new BoxRequestEntity { Id = parentId }
            };

            return await _boxClient.FoldersManager.CreateAsync(request);
        }
    }
}
