using Client.Helpers;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Client
{
    /// <summary>
    /// Interaction logic for UsersView.xaml
    /// </summary>
    public partial class UsersView : UserControl
    {
        public ObservableCollection<Korisnici> Users { get; set; }
        public UsersView(ObservableCollection<Korisnici> users)
        {
            InitializeComponent();
            Users = users;
            DataContext = this;
        }
    }
}
