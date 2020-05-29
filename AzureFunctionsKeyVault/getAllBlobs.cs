using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Azure.Storage.Blobs;
using Azure.Identity;
using Newtonsoft.Json;

namespace AzureFunctionsKeyVault
{
    public class getAllBlobs
    {
        private static IConfiguration Configuration { get; set; }
        private static IConfigurationRefresher ConfigurationRefresher { set; get; }

        static getAllBlobs()
        {
            var builder = new ConfigurationBuilder()
                .AddUserSecrets<getAllBlobs>();
            Configuration = builder.Build();
            builder.AddAzureAppConfiguration(options =>
                {
                    options.Connect(Configuration["ConnectionString:AppConfig"])
                        .ConfigureKeyVault(kv =>
                        {
                            kv.SetCredential(new DefaultAzureCredential());
                        });
                });
            Configuration = builder.Build();
        }

        [FunctionName("getAllBlobs")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("FunctionKeyVault.displayBlobs function processed a request.");

            BlobServiceClient blobServiceClient = new BlobServiceClient(Configuration["TestApp:Settings:StgConnString"]);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(Configuration["TestApp:Settings:StgContainerName"]);
            int blobCount = 0;
            foreach (var blob in containerClient.GetBlobs())
            {
                blobCount++;
            }

            string responseMessage = "Found " + blobCount.ToString() + " blobs in storage account " + containerClient.Uri.ToString() +
            " in container " + containerClient.Name.ToString();

            return new OkObjectResult(responseMessage);
        }
    }
}
