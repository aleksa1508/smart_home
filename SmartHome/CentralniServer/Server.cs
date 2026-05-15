using Common;
using Common.DTOs;
using Common.Enums;
using Common.Models;
using Common.Repositories.DevicesRepositories;
using Common.Repositories.SmartRulesRepositories;
using Common.Repositories.UsersRepositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography; // <-- dodaj
using System.Text;
using System.Text.Json;
using System.Threading;
//using UDPServer;
namespace TCPServer
{

    public class Server
    {
        // private static UdpServer udpServer;
        static List<Process> poknutiProcesi = new List<Process>();

        static void RunDevices(int brojKlijenata)
        {
            for (int i = 0; i < brojKlijenata; i++)
            {
                // Putanja do izvršnog fajla klijenta (potrebno je kompajlirati ga)
                string clientPath = @"C:\Users\Dell 3520\Desktop\AA\DIPLOMSKI\SmartHome\SmartHome\SmartHomeDevices\bin\Debug\DeviceClient.exe";
                Process klijentProces = new Process(); // Stvaranje novog procesa
                klijentProces.StartInfo.FileName = clientPath; //Zadavanje putanje za pokretanje
                klijentProces.StartInfo.Arguments = $"{i + 60001}"; // Argument - broj klijenta1
                klijentProces.Start(); // Pokretanje klijenta
                poknutiProcesi.Add(klijentProces); // ← sačuvaj referencu

                Console.WriteLine($"Pokrenut uredjaj #{i + 1}");
            }
        }




        static void Main(string[] args)
        {

            IDeviceRepository deviceRepository = new DeviceRepository();
            //List<Device> devices = new List<Device> {
            //            new Device(1,"Light Kitchen",60001,RoomType.KITCHEN,new Dictionary<int, Function>{ { 1, new Function { Name = "state", Value = "OFF" } },{ 2, new Function { Name = "brightness", Value = "23" } } },new List<Command>(),DateTime.Now),
            //            new Device(2,"Light Garage",60002,RoomType.GARAGE,new Dictionary<int, Function>{ { 1, new Function { Name = "state", Value = "OFF" } },{ 2, new Function { Name = "brightness", Value = "23" } } },new List<Command>(),DateTime.Now),
            //            new Device(3,"TV Living room",60003,RoomType.LIVING_ROOM,new Dictionary<int, Function>{ { 1, new Function { Name = "state", Value = "OFF" } },{ 2, new Function { Name = "channel", Value = "20" } } },new List<Command>(),DateTime.Now),
            //            new Device(4,"TV Bedroom",60004,RoomType.BEDROOM,new Dictionary<int, Function>{ { 1, new Function { Name = "state", Value = "OFF" } },{ 2, new Function { Name = "channel", Value = "3" } } },new List<Command>(),DateTime.Now),
            //            new Device(5, "Climate Living room", 60005,RoomType.LIVING_ROOM, new Dictionary < int, Function > { { 1, new Function { Name = "state", Value = "OFF" } }, { 2, new Function { Name = "temperature", Value = "12" } } }, new List < Command >(), DateTime.Now),
            //            new Device(6, "Climate Bedroom", 60006,RoomType.BEDROOM, new Dictionary < int, Function > { { 1, new Function { Name = "state", Value = "OFF" } }, { 2, new Function { Name = "temperature", Value = "20" } } }, new List < Command >(), DateTime.Now),
            //            new Device(7, "Door Garage", 60007,RoomType.GARAGE, new Dictionary < int, Function > { { 1, new Function { Name = "state", Value = "CLOSED" } } }, new List < Command >(), DateTime.Now),
            //            new Device(8, "Vault Bedroom", 60008,RoomType.BEDROOM, new Dictionary < int, Function > { { 1, new Function { Name = "state", Value = "CLOSED" } } }, new List < Command >(), DateTime.Now),
            //};
            //foreach (var d in devices)
            //{
            //    deviceRepository.AddDevice(d.Name, d.Port, d.Location, DateTime.Now);
            //    if (d.Functions.Count > 0)
            //    {
            //        foreach (var f in d.Functions)
            //        {
            //            deviceRepository.AddDeviceFunctions(f.Value.Name, f.Value.Value, d.Id);
            //        }
            //    }
            //    if (d.CommandRegister.Count > 0)
            //    {
            //        foreach (var f in d.CommandRegister)
            //        {
            //            deviceRepository.AddDeviceCommands(f.Log, DateTime.Now, d.Id,string.Empty);
            //        }
            //    }
            //}
            Random random = new Random();
            //byte[] key=new byte [16];
            //byte[] IV=new byte [16];
            //using (RandomNumberGenerator randomNumber=RandomNumberGenerator.Create())
            //{
            //    randomNumber.GetBytes(key);
            //    randomNumber.GetBytes(IV);
            //}
            //byte[] encryptMessage = EncryptMessage("Pera Peric",key,IV);


            //Console.WriteLine(Convert.ToBase64String(encryptMessage));
            //string decryptMessage = DecryptMessage(encryptMessage, key, IV);
            //Console.WriteLine(decryptMessage);
            List<SmartRule> rules1 = new List<SmartRule>
            {
                new SmartRule{ IsEnabled=false, Name="NightMode",Description="Limits temperature to 20°C, restricts lights and locks garage during night hours."},
                new SmartRule{ IsEnabled=false, Name="SecurityMode",Description="Lock all doors and vaults."},
                new SmartRule{ IsEnabled=false, Name="EnergySaving",Description="Limits brightness and reduces energy usage."},
            };
            ISmartRulesRepository smartRulesRepository = new SmartRulesRepository();
            foreach (var r in rules1)
            {
                smartRulesRepository.AddSmartRule(r.Name, r.Description, r.IsEnabled);
            }

            RuleEngine ruleEngine = new RuleEngine();
            AesClass aesClass = new AesClass();
            RsaClass rsaClass = new RsaClass();
            string publicKeyXml = rsaClass.ExportPublicKey();
            byte[] publicKeyBytes = Encoding.UTF8.GetBytes(publicKeyXml);
            Dictionary<Socket, (byte[] Key, byte[] IV)> klijentKljucevi = new Dictionary<Socket, (byte[], byte[])>();
            User k = new User();
            Device u = new Device();

            IUserReository userReository = new UserRepository();
            List<User> listaKorisnika = userReository.GetAllUsers().ToList();
            //inicijalizacija servera
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, 50001);

