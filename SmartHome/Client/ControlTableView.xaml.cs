using Client.Helpers;
using Common;
using Common.DTOs;
using Common.Enums;
using Common.Models;
using Notification.Wpf;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        public AesClass aesClass;
        public User user;
        public ControlTableView(ObservableCollection<Device> devices, ObservableCollection<Command> commands, NotificationManager manager, AesClass aes, User user)
        {
            InitializeComponent();
            CommandRegister = commands;
            aesClass = aes;
            SetDevices(devices, user);
            this.user = user;
            this.DataContext = this;
        }

        public void SetDevices(ObservableCollection<Device> devices, User user)
        {
            Devices = user.Role == UserRole.USER ? new ObservableCollection<Device>(devices.Where(x => !x.Name.Contains("Vault")).ToList())
                                                    : new ObservableCollection<Device>(devices);
            DeviceComboBox.ItemsSource = Devices;
        }

        private void DeviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var device = DeviceComboBox.SelectedItem as Device;
            if (device != null)
            {
                FunctionComboBox.ItemsSource = device.Functions;
                FunctionComboBox.SelectedIndex = 0; // optionally,selected first function in device
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
            var device = DeviceComboBox.SelectedItem as Device;
            Dashboard parentWindow = Window.GetWindow(this) as Dashboard;
            if (ValueTextBox.Text.Length == 0)
            {
                parentWindow?.ShowToastNotification(new ToastNotification("Error", "You can fill the data", NotificationType.Error));
                return;
            }
            string regexValue = "^(channel|temperature|brightness)$";
            Regex regex = new Regex(regexValue);
            var selectedFunction = (KeyValuePair<int, Function>)FunctionComboBox.SelectedItem;
            if (ValidateCommand.CommandValidation(regex, selectedFunction, ValueTextBox.Text, parentWindow, device))
            {
                var selected = (KeyValuePair<int, Function>)FunctionComboBox.SelectedItem;
                var content = new CommandDTO
                {
                    SelectedDevice = (DeviceComboBox.SelectedItem as Device),
                    FunctionID = selected.Key,
                    Function = selected.Value.Name,
                    Value = ValueTextBox.Text.Trim(),
                    Username = user.Username
                };

                string json = JsonSerializer.Serialize(content);

                ConnectionService.UdpSocket.SendTo(aesClass.EncryptMessage(json, aesClass.Key, aesClass.IV), ConnectionService.UdpEndpoint);

                ValueTextBox.Text = string.Empty;
                ValueTextBox.IsEnabled = true;
            }

        }
    }
}
