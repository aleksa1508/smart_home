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
    /// Interaction logic for PasswordView.xaml
    /// </summary>
    public partial class PasswordView : UserControl
    {
        private User user;
        private AesClass aesClass;
        public PasswordView(User u, AesClass aes)
        {
            InitializeComponent();
            user = u;
            aesClass = aes;
            DataContext = this;
        }

        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            Dashboard parentWindow = (Dashboard)Window.GetWindow(this);
            if (CurrentPasswordTextBox.Password.Length > 0 && NewPasswordTextBox.Password.Length > 0)
            {
                if (NewPasswordTextBox.Password.Length < 8)
                {
                    parentWindow.ShowToastNotification(new ToastNotification("Error", "Password must have minimum 8 characters", NotificationType.Error));
                    return;
                }
                var command = new OwnerCommandDTO
                {
                    Action = "updatePassword",
                    ChangedUser = new User
                    {
                        ID = user.ID,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Username = user.Username,
                        Password = NewPasswordTextBox.Password,
                        Role = user.Role,
                    },
                };

                string json = JsonSerializer.Serialize(command);
                byte[] data = aesClass.EncryptMessage(json, aesClass.Key, aesClass.IV);
                ConnectionService.UdpSocket.SendTo(data, ConnectionService.UdpEndpoint);
                //reset data
                button_save.IsEnabled = false;
                CurrentPasswordTextBox.Password = string.Empty;
                NewPasswordTextBox.Password = string.Empty;

            }
            else
            {
                parentWindow.ShowToastNotification(new ToastNotification("Error", "Fill current and new password", NotificationType.Error));
            }
        }
    }
}
