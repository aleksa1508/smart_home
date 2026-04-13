using Client.Helpers;
using Common;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Client
{
    /// <summary>
    /// Interaction logic for UsersView.xaml
    /// </summary>
    public partial class UsersView : UserControl
    {
        public ObservableCollection<User> Users { get; set; }
        public UsersView(ObservableCollection<User> users)
        {
            InitializeComponent();
            Users = users;
            DataContext = this;
        }
    }
}
