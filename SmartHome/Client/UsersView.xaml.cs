using Client.Helpers;
using Common;
using Common.DTOs;
using Common.Enums;
using Common.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Client
{
    /// <summary>
    /// Interaction logic for UsersView.xaml
    /// </summary>
    public partial class UsersView : UserControl
    {
        public ObservableCollection<User> AllUsers { get; set; }
        public ICollectionView EntitiesView { get; set; }
        public User currentUser;
        public AesClass aesClass;
        public UsersView(User user, ObservableCollection<User> users,AesClass aes)
        {
            InitializeComponent();
            currentUser = user;
            AllUsers = users;
            aesClass=aes;
            EntitiesView = CollectionViewSource.GetDefaultView(AllUsers);

            RolesComboBox.SelectedIndex = 0;
            StatusComboBox.SelectedIndex = 0;

            DataContext = this;
            // Sakrij action panel ako nije admin
            if (currentUser.Role != UserRole.OWNER)
                ActionPanel.Visibility = Visibility.Collapsed;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var role = (ComboBoxItem)RolesComboBox.SelectedItem;
            var status = (ComboBoxItem)StatusComboBox.SelectedItem;

            if (role == null || status == null) return;

            var roleValue = role.Content.ToString();
            var statusValue = status.Content.ToString();
            if (roleValue.Equals("All users") && statusValue.Equals("All users"))
            {
                EntitiesView.Filter = null;
                EntitiesView.Refresh();
            }

            EntitiesView.Filter = e1 =>
            {
                var entity = e1 as User;
                if (entity == null) return false;
                bool condition1 = true; bool condition2 = true;
                if (roleValue != "All users")
                {
                    condition1 = entity.Role == (UserRole)Enum.Parse(typeof(UserRole), roleValue.ToUpper());
                }
                if (statusValue != "All status")
                {
                    condition2 = entity.Status == (ActiveStatus)Enum.Parse(typeof(ActiveStatus), statusValue.ToUpper());
                }

                return condition1 && condition2;
            };
            EntitiesView.Refresh();



        }

        private void UsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User selectedUser && currentUser.Role == UserRole.OWNER)
            {
                // Ne dozvoli adminu da mijenja samog sebe
                if (selectedUser.ID == currentUser.ID)
                {
                    ActionPanel.Visibility = Visibility.Collapsed;
                    return;
                }

                SelectedUserText.Text = $"{selectedUser.FirstName} {selectedUser.LastName}";
                CurrentRoleText.Text = selectedUser.Role.ToString();
                ActionPanel.Visibility = Visibility.Visible;
            }
            else
            {
                ActionPanel.Visibility = Visibility.Collapsed;
            }
        }
        private void ChangeRoleButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem == null) return;
            User selectedUser = UsersDataGrid.SelectedItem as User;
            if (selectedUser == null) return;

            if (NewRoleComboBox.SelectedItem == null) return;
            ComboBoxItem selectedItem = NewRoleComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem == null) return;

            UserRole newRole = selectedItem.Content.ToString() == "OWNER"
                ? UserRole.OWNER
                : UserRole.USER;

            // Pošalji UDP komandu serveru
            var adminCmd = new OwnerCommandDTO
            {
                Action = "userRole",
                Owner = currentUser,
                ChangedUser = selectedUser,
                Role = newRole
            };

            string json = JsonSerializer.Serialize(adminCmd);
            byte[] data = aesClass.EncryptMessage(json, aesClass.Key, aesClass.IV);
            ConnectionService.UdpSocket.SendTo(data, ConnectionService.UdpEndpoint);
        }

    }
}
