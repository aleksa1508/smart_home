using CentralniServer.Helpers;
using CentralniServer.Repositories.DevicesRepositories;
using CentralniServer.Repositories.RuleActionRepository;
using CentralniServer.Repositories.SmartRulesRepositories;
using CentralniServer.Repositories.UsersRepositories;
using CentralniServer.Service;
using CentralniServer.Services;
using Common;
using Common.DTOs;
using Common.Enums;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
namespace TCPServer
{

    public class Server
    {
        static void Main(string[] args)
        {

            int deviceNumber = 8;
            IDeviceRepository deviceRepository = new DeviceRepository();
            IDeviceService serverService = new DeviceService();
            ISmartRulesRepository smartRulesRepository = new SmartRulesRepository();
            IRuleActionRepository ruleActionRepository = new RuleActionRepository();
            Random random = new Random();
            SmartRulesService smartRulesService = new SmartRulesService(smartRulesRepository, ruleActionRepository);
            smartRulesService.AddNewSmartRules();


            RuleEngine ruleEngine = new RuleEngine();
            AesClass aesClass = new AesClass();
            RsaClass rsaClass = new RsaClass();
            string publicKeyXml = rsaClass.ExportPublicKey();
            byte[] publicKeyBytes = Encoding.UTF8.GetBytes(publicKeyXml);
            Dictionary<Socket, (byte[] Key, byte[] IV)> klijentKljucevi = new Dictionary<Socket, (byte[], byte[])>();
            Dictionary<Socket, RsaClass> klijentiRsaKljucevi = new Dictionary<Socket, RsaClass>();

            const int MAX_NEAKTIVNIH_CIKLUSA = 20;
            User k = new User();
            Device u = new Device();
            IUserReository userReository = new UserRepository();
            //server initialization
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50001);

            serverSocket.Bind(serverEP);
            serverSocket.Blocking = false;
            serverSocket.Listen(5);

            List<Process> runningProcess = new List<Process>();
            serverService.RunDevices(runningProcess, deviceNumber);
            BinaryFormatter formatter = new BinaryFormatter();

            Console.WriteLine($"Server is listening and expecting communitcation on {serverEP}");
            Dictionary<Socket, Socket> tcpUdpVeza = new Dictionary<Socket, Socket>();

            List<Socket> klijenti = new List<Socket>();
            List<Socket> udpSockets = new List<Socket>();
            int udpPort1 = 0;
            Dictionary<Socket, int> udpNeaktivnost = new Dictionary<Socket, int>();

            bool kraj = false;

