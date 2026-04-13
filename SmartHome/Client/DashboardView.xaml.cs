using Client.Helpers;
using Common;
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
        public DashboardView(User u)
        {
            InitializeComponent();
            StartSlideshow();
            NetworkTextBlock.Text =u.Port != 0 ? "Connected" : "Disconnected";
            PortTextBlock.Text = u.Port.ToString();
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
