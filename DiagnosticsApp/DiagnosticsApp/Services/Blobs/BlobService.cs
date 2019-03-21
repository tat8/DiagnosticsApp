using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace DiagnosticsApp.Services.Blobs
{
    public class BlobService: IBlobService
    {
        private IHostingEnvironment appEnvironment;
        private const string pathTempFolderOriginal = @"\origin\";
        private const string pathTempFolderParsed = @"\parsed\";

        public BlobService(IHostingEnvironment appEnvironment)
        {
            this.appEnvironment = appEnvironment;
        }

        public string UploadBlob(string filename, IFormFile file)
        {
            string reference = CreateReference(filename);
            CloudBlobContainer container = GetCloudBlobContainer();
            CloudBlockBlob blob = container.GetBlockBlobReference(reference);
            using (var fileStream = file.OpenReadStream())
            {
                blob.UploadFromStreamAsync(fileStream).Wait();
            }
            return reference;
        }

        public string UploadBlob(string filename, byte[] data)
        {
            string reference = CreateReference(filename);
            CloudBlobContainer container = GetCloudBlobContainer();
            CloudBlockBlob blob = container.GetBlockBlobReference(reference);
            blob.UploadFromByteArrayAsync(data, 0, data.Length).Wait();
            return reference;
        }

        public string DownloadBlob(string reference)
        {
            var pathPart = pathTempFolderOriginal + reference + ".dcm";
            var path = appEnvironment.WebRootPath + pathPart;

            FileInfo fileInf = new FileInfo(path);
            if (fileInf.Exists)
            {
                return pathPart;
            }

            CloudBlobContainer container = GetCloudBlobContainer();
            CloudBlockBlob blob = container.GetBlockBlobReference(reference);
            var stream = new FileStream(path, FileMode.Create, System.IO.FileAccess.Write);
            using (var fileStream = stream)
            {
                blob.DownloadToStreamAsync(fileStream).Wait();
                stream.Position = 0;
                return pathPart;
            }
        }


        private CloudBlobContainer GetCloudBlobContainer()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            IConfigurationRoot Configuration = builder.Build();
            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse(Configuration["ConnectionStrings:AzureStorageConnectionString-1"]);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("diagnostics");
            return container;
        }

        private string CreateReference(string filename)
        {
            var reference = filename + DateTime.UtcNow;
            reference = reference.Replace(':', ' ');
            return reference;
        }

        public string GetTempFolderOrigin()
        {
            var path = appEnvironment.WebRootPath + pathTempFolderOriginal;
            return path;
        }

        public string GetTempFolderParsed()
        {
            var path = appEnvironment.WebRootPath + pathTempFolderParsed;
            return path;
        }
    }
}
