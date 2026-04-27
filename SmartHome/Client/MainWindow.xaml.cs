using Client.Helpers;
using Common;
using Common.Models;
using Common.Repositories.UsersRepositories;
using Notification.Wpf;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotificationManager notificationManager;
        private IUserReository userReository;

        public MainWindow()
        {
            InitializeComponent();
            notificationManager = new NotificationManager();
            Conncection();
            userReository = new UserRepository();
        }


        private void button_minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void button_close_Click(object sender, RoutedEventArgs e)
        {
            byte[] loginData = Encoding.UTF8.GetBytes("shutdown");
            ConnectionService.TcpSocket.Send(loginData);

            // receive UDP port
            byte[] buffer = new byte[1024];
            int bytes = ConnectionService.TcpSocket.Receive(buffer);
            string response = Encoding.UTF8.GetString(buffer, 0, bytes);
            if (response == "DISCONNECT_OK")
            {
                ConnectionService.TcpSocket.Close();
                ConnectionService.TcpSocket = null;
                this.Close();
            }
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
            mainWindow.ShowToastNotification(new ToastNotification("Success", "Successfully connected on server", NotificationType.Success));
        }
        private async void button_signIn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text) || string.IsNullOrWhiteSpace(PasswordTextBox.Password))
            {
                mainWindow.ShowToastNotification(new ToastNotification("Error", "Enter username and password!", NotificationType.Error));
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
                if (response != "SUCCESS")
                {
                    mainWindow.ShowToastNotification(new ToastNotification("Error", "Invalid username or password.", NotificationType.Error));
                    return;
                }
                buffer = new byte[1024];
                bytes = ConnectionService.TcpSocket.Receive(buffer);
                byte[] key = new byte[16];
                byte[] iv = new byte[16];
                Array.Copy(buffer, 0, key, 0, 16);
                Array.Copy(buffer, 16, iv, 0, 16);

                AesClass aes = new AesClass(key, iv);

                // Prima enkriptovani port
                buffer = new byte[1024];
                bytes = ConnectionService.TcpSocket.Receive(buffer);

                // Dekriptuj — šalješ samo primljene bajtove, ne cijeli buffer!
                byte[] primljeno = new byte[bytes];
                Array.Copy(buffer, primljeno, bytes);
                string portString = aes.DecryptMessage(primljeno, aes.Key, aes.IV);

                int port = int.Parse(portString);
                //int port = int.Parse(Encoding.UTF8.GetString(buffer, 0, bytes));
                mainWindow.ShowToastNotification(new ToastNotification("Success", $"You are successfully log in and port is {port}", NotificationType.Success));
                // UDP setup
                ConnectionService.UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                ConnectionService.UdpEndpoint = new IPEndPoint(IPAddress.Loopback, port);
                User user = userReository.GetKorisnik(UsernameTextBox.Text, PasswordTextBox.Password);

                // start loop

                // 🔥 otvori dashboard

                Dashboard dashboardWindow = new Dashboard(user, aes);
                dashboardWindow.Closed += DashboardWindow_Closed;
                dashboardWindow.Show();

                this.Hide();


            }
            catch (Exception ex)
            {
                mainWindow.ShowToastNotification(new ToastNotification("Warning", "I can not connect: " + ex.Message, NotificationType.Warning));
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