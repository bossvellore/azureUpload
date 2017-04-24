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
            string filePath = uploadFile.Path;
            //file.Dispose();
            //await DisplayAlert("File Location", file.Path, "OK");
            image.Source = ImageSource.FromFile(filePath);
            /*image.Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                file.Dispose();
                return stream;
            });
            */
        }

        private void uploadPicture_Clicked(object sender, EventArgs e)
        {
            status.Text = "";
            if (uploadFile == null)
            {
                DisplayAlert("No Picture.", "Please take picture first.", "OK");
                return;
            }
            status.Text = "Upload Started";
            AzureStorage.UploadFileAsync(ContainerType.Images, uploadFile.GetStream(), fileName).ContinueWith(r => {
                status.Text = "Upload Completd.";
            });
        }
    }
}
