using Client.Helpers;
using Common.DTOs;
using Common.Models;
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
        public ObservableCollection<Device> Devices { get; set; }
        public ObservableCollection<Command> CommandRegister { get; set; }

        public ControlTableView(ObservableCollection<Device> devices, ObservableCollection<Command> commands, NotificationManager manager)
        {
            InitializeComponent();
            CommandRegister = commands;
            Devices = devices;
            this.DataContext = this;
        }

        private void DeviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var device = DeviceComboBox.SelectedItem as Device;
            if (device != null)
            {
                // Postavi funkcije u drugi ComboBox
                FunctionComboBox.ItemsSource = device.Functions.Keys;
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


            var content = new CommandDTO
            {
                SelectedDevice = (DeviceComboBox.SelectedItem as Device),
                Function = FunctionComboBox.SelectedItem.ToString(),
                Value = ValueTextBox.Text
            };

            string json = JsonSerializer.Serialize(content);
            byte[] data = Encoding.UTF8.GetBytes(json);
            ConnectionService.UdpSocket.SendTo(data, ConnectionService.UdpEndpoint);

            ValueTextBox.Text=string.Empty;
            ValueTextBox.IsEnabled = false ;

        }


    }
}
