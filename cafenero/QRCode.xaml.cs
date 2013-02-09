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
using System.Windows.Media.Imaging;

namespace cafenero
{
    public partial class QRCode : PhoneApplicationPage
    {
        public QRCode()
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

            Progress.IsIndeterminate = true;

            string URL = string.Format("http://api.nero.mekanist.net/v2/QR/who/{0}",settings["ID"]);

            WebClient Download = new WebClient();
            Download.DownloadStringCompleted += Download_DownloadStringCompleted;
            Download.DownloadStringAsync(new Uri(URL, UriKind.Absolute));

            base.OnNavigatedTo(e);
        }

        void Download_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                string ReturnHTML = e.Result;
                int StartIndex = ReturnHTML.IndexOf("QRCodeImage.cshtml");
                string FoundImageURL = ReturnHTML.Substring(StartIndex, ReturnHTML.IndexOf("*end") - StartIndex);
                string FinalURL = string.Format("http://api.nero.mekanist.net/v2/{0}*end", FoundImageURL);

                BitmapImage IMG = new BitmapImage(new Uri(FinalURL,UriKind.Absolute));
                IMG.ImageOpened += IMG_ImageOpened;
                QRImage.Source = IMG;
            }
            catch (Exception)
            {
                MessageBox.Show("İnternet bağlantınızda sorun var. Lütfen daha sonra tekrar deneyiniz.");
                NavigationService.GoBack();
            }
        }

        void IMG_ImageOpened(object sender, RoutedEventArgs e)
        {
            Progress.IsIndeterminate = false;
        }
    }
}