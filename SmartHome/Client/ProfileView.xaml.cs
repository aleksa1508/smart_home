using Client.Helpers;
using Common;
using Common.Repositories.UsersRepositories;
using Microsoft.Win32;
using Notification.Wpf;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Client
{
    /// <summary>
    /// Interaction logic for ProfileView.xaml
    /// </summary>
    public partial class ProfileView : UserControl
    {
        private User user;
        private IUserReository userReository;
        public ProfileView(User u,IUserReository repo)
        {
            InitializeComponent();
            userReository=repo;
            user = u;
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
                userReository.UpdateData(user.ID,FirstNameTextBox.Text, LastNameTextBox.Text,UsernameTextBox.Text);
                Dashboard parentWindow = (Dashboard)Window.GetWindow(this);
                parentWindow.ShowToastNotification(new ToastNotification("Success", "Profile data is update successfully", NotificationType.Success));
                parentWindow.Title.Content = $"Hello,{user.FirstName}";
                button_save.IsEnabled = false;
            }
            else
            {
                Dashboard parentWindow = (Dashboard)Window.GetWindow(this);
                parentWindow.ShowToastNotification(new ToastNotification("Error", "Plesase enter the data", NotificationType.Error));
            }
        }

        private void button_picture_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.DefaultExt = "c:\\";
            openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
                string projectFolder = System.IO.Path.GetFullPath(System.IO.Path.Combine(exeFolder, @"..\..\.."));

                string imageFolder = System.IO.Path.Combine(projectFolder, "Images");
                Directory.CreateDirectory(imageFolder);

                string imageName = System.IO.Path.GetFileName(openFileDialog.FileName);
                string destinationPath = System.IO.Path.Combine(imageFolder, imageName);

                if (!File.Exists(destinationPath))
                {
                    File.Copy(openFileDialog.FileName, destinationPath, true);
                }

                string pathPicture = System.IO.Path.Combine(projectFolder, destinationPath);

                // Prikaz slike u UI
                string absolutePath = System.IO.Path.Combine(projectFolder, pathPicture);
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(absolutePath);
                bitmap.EndInit();

                ProfileImage.Source = bitmap;
            }
        }
    }
}
