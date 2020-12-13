using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;


namespace CommonLibrary.Storage
{
    public class StorageBlobUtil
    {
        private BlobClient blobClient;
        private Uri connectionString;
        private BlobContainerClient blobContainerClient;
        private BlobServiceClient blobServiceClient;

        // Constructor
        public StorageBlobUtil(string connectionString)
        {
            blobServiceClient = new BlobServiceClient(connectionString);
        }

        // Constructor Override
        public StorageBlobUtil(Uri connectionString)
        {
            blobServiceClient = new BlobServiceClient(connectionString);
        }

        // Constructor Override
        public StorageBlobUtil(string connectionString, string containerName)
        {
            blobContainerClient = new BlobContainerClient(connectionString, containerName);
        }

        // static method
        public static BlobClient GetBlobClient(string connectionString, string containerName, string fileName)
        {
            var blobContainerClient = new BlobContainerClient(connectionString, containerName);
            return blobContainerClient.GetBlobClient(fileName);

        }

        public BlobClient SetBlobClient(string fileName)
        {
            blobClient = blobContainerClient.GetBlobClient(fileName);
            return blobClient;
        }

        public async Task<BlobContainerClient> CreateBlobContainer(string containerName)
        {
            // Create the container and return a container client object
            blobContainerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
            return blobContainerClient;
        }

        public async Task<BlobContainerClient> CreateBlobClient(string connectionString, string containerName)
        {
            // Create the container and return a container client object
            blobContainerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
            return blobContainerClient;
        }


        public Task<Azure.Response<BlobContentInfo>> UploadBlobFromMemStream(MemoryStream memoryStream)
        {
            return blobClient.UploadAsync(memoryStream);
        }

        public Task<Azure.Response<BlobContentInfo>> UploadBlobFromJsonString(string str)
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(str));
            return blobClient.UploadAsync(ms);
            
        }
    }
}
