using Client.Helpers;
using Common;
using Common.DTOs;
using Common.Enums;
using Common.Models;
using Notification.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
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
        private ObservableCollection<SmartRule> SmartRules;
        private AesClass aesClass;
        private DashboardView dashboardView;
        public Dashboard(AesClass aes, User loggedUser, ResponseDTO initialData)
        {
            InitializeComponent();
            aesClass = aes;
            user = loggedUser;

            devices = new ObservableCollection<Device>();
            SmartRules = new ObservableCollection<SmartRule>();
            CommandRegister = new ObservableCollection<Command>();
            foreach (var d in initialData.Devices)
                devices.Add(d);
            foreach (var s in initialData.SmartRules)
                SmartRules.Add(s);
            foreach (var c in initialData.Commands)
                CommandRegister.Add(c);

            notificationManager = new NotificationManager();
            StartUdpListener();
            ConnectionService.OnServerMessage += ShowMessage;
            Title.Content = $"Hello,{user.FirstName}";
            if (user.Role == UserRole.USER)
            {
                users_menu_button.Visibility = Visibility.Collapsed;
                SmartRules_Tab_Button.Visibility = Visibility.Collapsed;
                Users.Visibility = Visibility.Collapsed;
            }
            dashboardView = new DashboardView(user, devices);
            MainContent.Content = dashboardView;
            DataContext = user;
        }
        private void button_minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void button_close_Click(object sender, RoutedEventArgs e)
        {
            ConnectionService.UdpSocket.SendTo(aesClass.EncryptMessage("ne", aesClass.Key, aesClass.IV), ConnectionService.UdpEndpoint);
            UserLogOut();
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
            dashboardView = new DashboardView(user, devices);
            MainContent.Content = dashboardView;
            Title.Content = $"Hello,{user.FirstName}";
        }

        private void StartUdpListener()
        {

            Task.Run(() =>
            {
                byte[] buffer = new byte[65507];
                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

                while (true)
                {
                    try
                    {
                        Console.WriteLine("UDP: Wait on ReceiveFrom...");
                        int bytesRead = ConnectionService.UdpSocket.ReceiveFrom(buffer, ref remoteEP);
                        Console.WriteLine($"UDP: Recieved {bytesRead} bytes");
                        byte[] primljeno = new byte[bytesRead];
                        Array.Copy(buffer, primljeno, bytesRead);
                        string msg = aesClass.DecryptMessage(primljeno, aesClass.Key, aesClass.IV);


                        if (msg.Contains("Session is expire"))
                        {
                            Dispatcher.Invoke(SessionExpired);
                            break;
                        }

                        Dispatcher.Invoke(() =>
                        {
                            var response = JsonSerializer.Deserialize<ResponseDTO>(msg);
                            HandleResponse(response);
                        });
                    }
                    catch (ObjectDisposedException)
                    {
                        // socket closed
                        break;
                    }
                    catch (SocketException ex)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Console.WriteLine($"SocketException kod: {ex.SocketErrorCode}\nPoruka: {ex.Message}");
                        });
                        Dispatcher.Invoke(SessionExpired);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            Console.WriteLine($"Exception tip: {ex.GetType().Name}\nPoruka: {ex.Message}");
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
            ConnectionService.UdpSocket.SendTo(aesClass.EncryptMessage("users", aesClass.Key, aesClass.IV), ConnectionService.UdpEndpoint);
        }

        private void exit_menu_button_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Do you sure that you want to exit now?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                button_close_Click(sender, e);
            }
        }

        private void devices_menu_button_Click(object sender, RoutedEventArgs e)
        {
            var d = devices;
            Title.Content = "Devices";
            MainContent.Content = new DevicesView(new ObservableCollection<Device>(d), user);
        }

        private void control_table_menu_button_Click(object sender, RoutedEventArgs e)
        {
            Title.Content = "Control Table";
            MainContent.Content = new ControlTableView(devices, CommandRegister, notificationManager, aesClass, user);
        }
        private void UserLogOut()
        {
            try
            {
                ConnectionService.SessionExpired(); // closing TCP and UDP socekt
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
            MainContent.Content = new ProfileView(user, aesClass);
        }

        private void button_password_Click(object sender, RoutedEventArgs e)
        {
            Title.Content = "Change password";
            MainContent.Content = new PasswordView(user, aesClass);
        }

        private void SmartRules_Tab_Button_Click(object sender, RoutedEventArgs e)
        {
            Title.Content = "Smart rules";
            MainContent.Content = new SmartRulesView(SmartRules, devices, aesClass);
        }

        private void HandleResponse(ResponseDTO response)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            switch (response.Message)
            {
                case "Command":
                    {
                        AddCommand(response, mainWindow);
                        break;
                    }
                case "AdminCommand":
                    {
                        UpdateAdminCommand(response, mainWindow);
                        break;
                    }
                case "SmartRuleCommand":
                    {
                        UpdateSmartRule(response, mainWindow);
                        break;
                    }
                case "Smart Rules":
                    {
                        AddSmartRules(response, mainWindow);
                        break;
                    }
                case "UpdateUsers":
                    {
                        UpdateUsers(response, mainWindow);
                        break;
                    }
                case "Devices List":
                    {
                        UpdateDevices(response);
                        break;
                    }
                case "AllUsers":
                    {
                        AllUsers(response);
                        break;
                    }
            }
        }
        private void AddCommand(ResponseDTO response, MainWindow mainWindow)
        {
            CommandRegister.Add(new Command { ID = CommandRegister.Count + 1, CreationDate = response.Timestamp, Log = $"[{response.Timestamp}] {response.Device.Name}: {response.Function} changed on {response.Value}", Username = response.Username });
            mainWindow.ShowToastNotification(new ToastNotification("Success", $"You are successfully set new value for device {response.Device.Name}", NotificationType.Success));
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes("da");
            ConnectionService.UdpSocket.SendTo(aesClass.EncryptMessage("da", aesClass.Key, aesClass.IV), ConnectionService.UdpEndpoint);
        }
        private void UpdateAdminCommand(ResponseDTO response, MainWindow mainWindow)
        {
            mainWindow.ShowToastNotification(new ToastNotification("Information", response.Value, NotificationType.Information));
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes("users");
            ConnectionService.UdpSocket.SendTo(aesClass.EncryptMessage("users", aesClass.Key, aesClass.IV), ConnectionService.UdpEndpoint);
        }
        private void UpdateSmartRule(ResponseDTO response, MainWindow mainWindow)
        {
            mainWindow.ShowToastNotification(new ToastNotification("Error", response.Value, NotificationType.Error));
        }
        private void AddSmartRules(ResponseDTO response, MainWindow mainWindow)
        {
            mainWindow.ShowToastNotification(new ToastNotification("Information", response.Value, NotificationType.Information));
            var list = response.SmartRules;
            SmartRules.Clear();
            foreach (var s in list)
                SmartRules.Add(s);
        }
        private void AllUsers(ResponseDTO response)
        {
            var users = response.Users;
            Title.Content = "Users";
            MainContent.Content = new UsersView(user, new ObservableCollection<User>(users), aesClass);
        }
        private void UpdateUsers(ResponseDTO response, MainWindow mainWindow)
        {
            mainWindow.ShowToastNotification(new ToastNotification("Success", response.Value, NotificationType.Success));
            if (!response.Value.Equals("Successfully update password"))
            {
                AllUsers(response);
            }
        }
        private void UpdateDevices(ResponseDTO response)
        {
            var list = response.Devices;
            devices.Clear();
            foreach (var u in list)
                devices.Add(u);

            var rules = response.SmartRules;
            SmartRules.Clear();
            foreach (var s in rules)
                SmartRules.Add(s);

            if (MainContent.Content is ControlTableView controlView)
            {
                controlView.DeviceComboBox.ItemsSource = null;
                controlView.SetDevices(devices, user);
            }
            dashboardView.UpdateDevices(devices);
            dashboardView.LoadingOverlay.Visibility = Visibility.Collapsed;
            dashboardView.DeviceScrollViewer.Visibility = Visibility.Visible;
            var listCommands = response.Commands;//all commands
            CommandRegister.Clear();
            foreach (var u in listCommands)
                CommandRegister.Add(u);
        }
    }
}
