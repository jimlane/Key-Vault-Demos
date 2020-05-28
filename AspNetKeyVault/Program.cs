using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Azure.Identity;

namespace AspNetKeyVault
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            webBuilder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                var settings = config.Build();

                //THIS ONE FOR LOCAL RUN USING user-secrets MANAGER for access to Azure App Configuration
                //you'll need to first run: 
                //      dotnet user-secrets set ConnectionString:AppConfig <your_connection_string>
                //
                //then uncomment the following line
                //config.AddAzureAppConfiguration(Configuration["ConnectionString:AppConfig"]);

                //THIS ONE FOR KEY VAULT SECRETS via Azure App Configuration
                //you'll need to first run:
                //
                //      setx AZURE_CLIENT_ID <clientId-of-your-service-principal>
                //      setx AZURE_CLIENT_SECRET <clientSecret-of-your-service-principal>
                //      setx AZURE_TENANT_ID <tenantId-of-your-service-principal>
                //      setx AZURE_USERNAME <service-principal-name>
                //
                //the setx commands won't be necessary when deployed to Azure as long as you have
                //enabled managed identity and granted the web app access to they key vault:
                //      az keyvault set-policy 
                //          -n <your-unique-keyvault-name> 
                //          --spn <clientId-of-your-service-principal> 
                //          --secret-permissions delete get list set 
                //          --key-permissions create decrypt delete encrypt get list unwrapKey wrapKey
                //
                //then uncomment the following line
                config.AddAzureAppConfiguration(options =>
                {
                    options.Connect(settings["ConnectionString:AppConfig"])
                            .ConfigureKeyVault(kv =>
                            {
                                kv.SetCredential(new DefaultAzureCredential());
                            });
                });
            })
            .UseStartup<Startup>());
    }
}
