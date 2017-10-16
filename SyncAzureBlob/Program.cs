using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Threading.Tasks;

namespace SyncAzureBlob
{
    class Program
    {
        static void Main(string[] args)
        {
            string storeToPath = "StoreIn".GetSettings();
            string[] runInterval = "RunInterval".GetSettings().Split(':');
            while(true)
            {
                ProcessBlobFiles(storeToPath).Wait();
                Task.Delay(new TimeSpan(int.Parse(runInterval[0]), int.Parse(runInterval[1]), int.Parse(runInterval[2]), int.Parse(runInterval[3]))).Wait();
            }
        }

        static async Task ProcessBlobFiles(string storeToPath)
        {
            try
            {
                // Parse the connection string and return a reference to the storage account.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                    CloudConfigurationManager.GetSetting("StorageConnectionString"));

                // Create a blob client
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve a reference to a container.
                CloudBlobContainer container = blobClient.GetContainerReference("logs");

                // Probably this is not required
                //container.CreateIfNotExists();

                // Loop over items within the container and output the length and URI.
                foreach (IListBlobItem item in container.ListBlobs(null, false))
                {
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        CloudBlockBlob blob = (CloudBlockBlob)item;

                        Console.WriteLine("Downloding: '{0}'", blob.Name);

                        // Retrieve reference to a blob named "photo1.jpg".
                        CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob.Name);

                        // Save blob contents to a file.
                        string toSave = storeToPath + blob.Name;
                        using (var fileStream = System.IO.File.OpenWrite(toSave))
                        {
                            await blockBlob.DownloadToStreamAsync(fileStream);
                        }
                        Console.WriteLine("Download completed for: {0}", blob.Name);
                        await blob.DeleteAsync();
                        Console.WriteLine("Download completed for: {0}", blob.Name);
                    }
                    else if (item.GetType() == typeof(CloudPageBlob))
                    {
                        CloudPageBlob pageBlob = (CloudPageBlob)item;

                        Console.WriteLine("Page blob of length {0}: {1}", pageBlob.Properties.Length, pageBlob.Uri);

                    }
                    else if (item.GetType() == typeof(CloudBlobDirectory))
                    {
                        CloudBlobDirectory directory = (CloudBlobDirectory)item;

                        Console.WriteLine("Directory: {0}", directory.Uri);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Encountered error: {0}", ex.Message);
            }
        }
    }
}
