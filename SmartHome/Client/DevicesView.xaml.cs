using Common.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Client
{
    /// <summary>
    /// Interaction logic for DevicesView.xaml
    /// </summary>
    public partial class DevicesView : UserControl
    {
        public ObservableCollection<Device> devices;
        public ObservableCollection<Button> buttons;
        public DevicesView(ObservableCollection<Device> oc_devices)
        {
            InitializeComponent();
            //preuzeti iz baze kad se ucita ovaj tab

            devices = oc_devices;
            buttons = new ObservableCollection<Button> { light_tab_button, tv_tab_button, climate_tab_button, door_tab_button };
            //preuzeti komande iz baze i ubaciti ovako ili kroz konstruktor uredjaja
            //uredjaji[0].EvidencijaKomandi = new List<Komanda> {
            //    new Komanda { ID=1,CreationDate=DateTime.Now,Log= $"[{DateTime.Now}] {uredjaji[0].Ime}: intezitet promenjena na 20" },
            //    new Komanda { ID=2,CreationDate=DateTime.Now,Log= $"[{DateTime.Now}] {uredjaji[0].Ime}: intezitet promenjena na 20" },
            //    new Komanda { ID=2,CreationDate=DateTime.Now,Log= $"[{DateTime.Now}] {uredjaji[0].Ime}: intezitet promenjena na 20" },
            //    new Komanda { ID=2,CreationDate=DateTime.Now,Log= $"[{DateTime.Now}] {uredjaji[0].Ime}: intezitet promenjena na 20" },
            //    new Komanda { ID=2,CreationDate=DateTime.Now,Log= $"[{DateTime.Now}] {uredjaji[0].Ime}: intezitet promenjena na 20" },
            //    new Komanda { ID=2,CreationDate=DateTime.Now,Log= $"[{DateTime.Now}] {uredjaji[0].Ime}: intezitet promenjena na 20" },
            //    new Komanda { ID=2,CreationDate=DateTime.Now,Log= $"[{DateTime.Now}] {uredjaji[0].Ime}: intezitet promenjena na 20" },
            //    new Komanda { ID=3,CreationDate=DateTime.Now,Log= $"[{DateTime.Now}] {uredjaji[0].Ime}: intezitet promenjena na 20" },
            //};
            //uredjaji[1].EvidencijaKomandi = new List<Komanda> {
            //    new Komanda { ID=1,CreationDate=DateTime.Now,Log= $"[{DateTime.Now}] {uredjaji[0].Ime}: intezitet promenjena na 20" },
            //};
            DataContext = this;
        }

        private void light_tab_button_Click(object sender, RoutedEventArgs e)
        {
            SetBackground(0);
            LogsDataGrid.ItemsSource = devices[0].CommandRegister;
            NameTextBox.Text = devices[0].Name;
            PortTextBox.Text = devices[0].Port.ToString();
            StatusTextBox.Text = devices[0].Functions["stanje"].ToString();
            ValueTextBox.Text = devices[0].Functions["intenzitet"].ToString();
        }

        private void tv_tab_button_Click(object sender, RoutedEventArgs e)
        {
            SetBackground(1);
            LogsDataGrid.ItemsSource = devices[1].CommandRegister;
            NameTextBox.Text = devices[1].Name;
            PortTextBox.Text = devices[1].Port.ToString();
            // StatusTextBox.Text = uredjaji[1].Funkcije["stanje"].ToString();
            StatusTextBox.Text = devices[1].Functions["stanje"].ToString();
            ValueTextBox.Text = devices[1].Functions["temperatura"].ToString();
        }

        private void climate_tab_button_Click(object sender, RoutedEventArgs e)
        {
            SetBackground(2);
            LogsDataGrid.ItemsSource = devices[2].CommandRegister;
            NameTextBox.Text = devices[2].Name;
            PortTextBox.Text = devices[2].Port.ToString();
            StatusTextBox.Text = devices[2].Functions["stanje"].ToString();
            ValueTextBox.Text = devices[2].Functions["temperatura"].ToString();
        }

        private void door_tab_button_Click(object sender, RoutedEventArgs e)
        {
            SetBackground(3);
            LogsDataGrid.ItemsSource = devices[3].CommandRegister;
            NameTextBox.Text = devices[3].Name;
            PortTextBox.Text = devices[3].Port.ToString();
            StatusTextBox.Text = devices[3].Functions["stanje"].ToString();
            ValueTextBox.Text = "-";
        }
        public void SetBackground(int index)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Tag = "inactive";
            }

            buttons[index].Tag = "active";

        }
    }
}
