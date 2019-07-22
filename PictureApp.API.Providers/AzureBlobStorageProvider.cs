using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var blockBlob = await GetOrCreateBlockBlob(fileId, folder);

            await blockBlob.UploadFromStreamAsync(fileStream);

            return FileUploadResult.Create(blockBlob.Uri);
        }

        public async Task Remove(string fileId, string folder = null)
        {
            var blobContainerAndBlockBlob = GetBlobContainerAndBlockBlob(fileId, folder);
            await Validate(blobContainerAndBlockBlob.cloudBlobContainer ,blobContainerAndBlockBlob.blockBlob);

            try
            {
                await blobContainerAndBlockBlob.blockBlob.DeleteAsync();
            }
            catch (StorageException e)
            {
                throw new FilesStorageException($"The specified file {fileId} can not be removed.", e);
            }
        }

        public async Task<FileDownloadResult> DownloadAsync(string fileId, string folder = null)
        {
            var blobContainerAndBlockBlob = GetBlobContainerAndBlockBlob(fileId, folder);
            await Validate(blobContainerAndBlockBlob.cloudBlobContainer, blobContainerAndBlockBlob.blockBlob);

            var stream = new MemoryStream();

            try
            {
                await blobContainerAndBlockBlob.blockBlob.DownloadToStreamAsync(stream);
            }
            catch (StorageException e)
            {
                throw new FilesStorageException($"The specified file {fileId} can not be downloaded.", e);
            }

            return FileDownloadResult.Create(stream, fileId);
        }

        public async Task<IEnumerable<FileItemResult>> GetFiles(string folder)
        {
            if (string.IsNullOrEmpty(folder))
            {
                throw new ArgumentException("The folder can not be null or empty");
            }

            var blobContainer = GetBlobContainer(folder);
            var continuationToken = new BlobContinuationToken();
            var blobResultSegment = await blobContainer.ListBlobsSegmentedAsync(continuationToken);

            return blobResultSegment.Results.Cast<CloudBlockBlob>().Select(x => FileItemResult.Create(x.Name));
        }

        private async Task<CloudBlockBlob> GetOrCreateBlockBlob(string blobName, string folder)
        {
            var blobContainerAndBlockBlob = GetBlobContainerAndBlockBlob(blobName, folder);
            var blobRequestOptions = new BlobRequestOptions();
            var operationContext = new OperationContext();

            await blobContainerAndBlockBlob.cloudBlobContainer.CreateIfNotExistsAsync(
                BlobContainerPublicAccessType.Blob, blobRequestOptions, operationContext);

            return blobContainerAndBlockBlob.cloudBlobContainer.GetBlockBlobReference(blobName);
        }

        private (CloudBlobContainer cloudBlobContainer, CloudBlockBlob blockBlob) GetBlobContainerAndBlockBlob(
            string blobName, string folder)
        {
            var blobContainer = GetBlobContainer(folder);

            return (blobContainer, blobContainer.GetBlockBlobReference(blobName));            
        }

        private CloudBlobContainer GetBlobContainer(string folder = "")
        {
            var connectionString = _configuration.GetSection("AzureCloud:BlobStorageConnectionString").Value;
            var defaultContainerName = _configuration.GetSection("AzureCloud:DefaultContainerName").Value;
            var containerName =
                string.IsNullOrEmpty(folder)
                    ? defaultContainerName
                    : string.Format(_configuration.GetSection("AzureCloud:ContainerNameFormat").Value, folder);
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var cloudBlobClient = storageAccount.CreateCloudBlobClient();
            
            return cloudBlobClient.GetContainerReference(containerName);
        }

        private async Task Validate(CloudBlobContainer blobContainer, CloudBlockBlob blockBlob)
        {
            if (!await blobContainer.ExistsAsync())
            {
                throw new FilesStorageException($"The specified folder {blobContainer.Name} does not exist.");
            }

            if (!await blockBlob.ExistsAsync())
            {
                throw new FilesStorageException($"The specified file {blockBlob.Name} does not exist.");
            }
        }
    }
}
