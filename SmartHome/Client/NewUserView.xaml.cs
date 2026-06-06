using Client.Helpers;
using Common;
using Common.DTOs;
using Common.Enums;
using Common.Models;
using Notification.Wpf;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Client
{
    /// <summary>
    /// Interaction logic for NewUserView.xaml
    /// </summary>
    public partial class NewUserView : UserControl
    {
        public AesClass aesClass;
        public NewUserView(AesClass aes)
        {
            InitializeComponent();
            aesClass = aes;
            RoleComboBox.SelectedIndex = 0;
        }

        private void add_user_button_Click(object sender, RoutedEventArgs e)
        {
            Dashboard parentWindow = (Dashboard)Window.GetWindow(this);
            if (FirstNameTextBox.Text.Length != 0 && LastNameTextBox.Text.Length != 0 && UsernameTextBox.Text.Length != 0 && PasswordBox.Password.Length != 0)
            {

                if (PasswordBox.Password.Length < 8)
                {
                    parentWindow.ShowToastNotification(new ToastNotification("Error", "Password must have minimum 8 characters", NotificationType.Error));
                    return;
                }
                var command = new OwnerCommandDTO
                {
                    Action = "newUser",
                    ChangedUser = new User
                    {
                        FirstName = FirstNameTextBox.Text,
                        LastName = LastNameTextBox.Text,
                        Username = UsernameTextBox.Text,
                        Password = PasswordBox.Password,
                        Role = (RoleComboBox.SelectedItem as ComboBoxItem).Content.ToString() == "OWNER" ? UserRole.OWNER : UserRole.USER
                    },
                };

                string json = JsonSerializer.Serialize(command);
                byte[] data = aesClass.EncryptMessage(json, aesClass.Key, aesClass.IV);
                ConnectionService.UdpSocket.SendTo(data, ConnectionService.UdpEndpoint);
                //reset data
                FirstNameTextBox.Text = string.Empty;
                LastNameTextBox.Text = string.Empty;
                UsernameTextBox.Text = string.Empty;
                PasswordBox.Password = string.Empty;
                RoleComboBox.SelectedIndex = 0;
            }
            else
            {
                parentWindow.ShowToastNotification(new ToastNotification("Error", "Plesase enter the data", NotificationType.Error));
            }
        }
    }
}
