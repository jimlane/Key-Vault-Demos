using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Azure.Storage.Blobs;
using Azure.Identity;

[assembly: FunctionsStartup(typeof(FunctionKeyVault.Startup))]
namespace FunctionKeyVault
{
    public class Startup : FunctionsStartup
    {
        public static IConfiguration Configuration { set; get; }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
               .SetBasePath(Environment.CurrentDirectory)
               .AddJsonFile("appsettings.json", false)
               .AddUserSecrets<FunctionsStartup>()
               .AddEnvironmentVariables();
            Configuration = config.Build();

            builder.Services.AddSingleton<IConfiguration>(Configuration);

            builder.Services.AddHttpClient();

            builder.Services.AddSingleton((s) =>{
                return new displayAllBlobs();
            });

            builder.Services.AddSingleton<ILoggerProvider>();
        }
    }

    public class displayAllBlobs
    {
        [FunctionName("displayAllBlobs")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("FunctionKeyVault.displayBlobs function processed a request.");

            BlobServiceClient blobServiceClient = new BlobServiceClient(FunctionKeyVault.Startup.Configuration["TestApp:Settings:StgConnString"]);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(FunctionKeyVault.Startup.Configuration["TestApp:Settings:StgContainerName"]);
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
