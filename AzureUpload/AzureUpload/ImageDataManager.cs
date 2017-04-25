using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using Plugin.Media;
using Plugin.Media.Abstractions;
//using static System.Net.Mime.MediaTypeNames;

namespace AzureUpload
{
    class ImageDataManager
    {
        static ImageDataManager defaultInstance = new ImageDataManager();
        IMobileServiceClient client;
        IMobileServiceSyncTable<ImageModel> imageTable;
        ContainerType containerType = ContainerType.Images;

        string localFileLocation;
        
    private ImageDataManager()
        {
            this.client = new MobileServiceClient(Constants.ApplicationURL);
            var store = new MobileServiceSQLiteStore("localstore.db");
            store.DefineTable<ImageModel>();
            this.client.SyncContext.InitializeAsync(store);
            this.imageTable = client.GetSyncTable<ImageModel>();
            // Should use PCL or Shared code to get OS dependent directory..
            var localDirectory = Android.OS.Environment.GetExternalStoragePublicDirectory(Constants.LocalFilePath);
            if (localDirectory.Exists() == false)
            {
                DirectoryInfo fi = Directory.CreateDirectory(localDirectory.AbsolutePath);
            }
            localFileLocation = localDirectory.AbsolutePath;
        }

        public static ImageDataManager Manager { get { return defaultInstance; } }

        public async Task SaveImageAsync(ImageModel imageR)
        {
            if (imageR.Id == null)
            {
                await imageTable.InsertAsync(imageR);
            }
            else
            {
                await imageTable.UpdateAsync(imageR);
            }
        }

        public async Task<ObservableCollection<ImageModel>> GetImagessAsync(bool syncItems = false)
        {
            try
            {
                if (syncItems)
                {
                    await this.SyncAsync();
                }
                IEnumerable<ImageModel> items = await imageTable
                                    .ToEnumerableAsync();

                return new ObservableCollection<ImageModel>(items);
            }
            catch (MobileServiceInvalidOperationException msioe)
            {
                Debug.WriteLine(@"Invalid sync operation: {0}", msioe.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(@"Sync error: {0}", e.Message);
            }
            return null;
        }

        public async Task SyncAsync()
        {
            ReadOnlyCollection<MobileServiceTableOperationError> syncErrors = null;

            try
            {
                await this.client.SyncContext.PushAsync();

                // The first parameter is a query name that is used internally by the client SDK to implement incremental sync.
                // Use a different query name for each unique query in your program.
                await this.imageTable.PullAsync("allImages", this.imageTable.CreateQuery());
            }
            catch (MobileServicePushFailedException exc)
            {
                if (exc.PushResult != null)
                {
                    syncErrors = exc.PushResult.Errors;
                }
            }

            // Simple error/conflict handling.
            if (syncErrors != null)
            {
                foreach (var error in syncErrors)
                {
                    if (error.OperationKind == MobileServiceTableOperationKind.Update && error.Result != null)
                    {
                        // Update failed, revert to server's copy
                        await error.CancelAndUpdateItemAsync(error.Result);
                    }
                    else
                    {
                        // Discard local change
                        await error.CancelAndDiscardItemAsync();
                    }

                    Debug.WriteLine(@"Error executing sync operation. Item: {0} ({1}). Operation discarded.", error.TableName, error.Item["id"]);
                }
            }
        }

        public async Task<List<ImageModel>> GetPendingImages()
        {
            return await imageTable.Where(image => !image.IsUploaded).ToListAsync();
        }
        public async Task SyncFilesAsync(bool download = false, bool delete = false)
        {
            try
            {
                //Uploading local images
                List<ImageModel> pendingImageList = await GetPendingImages();
                foreach (ImageModel pendingImage in pendingImageList)
                {
                    await AzureStorage.UploadFileAsync(containerType, File.ReadAllBytes(Android.OS.Environment.GetExternalStoragePublicDirectory( pendingImage.FilePath ).AbsolutePath), pendingImage.Name)
                        .ContinueWith(async (T) =>
                        {
                            if (T.IsCompleted == true)
                            {
                                pendingImage.IsUploaded = true;
                                await SaveImageAsync(pendingImage);
                            }
                        });

                }
                //Sync database changes
                await SyncAsync();
                //Downloading missing images
                if (download == true)
                {
                    IEnumerable<ImageModel> imageList = await GetImagessAsync();
                    DirectoryInfo d = new DirectoryInfo(localFileLocation);//Assuming Test is your Folder
                    FileInfo[] Files = d.GetFiles(); //Getting Text files

                    foreach (ImageModel image in imageList)
                    {
                        bool imageFileFound = false;
                        foreach (FileInfo file in Files)
                        {

                            if (file.Name == image.Name)
                            {
                                imageFileFound = true;
                                continue;
                            }
                        }
                        if (imageFileFound == false)
                        {
                            //download file;
                            byte[] downloadedImageByte = await AzureStorage.GetFileAsync(containerType, image.Name);
                            File.WriteAllBytes(Path.Combine(localFileLocation, image.Name), downloadedImageByte);
                        }
                    }
                }

                //Deleting extra files

            }
            catch(Exception ex)
            {
                var a = ex;
            }

        }
    }
}
