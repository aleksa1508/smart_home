using Client.Helpers;
using Common;
using Common.Repositories.UsersRepositories;
using Notification.Wpf;
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
        private IUserReository userReository;
        public PasswordView(User u, IUserReository repo)
        {
            InitializeComponent();
            userReository = repo;
            user = u;
            DataContext = this;
        }

        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            Dashboard parentWindow = (Dashboard)Window.GetWindow(this);
            if (CurrentPasswordTextBox.Password.Length > 0 && NewPasswordTextBox.Password.Length > 0)
            {
                if (userReository.GetKorisnik(user.Username, CurrentPasswordTextBox.Password) != null)
                {
                    userReository.UpdatePassword(user.ID, NewPasswordTextBox.Password);
                    parentWindow.ShowToastNotification(new ToastNotification("Success", "Password is update successfully", NotificationType.Success));
                    button_save.IsEnabled = false;
                    CurrentPasswordTextBox.Password = string.Empty;
                    NewPasswordTextBox.Password = string.Empty;
                }
                else
                {
                    parentWindow.ShowToastNotification(new ToastNotification("Error", "Current password is not valid", NotificationType.Error));
                }
            }
            else
            {
                parentWindow.ShowToastNotification(new ToastNotification("Error", "Fill current and new password", NotificationType.Error));
            }
        }
        private void CurrentPasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (CurrentPasswordTextBox.Password.Length > 0 && NewPasswordTextBox.Password.Length > 0)
            {
                button_save.IsEnabled = true;
            }
            else
            {
                button_save.IsEnabled = false;
            }
        }
    }
}