            serverSocket.Bind(serverEP);
            serverSocket.Blocking = false;
            serverSocket.Listen(5);

            BinaryFormatter formatter = new BinaryFormatter();

            Console.WriteLine($"Server is listening and expecting communitcation on {serverEP}");
            Dictionary<Socket, Socket> tcpUdpVeza = new Dictionary<Socket, Socket>();       //ako klijent crash-uje tj posalje shutdown prije ne (crashuje u dashboard i onda udp soket ostaje ziv a tcp konekcija pada)

            List<Socket> klijenti = new List<Socket>(); // Pravimo posebnu listu za klijentske sokete kako nam je ne bi obrisala Select funkcija
            List<Socket> udpSockets = new List<Socket>();
            int udpPort1 = 0;
            Dictionary<Socket, int> udpNeaktivnost = new Dictionary<Socket, int>(); // UDP soketi i broj ciklusa neaktivnosti
            const int MAX_NEAKTIVNIH_CIKLUSA = 20; // Maksimalan broj ciklusa neaktivnosti pre zatvaranja


            RunDevices(8);
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
                        foreach (Socket s in new List<Socket>(checkRead))           //raditi sa kopijom da ne diramo original jer prilikom brisanja i mjenjanja dolazi do exception-a
                        {
                            if (s == serverSocket)
                            {

                                Socket client = serverSocket.Accept();
                                client.Blocking = false;
                                klijenti.Add(client);
                                Console.WriteLine($"Client connected: {client.RemoteEndPoint}");

                            }
                            /*
                             
                             */
                            else if (udpSockets.Contains(s))
                            {

                                EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                                List<Device> uredjaji = deviceRepository.GetAllDevices().ToList();

                                int receivedBytes = s.ReceiveFrom(buffer, ref clientEP);
                                if (receivedBytes > 0)
                                {
                                    udpNeaktivnost[s] = 0;
                                    byte[] primljeno = new byte[receivedBytes];
                                    Array.Copy(buffer, primljeno, receivedBytes);
                                    //string receivedMessage = Encoding.UTF8.GetString(primljeno, 0, receivedBytes);
                                    string receivedMessage = aesClass.DecryptMessage(primljeno, klijentKljucevi[s].Key, klijentKljucevi[s].IV);
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
                                        //deviceRepository.PrintAllDevices();
                                        Console.WriteLine("All devices:");
                                        deviceRepository.PrintAllDevices("");
                                        uredjaji = deviceRepository.GetAllDevices().ToList();

                                        //using (MemoryStream ms = new MemoryStream())
                                        //{

                                        //    formatter.Serialize(ms, uredjaji);
                                        //    byte[] data = ms.ToArray();
                                        //    s.SendTo(data, clientEP);
                                        //}
                                        var commands = deviceRepository.GetAllCommands().ToList();
                                        var content = new ResponseDTO
                                        {
                                            Message = "Devices List",
                                            Devices = uredjaji,
                                            Commands = commands,
                                            SmartRules = smartRulesRepository.GetAllSmartRules().ToList(),
                                        };
                                        string json = JsonSerializer.Serialize(content);
                                        //byte[] data = Encoding.UTF8.GetBytes(json);
                                        byte[] data = aesClass.EncryptMessage(json, klijentKljucevi[s].Key, klijentKljucevi[s].IV);
                                        s.SendTo(data, clientEP);
                                    }
                                    else if (receivedMessage == "users")
                                    {
                                        //deviceRepository.PrintAllDevices();
                                        //Console.WriteLine(deviceRepository.PrintAllDevices());
                                        var users = userReository.GetAllUsers().ToList();

                                        //using (MemoryStream ms = new MemoryStream())
                                        //{

                                        //    formatter.Serialize(ms, uredjaji);
                                        //    byte[] data = ms.ToArray();
                                        //    s.SendTo(data, clientEP);
                                        //}
                                        //var commands = deviceRepository.GetAllCommands().ToList();
                                        var content = new ResponseDTO
                                        {
                                            Message = "Users",
                                            Users = users
                                        };
                                        string json = JsonSerializer.Serialize(content);
                                        //byte[] data = Encoding.UTF8.GetBytes(json);
                                        byte[] data = aesClass.EncryptMessage(json, klijentKljucevi[s].Key, klijentKljucevi[s].IV);
                                        s.SendTo(data, clientEP);
                                    }
                                    else if (receivedMessage.Contains("\"Action\":\"smartRule\""))
                                    {
                                        //deviceRepository.PrintAllDevices();
                                        //Console.WriteLine(deviceRepository.PrintAllDevices());
                                        var rule = JsonSerializer.Deserialize<SmartRuleDTO>(receivedMessage);
                                        smartRulesRepository.UpdateSmartRule(rule.SmartRule);

                                        ruleEngine.ApplyRuleEffects(rule.SmartRule,deviceRepository);
                                        //using (MemoryStream ms = new MemoryStream())
                                        //{

                                        //    formatter.Serialize(ms, uredjaji);
                                        //    byte[] data = ms.ToArray();
                                        //    s.SendTo(data, clientEP);
                                        //}
                                        //var commands = deviceRepository.GetAllCommands().ToList();
                                        var content = new ResponseDTO
                                        {
                                            Message = "Smart Rules",
                                            Value = $"Smart rule {rule.SmartRule.Name} is {(rule.SmartRule.IsEnabled == true ? "ON" : "OFF") }",
                                            SmartRules = smartRulesRepository.GetAllSmartRules().ToList()

                                        };
                                        string json = JsonSerializer.Serialize(content);
                                        //byte[] data = Encoding.UTF8.GetBytes(json);
                                        byte[] data = aesClass.EncryptMessage(json, klijentKljucevi[s].Key, klijentKljucevi[s].IV);
                                        s.SendTo(data, clientEP);

                                    }
                                    else if (receivedMessage.Contains("\"Action\":\"userRole\""))
                                    {
                                        //deviceRepository.PrintAllDevices();
                                        //Console.WriteLine(deviceRepository.PrintAllDevices());
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
                                                Value = "Role change unsuccessful"
                                            };
                                            json = JsonSerializer.Serialize(content);
                                        }
                                        else
                                        {
                                            userReository.UpdateUserRole(user.ID, newRole);
                                            var content = new ResponseDTO
                                            {
                                                Message = "AdminCommand",
                                                Value = "Role change successful"
                                            };
                                            json = JsonSerializer.Serialize(content);

                                        }

                                        byte[] data = aesClass.EncryptMessage(json, klijentKljucevi[s].Key, klijentKljucevi[s].IV);
                                        s.SendTo(data, clientEP);
                                    }

