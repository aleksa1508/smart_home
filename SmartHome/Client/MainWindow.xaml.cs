using Client.Helpers;
using Common;
using Common.DTOs;
using Common.Models;
using Notification.Wpf;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
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
            int maxRetries = 5;
            int delaySeconds = 3;

            for (int i = 1; i <= maxRetries; i++)
            {
                try
                {
                    ConnectionService.TcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    await Task.Run(() => ConnectionService.TcpSocket.Connect(IPAddress.Loopback, 50001));

                    ShowToastNotification(new ToastNotification("Success", "Successfully connected on server", NotificationType.Success));
                    return;
                }
                catch (SocketException)
                {
                    ConnectionService.TcpSocket?.Close();
                    ConnectionService.TcpSocket = null;

                    if (i < maxRetries)
                    {
                        ShowToastNotification(new ToastNotification("Warning", $"Server unavailable, retrying in {delaySeconds}s... ({i}/{maxRetries})", NotificationType.Warning));

                        await Task.Delay(delaySeconds * 1000);
                    }
                    else
                    {
                        ShowToastNotification(new ToastNotification("Error", "Could not connect to server.", NotificationType.Error));
                    }
                }
            }
        }
        private async void button_signIn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text) || string.IsNullOrWhiteSpace(PasswordTextBox.Password))
            {
                mainWindow.ShowToastNotification(new ToastNotification("Error", "Enter username and password!", NotificationType.Error));
                return;
            }

            button_signIn.IsEnabled = false;
            try
            {
                ConnectionService.TcpSocket.Send(Encoding.UTF8.GetBytes("GET_PUBLIC_KEY"));
                //4 bytes length of key
                byte[] lenBuffer = new byte[4];
                ConnectionService.TcpSocket.Receive(lenBuffer);
                int keyLength = BitConverter.ToInt32(lenBuffer, 0);

                // XML key
                byte[] keyBytes = new byte[keyLength];
                int totalReceived = 0;
                while (totalReceived < keyLength)
                {
                    int received = ConnectionService.TcpSocket.Receive(
                        keyBytes, totalReceived, keyLength - totalReceived, SocketFlags.None);
                    totalReceived += received;
                }
                string publicKeyXml = Encoding.UTF8.GetString(keyBytes);

                RsaClass rsa = new RsaClass(publicKeyXml);

                RsaClass clientRsa = new RsaClass();
                string clientPublicKeyXml = clientRsa.ExportPublicKey();
                byte[] clientPubKeyBytes = Encoding.UTF8.GetBytes(clientPublicKeyXml);

                string login = $"{UsernameTextBox.Text}:{PasswordTextBox.Password}";
                byte[] encryptedLogin = rsa.Encrypt(login);

                // NOVO: saljemo u JEDNOM Send pozivu: [4 bajta duzina klijentovog javnog kljuca]
                // [klijentov javni kljuc XML][RSA sifrovani login], da server sve dobije u istom
                // dogadjaju citanja (isti stil kao i ostatak protokola)
                byte[] clientKeyLenBytes = BitConverter.GetBytes(clientPubKeyBytes.Length);
                byte[] combined = new byte[clientKeyLenBytes.Length + clientPubKeyBytes.Length + encryptedLogin.Length];
                Buffer.BlockCopy(clientKeyLenBytes, 0, combined, 0, clientKeyLenBytes.Length);
                Buffer.BlockCopy(clientPubKeyBytes, 0, combined, clientKeyLenBytes.Length, clientPubKeyBytes.Length);
                Buffer.BlockCopy(encryptedLogin, 0, combined, clientKeyLenBytes.Length + clientPubKeyBytes.Length, encryptedLogin.Length);

                ConnectionService.TcpSocket.Send(combined);


                // receive UDP port
                byte[] buffer = new byte[1024];
                int bytes = ConnectionService.TcpSocket.Receive(buffer);
                string response = Encoding.UTF8.GetString(buffer, 0, bytes);
                if (response != "SUCCESS")
                {
                    mainWindow.ShowToastNotification(new ToastNotification("Error", "Invalid username or password.", NotificationType.Error));
                    return;
                }

                byte[] encKeyLenBuf = new byte[4];
                ConnectionService.TcpSocket.Receive(encKeyLenBuf);
                int encKeyLen = BitConverter.ToInt32(encKeyLenBuf, 0);

                byte[] encKeyData = new byte[encKeyLen];
                int totalKeyReceived = 0;
                while (totalKeyReceived < encKeyLen)
                {
                    int received = ConnectionService.TcpSocket.Receive(
                        encKeyData, totalKeyReceived, encKeyLen - totalKeyReceived, SocketFlags.None);
                    totalKeyReceived += received;
                }

                byte[] keyData = clientRsa.DecryptBytes(encKeyData);
                byte[] key = new byte[16];
                byte[] iv = new byte[16];
                Array.Copy(keyData, 0, key, 0, 16);
                Array.Copy(keyData, 16, iv, 0, 16);

                AesClass aes = new AesClass(key, iv);

                // recieve encripted port
                buffer = new byte[1024];
                bytes = ConnectionService.TcpSocket.Receive(buffer);

                // decription 
                byte[] primljeno = new byte[bytes];
                Array.Copy(buffer, primljeno, bytes);
                string portString = aes.DecryptMessage(primljeno, aes.Key, aes.IV);

                int port = int.Parse(portString);
                mainWindow.ShowToastNotification(new ToastNotification("Success", $"You are successfully log in and port is {port}", NotificationType.Success));
                // UDP setup
                ConnectionService.UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                ConnectionService.UdpEndpoint = new IPEndPoint(IPAddress.Loopback, port);

                ConnectionService.UdpSocket.Blocking = true;

                byte[] initMsg = aes.EncryptMessage($"Client is connected on UDP port: {port}", aes.Key, aes.IV);
                ConnectionService.UdpSocket.SendTo(initMsg, ConnectionService.UdpEndpoint);
                byte[] udpBuffer = new byte[65507];
                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                int udpBytes = ConnectionService.UdpSocket.ReceiveFrom(udpBuffer, ref remoteEP);

                byte[] udpData = new byte[udpBytes];
                Array.Copy(udpBuffer, udpData, udpBytes);
                string udpJson = aes.DecryptMessage(udpData, aes.Key, aes.IV);

                var firstResponse = JsonSerializer.Deserialize<ResponseDTO>(udpJson);
                User loggedUser = firstResponse.Users.FirstOrDefault();
                //ConnectionService.UdpSocket.Blocking = false;
                Dashboard dashboardWindow = new Dashboard(aes, loggedUser, firstResponse);
                dashboardWindow.Closed += DashboardWindow_Closed;
                dashboardWindow.Show();

                this.Hide();


            }
            catch (Exception ex)
            {
                mainWindow.ShowToastNotification(new ToastNotification("Warning", "I can not connect: " + ex.Message, NotificationType.Warning));
            }
            finally
            {
                button_signIn.IsEnabled = true;
            }

        }

        private void DashboardWindow_Closed(object sender, EventArgs e)
        {
            this.Show(); // show previouse window when new window closed
        }

    }
}