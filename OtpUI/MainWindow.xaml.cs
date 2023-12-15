using Example.WebAPI.Controllers;
using Example.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http.Json;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Threading;
using System.Globalization;

namespace OtpUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TimeSpan timeLeft = new TimeSpan();
        DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
        }


        void StartTimer(string expiryDate)
        {
            InfoTxtB.Text = "";


            DateTime parsedDate;
            if (DateTime.TryParseExact(expiryDate, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                timeLeft = parsedDate - DateTime.Now;
            }
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += OnTick;
            timer.Start();
        }

        void OnTick(object sender, EventArgs e)
        {
            TimeLeftTxtB.Text = $"{timeLeft.TotalMinutes:00}:{timeLeft.Seconds:00}";
            timeLeft -= TimeSpan.FromSeconds(1);

            if (timeLeft.TotalSeconds <= 0)
            {
                TimeLeftTxtB.Text = "expired";
                timer.Stop();
            }
        }

        private async void AddNewUser(User user)
        {
            try
            {
                InfoTxtB.Text = "";

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5249");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.PostAsJsonAsync("api/Otp/CreateUser", user);

                    if (response.IsSuccessStatusCode)
                    {
                        var rawResponse = await response.Content.ReadAsStringAsync();

                        var result = JsonConvert.DeserializeObject<dynamic>(rawResponse);

                        string code = result.code.code;
                        string expireDate = result.expireDate;


                        NewCodeTxtB.Text = $"{code}";
                        StartTimer(expireDate);
                        
                        
                        GetAllUsers();
                    }

                    else
                    {
                        InfoTxtB.Text += $"Failed to add user. Request URL: {client.BaseAddress}api/Otp/CreateUser\n";
                        InfoTxtB.Text += $"User details: {Newtonsoft.Json.JsonConvert.SerializeObject(user)}\n";
                        InfoTxtB.Text += await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                InfoTxtB.Text = $"HttpRequestException: {ex.Message}";

                if (ex.InnerException is System.IO.IOException ioException)
                {
                    InfoTxtB.Text += $"\nIOException: {ioException.Message}";
                }
            }
            catch (Exception ex)
            {
                InfoTxtB.Text += ex.ToString();
            }
        }


        private void LoginUser(User user)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5249");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string code = LoginCodeTxtB.Text;
            HttpResponseMessage response = client.PostAsync($"api/Otp/VerifyUser?userId={user.Id}&code={code}", null).Result;

            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                InfoTxtB.Text = result;
                GetAllUsers(); // Refresh the user list after a successful login
            }
            else
            {
                InfoTxtB.Text = ("User ID or Code is not valid");
            }
        }


        private async void GetAllUsers()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5249");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync("api/Otp/GetUsers");

            if (response.IsSuccessStatusCode)
            {
                var fetchedUsers = await response.Content.ReadFromJsonAsync<ObservableCollection<User>>();
                UserListView.ItemsSource = fetchedUsers;
            }
            else
            {
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }
        }


        private async void RefreshUserCode(User user)
        {
            try
            {
                InfoTxtB.Text = "";

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5249");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    
                    string code = LoginCodeTxtB.Text;

                    var data = new { code = code };
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync($"api/Otp/RefreshUser/{user.Id}", jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
                        var rawResponse = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<dynamic>(rawResponse);

                        string otp = result.code.code;
                        string expireDate = result.expireDate;

                        NewCodeTxtB.Text = $"{otp}";
                        StartTimer(expireDate);
                        GetAllUsers();

                    }
                    else
                    {
                        InfoTxtB.Text += $"Failed to refresh user. Request URL: {client.BaseAddress}api/Otp/RefreshUser/{user.Id}\n";
                        InfoTxtB.Text += await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                InfoTxtB.Text = $"HttpRequestException: {ex.Message}";

                if (ex.InnerException is System.IO.IOException ioException)
                {
                    InfoTxtB.Text += $"\nIOException: {ioException.Message}";
                }
            }
            catch (Exception ex)
            {
                InfoTxtB.Text += ex.ToString();
            }
        }


        // #######################################################################################
        private void AddUserBtn_Click(object sender, RoutedEventArgs e)
        {
            AddNewUser(new User(AddUserNameTxtB.Text));
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            int id;
            int.TryParse(LoginTxtB.Text, out id);
            User user = new User("") { Id = id };
            LoginUser(user);
        }

        private void RefreshUserBtn_Click(object sender, RoutedEventArgs e)
        {
            if(UserListView.SelectedItem != null)
            {
                RefreshUserCode(UserListView.SelectedItem as User);   
            }
        }

        private void CopyCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(NewCodeTxtB.Text);
        }

        private User? GetSelectedUser()
        {
            if(UserListView.SelectedItem != null)
            {
                return (UserListView.SelectedItem as User);
            }
            return null;
        }

        private void UserListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AddUserNameTxtB.Text = GetSelectedUser()?.FullName;
            if(GetSelectedUser() != null)
            {
                RefreshUserBtn.IsEnabled = true;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
        }
    }
}