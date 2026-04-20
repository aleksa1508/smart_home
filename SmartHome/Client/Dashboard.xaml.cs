using Client.Helpers;
using Common;
using Common.DTOs;
using Common.Enums;
using Common.Models;
using Common.Repositories.DevicesRepositories;
using Common.Repositories.UsersRepositories;
using Notification.Wpf;
using System;
using System.Collections.ObjectModel;
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
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : Window
    {
        private NotificationManager notificationManager;
        private User user;
        private ObservableCollection<Device> devices;
        private ObservableCollection<Command> CommandRegister;
        private IUserReository userReository;
        private IDeviceRepository deviceRepository;
        private AesClass aesClass;
        public Dashboard(User userParameter,AesClass aes)
        {
            InitializeComponent();
            aesClass = aes;
            userReository = new UserRepository();
            deviceRepository = new DeviceRepository();
            user = userParameter;
            devices = new ObservableCollection<Device>();
            CommandRegister = new ObservableCollection<Command>();
            notificationManager = new NotificationManager();
            StartUdpListener();
            ConnectionService.OnServerMessage += ShowMessage;
            Title.Content = $"Hello,{user.FirstName}";
            if (user.Role == UserRole.USER)
            {
                users_menu_button.Visibility = Visibility.Collapsed;
                Users.Visibility = Visibility.Collapsed;
            }
            MainContent.Content = new DashboardView(user);
            DataContext = userParameter;
        }
        private void button_minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void button_close_Click(object sender, RoutedEventArgs e)
        {
            //byte[] buffer = Encoding.UTF8.GetBytes("ne");
            ConnectionService.UdpSocket.SendTo(aesClass.EncryptMessage("ne", aesClass.Key, aesClass.IV), ConnectionService.UdpEndpoint);
            OdjavaKlijenta();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        public void ShowToastNotification(ToastNotification toastNotification)
        {
            notificationManager.Show(toastNotification.Title, toastNotification.Message, toastNotification.Type, "WindowNotificationArea");
        }

        private void Dashboard_Tab_Button_Click(object sender, RoutedEventArgs e)
        {
            var u = userReository.GetUserById(user.ID);
            MainContent.Content = new DashboardView(u);
            Title.Content = $"Hello,{u.FirstName}";
        }

        private void StartUdpListener()
        {

            Task.Run(() =>
            {
                byte[] request = aesClass.EncryptMessage($"Klijent se povezao na UDP port: {ConnectionService.UdpEndpoint.Port}", aesClass.Key, aesClass.IV);
                //byte[] request = System.Text.Encoding.UTF8.GetBytes($"Klijent se povezao na UDP port: {ConnectionService.UdpEndpoint.Port}");
                ConnectionService.UdpSocket.SendTo(request, ConnectionService.UdpEndpoint);
                byte[] buffer = new byte[4096];
                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

                while (true)
                {
                    try
                    {
                        int bytesRead = ConnectionService.UdpSocket.ReceiveFrom(buffer, ref remoteEP);
                        // string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        byte[] primljeno = new byte[bytesRead];
                        Array.Copy(buffer, primljeno, bytesRead);
                        string msg=aesClass.DecryptMessage(primljeno, aesClass.Key,aesClass.IV);


                        if (msg.Contains("Session is expire"))
                        {
                            Dispatcher.Invoke(SessionExpired);
                            break;
                        }

                        Dispatcher.Invoke(() =>
                        {
                            var response = JsonSerializer.Deserialize<ResponseDTO>(msg);
                            if (response.Message.Equals("Command"))
                            {
                                MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
                                CommandRegister.Add(new Command { ID = CommandRegister.Count + 1, CreationDate = response.Timestamp, Log = $"[{response.Timestamp}] {response.Device.Name}: {response.Function} promenjena na {response.Value}" });
                                mainWindow.ShowToastNotification(new ToastNotification("Success", $"You are successfully set new value for device {response.Device.Name}", NotificationType.Success));
                                byte[] bytes = System.Text.Encoding.UTF8.GetBytes("da");
                                ConnectionService.UdpSocket.SendTo(aesClass.EncryptMessage("da",aesClass.Key,aesClass.IV), ConnectionService.UdpEndpoint);
                            }
                            else if (response.Message.Equals("Devices List"))
                            {
                                var list = response.Devices;
                                devices.Clear();
                                foreach (var u in list)
                                    devices.Add(u);
                                var listCommands = response.Commands;//all commands
                                CommandRegister.Clear();
                                foreach (var u in listCommands)
                                    CommandRegister.Add(u);
                            }
                            else if (response.Message.Equals("Users"))
                            {
                                var users = response.Users;
                                Title.Content = "Users";
                                MainContent.Content = new UsersView(new ObservableCollection<User>(users));
                            }
                        });
                    }
                    catch (ObjectDisposedException)
                    {
                        // socket zatvoren, prekini loop
                        break;
                    }
                    catch (SocketException)
                    {
                        // Sesija ili konekcija je prekinuta
                        Dispatcher.Invoke(SessionExpired);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Error: {ex.Message}");
                        });
                        break;
                    }
                }
            });
        }

        private void ShowMessage(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                // npr notification
                Console.WriteLine(msg);
            });
        }
        private void SessionExpired()
        {
            Dispatcher.Invoke(() =>
            {

                MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
                mainWindow.ShowToastNotification(new ToastNotification("Information", $"Your seession has expired", NotificationType.Notification));
                ConnectionService.SessionExpired();
                this.Close();
            });
        }

        private void users_menu_button_Click(object sender, RoutedEventArgs e)
        {
            //byte[] bytes = System.Text.Encoding.UTF8.GetBytes("users");
            ConnectionService.UdpSocket.SendTo(aesClass.EncryptMessage("users", aesClass.Key, aesClass.IV), ConnectionService.UdpEndpoint);
            //var users = userReository.GetAllUsers();
            //Title.Content = "Users";
            //MainContent.Content = new UsersView(new ObservableCollection<User>(users));
        }

        private void exit_menu_button_Click(object sender, RoutedEventArgs e)
        {
            //button_close_Click(sender, e);
            var result = MessageBox.Show("Do you sure that you want to exit now?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                button_close_Click(sender, e);
            }
        }

        private void devices_menu_button_Click(object sender, RoutedEventArgs e)
        {
            var d = deviceRepository.GetAllDevices().ToList();
            Title.Content = "Devices";
            MainContent.Content = new DevicesView(new ObservableCollection<Device>(d));
        }

        private void control_table_menu_button_Click(object sender, RoutedEventArgs e)
        {
            Title.Content = "Control Table";
            MainContent.Content = new ControlTableView(devices, CommandRegister, notificationManager,aesClass);
        }

        // metoda koja odjavljuje korisnika
        private void OdjavaKlijenta()
        {
            try
            {
                ConnectionService.SessionExpired(); // zatvaranje TCP i UDP soketa
            }
            catch { }

            this.Close();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            ProfilePopup.IsOpen = true;
        }

        private void button_logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Do you sure that you want to logout now?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                button_close_Click(sender, e);
            }
        }

        private void button_profile_Click(object sender, RoutedEventArgs e)
        {
            Title.Content = "Edit profile";
            MainContent.Content = new ProfileView(user,userReository);
        }

        private void button_password_Click(object sender, RoutedEventArgs e)
        {
            Title.Content = "Change password";
            MainContent.Content = new PasswordView(user, userReository);
        }
    }
}
