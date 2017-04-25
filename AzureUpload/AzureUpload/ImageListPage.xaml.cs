using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AzureUpload
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ImageListPage : ContentPage
	{
		public ImageListPage ()
		{
			InitializeComponent ();
		}

        public async void OnItemAdded(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new MainPage());
            }
            catch(Exception ex)
            {
                var a = ex;
            }
        }

        public async void OnSync(object sender, EventArgs e)
        {
            // Check internet connection is available and then allow
            await ImageDataManager.Manager.SyncFilesAsync(true);
            imageListView.ItemsSource = await ImageDataManager.Manager.GetImagessAsync();
        }

        protected async override void OnAppearing()
        {
            try
            {
                base.OnAppearing();
                //await ImageDataManager.Manager.SyncAsync();
                imageListView.ItemsSource = await ImageDataManager.Manager.GetImagessAsync();
            }
            catch(Exception ex)
            {
                var a = ex;
            }
        }
    }
}
