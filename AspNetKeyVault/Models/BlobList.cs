using System;
using System.Collections.Generic;

namespace AspNetKeyVault.Models
{
    public class BlobList 
    {
        public string AccountName { get; set; }
        public string ContainerName { get; set; }
        public List<String> Blobs { get; set; }
    }
}