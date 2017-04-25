using Android.Widget;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AzureUpload
{
	public partial class MainPage : ContentPage
	{
        MediaFile uploadFile;
        ImageModel imageModel;
        string fileName;
		public MainPage()
		{
			InitializeComponent();
		}

        private async void takePicture_Clicked(object sender, EventArgs e)
        {
            status.Text = "";
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera avaialble.", "OK");
                return;
            }
            fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".jpg";
            uploadFile = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Full,
                Directory = "Sample",
                Name = fileName
            });
            if (uploadFile == null)
                return;
            image.Source = ImageSource.FromFile(uploadFile.Path);
            imageModel = new ImageModel();
            imageModel.Name = fileName;
            imageModel.FilePath = Constants.LocalFilePath+"/"+fileName;
            await ImageDataManager.Manager.SaveImageAsync(imageModel);
        }

        private async void uploadPicture_Clicked(object sender, EventArgs e)
        {
            // Check internet connection is available and then allow
            status.Text = "";
            
            if (uploadFile == null)
            {
                await DisplayAlert("No Picture.", "Please take picture first.", "OK");
                return;
            }
            status.Text = "Upload Started";
            await AzureStorage.UploadFileAsync(ContainerType.Images, uploadFile.GetStream(), fileName);
            imageModel.IsUploaded = true;
            await ImageDataManager.Manager.SaveImageAsync(imageModel);
            
        }
    }
}
