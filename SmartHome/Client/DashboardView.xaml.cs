using Common;
using Common.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Client
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardView : UserControl
    {
        private ObservableCollection<string> pictures = new ObservableCollection<string> { "/Images/living_room.jpg", "/Images/bedroom.jpg", "/Images/garage.png" };
        private DispatcherTimer timer;
        private int currentIndex = 0;
        private ObservableCollection<Device> Devices;
        private ObservableCollection<DeviceCardViewModel> LoopDevices;
        public class DeviceCardViewModel
        {
            public string Icon { get; set; }
            public string Name { get; set; }
            public string Status { get; set; }
            public string Room { get; set; }
        }
        private DispatcherTimer spinnerTimer;
        private double angle = 0;

        public DashboardView(User u, ObservableCollection<Device> devices)
        {
            InitializeComponent();
            StartSlideshow();
            NetworkTextBlock.Text = u.Port != 0 ? "Connected" : "Disconnected";
            PortTextBlock.Text = u.Port.ToString();
            Devices = devices;
            if (devices.Count == 0)
            {
                LoadingOverlay.Visibility = Visibility.Visible;
                DeviceScrollViewer.Visibility = Visibility.Collapsed;
                StartSpinner();

            }
            else
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
                DeviceScrollViewer.Visibility = Visibility.Visible;
            }
            var uiDevices = Devices.Select(d => new DeviceCardViewModel
            {
                Name = d.Name,
                Icon = GetIcon(d.Name),
                Status = d.Name.Equals("Climate") ? GetStatus(d) : GetStatus(d),
                Room = d.Location.ToString()
            }).ToList();

            LoopDevices = new ObservableCollection<DeviceCardViewModel>();

            for (int i = 0; i < 3; i++)
            {
                foreach (var d in uiDevices)
                    LoopDevices.Add(d);
            }
            DeviceItems.ItemsSource = LoopDevices;
            index = devices.Count;
            DeviceScrollViewer.ScrollToHorizontalOffset(index * step);

        }
        private void StartSlideshow()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(10); // 10 sekundi
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Promjena slike
            HomeImage.Source = new BitmapImage(new Uri(pictures[currentIndex], UriKind.Relative));

            // Sljedeća slika
            currentIndex++;
            if (currentIndex >= pictures.Count)
                currentIndex = 0;
        }

        private const double step = 130;
        private int index = 0;

        private void Right_Click(object sender, RoutedEventArgs e)
        {
            index++;

            if (index >= LoopDevices.Count - Devices.Count)
            {
                index = Devices.Count;
                DeviceScrollViewer.ScrollToHorizontalOffset(index * step);
                return;
            }

            Animate(index * step);
        }

        private void Left_Click(object sender, RoutedEventArgs e)
        {
            index--;

            if (index < Devices.Count)
            {
                index = LoopDevices.Count - Devices.Count;
                DeviceScrollViewer.ScrollToHorizontalOffset(index * step);
                return;
            }

            Animate(index * step);
        }
        private void Animate(double to)
        {
            DeviceScrollViewer.ScrollToHorizontalOffset(to);
        }
        private string GetIcon(string name)
        {
            switch (name)
            {
                case "Light":
                    return "💡";

                case "AC":
                    return "❄️";

                case "TV":
                    return "📺";

                case "Gate":
                    return "🚪";

                default:
                    return "🔌";
            }
        }


        private string GetStatus(Device d)
        {
            if (d.Name.Equals("Climate"))
            {
                var tempFunction = d.Functions.Values.FirstOrDefault(f => f.Name == "temperature");
                return tempFunction.Value + "°C";
            }
            var stateFunction = d.Functions.Values.FirstOrDefault(f => f.Name == "state");
            return stateFunction.Value;
        }
        public void UpdateDevices(ObservableCollection<Device> devices)
        {
            Devices = devices;

            var uiDevices = Devices.Select(d => new DeviceCardViewModel
            {
                Name = d.Name,
                Icon = GetIcon(d.Name),
                Status = GetStatus(d),
                Room = d.Location.ToString()
            }).ToList();

            LoopDevices.Clear();

            for (int i = 0; i < 3; i++)
                foreach (var d in uiDevices)
                    LoopDevices.Add(d);
        }
        private void StartSpinner()
        {
            spinnerTimer = new DispatcherTimer();
            spinnerTimer.Interval = TimeSpan.FromMilliseconds(30);

            spinnerTimer.Tick += (s, e) =>
            {
                angle += 10;
                if (angle >= 360) angle = 0;

                SpinnerRotate.Angle = angle;
            };

            spinnerTimer.Start();
        }

    }
}
