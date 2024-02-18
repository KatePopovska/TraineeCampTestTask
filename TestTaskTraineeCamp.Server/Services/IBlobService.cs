using TestTaskTraineeCamp.Server.Models;

namespace TestTaskTraineeCamp.Server.Services
{
    public interface IBlobService
    {
        Task<BlobResponseDto> UploadAsync(IFormFile file, string userEmail);

        Task<BlobDto> DownloadAsync(string blobFileName);

        Task<BlobResponseDto> DeleteAsync(string blobFileName);

        Task<List<BlobDto>> GetAllAsync();
    }
}
