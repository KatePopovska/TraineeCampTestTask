using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Dasync.Collections;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestTaskTraineeCamp.Server;
using TestTaskTraineeCamp.Server.Services;
using Xunit.Sdk;

namespace UnitTests
{
    public class BlobServiceTests
    {
        private readonly AzureBlobSettingsOption _azBlobSettingsOption;
        private readonly Mock<ILogger<BlobService>> _logger;
        private const string _connectionName = "testconnection";
        public BlobServiceTests()
        {
            _azBlobSettingsOption = new AzureBlobSettingsOption();
            _logger = new Mock<ILogger<BlobService>>();           
        }

        [Fact]
        public async Task DeleteAsync_Success()
        {
            string testFileName = "test.docx";

            var blobServiceClientFactory = new Mock<IAzureClientFactory<BlobServiceClient>>();
            var blobServiceClient = new Mock<BlobServiceClient>();
            var blobClient = new Mock<BlobClient>();
            var blobContainerClient = new Mock<BlobContainerClient>();

            blobClient.Setup(client => client.DeleteAsync(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), default)).ReturnsAsync(Mock.Of<Response>());
            blobServiceClient.Setup(c => c.GetBlobContainerClient(It.IsAny<string>())).Returns(blobContainerClient.Object);
            blobContainerClient.Setup(c => c.GetBlobClient(It.IsAny<string>())).Returns(blobClient.Object);

            _azBlobSettingsOption.ConnectionName = _connectionName;

            blobServiceClientFactory.Setup(c => c.CreateClient(_connectionName)).Returns(blobServiceClient.Object);

            var blobService = new BlobService(blobServiceClientFactory.Object, _azBlobSettingsOption, _logger.Object );

            var result = await blobService.DeleteAsync(testFileName);

            result.Should().NotBeNull();
            result.Error.Should().BeFalse();
            result.Status.Should().Be($"File: {testFileName} has been successfully deleted!");
        }
        
        [Fact] 
        public async Task UploadAsync_Success()
        {
            string testFileName = "test.docx";
            string testEmail = "test@gmail.com";
            string testUri = "https://example.blob.core.windows.net/...";

            var blobServiceClientFactory = new Mock<IAzureClientFactory<BlobServiceClient>>();
            var blobServiceClient = new Mock<BlobServiceClient>();
            var blobClient = new Mock<BlobClient>();
            var blobContainerClient = new Mock<BlobContainerClient>();

            var blobContainerInfo = BlobsModelFactory.BlobContainerInfo(default, default);
            var blobContentInfo = BlobsModelFactory.BlobContentInfo(default, default, default, default, default);


            blobContainerClient.Setup(client => client.CreateIfNotExistsAsync(default, default, default, default)).ReturnsAsync(Response.FromValue(blobContainerInfo, default!));
            blobContainerClient.Setup(client => client.GetBlobClient(It.IsAny<string>())).Returns(blobClient.Object);
            blobServiceClient.Setup(client => client.GetBlobContainerClient(It.IsAny<string>())).Returns(blobContainerClient.Object);
            blobClient.Setup(client => client.UploadAsync(It.IsAny<Stream>())).ReturnsAsync(Response.FromValue(blobContentInfo, default!));
            blobContainerClient.Setup(x => x.GenerateSasUri(It.IsAny<BlobSasBuilder>())).Returns(new Uri(testUri));
            blobClient.Setup(x => x.Name).Returns(testFileName);

            _azBlobSettingsOption.ConnectionName = _connectionName;

            blobServiceClientFactory.Setup(c => c.CreateClient(_connectionName)).Returns(blobServiceClient.Object);
            var blobService = new BlobService(blobServiceClientFactory.Object, _azBlobSettingsOption, _logger.Object);

            var file = new Mock<IFormFile>();
            file.Setup(x => x.FileName).Returns(testFileName);
            file.Setup(x => x.OpenReadStream()).Returns(new MemoryStream());

            var result = await blobService.UploadAsync(file.Object, testEmail);

            result.Should().NotBeNull();
            result.Error.Should().BeFalse();
            result.Status.Should().Be($"File {testFileName} uploaded successfully.");
            result.Blob.Name.Should().Be(testFileName);
            result.Blob.Uri.Should().Be(testUri);
        }

    }
}
