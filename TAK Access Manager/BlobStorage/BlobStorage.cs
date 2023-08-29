using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
namespace AzureStorage
{
    public class BlobStorage
    {
        public IConfiguration Configuration { get; }

        public BlobStorage(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public CloudBlobContainer AuthBlob()
        {
            CloudBlobContainer blobContainer = new CloudBlobContainer(new Uri("https://dummystorageaccountname.blob.core.usgovcloudapi.net"));
            string authenticationMethod = Configuration["StorageAccountInfo:AuthenticationMethod"];

            if (authenticationMethod == "StorageAccountKey") 
            {

                StorageCredentials storageCredentials =
                    new StorageCredentials(Configuration["StorageAccountInfo:StorageAccountName"],
                    Configuration["StorageAccountInfo:StorageAccountKey"]);

                CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, "core.usgovcloudapi.net", useHttps: true);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                blobContainer = blobClient.GetContainerReference(Configuration["StorageAccountInfo:StorageAccountContainerName"]);

            }
            return blobContainer;
        }

        public string getAcceptedFileTypes()
        {
            return Configuration["StorageAccountInfo:AcceptedFileTypes"].ToString();
        }
        public bool checkExtension(string fileName)
        {
            var extension = $".{fileName.Split('.').Last().ToLower()}";
            var approved = getAcceptedFileTypes().ToLower().Contains(extension);

            return approved;
        }

        public Task<bool> DeleteFile(string fullFileName)
        {
            var blockBlob = AuthBlob().GetBlockBlobReference(fullFileName);

            return blockBlob.DeleteIfExistsAsync();
        }

        public void DeleteAllFiles(List<string> fileNames)
        {
            foreach (var file in fileNames)
            {
                var blockBlob = AuthBlob().GetBlockBlobReference(file);
                blockBlob.DeleteAsync();
            }
        }

        public string GetFile(string fileName)
        {
            var blockBlob = AuthBlob().GetBlockBlobReference(fileName);

            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5);
            sasConstraints.SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(10);
            sasConstraints.Permissions = SharedAccessBlobPermissions.Read;

            var sasBlobToken = blockBlob.GetSharedAccessSignature(sasConstraints);

            string URL = blockBlob.Uri + sasBlobToken;

            return URL;
        }

        public async Task<bool> UploadToBlobStorage(CloudBlockBlob blockBlob, FileStream str)
        {
            try
            {
                await blockBlob.UploadFromStreamAsync(str);
                return true;   
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> FileExists(string blobPath)
        {
            var blockBlob = AuthBlob().GetBlockBlobReference(blobPath);
            var fileExists = blockBlob.ExistsAsync();
            return await fileExists;
        }
    }
}