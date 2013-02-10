using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using Newtonsoft.Json;

namespace cafenero
{
    public partial class Landing : PhoneApplicationPage
    {
        public Landing()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            if (!settings.Contains("ID"))
            {
                NavigationService.Navigate(new Uri("/Login.xaml", UriKind.Relative));
                return;
            }

            CheckPromotions();
            base.OnNavigatedTo(e);
        }

        void CheckPromotions()
        {
            Progress.IsIndeterminate = true;

            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

            WebClient web = new WebClient();
            web.Headers["Accept-Language"] = "en-us";
            web.Headers["User-Agent"] = "NeroIOS4/1.0.1 CFNetwork/609.1.4 Darwin/13.0.0";
            web.DownloadStringCompleted += web_DownloadStringCompleted;
            web.DownloadStringAsync(new Uri(string.Format("http://api.nero.mekanist.net/v2/loyality/promocards/{0}", settings["ID"]), UriKind.Absolute));
        }

        void web_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Progress.IsIndeterminate = false;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = true;

            try
            {
                PromoCardObject deserializedResponse = JsonConvert.DeserializeObject<PromoCardObject>(e.Result);
                if (deserializedResponse.error == null)
                {
                    foreach (Image item in stampCanvas.Children)
                    {
                        item.Opacity = 0;
                    }
                    for (int i = 0; i < deserializedResponse.data[0].AvaliableCount; i++)
                    {
                        stampCanvas.Children[i].Opacity = 1;
                    }
                }
                else
                {
                    MessageBox.Show(deserializedResponse.error.Message);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("İnternet bağlantınızla ilgili bir sorun var");
            }            
        }

        private void btn_QR(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/QRCode.xaml", UriKind.Relative));
        }

        private void btn_Logout(object sender, EventArgs e)
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            if (settings.Contains("ID"))
            {
                settings.Remove("ID");
                NavigationService.Navigate(new Uri("/Login.xaml", UriKind.Relative));
            }
        }

        private void btn_About(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        public class Datum
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public int MinReqItCount { get; set; }
            public int AvaliableCount { get; set; }
            public int DirtyBit { get; set; }
        }

        public class PromoCardObject
        {
            public List<Datum> data { get; set; }
            public Error error { get; set; }
        }

        public class Error
        {
            public int Code { get; set; }
            public string Message { get; set; }
        }

        private void btn_Refresh(object sender, EventArgs e)
        {
            ((ApplicationBarIconButton)sender).IsEnabled = false;
            CheckPromotions();
        }
    }
}