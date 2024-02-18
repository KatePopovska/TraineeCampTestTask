using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AzureBlobTriggerFunction
{
    [StorageAccount("BlobConnectionString")]
    public class BlobTriggerFunction
    {
        [FunctionName("BlobTriggerFunction")]
        public async Task Run([BlobTrigger("files/{name}")] Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            string userEmail = GetUserEmailFromBlobMetadata(name, log);

            await SendEmailNotification(userEmail);
        }

        private static string GetUserEmailFromBlobMetadata(string blobName, ILogger logger)
        {
            try
            {
                BlobContainerClient containerClient = new BlobContainerClient(Environment.GetEnvironmentVariable("BlobConnectionString"),
                    Environment.GetEnvironmentVariable("BlobContainerName"));

                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                BlobProperties properties = blobClient.GetProperties().Value;

                if (properties.Metadata.TryGetValue("UserEmail", out string userEmail))
                {
                    logger.LogInformation($"User email: {userEmail}");
                    return userEmail;
                }
                logger.LogInformation("User email is null");

                return null;
            }

            catch(Exception ex)
            {
                logger.LogError($"Error retrieving metadata: {ex.Message}");
                return null;
            }
        }

        private async Task SendEmailNotification(string userEmail)
        {
            SendGridClient sendGridClient = new SendGridClient(Environment.GetEnvironmentVariable("SendGridApi"));

            var msg = new SendGridMessage()
            {
                From = new EmailAddress("katyapopovska08@gmail.com", "Kateryna"),
                Subject = "The file is successfully uploaded!",
            };

            msg.PlainTextContent = "The file has been successfully uploaded. Thank you!";
            msg.AddTo(userEmail);

            var response = await sendGridClient.SendEmailAsync(msg);
            string responseBody = await response.Body.ReadAsStringAsync();
            Console.WriteLine($"Response body: {responseBody}");
        }
    }
}
