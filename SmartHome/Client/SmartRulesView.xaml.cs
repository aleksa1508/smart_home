using Client.Helpers;
using Common.DTOs;
using Common.Models;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Client
{
    /// <summary>
    /// Interaction logic for SmartRulesView.xaml
    /// </summary>
    public partial class SmartRulesView : UserControl
    {
        public ObservableCollection<SmartRule> SmartRules { get; set; }
        private AesClass aesClass;
        public SmartRulesView(ObservableCollection<SmartRule> rules, AesClass aesClass)
        {
            InitializeComponent();
            //SmartRules = new ObservableCollection<SmartRule>(new List<SmartRule>
            //{
            //    new SmartRule{ IsEnabled=false, Name="NightMode",Description="Limits temperature to 20°C, restricts lights and locks garage during night hours."},
            //    new SmartRule{ IsEnabled=false, Name="SecurityMode",Description="Limits temperature to 20°C, restricts lights and locks garage during night hours."},
            //    new SmartRule{ IsEnabled=false, Name="EnergySaving",Description="Limits temperature to 20°C, restricts lights and locks garage during night hours."},
            //});
            SmartRules = rules;
            DataContext = this;
            this.aesClass = aesClass;
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleButton;

            var rule = toggle?.Tag as SmartRule;

            if (rule != null)
            {
                var content = new SmartRuleDTO
                {
                    Action = "smartRule",
                    SmartRule = rule
                };

                string json = JsonSerializer.Serialize(content);
                ConnectionService.UdpSocket.SendTo(aesClass.EncryptMessage(json, aesClass.Key, aesClass.IV), ConnectionService.UdpEndpoint);
            }
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            var toggle = sender as ToggleButton;

            var rule = toggle?.Tag as SmartRule;

            if (rule != null)
            {
                var content = new SmartRuleDTO
                {
                    Action = "smartRule",
                    SmartRule = rule
                };

                string json = JsonSerializer.Serialize(content);

                //byte[] data = Encoding.UTF8.GetBytes(json);
                ConnectionService.UdpSocket.SendTo(aesClass.EncryptMessage(json, aesClass.Key, aesClass.IV), ConnectionService.UdpEndpoint);
            }
        }
    }
}
