using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using TestTaskTraineeCamp.Server.Models;
using TestTaskTraineeCamp.Server.Services;
using TestTaskTraineeCamp.Server.Validators;

namespace TestTaskTraineeCamp.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlobStorageController : ControllerBase
    {
        private readonly IBlobService _blobService;

        public BlobStorageController(IBlobService blobService)
        {
            _blobService = blobService;
        }

        [HttpGet(nameof(Get))]
        [ProducesResponseType(typeof(List<BlobDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get()
        {
            List<BlobDto?> files = await _blobService.GetAllAsync();

            return Ok(files);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BlobResponseDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Upload(IFormFile file, string userEmail)
        {
            var validationResult = await new FileUploadValidator().ValidateAsync(file);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            BlobResponseDto? response = await _blobService.UploadAsync(file, userEmail);

            if (response.Error == true)
            {
                return BadRequest(response.Status);
            }

            else
            {
                return Ok(response);
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(BlobDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Download(string filename)
        {
            BlobDto? file = await _blobService.DownloadAsync(filename);

            if (file == null)
            {
                return BadRequest($"Unable to download {filename}");
            }

            else
            {
                return File(file.Content, file.ContentType, file.Name);
            }
        }

        [HttpDelete]
        [ProducesResponseType(typeof(BlobResponseDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Delete(string fileName)
        {
            BlobResponseDto response = await _blobService.DeleteAsync(fileName);

            if (response.Error == true)
            {
                return BadRequest(response.Status);
            }

            else
            {
                return Ok(response.Status);
            }
        }
    }
}
