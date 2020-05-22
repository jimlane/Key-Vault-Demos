using System;
using System.Configuration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ConsoleKeyVault
{
    class Program
    {
        static void Main(string[] args)
        {
            //get config settings
            string keyVaultName = ConfigurationManager.AppSettings.Get("KEY_VAULT_NAME");
            string tenId = ConfigurationManager.AppSettings.Get("TENANT_ID");
            string spId = ConfigurationManager.AppSettings.Get("SERVICE_PRINCIPAL_CLIENT_ID");
            string spSec = ConfigurationManager.AppSettings.Get("SERVICE_PRINCIPAL_SECRET");
            string container = ConfigurationManager.AppSettings.Get("CONTAINER_NAME");

            //setup vault uri
            string kvUri = "https://" + keyVaultName + ".vault.azure.net";

            //get access token for vault access
            var credential = new ClientSecretCredential(tenId, spId, spSec);

            //instantiate vault client using access token
            var client = new SecretClient(new Uri(kvUri), credential);

            //retrieve connection string for storage from vault
            KeyVaultSecret secret = client.GetSecret("DemoStoreConnectionString");
            string storageConnectionString = secret.Value;

            //create a BlobServiceClient object for container reference
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);

            //sink reference to specified container
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(container);

            Console.WriteLine("Listing blobs...");

            // List all blobs in the container
            foreach (BlobItem blobItem in containerClient.GetBlobs())
            {
                Console.WriteLine("\t" + blobItem.Name);
            }
        }
    }
}
