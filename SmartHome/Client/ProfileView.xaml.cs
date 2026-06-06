using Client.Helpers;
using Common;
using Common.DTOs;
using Common.Models;
using Notification.Wpf;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Client
{
    /// <summary>
    /// Interaction logic for ProfileView.xaml
    /// </summary>
    public partial class ProfileView : UserControl
    {
        private User user;
        private AesClass aesClass;
        public ProfileView(User u, AesClass aes)
        {
            InitializeComponent();
            user = u;
            aesClass = aes;
            DataContext = u;

        }
        private void UserTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidationOriginalData();
        }

        private void ValidationOriginalData()
        {
            if (!user.FirstName.Equals(FirstNameTextBox.Text) || !user.LastName.Equals(LastNameTextBox.Text) || !user.Username.Equals(UsernameTextBox.Text))
            {
                button_save.IsEnabled = true;
            }
            else
            {
                button_save.IsEnabled = false;
            }
        }
        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            if (FirstNameTextBox.Text.Length != 0 && LastNameTextBox.Text.Length != 0 && UsernameTextBox.Text.Length != 0)
            {
                var adminCmd = new OwnerCommandDTO
                {
                    Action = "updateUser",
                    ChangedUser = new User { ID = user.ID, FirstName = FirstNameTextBox.Text, LastName = LastNameTextBox.Text, Username = UsernameTextBox.Text }
                };

                string json = JsonSerializer.Serialize(adminCmd);
                byte[] data = aesClass.EncryptMessage(json, aesClass.Key, aesClass.IV);
                ConnectionService.UdpSocket.SendTo(data, ConnectionService.UdpEndpoint);
                button_save.IsEnabled = false;
            }
            else
            {
                Dashboard parentWindow = (Dashboard)Window.GetWindow(this);
                parentWindow.ShowToastNotification(new ToastNotification("Error", "Plesase enter the data", NotificationType.Error));
            }
        }
    }
}
