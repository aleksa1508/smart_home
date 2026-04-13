using Client.Helpers;
using Common;
using Common.Enums;
using Common.Repositories.UsersRepositories;
using Notification.Wpf;
using System;
using System.Collections.ObjectModel;
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
        private ObservableCollection<Uredjaj> uredjaji;
        private ObservableCollection<Komanda> EvidencijaKomandi;
        private IUserReository userReository;
        public Dashboard(User userParameter)
        {
            InitializeComponent();
            userReository = new UserRepository();
            user = userParameter;
            uredjaji = new ObservableCollection<Uredjaj>();
            EvidencijaKomandi = new ObservableCollection<Komanda>();
            notificationManager = new NotificationManager();
            StartUdpListener();
            ConnectionService.OnServerMessage += ShowMessage;
            Title.Content = $"Hello,{user.FirstName}";
            if (user.Role == UserRole.USER)
            {
                users_menu_button.Visibility = Visibility.Collapsed;
                Users.Visibility = Visibility.Collapsed;
            }
            MainContent.Content = new DashboardView();
            DataContext = userParameter;
        }
        private void button_minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void button_close_Click(object sender, RoutedEventArgs e)
        {
            byte[] buffer = Encoding.UTF8.GetBytes("ne");
            ConnectionService.UdpSocket.SendTo(buffer, ConnectionService.UdpEndpoint);
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
            MainContent.Content = new DashboardView();
            Title.Content = $"Hello,{user.FirstName}";
        }

        private void StartUdpListener()
        {

            Task.Run(() =>
            {
                byte[] request = System.Text.Encoding.UTF8.GetBytes($"Klijent se povezao na UDP port: {ConnectionService.UdpEndpoint.Port}");
                ConnectionService.UdpSocket.SendTo(request, ConnectionService.UdpEndpoint);
                byte[] buffer = new byte[4096];
                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

                while (true)
                {
                    try
                    {
                        int bytesRead = ConnectionService.UdpSocket.ReceiveFrom(buffer, ref remoteEP);
                        string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        if (msg.Contains("Sesija je istekla"))
                        {
                            Dispatcher.Invoke(SessionExpired);
                            break;
                        }

                        Dispatcher.Invoke(() =>
                        {
                            var response = JsonSerializer.Deserialize<ResponseDto>(msg);
                            if (response.Message.Equals("Command"))
                            {
                                MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
                                EvidencijaKomandi.Add(new Komanda { ID = EvidencijaKomandi.Count + 1, CreationDate = DateTime.Now, Log = $"[{DateTime.Now}] {response.Uredjaj.Ime}: {response.Function} promenjena na {response.Value}" });
                                mainWindow.ShowToastNotification(new ToastNotification("Success", $"You are successfully set new value for device {response.Uredjaj.Ime}", NotificationType.Success));
                                byte[] bytes = System.Text.Encoding.UTF8.GetBytes("da");
                                ConnectionService.UdpSocket.SendTo(bytes, ConnectionService.UdpEndpoint);
                            }
                            else if (response.Message.Equals("Devices List"))
                            {
                                var lista = response.Uredjaji;
                                uredjaji.Clear();
                                foreach (var u in lista)
                                    uredjaji.Add(u);
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
                mainWindow.ShowToastNotification(new ToastNotification("Information", $"Your seession is expire", NotificationType.Notification));
                ConnectionService.SessionExpired();
                this.Close();
            });
        }

        private void users_menu_button_Click(object sender, RoutedEventArgs e)
        {
            var users = userReository.GetAllUsers();
            Title.Content = "Users";
            MainContent.Content = new UsersView(new ObservableCollection<User>(users));
        }

        private void exit_menu_button_Click(object sender, RoutedEventArgs e)
        {
            button_close_Click(sender, e);
        }

        private void devices_menu_button_Click(object sender, RoutedEventArgs e)
        {
            Title.Content = "Devices";
            MainContent.Content = new DevicesView(uredjaji);
        }

        private void control_table_menu_button_Click(object sender, RoutedEventArgs e)
        {
            Title.Content = "Control Table";
            MainContent.Content = new ControlTableView(uredjaji, EvidencijaKomandi, notificationManager);
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
            MainContent.Content = new ProfileView(user,userReository);
        }
    }
}
