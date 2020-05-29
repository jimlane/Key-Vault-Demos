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
    //preferred methods to save connection strings locally is with environment variables or
    //local user-secrets cache instead of embedding them in local.settings.json
    //
    //  dotnet user-secrets set <your_config_name> <your_connection_string>
    //  builder.AddAzureAppConfiguration(Configuration["<your_config_name>"]);
    //
    //  setx <your_config_name> <your_connection_string>
    //  builder.AddAzureAppConfiguration(Environment.GetEnvironmentVariable("<your_config_name>"));
    //
    //this function utilized user-secrets

    //to reference the local assembly in the .AddUserSecrets call the class cannot be static
    public class getAllBlobs
    {
        //we'll need a custom configuration object in order to access user secrets
        private static IConfiguration Configuration { get; set; }
        private static IConfigurationRefresher ConfigurationRefresher { set; get; }

        //in order to access user secrets a custom constructor must be injected into the function
        static getAllBlobs()
        {
            //instantiate your own ConfigurationBuilder and inject user-secrets
            var builder = new ConfigurationBuilder()
                .AddUserSecrets<getAllBlobs>();
            Configuration = builder.Build();
            //tell the config to get stuff from and Azure AppConfig object
            builder.AddAzureAppConfiguration(options =>
                {
                    //here we retrieve the connection string for Azure AppConfig from our secret
                    options.Connect(Configuration["ConnectionString:AppConfig"])
                        //this step is required if your AppConfig contains entries stored in a KeyVault
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
