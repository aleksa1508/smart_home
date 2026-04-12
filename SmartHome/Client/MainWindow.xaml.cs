using Client.Helpers;
using Notification.Wpf;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UsersLibrary;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotificationManager notificationManager;

        public MainWindow()
        {
            InitializeComponent();
            notificationManager = new NotificationManager();
            Conncection();
        }


        private void button_minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void button_close_Click(object sender, RoutedEventArgs e)
        {
            ConnectionService.TcpSocket.Close();
            ConnectionService.TcpSocket = null;
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        public void ShowToastNotification(ToastNotification toastNotification)
        {
            notificationManager.Show(toastNotification.Title, toastNotification.Message, toastNotification.Type, "WindowNotificationArea");
        }
        private async void Conncection()
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            ConnectionService.TcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await Task.Run(() => ConnectionService.TcpSocket.Connect(IPAddress.Loopback, 50001));
            mainWindow.ShowToastNotification(new ToastNotification("Success", "Uspesno povezan", NotificationType.Success));
        }
        private async void button_signIn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text) || string.IsNullOrWhiteSpace(PasswordTextBox.Password))
            {
                mainWindow.ShowToastNotification(new ToastNotification("Error", "Unesite username i password!", NotificationType.Error));
                return;
            }

            try
            {

                string login = $"{UsernameTextBox.Text}:{PasswordTextBox.Password}";
                byte[] loginData = Encoding.UTF8.GetBytes(login);
                ConnectionService.TcpSocket.Send(loginData);


                // receive UDP port
                byte[] buffer = new byte[1024];
                int bytes = ConnectionService.TcpSocket.Receive(buffer);
                string response = Encoding.UTF8.GetString(buffer, 0, bytes);
                if (response != "USPESNO")
                {
                    mainWindow.ShowToastNotification(new ToastNotification("Success", "Pogresan username ili password.", NotificationType.Success));
                    return;
                }
                buffer = new byte[1024];
                bytes = ConnectionService.TcpSocket.Receive(buffer);
                int port = int.Parse(Encoding.UTF8.GetString(buffer, 0, bytes));
                mainWindow.ShowToastNotification(new ToastNotification("Success", $"Uspjesno povezan i prijavljen,a port je {port}", NotificationType.Success));
                // UDP setup
                ConnectionService.UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                ConnectionService.UdpEndpoint = new IPEndPoint(IPAddress.Loopback, port);
                Korisnici k = new Korisnici().GetKorisnik(UsernameTextBox.Text,PasswordTextBox.Password);

                // start loop

                // 🔥 otvori dashboard
                
                Dashboard dashboardWindow = new Dashboard(k);
                dashboardWindow.Closed += DashboardWindow_Closed;
                dashboardWindow.Show();

                this.Hide();


            }
            catch (Exception ex)
            {
                mainWindow.ShowToastNotification(new ToastNotification("Warning", "Ne mogu da se povežem: " + ex.Message, NotificationType.Warning));
            }
            //Dashboard d = new Dashboard();
            //d.Show();

            //this.Hide();

        }

        private void DashboardWindow_Closed(object sender, EventArgs e)
        {
            this.Show(); // Kad se novi prozor zatvori, ovaj se ponovo prikazuje
        }




    }
}