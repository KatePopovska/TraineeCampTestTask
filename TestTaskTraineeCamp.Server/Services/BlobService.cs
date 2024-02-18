using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using TestTaskTraineeCamp.Server.Models;

namespace TestTaskTraineeCamp.Server.Services
{
    public class BlobService : IBlobService
    {
        private readonly string _storageConnectionString;
        private readonly string _storageContainerName;
        private readonly ILogger<BlobService> _logger;

        public BlobService(IConfiguration configuration, ILogger<BlobService> logger)
        {
            _storageConnectionString = configuration.GetValue<string>("BlobConnectionString");
            _storageContainerName = configuration.GetValue<string>("BlobContainerName");
            _logger = logger;
        }

        public async Task<BlobResponseDto> DeleteAsync(string blobFileName)
        {
            BlobContainerClient client = new BlobContainerClient(_storageConnectionString, _storageContainerName);

            BlobClient file = client.GetBlobClient(blobFileName);

            try
            {
                await file.DeleteAsync();
            }
            catch (RequestFailedException ex) when (ex.ErrorCode ==BlobErrorCode.BlobNotFound)
            { 
                _logger.LogError(ex.Message);

                return new BlobResponseDto { Error = true, Status = $"File {blobFileName} not found." };
            }

            return new BlobResponseDto { Error = false, Status = $"File: {blobFileName} has been successfully deleted!" };
        }

        public async Task<BlobDto> DownloadAsync(string blobFileName)
        {
            BlobContainerClient container = new BlobContainerClient(_storageConnectionString, _storageContainerName);

            try
            {
                BlobClient file = container.GetBlobClient(blobFileName);

                if (await file.ExistsAsync())
                {
                    var data = await file.OpenReadAsync();

                    var content = await file.DownloadContentAsync();

                    return new BlobDto { Content = data, Name = blobFileName, ContentType = content.Value.Details.ContentType };
                }
            }

            catch(RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                _logger.LogError($"File {blobFileName} was not found.");
            }

            return null;
        }

        public async Task<List<BlobDto>> GetAllAsync()
        {
            BlobContainerClient container = new BlobContainerClient( _storageConnectionString, _storageContainerName);

            List<BlobDto> files = new List<BlobDto>();

            await foreach (var file in container.GetBlobsAsync())
            {
                string uri = container.Uri.ToString();
                var name = file.Name;
                var fullUri = $"{uri}/{name}";

                files.Add(new BlobDto
                {
                    Uri = fullUri,
                    Name = name,
                    ContentType = file.Properties.ContentType
                });
            }

            return files;
        }

        public async Task<BlobResponseDto> UploadAsync(IFormFile file, string userEmail)
        {
           BlobContainerClient container = new BlobContainerClient(_storageConnectionString, _storageContainerName);

            BlobResponseDto response = new();

            await container.CreateIfNotExistsAsync();

            Dictionary<string, string> metadata = new Dictionary<string, string> { { "UserEmail", userEmail } };

            try
            {
                BlobClient client = container.GetBlobClient(file.FileName);

                await using (Stream? data = file.OpenReadStream())
                {
                    await client.UploadAsync(data);
                }

                BlobSasBuilder blobSasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = _storageContainerName,
                    ExpiresOn = DateTime.UtcNow.AddHours(1),
                };
                blobSasBuilder.SetPermissions(BlobAccountSasPermissions.All);

                _logger.LogInformation($"Time now: {DateTime.UtcNow.ToString()}");
               var uri = container.GenerateSasUri(blobSasBuilder).AbsoluteUri;

                await client.SetMetadataAsync(metadata);

                response.Status = $"File {file.FileName} uploaded successfully.";
                response.Error = false;
                response.Blob.Uri = uri;
                response.Blob.Name = client.Name;


                _logger.LogInformation($"File {file.FileName} successfully uploaded. {DateTime.Now}");
            }

            catch (RequestFailedException ex ) when (ex.ErrorCode == BlobErrorCode.BlobAlreadyExists)
            {
                _logger.LogError($"File {file.FileName} already exist in container.");
                
                response.Error = true;
                response.Status = $"File {file.FileName} already exist in container.";

                return response;
            }

            catch (Exception ex)
            {
                _logger.LogError($"Unhandled exception ID: {ex.StackTrace} - Message: {ex.Message}");

                response.Error = true;
                response.Status = $"Unexpected error: {ex.StackTrace}";

                return response;
            }

            return response;
        }
    }
}
