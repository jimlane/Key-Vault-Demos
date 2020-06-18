using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration.AzureKeyVault;
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
        // To connect directly to Azure Key Vault set the following environment variable:
        //      KEYVAULT_ENDPOINT https://<kv_name>.vault.azure.net/
        // then uncomment the following code block

            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, builder) =>
                {
                    var keyVaultEndpoint = getKeyVaultEndpoint();
                    if (!string.IsNullOrEmpty(keyVaultEndpoint))
                    {
                        var azureServiceTokenProvider = new AzureServiceTokenProvider();
                        var keyVaultClient = new KeyVaultClient(
                            new KeyVaultClient.AuthenticationCallback(
                                azureServiceTokenProvider.KeyVaultTokenCallback));
                        builder.AddAzureKeyVault(
                            keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());
                    }
                })
                .ConfigureWebHostDefaults(webBuilder => 
                {
                    webBuilder.UseStartup<Startup>();
                });
        
            private static string getKeyVaultEndpoint() => Environment.GetEnvironmentVariable("KEYVAULT_ENDPOINT");


            // To use an Azure App Configuration to connect to Azure Key Vault set the following environment variable:
            //      ConnectionString:AppConfig <ac_connection_string>
            // then uncomment the following code block
/*
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            webBuilder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                var settings = config.Build();
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

            //IF YOU WANT TO USE MANAGED IDENTITIES FOR ACCESS INSTEAD
            //for local runs you'll need to first run these commands:
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
    */
    }
}
