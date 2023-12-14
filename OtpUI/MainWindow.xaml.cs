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

namespace OtpUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void AddUserBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InfoTxtB.Text = "";

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5249");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    User newUser = new User(AddUserNameTxtB.Text);
                    HttpResponseMessage response = await client.PostAsJsonAsync("api/Otp/CreateUser", newUser);

                    if (response.IsSuccessStatusCode)
                    {
                        var rawResponse = await response.Content.ReadAsStringAsync();

                        var result = JsonConvert.DeserializeObject<dynamic>(rawResponse);

                        string code = result.code.code;
                        string expireDate = result.expireDate;

                        NewCodeTxtB.Text = $"{code}";
                        TimeLeftTxtB.Text = $"{expireDate}";
                        GetData();
                    }

                    else
                    {
                        InfoTxtB.Text += $"Failed to add user. Request URL: {client.BaseAddress}api/Otp/CreateUser\n";
                        InfoTxtB.Text += $"User details: {Newtonsoft.Json.JsonConvert.SerializeObject(newUser)}\n";
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
                // Log the exception for further analysis
                InfoTxtB.Text += ex.ToString();
            }
        }





        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5249");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            int userId;
            if (int.TryParse(LoginTxtB.Text, out userId))
            {
                string code = LoginCodeTxtB.Text;

                HttpResponseMessage response = client.PostAsync($"api/Otp/VerifyUser?userId={userId}&code={code}", null).Result;



                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    InfoTxtB.Text = result;
                    GetData(); // Refresh the user list after a successful login
                }
                else
                {
                    InfoTxtB.Text = ("User ID or Code is not valid");
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid user ID.");
            }
        }


        private async void GetData()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5249");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.GetAsync("api/Otp/GetUsers");

            if (response.IsSuccessStatusCode)
            {
                var users = await response.Content.ReadFromJsonAsync<IEnumerable<User>>();
                UserListBox.ItemsSource = users;
            }
            else
            {
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }
        }


        private async void RefreshUserBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InfoTxtB.Text = "";

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5249");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    int userId;
                    if (int.TryParse(LoginTxtB.Text, out userId))
                    {
                        string code = LoginCodeTxtB.Text;

                        // Create a data object to send in the request body
                        var data = new { code = code };

                        // Serialize the data object to JSON
                        var jsonContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

                        // Send the POST request with the JSON content in the request body
                        HttpResponseMessage response = await client.PostAsync($"api/Otp/RefreshUser/{userId}", jsonContent);

                        // Handle the response as needed
                        if (response.IsSuccessStatusCode)
                        {
                            var rawResponse = await response.Content.ReadAsStringAsync();
                            var result = JsonConvert.DeserializeObject<dynamic>(rawResponse);

                            string otp = result.code.code;
                            string expireDate = result.expireDate;

                            NewCodeTxtB.Text = $"{otp}";
                            TimeLeftTxtB.Text = $"{expireDate}";
                            GetData();

                        }
                        else
                        {
                            InfoTxtB.Text += $"Failed to refresh user. Request URL: {client.BaseAddress}api/Otp/RefreshUser/{userId}\n";
                            InfoTxtB.Text += await response.Content.ReadAsStringAsync();
                        }
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
                // Log the exception for further analysis
                InfoTxtB.Text += ex.ToString();
            }
        }


        private void GetUsersBtn_Click(object sender, RoutedEventArgs e)
        {
            GetData();
        }
    }
}