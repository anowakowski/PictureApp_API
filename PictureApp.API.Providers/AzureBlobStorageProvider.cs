using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using PictureApp.API.Providers.Exceptions;

namespace PictureApp.API.Providers
{
    public class AzureBlobStorageProvider : IFilesStorageProvider
    {
        private readonly IConfiguration _configuration;

        public AzureBlobStorageProvider(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<FileUploadResult> UploadAsync(Stream fileStream, string fileId)
        {
            var blockBlob = GetBlockBlob(fileId);
            
            await blockBlob.UploadFromStreamAsync(fileStream);

            return FileUploadResult.Create(blockBlob.Uri);
        }

        public async Task Remove(string fileId)
        {
            var blockBlob = GetBlockBlob(fileId);

            try
            {
                await blockBlob.DeleteAsync();
            }
            catch (StorageException e)
            {
                throw new FilesStorageException($"The specified file {fileId} does not exist.", e);
            }
        }

        public async Task<FileDownloadResult> DownloadAsync(string fileId)
        {
            var blockBlob = GetBlockBlob(fileId);

            var stream = new MemoryStream();

            try
            {
                await blockBlob.DownloadToStreamAsync(stream);
            }
            catch (StorageException e)
            {
                throw new FilesStorageException($"The specified file {fileId} does not exist.", e);
            }

            return FileDownloadResult.Create(stream, fileId);
        }

        private CloudBlockBlob GetBlockBlob(string blobName)
        {
            var connectionString = _configuration.GetSection("AzureCloud:BlobStorageConnectionString").Value;
            var containerName = _configuration.GetSection("AzureCloud:ContainerName").Value;

            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var cloudBlobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = cloudBlobClient.GetContainerReference(containerName);
            return blobContainer.GetBlockBlobReference(blobName);
        }
    }
}
