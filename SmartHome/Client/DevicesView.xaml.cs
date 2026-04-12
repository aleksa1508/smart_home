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
        ObservableCollection<Uredjaj> uredjaji;
        ObservableCollection<Button> dugmadi;
        public DevicesView(ObservableCollection<Uredjaj> oc_uredjaji)
        {
            InitializeComponent();
            //preuzeti iz baze kad se ucita ovaj tab

            uredjaji = oc_uredjaji;
            dugmadi = new ObservableCollection<Button> { light_tab_button, tv_tab_button, climate_tab_button, door_tab_button };
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
            LogsDataGrid.ItemsSource = uredjaji[0].EvidencijaKomandi;
            NameTextBox.Text = uredjaji[0].Ime;
            PortTextBox.Text = uredjaji[0].Port.ToString();
            StatusTextBox.Text = uredjaji[0].Funkcije["stanje"].ToString();
            ValueTextBox.Text = uredjaji[0].Funkcije["intenzitet"].ToString();
        }

        private void tv_tab_button_Click(object sender, RoutedEventArgs e)
        {
            SetBackground(1);
            LogsDataGrid.ItemsSource = uredjaji[1].EvidencijaKomandi;
            NameTextBox.Text = uredjaji[1].Ime;
            PortTextBox.Text = uredjaji[1].Port.ToString();
            // StatusTextBox.Text = uredjaji[1].Funkcije["stanje"].ToString();
            StatusTextBox.Text = uredjaji[1].Funkcije["stanje"].ToString();
            ValueTextBox.Text = uredjaji[1].Funkcije["temperatura"].ToString();
        }

        private void climate_tab_button_Click(object sender, RoutedEventArgs e)
        {
            SetBackground(2);
            LogsDataGrid.ItemsSource = uredjaji[2].EvidencijaKomandi;
            NameTextBox.Text = uredjaji[2].Ime;
            PortTextBox.Text = uredjaji[2].Port.ToString();
            StatusTextBox.Text = uredjaji[2].Funkcije["stanje"].ToString();
            ValueTextBox.Text = uredjaji[2].Funkcije["temperatura"].ToString();
        }

        private void door_tab_button_Click(object sender, RoutedEventArgs e)
        {
            SetBackground(3);
            LogsDataGrid.ItemsSource = uredjaji[3].EvidencijaKomandi;
            NameTextBox.Text = uredjaji[3].Ime;
            PortTextBox.Text = uredjaji[3].Port.ToString();
            StatusTextBox.Text = uredjaji[3].Funkcije["stanje"].ToString();
            ValueTextBox.Text = "-";
        }
        public void SetBackground(int index)
        {
            for (int i = 0; i < dugmadi.Count; i++)
            {
                dugmadi[i].Tag = "inactive";
            }

            dugmadi[index].Tag = "active";

        }
    }
}
