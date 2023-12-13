using Example.WebAPI.Controllers;
using Example.WebAPI.Models;
using OtpVerification.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace OTP_Verification_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {



        private readonly OtpController _otpController = new OtpController(new OtpVerificationService());

        public MainWindow()
        {
            InitializeComponent();
            UsersDataGrid.ItemsSource = OtpController.users;
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            UserNameTextBox.Clear();
            GeneratedCodeTextBox.Clear();
            TimeLeftTextBlock.Text = string.Empty;
        }

        private void CreateUser_Click(object sender, RoutedEventArgs e)
        {
            string userName = UserNameTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(userName))
            {
                var user = new User(userName);
                var result = _otpController.CreateUser(user);
                GeneratedCodeTextBox.Text = result.ToString();
                TimeLeftTextBlock.Text = $"Will expire at: {DateTime.Now.AddSeconds(30)}"; // Adjust as needed
                UsersDataGrid.ItemsSource = null;
                UsersDataGrid.ItemsSource = OtpController.users;
            }
            else
            {
                MessageBox.Show("Please enter a valid username.");
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = LoginUsernameTextBox.Text.Trim();
            string otp = LoginOtpTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(otp))
            {
                int userId;
                if (int.TryParse(otp, out userId))
                {
                    var result = _otpController.VerifyUser(userId, otp);
                    LoginResultTextBlock.Text = result;
                }
                else
                {
                    MessageBox.Show("Invalid OTP. Please enter a numeric OTP.");
                }
            }
            else
            {
                MessageBox.Show("Please enter both username and OTP.");
            }
        }
    }
}
