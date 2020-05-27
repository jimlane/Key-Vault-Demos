using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AspNetKeyVault.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace AspNetKeyVault.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration["TestApp:Settings:StgConnString"]);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_configuration["TestApp:Settings:StgContainerName"]);
            BlobList blobList = new BlobList();
            blobList.AccountName = containerClient.Uri.ToString();
            blobList.ContainerName = containerClient.Name.ToString();
            blobList.Blobs = new List<string>();
            foreach (var blob in containerClient.GetBlobs())
            {
                blobList.Blobs.Add(blob.Name);
            }
            ViewData["blobs"] = blobList;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
