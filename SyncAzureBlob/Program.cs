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
            bool isDeleteRequired = Convert.ToBoolean("IsDeleteRequired".GetSettings());
            string storeToPath = "StoreIn".GetSettings();
            string runInterval = string.Empty;
            try
            {
                runInterval = "RunInterval".GetSettings();
            }
            catch(Exception)
            {
                Console.WriteLine("Couldn't find RunInterval in config. So, the program will run only once.");
            }
            string containerNames = "ContainerNames".GetSettings();
            if (string.IsNullOrEmpty(runInterval) ||
                (!string.IsNullOrEmpty(runInterval) && "0:0:0:0".Equals(runInterval)))
            {
                ProcessBlobFilesWithinContainer(storeToPath, containerNames, isDeleteRequired).Wait();
            }
            else
            {
                string[] runIntervals = runInterval.Split(':');
                while (true)
                {
                    ProcessBlobFilesWithinContainer(storeToPath, containerNames, isDeleteRequired).Wait();
                    Task.Delay(new TimeSpan(int.Parse(runIntervals[0]), int.Parse(runIntervals[1]), int.Parse(runIntervals[2]), int.Parse(runIntervals[3]))).Wait();
                }
            }
        }

        static async Task ProcessBlobFilesWithinContainer(string storeToPath, string containerNames, bool isDeleteRequired)
        {
            var containers = containerNames.Split(':');
            foreach(var container in containers)
            {
                await ProcessBlobFiles(storeToPath, containerNames, isDeleteRequired);
            }
        }

        static async Task ProcessBlobFiles(string storeToPath, string containerName, bool isDeleteRequired)
        {
            try
            {
                // Parse the connection string and return a reference to the storage account.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                    CloudConfigurationManager.GetSetting("StorageConnectionString"));

                // Create a blob client
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve a reference to a container.
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);

                // Probably this is not required
                //container.CreateIfNotExists();
                
                // Loop over items within the container and output the length and URI.
                foreach (IListBlobItem item in container.ListBlobs(null, useFlatBlobListing: true))
                {
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        CloudBlockBlob blob = (CloudBlockBlob)item;

                        Console.WriteLine("Downloding: '{0}'", blob.Name);

                        // Retrieve reference to a blob named "photo1.jpg".
                        CloudBlockBlob blockBlob = container.GetBlockBlobReference(blob.Name);

                        // Save blob contents to a file.
                        string toSave = storeToPath + blob.Name.Replace('/', '\\');
                        string path = toSave.Substring(0, toSave.LastIndexOf('\\'));

                        bool exists = System.IO.Directory.Exists(path);
                        if (!exists)
                            System.IO.Directory.CreateDirectory(path);
                        using (var fileStream = System.IO.File.OpenWrite(toSave))
                        {
                            await blockBlob.DownloadToStreamAsync(fileStream);
                        }
                        Console.WriteLine("Download completed for: {0}", blob.Name);
                        if (isDeleteRequired)
                        {
                            await blob.DeleteAsync();
                            Console.WriteLine("Deleted: {0}", blob.Name);
                        }
                    }
                    //else if (item.GetType() == typeof(CloudPageBlob))
                    //{
                    //    CloudPageBlob pageBlob = (CloudPageBlob)item;

                    //    Console.WriteLine("Page blob of length {0}: {1}", pageBlob.Properties.Length, pageBlob.Uri);

                    //}
                    //else if (item.GetType() == typeof(CloudBlobDirectory))
                    //{
                    //    CloudBlobDirectory directory = (CloudBlobDirectory)item;

                    //    var subPath = storeToPath + directory.Prefix.Substring(0, directory.Prefix.Length - 1).Replace('/', '\\');
                    //    bool exists = System.IO.Directory.Exists(subPath);

                    //    if (!exists)
                    //        System.IO.Directory.CreateDirectory(subPath);

                    //    Console.WriteLine("Directory: {0}", directory.Uri);
                    //}
                    Console.WriteLine("{0}{0}", Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Encountered error: {0}", ex.Message);
            }
        }
    }
}