                                    else
                                    {

                                        //int udpPortUrejdjaja = 0;
                                        //string funkcija = "";
                                        //string vrednost = "";
                                        //string ime = "";
                                        //Uredjaj u1 = new Uredjaj();
                                        //using (MemoryStream ms = new MemoryStream(buffer, 0, receivedBytes))
                                        //{
                                        //    u1 = (Uredjaj)formatter.Deserialize(ms);
                                        //    funkcija = (string)formatter.Deserialize(ms);
                                        //    vrednost = (string)formatter.Deserialize(ms);
                                        //    udpPortUrejdjaja = u1.Port;
                                        //    ime = u1.Ime;


                                        //}
                                        // primljeno = new byte[receivedBytes];
                                        //Array.Copy(buffer, primljeno, receivedBytes);
                                        //string json= aesClass.DecryptMessage(primljeno, klijentKljucevi[s].Key, klijentKljucevi[s].IV);

                                        var komanda = JsonSerializer.Deserialize<CommandDTO>(receivedMessage);
                                        var users = userReository.GetAllUsers().ToList();
                                        var rules = smartRulesRepository.GetAllSmartRules().ToList();
                                        var u1 = komanda.SelectedDevice;
                                        var id = komanda.FunctionID;
                                        var funkcija = komanda.Function;
                                        var vrednost = komanda.Value;
                                        var username = komanda.Username;
                                        var existUser = users.Find(x => x.Username == username);
                                        //foreach (var s1 in uredjaji)  device client do this uodate
                                        //{
                                        //    if (s1.Name == u1.Name)
                                        //    {
                                        //        s1.AzurirajFunkciju(funkcija, vrednost);
                                        //        break;
                                        //    }
                                        //}
                                        string blockMessage;
                                        if (ruleEngine.BlockCommand(rules, komanda, existUser, out blockMessage))
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
                                        deviceRepository.UpdateDeviceFunction(u1.Id, id, funkcija, vrednost);
                                        //povezivanje uredjaja i servera
                                        IPEndPoint uredjajEP = new IPEndPoint(IPAddress.Loopback, u1.Port);
                                        // udpSocket.Bind(uredjajEP); ovo ja mislim ne treba!!!!!
                                        byte[] initialData = Encoding.UTF8.GetBytes(u1.Name + ":" + funkcija + ":" + vrednost);
                                        s.SendTo(initialData, uredjajEP);

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
                                        //deviceRepository.PrintAllDevices();
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
                                    //Console.WriteLine($"Poruka od klijenta: {poruka}");
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
                                        Console.WriteLine($"Number of active clients: {klijenti.Count}");

                                        // Provjeri da li je ostao još neki klijent
                                        if (klijenti.Count == 0)
                                        {
                                            IPEndPoint device1 = new IPEndPoint(IPAddress.Loopback, 60001);//za sad kasnije moze for 4 puta da samo posalje svim uredjajima ako ih je toliko
                                            IPEndPoint device2 = new IPEndPoint(IPAddress.Loopback, 60002);
                                            using (Socket tempUdp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                                            {
                                                byte[] initialData = Encoding.UTF8.GetBytes("Server has stopped working");
                                                tempUdp.SendTo(initialData, device1);
                                                tempUdp.SendTo(initialData, device2);
                                                Thread.Sleep(2000);
                                            }
                                            Console.WriteLine("No more connected clients.Server is shutting down");
                                            kraj = true;
                                        }

                                        continue;
                                    }
                                    else if (poruka == "GET_PUBLIC_KEY")
                                    {
                                        // Klijent traži RSA javni ključ
                                        byte[] keyLengthBytes = BitConverter.GetBytes(publicKeyBytes.Length);
                                        s.Send(keyLengthBytes);
                                        Thread.Sleep(50);
                                        s.Send(publicKeyBytes);
                                        Console.WriteLine("RSA public key successfully sent to client.");
                                    }
                                    else
                                    {
                                        // Enkriptovani login — dekriptuj RSA-om
                                        byte[] encryptedLogin = new byte[receivedBytes1];
                                        Array.Copy(buffer, encryptedLogin, receivedBytes1);

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

                                            byte[] key = new byte[16];
                                            byte[] iv = new byte[16];
                                            using (RandomNumberGenerator randomNumber = RandomNumberGenerator.Create())
                                            {
                                                randomNumber.GetBytes(key);
                                                randomNumber.GetBytes(iv);
                                            }
                                            // Клијент валидан, шаљемо одговор
                                            string odgovor = "SUCCESS";
                                            s.Send(Encoding.UTF8.GetBytes(odgovor));
                                            Thread.Sleep(200);
                                            // Креирамо UDP сокет за комуникацију
                                            byte[] keyData = new byte[32];
                                            Array.Copy(key, 0, keyData, 0, 16);
                                            Array.Copy(iv, 0, keyData, 16, 16);
                                            s.Send(keyData);
                                            Thread.Sleep(200);
                                            udpPort1 = random.Next(50002, 60000);

                                            logInUser.Port = udpPort1;
                                            userReository.UpdateStatus(logInUser.ID, ActiveStatus.ACTIVE, udpPort1);
                                            ////////////////////////////////////////////////////////////////
                                            s.Send(aesClass.EncryptMessage(udpPort1.ToString(), key, iv));
                                            //s.Send(Encoding.UTF8.GetBytes(udpPort1.ToString()));
                                            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                                            IPEndPoint udpServerEP = new IPEndPoint(IPAddress.Loopback, udpPort1);
                                            udpSocket.Bind(udpServerEP);
                                            udpSockets.Add(udpSocket);
                                            udpNeaktivnost[udpSocket] = 0;
                                            tcpUdpVeza[s] = udpSocket; //radi moguceg crash-a
                                            Console.WriteLine($"UDP socket created on port {udpPort1}");
                                            //for petlja i
                                            EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                                            int receivedBytes = udpSocket.ReceiveFrom(buffer, ref clientEP);
                                            byte[] primljeno = new byte[receivedBytes];
                                            Array.Copy(buffer, primljeno, receivedBytes);

                                            string receivedMessage = aesClass.DecryptMessage(primljeno, key, iv);
                                            //string receivedMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);

                                            udpSocket.Blocking = false;
                                            Console.WriteLine($"Message from UDP client : {receivedMessage}");

                                            userReository.PrintAllUsers();

                                            klijentKljucevi[udpSocket] = (key, iv); //dodati kljucevi u dict

                                            List<Device> uredjaji = deviceRepository.GetAllDevices().ToList();
                                            List<SmartRule> smartRules = smartRulesRepository.GetAllSmartRules().ToList();
                                            Console.WriteLine("All devices:");
                                            deviceRepository.PrintAllDevices("");


                                            //using (MemoryStream ms = new MemoryStream())
                                            //{

                                            //    formatter.Serialize(ms, uredjaji);
                                            //    byte[] data = ms.ToArray();
                                            //    udpSocket.SendTo(data, clientEP);
                                            //}
                                            var commands = deviceRepository.GetAllCommands().ToList();
                                            var content = new ResponseDTO
                                            {
                                                Message = "Devices List",
                                                Devices = uredjaji,
                                                Commands = commands,
                                                SmartRules = smartRules
                                            };
                                            string json = JsonSerializer.Serialize(content);
                                            //byte[] data = Encoding.UTF8.GetBytes(json);
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
                                    Console.WriteLine("Client connection lost.");//ako tcp konekcija pukne
                                                                                   // Cleanup UDP ako postoji veza
                                    if (tcpUdpVeza.ContainsKey(s))
                                    {
                                        Socket udpZaOvogKorisnika = tcpUdpVeza[s];
                                        if (udpSockets.Contains(udpZaOvogKorisnika))
                                        {
                                            userReository.DeactivateByPort(
                                                ((IPEndPoint)udpZaOvogKorisnika.LocalEndPoint).Port
                                            );
                                            udpZaOvogKorisnika.Close();
                                            udpSockets.Remove(udpZaOvogKorisnika);
                                            udpNeaktivnost.Remove(udpZaOvogKorisnika);
                                            klijentKljucevi.Remove(udpZaOvogKorisnika);
                                        }
                                        tcpUdpVeza.Remove(s);
                                    }
                                    s.Close();
                                    checkRead.Remove(s);
                                    klijenti.Remove(s); //  ukloniti odmah iz svih lista da ne bi ostala referenca na te sokete,nece u Select provjeravati nepostojece sokete
                                    continue;
                                }
                            }


                        }
                    }
                    foreach (var udpSocket in new List<Socket>(udpNeaktivnost.Keys))
                    {
                        udpNeaktivnost[udpSocket]++; // Povećaj broj neaktivnih ciklusa

                        if (udpNeaktivnost[udpSocket] >= MAX_NEAKTIVNIH_CIKLUSA)
                        {
                            Console.WriteLine($"UDP session on port {((IPEndPoint)udpSocket.LocalEndPoint).Port}  has been closed due the client inactivity.");
                            userReository.DeactivateByPort(((IPEndPoint)udpSocket.LocalEndPoint).Port);
                            userReository.PrintAllUsers();
                            // Pronaći TCP socket povezan sa ovim UDP socketom
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

                                // Zatvoriti TCP konekciju i ukloniti korisnika iz liste
                                tcpSocket.Close();
                                klijenti.Remove(tcpSocket);
                            }

                            // Zatvaramo UDP socket i uklanjamo ga iz liste
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

            foreach (Process p in poknutiProcesi)
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
