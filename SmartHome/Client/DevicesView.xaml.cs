using Common;
using Common.Enums;
using Common.Models;
using System.Collections.ObjectModel;
using System.Linq;
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
        public DevicesView(ObservableCollection<Device> oc_devices, User user)
        {
            InitializeComponent();

            devices = oc_devices;
            if (user.Role == UserRole.USER)
            {
                devices = new ObservableCollection<Device>(oc_devices.Where(x => !x.Name.Contains("Vault")).ToList());
            }
            buttons = new ObservableCollection<Button> { kitchen_tab_button, bedroom_tab_button, living_room_tab_button, garage_tab_button };

            DataContext = this;
        }

        private void kitchen_tab_button_Click(object sender, RoutedEventArgs e)
        {
            DeviceStackPanel.Visibility = Visibility.Visible;
            SetBackground(0);
            DevicesComboBox.ItemsSource = new ObservableCollection<Device>(devices.Where(d => d.Location == RoomType.KITCHEN).ToList());
            DevicesComboBox.SelectedIndex = 0;
        }

        private void bedroom_tab_button_Click(object sender, RoutedEventArgs e)
        {
            DeviceStackPanel.Visibility = Visibility.Visible;
            SetBackground(1);
            DevicesComboBox.ItemsSource = new ObservableCollection<Device>(devices.Where(d => d.Location == RoomType.BEDROOM).ToList());
            DevicesComboBox.SelectedIndex = 0;
        }

        private void living_room_tab_button_Click(object sender, RoutedEventArgs e)
        {
            DeviceStackPanel.Visibility = Visibility.Visible;
            SetBackground(2);
            DevicesComboBox.ItemsSource = new ObservableCollection<Device>(devices.Where(d => d.Location == RoomType.LIVING_ROOM).ToList());
            DevicesComboBox.SelectedIndex = 0;
        }

        private void garage_tab_button_Click(object sender, RoutedEventArgs e)
        {
            DeviceStackPanel.Visibility = Visibility.Visible;
            SetBackground(3);
            DevicesComboBox.ItemsSource = new ObservableCollection<Device>(devices.Where(d => d.Location == RoomType.GARAGE).ToList());
            DevicesComboBox.SelectedIndex = 0;
        }
        public void SetBackground(int index)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Tag = "inactive";
            }

            buttons[index].Tag = "active";

        }

        private void DevicesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeviceDetailsGrid.Visibility = Visibility.Visible;
            var device = DevicesComboBox.SelectedItem as Device;
            if (device == null)
            {
                return;
            }
            LogsDataGrid.ItemsSource = device.CommandRegister;
            NameTextBox.Text = device.Name;
            PortTextBox.Text = device.Port.ToString();
            StatusTextBox.Text = device.Functions.Values.FirstOrDefault(f => f.Name == "state")?.Value;
            if (device.Name.Contains("Light"))
            {
                ValueTextBox.Text = device.Functions.Values.FirstOrDefault(f => f.Name == "brightness")?.Value;

            }
            else if (device.Name.Contains("Climate"))
            {
                ValueTextBox.Text = device.Functions.Values.FirstOrDefault(f => f.Name == "temperature")?.Value;

            }
            else if (device.Name.Contains("TV"))
            {
                ValueTextBox.Text = device.Functions.Values.FirstOrDefault(f => f.Name == "channel")?.Value;

            }
            else if (device.Name.Contains("Door") || device.Name.Contains("Vault"))
            {
                ValueTextBox.Text = "-";

            }
        }

    }
}
