using Client.Helpers;
using Common.DTOs;
using Common.Models;
using Notification.Wpf;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Client
{
    /// <summary>
    /// Interaction logic for NewSmartRuleView.xaml
    /// </summary>
    public partial class NewSmartRuleView : UserControl
    {
        public ObservableCollection<Device> Devices { get; set; }
        private List<RuleActionDTO> actions;
        public AesClass aesClass;
        Dictionary<string, List<ValueOption>> functionMap = new Dictionary<string, List<ValueOption>>()
        {
            ["Light"] = new List<ValueOption> { new ValueOption { Name = "brightness" }, new ValueOption { Name = "state" } },
            ["Climate"] = new List<ValueOption> { new ValueOption { Name = "temperature" }, new ValueOption { Name = "state" } },
            ["Door"] = new List<ValueOption> { new ValueOption { Name = "state" } },
            ["Vault"] = new List<ValueOption> { new ValueOption { Name = "state" } }
        };
        public NewSmartRuleView(ObservableCollection<Device> devices, AesClass aes)
        {
            InitializeComponent();
            SetDevices(devices);
            actions = new List<RuleActionDTO>();
            ActionsListBox.ItemsSource = null;
            aesClass = aes;
            DataContext = this;
        }
        public void SetDevices(ObservableCollection<Device> devices)
        {
            Devices = new ObservableCollection<Device>(devices.Select(d => new Device
            {
                Id = d.Id,
                Name = d.Name,
                Functions = d.Functions
                    .Where(f => f.Value.Name != "channel")
                    .ToDictionary(f => f.Key, f => f.Value)
            }).ToList());
            DeviceComboBox.ItemsSource = Devices;
        }

        private void DeviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var device = DeviceComboBox.SelectedItem as Device;

            if (device == null)
            {
                FunctionComboBox.ItemsSource = null;
                return;
            }

            FunctionComboBox.ItemsSource = device.Functions.Select(x => new ValueOption { FunctionId = x.Key, Name = x.Value.Name });
            FunctionComboBox.IsEnabled = true;
            if (device.Functions.Any())
            {
                FunctionComboBox.SelectedIndex = 0;
            }
        }

        private void save_button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var device = DeviceComboBox.SelectedItem as Device;
            Dashboard parentWindow = Window.GetWindow(this) as Dashboard;
            if (RuleNameTextBox.Text.Equals(string.Empty) || DesriptionTextBox.Text.Equals(string.Empty))
            {
                parentWindow?.ShowToastNotification(new ToastNotification("Error", "Name and description are required", NotificationType.Error));
                return;
            }
            if (actions.Count == 0)
            {
                parentWindow?.ShowToastNotification(new ToastNotification("Error", "Number of action must be greather than 0", NotificationType.Error));
                return;
            }

            var rule = new SmartRuleDTO
            {
                Action = "newRule",
                Actions = actions,
                SmartRule = new SmartRule { Name = RuleNameTextBox.Text.Trim(), Description = DesriptionTextBox.Text.Trim(), IsEnabled = false }
            };

            string json = JsonSerializer.Serialize(rule);

            ConnectionService.UdpSocket.SendTo(aesClass.EncryptMessage(json, aesClass.Key, aesClass.IV), ConnectionService.UdpEndpoint);
            RuleNameTextBox.Text = string.Empty;
            DesriptionTextBox.Text = string.Empty;
            DeviceComboBox.SelectedIndex = -1;
            ScopeComboBox.SelectedIndex = -1;
            ActionsListBox.ItemsSource = null;
            ValueTextBox.Text = string.Empty;
            ValueTextBox.IsEnabled = true;
        }

        private void action_button_Click(object sender, RoutedEventArgs e)
        {
            var device = DeviceComboBox.SelectedItem as Device;
            Dashboard parentWindow = Window.GetWindow(this) as Dashboard;

            if (FunctionComboBox.SelectedItem == null) return;

            var function = (ValueOption)FunctionComboBox.SelectedItem;
            string value = ValueTextBox.Text.Trim().ToUpper();

            string scope = (ScopeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            bool isGroupMode = scope != "Single Device";
            if (!isGroupMode)
            {

                if (ValidateCommand.ValidateAction(function.Name, parentWindow, device, value, value))
                {
                    bool exists = actions.Any(x => x.Device!=null && x.Device.Id == device.Id && x.FunctionName == function.Name);

                    if (exists)
                    {
                        parentWindow?.ShowToastNotification(new ToastNotification("Error", "This action has already existed", NotificationType.Error));
                        return;
                    }
                    var action = new RuleActionDTO
                    {
                        Device = device,
                        FunctionId = function.FunctionId,
                        FunctionName = function.Name,
                        Value = value
                    };
                    actions.Add(action);
                }
            }
            else
            {
                if (ValidateCommand.ValidateValue(scope, function.Name, value, parentWindow))
                {

                    var action = new RuleActionDTO
                    {
                        Device = null,
                        FunctionName = function.Name,
                        Value = ValueTextBox.Text.Trim().ToUpper()
                    };
                    switch (scope)
                    {
                        case "All Lights":
                            action.DeviceGroup = "ALL_LIGHTS";
                            break;

                        case "All Doors":
                            action.DeviceGroup = "ALL_DOORS";
                            break;

                        case "All Climates":
                            action.DeviceGroup = "ALL_CLIMATES";
                            break;
                    }
                    bool exists = actions.Any(x =>x.Device == null && x.DeviceGroup == action.DeviceGroup && x.FunctionName == action.FunctionName);

                    if (exists)
                    {
                        parentWindow?.ShowToastNotification(new ToastNotification("Error", "This action has already existed", NotificationType.Error));
                        return;
                    }
                    actions.Add(action);
                }
            }
            ActionsListBox.ItemsSource = null;
            ActionsListBox.ItemsSource = actions;
            ValueTextBox.Text = string.Empty;

        }
        private void FunctionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DeviceComboBox.SelectedItem == null || FunctionComboBox.SelectedItem == null)
                return;

            var function = (ValueOption)FunctionComboBox.SelectedItem;
            ConfigureValueControl(function.Name);
        }
        private void ConfigureValueControl(string function)
        {
            string functionName = function.ToLower();
            ValueTextBox.IsEnabled = true;
            ValueTextBox.Text = string.Empty;
        }

        private void ScopeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string scope = (ScopeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            bool isGroupMode = scope != "Single Device";

            if (isGroupMode)
            {
                DeviceComboBox.IsEnabled = false;
                DeviceComboBox.SelectedIndex = -1;
                UpdateFunctions(scope);
            }
            else
            {
                DeviceComboBox.IsEnabled = true;
            }
        }
        private void UpdateFunctions(string scope)
        {
            string deviceType = null;

            if (scope == "All Lights")
            {
                deviceType = "Light";
            }
            else if (scope == "All Doors")
            {
                deviceType = "Door";
            }
            else if (scope == "All Climates")
            {
                deviceType = "Climate";
            }

            if (deviceType != null && functionMap.ContainsKey(deviceType))
            {
                FunctionComboBox.ItemsSource = functionMap[deviceType];
                FunctionComboBox.SelectedIndex = 0;
                FunctionComboBox.IsEnabled = true;
            }
        }
    }
}