            byte[] buffer = new byte[65507];
            try
            {
                while (!kraj)
                {
                    List<Socket> checkRead = new List<Socket>();
                    List<Socket> checkError = new List<Socket>();

                    if (klijenti.Count < 5)
                    {
                        checkRead.Add(serverSocket);

                    }
                    checkError.Add(serverSocket);

                    foreach (Socket s in klijenti)
                    {
                        checkRead.Add(s);
                        checkError.Add(s);
                    }

                    foreach (Socket s in udpSockets)
                    {
                        checkRead.Add(s);
                        checkError.Add(s);
                    }
                    Socket.Select(checkRead, null, checkError, 1000 * 1000);




                    if (checkRead.Count > 0)
                    {
                        Console.WriteLine($"Events number: {checkRead.Count}");
                        foreach (Socket s in new List<Socket>(checkRead))           //working with copy because when we delete or changing sockets ->exception
                        {
                            if (s == serverSocket)
                            {

                                Socket client = serverSocket.Accept();
                                client.Blocking = false;
                                klijenti.Add(client);
                                Console.WriteLine($"Client connected: {client.RemoteEndPoint}");

                            }
                            else if (udpSockets.Contains(s))
                            {

                                EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                                List<Device> allDevices = deviceRepository.GetAllDevices().ToList();

                                int receivedBytes = s.ReceiveFrom(buffer, ref clientEP);
                                if (receivedBytes > 0)
                                {
                                    udpNeaktivnost[s] = 0;
                                    byte[] recievedBytes = new byte[receivedBytes];
                                    Array.Copy(buffer, recievedBytes, receivedBytes);
                                    string receivedMessage = aesClass.DecryptMessage(recievedBytes, klijentKljucevi[s].Key, klijentKljucevi[s].IV);
                                    if (receivedMessage == "ne")
                                    {
                                        userReository.DeactivateByPort(((IPEndPoint)s.LocalEndPoint).Port);
                                        userReository.PrintAllUsers();

                                        s.Close();
                                        udpSockets.Remove(s);
                                        udpNeaktivnost.Remove(s);
                                        klijentKljucevi.Remove(s);

                                    }
                                    else if (receivedMessage == "da")
                                    {
                                        Console.WriteLine("All devices:");
                                        deviceRepository.PrintAllDevices("");
                                        allDevices = deviceRepository.GetAllDevices().ToList();

                                        var commands = deviceRepository.GetAllCommands().ToList();
                                        var content = new ResponseDTO
                                        {
                                            Message = "Devices List",
                                            Devices = allDevices,
                                            Commands = commands,
                                            SmartRules = smartRulesRepository.GetAllSmartRules().ToList(),
                                        };
                                        string json = JsonSerializer.Serialize(content);
                                        byte[] data = aesClass.EncryptMessage(json, klijentKljucevi[s].Key, klijentKljucevi[s].IV);
                                        s.SendTo(data, clientEP);
                                    }
                                    else if (receivedMessage == "users")
                                    {
                                        var users = userReository.GetAllUsers().ToList();
                                        var content = new ResponseDTO
                                        {
                                            Message = "AllUsers",
                                            Users = users
                                        };
                                        string json = JsonSerializer.Serialize(content);
                                        byte[] data = aesClass.EncryptMessage(json, klijentKljucevi[s].Key, klijentKljucevi[s].IV);
                                        s.SendTo(data, clientEP);
                                    }
                                    else if (receivedMessage.Contains("\"Action\":\"deleteUser\""))
                                    {
                                        var user = JsonSerializer.Deserialize<OwnerCommandDTO>(receivedMessage);
                                        userReository.DeleteUser(user.ChangedUser.ID);
                                        var content = new ResponseDTO
                                        {
                                            Message = "UpdateUsers",
                                            Value = "Successfully delete user",
                                            Users = userReository.GetAllUsers().ToList()
                                        };
                                        userReository.PrintAllUsers();
                                        string json = JsonSerializer.Serialize(content);
                                        byte[] data = aesClass.EncryptMessage(json, klijentKljucevi[s].Key, klijentKljucevi[s].IV);
                                        s.SendTo(data, clientEP);

                                    }
                                    else if (receivedMessage.Contains("\"Action\":\"updateUser\""))
                                    {
                                        var user = JsonSerializer.Deserialize<OwnerCommandDTO>(receivedMessage);
                                        userReository.UpdateData(user.ChangedUser.ID, user.ChangedUser.FirstName, user.ChangedUser.LastName, user.ChangedUser.Username);
                                        var content = new ResponseDTO
                                        {
                                            Message = "UpdateUsers",
                                            Value = "Successfully update user details",
                                            Users = userReository.GetAllUsers().ToList()
                                        };
                                        userReository.PrintAllUsers();
                                        string json = JsonSerializer.Serialize(content);
                                        byte[] data = aesClass.EncryptMessage(json, klijentKljucevi[s].Key, klijentKljucevi[s].IV);
                                        s.SendTo(data, clientEP);

                                    }
                                    else if (receivedMessage.Contains("\"Action\":\"updatePassword\""))
                                    {
                                        var user = JsonSerializer.Deserialize<OwnerCommandDTO>(receivedMessage);
                                        userReository.UpdatePassword(user.ChangedUser.ID, user.ChangedUser.Password);
                                        var content = new ResponseDTO
                                        {
                                            Message = "UpdateUsers",
                                            Value = "Successfully update password",
                                            Users = userReository.GetAllUsers().ToList()
                                        };
                                        userReository.PrintAllUsers();
                                        string json = JsonSerializer.Serialize(content);
                                        byte[] data = aesClass.EncryptMessage(json, klijentKljucevi[s].Key, klijentKljucevi[s].IV);
                                        s.SendTo(data, clientEP);

                                    }
                                    else if (receivedMessage.Contains("\"Action\":\"newUser\""))
                                    {
                                        var user = JsonSerializer.Deserialize<OwnerCommandDTO>(receivedMessage);
                                        var exist = userReository.GetAllUsers().Where(x => x.Username == user.ChangedUser.Username).ToList();
                                        string response = exist.Count > 0 ? "Username has already exist" : "Successfully create user";
                                        userReository.AddUser(user.ChangedUser.FirstName, user.ChangedUser.LastName, user.ChangedUser.Username, user.ChangedUser.Password, user.ChangedUser.Role.ToString());
                                        var content = new ResponseDTO
                                        {
                                            Message = "UpdateUsers",
                                            Value = response,
                                            Users = userReository.GetAllUsers().ToList()
                                        };
                                        userReository.PrintAllUsers();
                                        string json = JsonSerializer.Serialize(content);
                                        byte[] data = aesClass.EncryptMessage(json, klijentKljucevi[s].Key, klijentKljucevi[s].IV);
                                        s.SendTo(data, clientEP);

                                    }
                                    else if (receivedMessage.Contains("\"Action\":\"smartRule\""))
                                    {
                                        var rule = JsonSerializer.Deserialize<SmartRuleDTO>(receivedMessage);
                                        smartRulesRepository.UpdateSmartRule(rule.SmartRule);
                                        var actions = ruleActionRepository.GetAllActionsByRuleId(rule.SmartRule.Id).ToList();
                                        ruleEngine.ApplyRuleEffects(rule.SmartRule, actions, deviceRepository);
                                        var content = new ResponseDTO
                                        {
                                            Message = "Smart Rules",
                                            Value = $"Smart rule {rule.SmartRule.Name} is {(rule.SmartRule.IsEnabled == true ? "ON" : "OFF")}",
                                            SmartRules = smartRulesRepository.GetAllSmartRules().ToList()

                                        };
                                        smartRulesRepository.PrintAllSmartRules();
                                        string json = JsonSerializer.Serialize(content);
                                        byte[] data = aesClass.EncryptMessage(json, klijentKljucevi[s].Key, klijentKljucevi[s].IV);
                                        s.SendTo(data, clientEP);

                                    }
                                    else if (receivedMessage.Contains("\"Action\":\"deleteRule\""))
                                    {
                                        var rule = JsonSerializer.Deserialize<SmartRuleDTO>(receivedMessage);
                                        smartRulesRepository.DeleteSmartRule(rule.SmartRule);
                                        ruleActionRepository.DeleteRuleAction(rule.SmartRule.Id);
                                        var content = new ResponseDTO
                                        {
                                            Message = "Smart Rules",
                                            Value = "Successfully delete smart rule",
                                            SmartRules = smartRulesRepository.GetAllSmartRules().ToList()

                                        };
                                        smartRulesRepository.PrintAllSmartRules();
                                        string json = JsonSerializer.Serialize(content);
                                        byte[] data = aesClass.EncryptMessage(json, klijentKljucevi[s].Key, klijentKljucevi[s].IV);
                                        s.SendTo(data, clientEP);

                                    }
                                    else if (receivedMessage.Contains("\"Action\":\"newRule\""))
                                    {
                                        var rule = JsonSerializer.Deserialize<SmartRuleDTO>(receivedMessage);
                                        int id = 0;
                                        id = smartRulesRepository.GetSmartRuleByName(rule.SmartRule.Name);
                                        if (id != 0)
                                        {
                                            var cont = new ResponseDTO
                                            {
                                                Message = "Smart Rules",
                                                Value = $"Smart rule with this name has already exist",
                                                SmartRules = smartRulesRepository.GetAllSmartRules().ToList()
                                            };
                                            string jsonContent = JsonSerializer.Serialize(cont);
                                            byte[] dataContent = aesClass.EncryptMessage(jsonContent, klijentKljucevi[s].Key, klijentKljucevi[s].IV);
                                            s.SendTo(dataContent, clientEP);
                                            continue;
                                        }
                                        smartRulesRepository.AddSmartRule(rule.SmartRule.Name, rule.SmartRule.Description, rule.SmartRule.IsEnabled);
                                        foreach (var a in rule.Actions)
                                        {

                                            if (a.Device is null)
                                            {
                                                ruleActionRepository.AddRuleAction(id, a.FunctionName, a.Value, a.DeviceGroup, null, null);
                                            }
                                            else
                                            {
                                                ruleActionRepository.AddRuleAction(id, a.FunctionName, a.Value, null, a.FunctionId, a.Device.Id);

                                            }
                                        }
                                        var content = new ResponseDTO
                                        {
                                            Message = "Smart Rules",
                                            Value = $"Smart rule is successfully created",
                                            SmartRules = smartRulesRepository.GetAllSmartRules().ToList()

                                        };
                                        smartRulesRepository.PrintAllSmartRules();
                                        string json = JsonSerializer.Serialize(content);
                                        byte[] data = aesClass.EncryptMessage(json, klijentKljucevi[s].Key, klijentKljucevi[s].IV);
                                        s.SendTo(data, clientEP);

                                    }
                                    else if (receivedMessage.Contains("\"Action\":\"userRole\""))
                                    {
                                        var users = userReository.GetAllUsers().ToList();
                                        var komanda = JsonSerializer.Deserialize<OwnerCommandDTO>(receivedMessage);

                                        var owner = komanda.Owner;
                                        var user = komanda.ChangedUser;
                                        var newRole = komanda.Role;
                                        var existUser = users.Find(x => x.ID == user.ID);

                                        string json = string.Empty;
                                        if (owner.Role != UserRole.OWNER || existUser == null)
                                        {
                                            var content = new ResponseDTO
                                            {
                                                Message = "AdminCommand",
                                                Value = "Role change unsuccessful",
                                            };
                                            json = JsonSerializer.Serialize(content);
                                        }
                                        else
                                        {
                                            userReository.UpdateUserRole(user.ID, newRole);
                                            var content = new ResponseDTO
                                            {
                                                Message = "UpdateUsers",
                                                Value = "Role change successful",
                                                Users = userReository.GetAllUsers().ToList()
                                            };
                                            json = JsonSerializer.Serialize(content);

                                        }

                                        byte[] data = aesClass.EncryptMessage(json, klijentKljucevi[s].Key, klijentKljucevi[s].IV);
                                        s.SendTo(data, clientEP);
                                    }

                                    else
                                    {
                                        var komanda = JsonSerializer.Deserialize<CommandDTO>(receivedMessage);
                                        var users = userReository.GetAllUsers().ToList();
                                        var rules = smartRulesRepository.GetAllSmartRules().ToList();
                                        var actions = ruleActionRepository.GetAllActions().ToList();
                                        var u1 = komanda.SelectedDevice;
                                        var id = komanda.FunctionID;
                                        var funkcija = komanda.Function;
                                        var vrednost = komanda.Value;
                                        var username = komanda.Username;
                                        var existUser = users.Find(x => x.Username == username);

                                        string blockMessage;
                                        if (existUser.Role != UserRole.OWNER)
                                        {
                                            if (ruleEngine.BlockCommand(rules, actions, komanda, out blockMessage))
                                            {
                                                var blockedResponse = new ResponseDTO
                                                {
                                                    Message = "SmartRuleCommand",
                                                    Value = blockMessage
                                                };

                                                string blockedJson = JsonSerializer.Serialize(blockedResponse);

                                                byte[] blockedData = aesClass.EncryptMessage(
                                                    blockedJson,
                                                    klijentKljucevi[s].Key,
                                                    klijentKljucevi[s].IV);

                                                s.SendTo(blockedData, clientEP);

                                                continue;
                                            }
                                        }
                                        deviceRepository.UpdateDeviceFunction(u1.Id, id, funkcija, vrednost);
                                        //connecting between device and server
                                        IPEndPoint deviceEP = new IPEndPoint(IPAddress.Loopback, u1.Port);
                                        byte[] initialData = Encoding.UTF8.GetBytes(u1.Name + ":" + funkcija + ":" + vrednost);
                                        s.SendTo(initialData, deviceEP);

                                        DateTime timestamp = DateTime.Now;
                                        string log = $"[{timestamp}] {u1.Name}: {funkcija} changed on {vrednost}";
                                        deviceRepository.AddDeviceCommands(log, DateTime.Now, u1.Id, username);

                                        var content = new ResponseDTO
                                        {
                                            Message = "Command",
                                            Device = u1,
                                            Function = funkcija,
                                            Value = vrednost,
                                            Username = username,
                                            Timestamp = timestamp
                                        };
                                        string json = JsonSerializer.Serialize(content);
                                        byte[] data = aesClass.EncryptMessage(json, klijentKljucevi[s].Key, klijentKljucevi[s].IV);
                                        s.SendTo(data, clientEP);
                                        Console.WriteLine("All devices (green device currenty changed):");
                                        deviceRepository.PrintAllDevices(u1.Name);

                                    }
                                }
                            }
                            else
                            {
                                int receivedBytes1 = s.Receive(buffer);
                                if (receivedBytes1 > 0)
                                {
                                    string poruka = Encoding.UTF8.GetString(buffer, 0, receivedBytes1);
                                    if (poruka == "shutdown")
                                    {

                                        try
                                        {
                                            s.Send(Encoding.UTF8.GetBytes("DISCONNECT_OK"));
                                            Thread.Sleep(1000);
                                        }
                                        catch { }
                                        s.Close();
                                        klijenti.Remove(s);
                                        klijentiRsaKljucevi.Remove(s);
                                        Console.WriteLine($"Number of active clients: {klijenti.Count}");

                                        // check active clients
                                        if (klijenti.Count == 0)
                                        {
                                            using (Socket tempUdp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                                            {
                                                byte[] initialData = Encoding.UTF8.GetBytes("Server has stopped working");
                                                for (int i = 0; i < deviceNumber; i++)
                                                {
                                                    IPEndPoint device = new IPEndPoint(IPAddress.Loopback, 60001);  //sending message to device shut down
                                                    tempUdp.SendTo(initialData, device);
                                                }

                                                Thread.Sleep(2000);
                                            }
                                            Console.WriteLine("No more connected clients.Server is shutting down");
                                            kraj = true;
                                        }

                                        continue;
                                    }
                                    else if (poruka == "GET_PUBLIC_KEY")
                                    {
                                        //sent client RSA public key
                                        byte[] keyLengthBytes = BitConverter.GetBytes(publicKeyBytes.Length);
                                        s.Send(keyLengthBytes);
                                        Thread.Sleep(50);
                                        s.Send(publicKeyBytes);
                                        Console.WriteLine("RSA public key successfully sent to client.");
                                    }
                                    else
                                    {
                                        //login data and client RSA public key
                                        byte[] primljenoSve = new byte[receivedBytes1];
                                        Array.Copy(buffer, primljenoSve, receivedBytes1);

                                        RsaClass clientRsa;
                                        byte[] encryptedLogin;
                                        try
                                        {
                                            int clientKeyLen = BitConverter.ToInt32(primljenoSve, 0);
                                            byte[] clientPubKeyBytes = new byte[clientKeyLen];
                                            Array.Copy(primljenoSve, 4, clientPubKeyBytes, 0, clientKeyLen);
                                            string clientPublicKeyXml = Encoding.UTF8.GetString(clientPubKeyBytes);
                                            clientRsa = new RsaClass(clientPublicKeyXml);

                                            int loginOffset = 4 + clientKeyLen;
                                            int loginLen = receivedBytes1 - loginOffset;
                                            encryptedLogin = new byte[loginLen];
                                            Array.Copy(primljenoSve, loginOffset, encryptedLogin, 0, loginLen);
                                        }
                                        catch
                                        {
                                            s.Send(Encoding.UTF8.GetBytes("UNSUCCESS"));
                                            continue;
                                        }

                                        string loginData;
                                        try
                                        {
                                            loginData = rsaClass.Decrypt(encryptedLogin);
                                        }
                                        catch
                                        {
                                            s.Send(Encoding.UTF8.GetBytes("UNSUCCESS"));
                                            continue;
                                        }

                                        string[] djelovi = loginData.Split(':');
                                        User logInUser = userReository.GetKorisnik(djelovi[0], djelovi[1]);


                                        if (djelovi.Length == 2 && logInUser != null)
                                        {
                                            //save client RSA public key
                                            klijentiRsaKljucevi[s] = clientRsa;

                                            byte[] key = new byte[16];
                                            byte[] iv = new byte[16];
                                            using (RandomNumberGenerator randomNumber = RandomNumberGenerator.Create())
                                            {
                                                randomNumber.GetBytes(key);
                                                randomNumber.GetBytes(iv);
                                            }

                                            string odgovor = "SUCCESS";
                                            s.Send(Encoding.UTF8.GetBytes(odgovor));
                                            Thread.Sleep(200);

                                            byte[] keyData = new byte[32];
                                            Array.Copy(key, 0, keyData, 0, 16);
                                            Array.Copy(iv, 0, keyData, 16, 16);

                                            byte[] encryptedKeyData = clientRsa.EncryptBytes(keyData);
                                            byte[] encKeyLenBytes = BitConverter.GetBytes(encryptedKeyData.Length);
                                            s.Send(encKeyLenBytes);
                                            Thread.Sleep(50);
                                            s.Send(encryptedKeyData);
                                            Thread.Sleep(200);

                                            udpPort1 = random.Next(50002, 60000);

                                            logInUser.Port = udpPort1;
                                            userReository.UpdateStatus(logInUser.ID, ActiveStatus.ACTIVE, udpPort1);
                                            s.Send(aesClass.EncryptMessage(udpPort1.ToString(), key, iv));

                                            if (tcpUdpVeza.ContainsKey(s))
                                            {
                                                Socket stariUdp = tcpUdpVeza[s];
                                                if (udpSockets.Contains(stariUdp))
                                                {
                                                    stariUdp.Close();
                                                    udpSockets.Remove(stariUdp);
                                                    udpNeaktivnost.Remove(stariUdp);
                                                    klijentKljucevi.Remove(stariUdp);
                                                }
                                                tcpUdpVeza.Remove(s);
                                            }


                                            //creating udp socket for comunnication with client
                                            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                                            IPEndPoint udpServerEP = new IPEndPoint(IPAddress.Loopback, udpPort1);
                                            udpSocket.Bind(udpServerEP);
                                            udpSockets.Add(udpSocket);
                                            udpNeaktivnost[udpSocket] = 0;
                                            tcpUdpVeza[s] = udpSocket;      //for crach connection
                                            Console.WriteLine($"UDP socket created on port {udpPort1}");

                                            EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                                            int receivedBytes = udpSocket.ReceiveFrom(buffer, ref clientEP);
                                            byte[] recievedBytes = new byte[receivedBytes];
                                            Array.Copy(buffer, recievedBytes, receivedBytes);

                                            string receivedMessage = aesClass.DecryptMessage(recievedBytes, key, iv);

                                            udpSocket.Blocking = false;
                                            Console.WriteLine($"Message from UDP client : {receivedMessage}");
                                            Console.WriteLine("All users:");
                                            userReository.PrintAllUsers();

                                            klijentKljucevi[udpSocket] = (key, iv); //add keys in dictionary

                                            List<Device> devices = deviceRepository.GetAllDevices().ToList();
                                            List<SmartRule> smartRules = smartRulesRepository.GetAllSmartRules().ToList();
                                            Console.WriteLine("All devices:");
                                            deviceRepository.PrintAllDevices("");
                                            Console.WriteLine("All smart rules:");
                                            smartRulesRepository.PrintAllSmartRules();

                                            var commands = deviceRepository.GetAllCommands().ToList();
                                            var content = new ResponseDTO
                                            {
                                                Message = "Devices List",
                                                Devices = devices,
                                                Commands = commands,
                                                SmartRules = smartRules,
                                                Users = new List<User> { logInUser }
                                            };
                                            string json = JsonSerializer.Serialize(content);
                                            byte[] data = aesClass.EncryptMessage(json, key, iv);
                                            udpSocket.SendTo(data, clientEP);

                                        }
                                        else
                                        {
                                            string odgovor = "UNSUCCESS";
                                            s.Send(Encoding.UTF8.GetBytes(odgovor));
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Client connection lost.");//if tcp connection will be forced closed

                                    if (tcpUdpVeza.ContainsKey(s))
                                    {
                                        Socket udpZaOvogKorisnika = tcpUdpVeza[s];
                                        if (udpSockets.Contains(udpZaOvogKorisnika))
                                        {
                                            userReository.DeactivateByPort(((IPEndPoint)udpZaOvogKorisnika.LocalEndPoint).Port);
                                            udpZaOvogKorisnika.Close();
                                            udpSockets.Remove(udpZaOvogKorisnika);
                                            udpNeaktivnost.Remove(udpZaOvogKorisnika);
                                            klijentKljucevi.Remove(udpZaOvogKorisnika);
                                        }
                                        tcpUdpVeza.Remove(s);
                                    }
                                    s.Close();
                                    checkRead.Remove(s);      //remove socket form lists
                                    klijenti.Remove(s);
                                    klijentiRsaKljucevi.Remove(s); // NOVO: ciscenje sacuvanog klijentovog RSA kljuca
                                    continue;
                                }
                            }


                        }
                    }
                    foreach (var udpSocket in new List<Socket>(udpNeaktivnost.Keys))
                    {
                        udpNeaktivnost[udpSocket]++;

                        if (udpNeaktivnost[udpSocket] >= MAX_NEAKTIVNIH_CIKLUSA)
                        {
                            Console.WriteLine($"UDP session on port {((IPEndPoint)udpSocket.LocalEndPoint).Port}  has been closed due the client inactivity.");
                            userReository.DeactivateByPort(((IPEndPoint)udpSocket.LocalEndPoint).Port);
                            userReository.PrintAllUsers();
                            Socket tcpSocket = klijenti.FirstOrDefault(s => ((IPEndPoint)s.RemoteEndPoint).Port == ((IPEndPoint)udpSocket.LocalEndPoint).Port);

                            if (tcpSocket != null)
                            {
                                try
                                {
                                    string obavestenje = "Session is expire. Please log in again.";
                                    tcpSocket.Send(Encoding.UTF8.GetBytes(obavestenje));
                                }
                                catch (SocketException)
                                {
                                    Console.WriteLine("Error while sending notification to user");
                                }

                                // closing TCP connection and removing user from the list
                                tcpSocket.Close();
                                klijenti.Remove(tcpSocket);
                                klijentiRsaKljucevi.Remove(tcpSocket); // NOVO: ciscenje sacuvanog klijentovog RSA kljuca
                            }

                            // closing UDP socket and removing from the list
                            udpSocket.Close();
                            udpSockets.Remove(udpSocket);
                            udpNeaktivnost.Remove(udpSocket);
                            klijentKljucevi.Remove(udpSocket);
                        }
                    }
                    checkError.Clear();
                    checkRead.Clear();
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Error: {ex}");
            }

            foreach (Socket s in klijenti)
            {
                s.Close();
            }
            foreach (var udpSocket in udpSockets)
            {
                udpSocket.Close();
            }

            foreach (Process p in runningProcess)
            {
                try
                {
                    if (!p.HasExited)
                    {
                        p.Kill();
                        Console.WriteLine($"Device process with ID: {p.Id} has closed");
                    }
                }
                catch { }
            }
            serverSocket.Close();
            Console.WriteLine("Server is shutting down");
            Console.ReadKey();
        }

    }
}