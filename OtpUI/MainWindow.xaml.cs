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

        private void AddUserBtn_Click(object sender, RoutedEventArgs e)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:55033");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            User newUser = new User(AddUserNameTxtB.Text);
            HttpResponseMessage response = client.PostAsJsonAsync("api/Otp/CreateUser", newUser).Result;

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("User added successfully!");
                GetData(); // Refresh the user list after adding a new user
            }
            else
            {
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }
        }



        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:55033");
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
                    //GetData(); // Refresh the user list after a successful login
                }
                else
                {
                    MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid user ID.");
            }
        }


        private void GetData()
        {
            //HttpClient client = new HttpClient();
            //client.BaseAddress = new Uri("http://localhost:55033");
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //HttpResponseMessage response = client.GetAsync("api/Otp/GetUsers").Result;

            //if (response.IsSuccessStatusCode)
            //{
            //    var users = response.Content.ReadAsAsync<IEnumerable<User>>().Result;
            //    UserListBox.ItemsSource = users;
            //}
            //else
            //{
            //    MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            //}
        }


    }
}
