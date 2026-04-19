using Client.Helpers;
using Client.Helpers;
using Common;
using Common.Enums;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
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
        public UsersView(ObservableCollection<User> users)
        {
            InitializeComponent();
            AllUsers = users;
            EntitiesView = CollectionViewSource.GetDefaultView(AllUsers);

                RolesComboBox.SelectedIndex = 0;
                StatusComboBox.SelectedIndex = 0;

            DataContext = this;
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
                bool condition1 = true;bool condition2 = true;
                if (roleValue != "All users")
                {
                    condition1 = entity.Role == (UserRole)Enum.Parse(typeof(UserRole),roleValue.ToUpper());
                }
                if (statusValue != "All status")
                {
                    condition2 = entity.Status == (ActiveStatus)Enum.Parse(typeof(ActiveStatus),statusValue.ToUpper());
                }

                return condition1 && condition2;
            };
            EntitiesView.Refresh();



        }
    }
}
