using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using cafenero.Resources;
using Newtonsoft.Json;
using System.IO.IsolatedStorage;

namespace cafenero
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.BackKeyPress += MainPage_BackKeyPress;
        }

        void MainPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            while (this.NavigationService.BackStack.Any())
            {
                this.NavigationService.RemoveBackEntry();
            }
        }

        private void btn_Login(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUserName.Text) || string.IsNullOrEmpty(txtPassword.Password ))
            {
                MessageBox.Show("Kullanıcı adı ve şifre alanları boş geçilemez.");
                return;
            }

            Progress.IsIndeterminate = true;
            txtUserName.IsEnabled = false;
            txtPassword.IsEnabled = false;

            LoginObject log = new LoginObject();
            log.e = txtUserName.Text;
            log.p = txtPassword.Password;

            WebClient web = new WebClient();
            web.Headers["Content-Type"] = "application/json";
            web.Headers["User-Agent"] = "NeroIOS4/1.0.1 CFNetwork/609 Darwin/13.0.0";
            web.UploadStringCompleted += web_UploadStringCompleted;
            string PostData = JsonConvert.SerializeObject(log);

            ((ApplicationBarIconButton)sender).IsEnabled = false;
            web.UploadStringAsync(new Uri("http://api.nero.mekanist.net/v2/user/login", UriKind.Absolute), "POST", PostData, sender);
        }

        void web_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            Progress.IsIndeterminate = false;            

            ((ApplicationBarIconButton)e.UserState).IsEnabled = true;

            try
            {
                LoginResponse deserializedResponse = JsonConvert.DeserializeObject<LoginResponse>(e.Result);
                if (deserializedResponse.error == null)
                {
                    IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
                    if (settings.Contains("ID"))
                    {
                        settings.Remove("ID");
                    }
                    settings.Add("ID", deserializedResponse.data.Id);
                    NavigationService.Navigate(new Uri("/Landing.xaml", UriKind.Relative));
                }
                else
                {
                    MessageBox.Show(deserializedResponse.error.Message);
                    txtUserName.IsEnabled = true;
                    txtPassword.IsEnabled = true;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("İnternet bağlantınızla ilgili bir sorun var");
                txtUserName.IsEnabled = true;
                txtPassword.IsEnabled = true;
            }            
        }

        public class LoginObject
        {
            public string e { get; set; }
            public string p { get; set; }
        }

        public class Data
        {
            public string Id { get; set; }
            public string FName { get; set; }
            public string LName { get; set; }
            public string Email { get; set; }
            public object DisplayMessage { get; set; }
        }

        public class LoginResponse
        {
            public Data data { get; set; }
            public Error error { get; set; }
        }

        public class Error
        {
            public int Code { get; set; }
            public string Message { get; set; }
        }

        private void btn_Register(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Register.xaml", UriKind.Relative));
        }

        private void btn_About(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }
    }
}