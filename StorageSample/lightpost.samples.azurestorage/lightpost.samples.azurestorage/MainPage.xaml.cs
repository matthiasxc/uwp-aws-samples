using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using System.Threading.Tasks;
using System.Diagnostics;
using lightpost.samples.azurestorage.Models;
using Windows.Storage;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace lightpost.samples.azurestorage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async Task BasicAzureFileOperationsAsync()
        {
            const string DemoShare = "demofileshare";
            const string DemoDirectory = "demofiledirectory";
            const string ImageToUpload = "HelloWorld.png";

            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(AppConfig.ConnectionString);

            CloudFileClient fileClient = storageAccount.CreateCloudFileClient();
            CloudFileShare share = fileClient.GetShareReference(DemoShare);
            try
            {
                await share.CreateIfNotExistsAsync();
            }
            catch (StorageException)
            {
                Debug.WriteLine("Please make sure your storage account has storage file endpoint enabled and specified correctly in the app.config - then restart the sample.");
            }

            CloudFileDirectory root = share.GetRootDirectoryReference();

            Debug.WriteLine("2. Creating a directory under the root directory");
            CloudFileDirectory dir = root.GetDirectoryReference(DemoDirectory);
            await dir.CreateIfNotExistsAsync();

            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            var realFile = await storageFolder.CreateFileAsync(ImageToUpload, Windows.Storage.CreationCollisionOption.ReplaceExisting);
            Debug.WriteLine("3. Uploading a file to directory");
            CloudFile file = dir.GetFileReference(ImageToUpload);

            await file.UploadFromFileAsync(realFile);


            // List all files/directories under the root directory
            Debug.WriteLine("4. List Files/Directories in root directory");
            List<IListFileItem> results = new List<IListFileItem>();
            FileContinuationToken token = null;
            do
            {
                FileResultSegment resultSegment = await share.GetRootDirectoryReference().ListFilesAndDirectoriesSegmentedAsync(token);
                results.AddRange(resultSegment.Results);
                token = resultSegment.ContinuationToken;
            }
            while (token != null);

            // Print all files/directories listed above
            foreach (IListFileItem listItem in results)
            {
                // listItem type will be CloudFile or CloudFileDirectory
                Debug.WriteLine("- {0} (type: {1})", listItem.Uri, listItem.GetType());
            }

            // Download the uploaded file to your file system
            Debug.WriteLine("5. Download file from {0}", file.Uri.AbsoluteUri);
            var newFile = await storageFolder.CreateFileAsync(string.Format("./CopyOf{0}", ImageToUpload), Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await file.DownloadToFileAsync(newFile);

            // Clean up after the demo 
            Debug.WriteLine("6. Delete file");
            await file.DeleteAsync();

            // When you delete a share it could take several seconds before you can recreate a share with the same
            // name - hence to enable you to run the demo in quick succession the share is not deleted. If you want 
            // to delete the share uncomment the line of code below. 
            // Console.WriteLine("7. Delete Share");
            // await share.DeleteAsync();

        }

        private static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Debug.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                throw;
            }
            catch (ArgumentException)
            {
                Debug.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                throw;
            }

            return storageAccount;
        }
    }
}
