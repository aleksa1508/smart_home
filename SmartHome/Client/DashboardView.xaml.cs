using Client.Helpers;
using System;
using System.Collections.ObjectModel;
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
        private ObservableCollection<string> pictures = new ObservableCollection<string> { "/Images/living_room.jpg", "/Images/bedroom.jpg" };
        private DispatcherTimer timer;
        private int currentIndex = 0;
        public DashboardView()
        {
            InitializeComponent();
            StartSlideshow();
            NetworkTextBlock.Text = ConnectionService.UdpEndpoint.Port != 0 ? "Connected" : "Disconnected";
            PortTextBlock.Text = ConnectionService.UdpEndpoint.Port.ToString();
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
    }
}
