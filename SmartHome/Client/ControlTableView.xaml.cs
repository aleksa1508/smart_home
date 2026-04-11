using Client.Helpers;
using Notification.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Client
{
    /// <summary>
    /// Interaction logic for ControlTableView.xaml
    /// </summary>
    public partial class ControlTableView : UserControl
    {
        public ObservableCollection<Uredjaj> Uredjaji { get; set; }
        public ObservableCollection<Komanda> EvidencijaKomandi { get; set; }

        public ControlTableView(ObservableCollection<Uredjaj> uredjaji, ObservableCollection<Komanda> komande, NotificationManager manager)
        {
            InitializeComponent();
            EvidencijaKomandi = komande;
            //Uredjaji = new ObservableCollection<Uredjaj>
            //{
            //            new Uredjaj("Svetlo",60001,new Dictionary<string, string>{{ "intezitet", "70" }, { "stanje", "iskljuceno" }, { "boja crvena", "110" }}),
            //            new Uredjaj("TV",60003,new Dictionary<string, string>{{ "stanje", "iskljuceno" },{ "temperatura", "15" }}),
            //            new Uredjaj("Klima",60002,new Dictionary<string, string>{{ "stanje", "iskljuceno" },{ "temperatura", "15" }}),
            //            new Uredjaj("Door",60004,new Dictionary<string, string>{{ "stanje", "iskljuceno" }})
            //};
            Uredjaji = uredjaji;
            this.DataContext = this;

        }

        private void DeviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var uredjaj = DeviceComboBox.SelectedItem as Uredjaj;
            if (uredjaj != null)
            {
                // Postavi funkcije u drugi ComboBox
                FunctionComboBox.ItemsSource = uredjaj.Funkcije.Keys;
                FunctionComboBox.SelectedIndex = 0; // opcionalno, da automatski izabere prvi
                ValueTextBox.IsEnabled = true;
            }
            else
            {
                FunctionComboBox.ItemsSource = null;
                ValueTextBox.IsEnabled = false;

            }
        }

        private void submit_button_Click(object sender, RoutedEventArgs e)
        {
            Dashboard parentWindow = Window.GetWindow(this) as Dashboard;
            if (ValueTextBox.Text.Length == 0)
            {
                parentWindow?.ShowToastNotification(new ToastNotification("Error", "You can fill the data", NotificationType.Error));
                return;
            }
            string regexValue = "^(intenzitet|temperatura|boja plava|boja crvena)$";
            Regex regex = new Regex(regexValue);
            if (regex.IsMatch(FunctionComboBox.SelectedItem.ToString()))
            {
                if (!Int32.TryParse(ValueTextBox.Text, out int value))
                {
                    parentWindow?.ShowToastNotification(new ToastNotification("Error", "Value might be a number", NotificationType.Error));
                    return;
                }

            }
            else
            {
                if (!ValueTextBox.Text.Equals("ON") && !ValueTextBox.Text.Equals("OFF"))
                {
                    parentWindow?.ShowToastNotification(new ToastNotification("Error", "Value of device state might be ON/OFF", NotificationType.Error));
                    return;
                }
            }


            var content = new
            {
                IzabraniUredjaj = (DeviceComboBox.SelectedItem as Uredjaj),
                Funkcija = FunctionComboBox.SelectedItem.ToString(),
                Vrednost = ValueTextBox.Text
            };

            string json = JsonSerializer.Serialize(content);
            byte[] data = Encoding.UTF8.GetBytes(json);
            ConnectionService.UdpSocket.SendTo(data, ConnectionService.UdpEndpoint);

            ValueTextBox.Text=string.Empty;
            ValueTextBox.IsEnabled = false ;

        }


    }
}
