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

        public async Task<FileUploadResult> UploadAsync(Stream fileStream, string fileId, string folder = null)
        {
            var blockBlob = await GetBlockBlob(fileId, folder);
            
            await blockBlob.UploadFromStreamAsync(fileStream);

            return FileUploadResult.Create(blockBlob.Uri);
        }

        public async Task Remove(string fileId, string folder = null)
        {
            var blockBlob = await GetBlockBlob(fileId, folder);

            try
            {
                await blockBlob.DeleteAsync();
            }
            catch (StorageException e)
            {
                throw new FilesStorageException($"The specified file {fileId} does not exist.", e);
            }
        }

        public async Task<FileDownloadResult> DownloadAsync(string fileId, string folder = null)
        {
            var blockBlob = await GetBlockBlob(fileId, folder);

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

        private async Task<CloudBlockBlob> GetBlockBlob(string blobName, string folder)
        {
            var connectionString = _configuration.GetSection("AzureCloud:BlobStorageConnectionString").Value;
            var defaultContainerName = _configuration.GetSection("AzureCloud:DefaultContainerName").Value;
            var containerName =
                string.IsNullOrEmpty(folder)
                    ? defaultContainerName
                    : string.Format(_configuration.GetSection("AzureCloud:ContainerNameFormat").Value, folder);

            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var cloudBlobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = cloudBlobClient.GetContainerReference(containerName);
            var blobRequestOptions = new BlobRequestOptions();
            var operationContext = new OperationContext();

            await blobContainer.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Blob, blobRequestOptions, operationContext);

            return blobContainer.GetBlockBlobReference(blobName);
        }
    }
}
