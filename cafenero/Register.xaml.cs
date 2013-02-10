using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using System.IO.IsolatedStorage;
using System.Device.Location;

namespace cafenero
{
    public partial class Register : PhoneApplicationPage
    {
        public Register()
        {
            InitializeComponent();
        }

        GeoCoordinate currentLocation;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            GeoCoordinateWatcher geoWatch = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
            geoWatch.MovementThreshold = 500;
            geoWatch.PositionChanged += geoWatch_PositionChanged;
            geoWatch.Start();

            base.OnNavigatedTo(e);
        }

        void geoWatch_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            currentLocation = e.Position.Location;
        }

        private void btn_Register(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFirstName.Text) || string.IsNullOrEmpty(txtLastName.Text) || string.IsNullOrEmpty(txtPassword.Password) || string.IsNullOrEmpty(txtUserName.Text))
            {
                MessageBox.Show("Lütfen tüm bilgileri doldurunuz.");
                return;
            }

            Progress.IsIndeterminate = true;
            ((ApplicationBarIconButton)sender).IsEnabled = false;
            txtFirstName.IsEnabled = false;
            txtLastName.IsEnabled = false;
            txtPassword.IsEnabled = false;
            txtUserName.IsEnabled = false;

            RegisterObject reg = new RegisterObject();
            reg.ln = txtLastName.Text;
            reg.fn = txtFirstName.Text;
            reg.e = txtUserName.Text;
            reg.p = txtPassword.Password;
            
            WebClient web = new WebClient();
            web.Headers["Content-Type"] = "application/json";
            web.Headers["User-Agent"] = "NeroIOS4/1.0.1 CFNetwork/609.1.4 Darwin/13.0.0";
            web.UploadStringCompleted += web_UploadStringCompleted;
            string PostData = JsonConvert.SerializeObject(reg);
                        
            string PostURL;
            if (currentLocation != null)
            {
                PostURL = string.Format("http://api.nero.mekanist.net/v2/user/register?lat={0}&lng={1}", currentLocation.Latitude, currentLocation.Longitude);
            }
            else
            {
                PostURL = "http://api.nero.mekanist.net/v2/user/register";
            }
            web.UploadStringAsync(new Uri(PostURL, UriKind.Absolute), "POST", PostData, sender);
        }

        void web_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            Progress.IsIndeterminate = false;

            ((ApplicationBarIconButton)e.UserState).IsEnabled = true;

            try
            {
                RegisterResponseObject deserializedResponse = JsonConvert.DeserializeObject<RegisterResponseObject>(e.Result);
                if (deserializedResponse.error == null)
                {
                    IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
                    if (settings.Contains("ID"))
                    {
                        settings.Remove("ID");
                    }
                    settings.Add("ID", deserializedResponse.data.Id);
                    MessageBox.Show(deserializedResponse.data.DisplayMessage);
                    NavigationService.Navigate(new Uri("/Landing.xaml", UriKind.Relative));
                }
                else
                {
                    MessageBox.Show(deserializedResponse.error.Message);
                    txtFirstName.IsEnabled = true;
                    txtLastName.IsEnabled = true;
                    txtPassword.IsEnabled = true;
                    txtUserName.IsEnabled = true;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("İnternet bağlantınızla ilgili bir sorun var");
                txtFirstName.IsEnabled = true;
                txtLastName.IsEnabled = true;
                txtPassword.IsEnabled = true;
                txtUserName.IsEnabled = true;
            }

        }

        private void btn_Login(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Login.xaml", UriKind.Relative));
        }

        private void btn_About(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        public class RegisterObject
        {
            public string ln { get; set; }
            public string p { get; set; }
            public string e { get; set; }
            public string fn { get; set; }
        }

        public class Data
        {
            public string Id { get; set; }
            public string FName { get; set; }
            public string LName { get; set; }
            public string Email { get; set; }
            public string DisplayMessage { get; set; }
        }

        public class RegisterResponseObject
        {
            public Data data { get; set; }
            public Error error { get; set; }
        }

        public class Error
        {
            public int Code { get; set; }
            public string Message { get; set; }
        }
    }
}