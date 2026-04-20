using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Text.Json;
using System.Data.SqlClient;
using Common;
using Common.Repositories.UsersRepositories;
using Common.Enums;
using Common.DTOs;
using Common.Models;
using Common.Repositories.DevicesRepositories;
using System.Security.Cryptography; // <-- dodaj
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
            //            new Device(1,"Light",60001,RoomType.KITCHEN,new Dictionary<int, Function>{ { 1, new Function { Name = "value", Value = "40" } },{ 2, new Function { Name = "state", Value = "OFF" } }, { 3,new Function { Name = "red color", Value = "120"  } } },new List<Command>(),DateTime.Now),
            //            new Device(2,"TV",60002,RoomType.BEDROOM,new Dictionary<int, Function>{ { 1, new Function { Name = "state", Value = "OFF" } },{ 2, new Function { Name = "temperature", Value = "23" } } },new List<Command>(),DateTime.Now),
            //            new Device(3, "Climate", 60003,RoomType.BATHROOM, new Dictionary < int, Function > { { 1, new Function { Name = "state", Value = "OFF" } }, { 2, new Function { Name = "temperature", Value = "12" } } }, new List < Command >(), DateTime.Now),
            //            new Device(4, "Door",60004,RoomType.GARAGE, new Dictionary < int, Function > { { 1, new Function { Name = "state", Value = "OFF" } } }, new List < Command >(), DateTime.Now),
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
            //            deviceRepository.AddDeviceCommands(f.Log, DateTime.Now, d.Id);
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

            AesClass aesClass=new AesClass();
            Dictionary<Socket, (byte[] Key, byte[] IV)> klijentKljucevi =new Dictionary<Socket, (byte[], byte[])>();
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

            Console.WriteLine($"Server je stavljen u stanje osluskivanja i ocekuje komunikaciju na {serverEP}");
            Dictionary<Socket, Socket> tcpUdpVeza = new Dictionary<Socket, Socket>();       //ako klijent crash-uje tj posalje shutdown prije ne (crashuje u dashboard i onda udp soket ostaje ziv a tcp konekcija pada)

            List<Socket> klijenti = new List<Socket>(); // Pravimo posebnu listu za klijentske sokete kako nam je ne bi obrisala Select funkcija
            List<Socket> udpSockets = new List<Socket>();
            int udpPort1 = 0;
            Dictionary<Socket, int> udpNeaktivnost = new Dictionary<Socket, int>(); // UDP soketi i broj ciklusa neaktivnosti
            const int MAX_NEAKTIVNIH_CIKLUSA = 20; // Maksimalan broj ciklusa neaktivnosti pre zatvaranja


            RunDevices(2);
            bool kraj = false;

            byte[] buffer = new byte[4096];
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
                                Console.WriteLine($"Klijent se povezao sa {client.RemoteEndPoint}");

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
                                        Console.WriteLine(deviceRepository.PrintAllDevices());
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
                                            Commands = commands
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

                                        var u1 = komanda.SelectedDevice;
                                        var id = komanda.FunctionID;
                                        var funkcija = komanda.Function;
                                        var vrednost = komanda.Value;

                                        //foreach (var s1 in uredjaji)  device client do this uodate
                                        //{
                                        //    if (s1.Name == u1.Name)
                                        //    {
                                        //        s1.AzurirajFunkciju(funkcija, vrednost);
                                        //        break;
                                        //    }
                                        //}

                                        deviceRepository.UpdateDeviceFunction(u1.Id, id, funkcija, vrednost);
                                        //povezivanje uredjaja i servera
                                        IPEndPoint uredjajEP = new IPEndPoint(IPAddress.Loopback, u1.Port);
                                        // udpSocket.Bind(uredjajEP); ovo ja mislim ne treba!!!!!
                                        byte[] initialData = Encoding.UTF8.GetBytes(u1.Name + ":" + funkcija + ":" + vrednost);
                                        s.SendTo(initialData, uredjajEP);

                                        DateTime timestamp = DateTime.Now;
                                        string log = $"[{timestamp}] {u1.Name}: {funkcija} promenjena na {vrednost}";
                                        deviceRepository.AddDeviceCommands(log, DateTime.Now, u1.Id);

                                        var content = new ResponseDTO
                                        {
                                            Message = "Command",
                                            Device = u1,
                                            Function = funkcija,
                                            Value = vrednost,
                                            Timestamp = timestamp
                                        };
                                        string json = JsonSerializer.Serialize(content);
                                        byte[] data = aesClass.EncryptMessage(json, klijentKljucevi[s].Key, klijentKljucevi[s].IV);
                                        s.SendTo(data, clientEP);
                                        //deviceRepository.PrintAllDevices();
                                        Console.WriteLine(deviceRepository.PrintAllDevices());

                                    }

                                }



                            }
                            else
                            {
                                int receivedBytes1 = s.Receive(buffer);
                                if (receivedBytes1 > 0)
                                {
                                    string poruka = Encoding.UTF8.GetString(buffer, 0, receivedBytes1);
                                    Console.WriteLine($"Poruka od klijenta: {poruka}");
                                    if (poruka == "shutdown")
                                    {

                                        try { 
                                            s.Send(Encoding.UTF8.GetBytes("DISCONNECT_OK"));
                                            Thread.Sleep(1000); 
                                        } catch { }
                                        s.Close();
                                        klijenti.Remove(s);
                                        Console.WriteLine($"Preostalo klijenata: {klijenti.Count}");

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
                                            Console.WriteLine("Nema više povezanih klijenata. Server se gasi.");
                                            kraj = true;
                                        }

                                        continue;
                                    }

                                    string[] djelovi = poruka.Split(':');
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
                                        Array.Copy(iv, 0, keyData,16, 16);
                                        s.Send(keyData);
                                        Thread.Sleep(200);
                                        udpPort1 = random.Next(50002, 60000);

                                        logInUser.Port = udpPort1;
                                        userReository.UpdateStatus(logInUser.ID, ActiveStatus.ACTIVE,udpPort1);
                                        ////////////////////////////////////////////////////////////////
                                        s.Send(aesClass.EncryptMessage(udpPort1.ToString(),key,iv));
                                        //s.Send(Encoding.UTF8.GetBytes(udpPort1.ToString()));
                                        Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                                        IPEndPoint udpServerEP = new IPEndPoint(IPAddress.Loopback, udpPort1);
                                        udpSocket.Bind(udpServerEP);
                                        udpSockets.Add(udpSocket);
                                        udpNeaktivnost[udpSocket] = 0;
                                        tcpUdpVeza[s] = udpSocket; //radi moguceg crash-a
                                        Console.WriteLine($"UDP soket kreiran na portu {udpPort1}");
                                        //for petlja i
                                        EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                                        int receivedBytes = udpSocket.ReceiveFrom(buffer, ref clientEP);
                                        byte[] primljeno = new byte[receivedBytes];
                                        Array.Copy(buffer, primljeno, receivedBytes);

                                        string receivedMessage = aesClass.DecryptMessage(primljeno, key, iv);////////////////////
                                        //string receivedMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);

                                        udpSocket.Blocking = false;
                                        Console.WriteLine($"Poruka od UDP klijenta : {receivedMessage}");

                                        userReository.PrintAllUsers();

                                        klijentKljucevi[udpSocket] = (key, iv); //dodati kljucevi u dict

                                        List<Device> uredjaji = deviceRepository.GetAllDevices().ToList() ;

                                        Console.WriteLine(deviceRepository.PrintAllDevices());


                                        //using (MemoryStream ms = new MemoryStream())
                                        //{

                                        //    formatter.Serialize(ms, uredjaji);
                                        //    byte[] data = ms.ToArray();
                                        //    udpSocket.SendTo(data, clientEP);
                                        //}
                                        var commands=deviceRepository.GetAllCommands().ToList();
                                        var content = new ResponseDTO
                                        {
                                            Message = "Devices List", 
                                            Devices = uredjaji,
                                            Commands=commands
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
                                else
                                {
                                    Console.WriteLine("Klijent je prekinuo vezu.");//ako tcp konekcija pukne
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
                            Console.WriteLine($"UDP sesija na portu {((IPEndPoint)udpSocket.LocalEndPoint).Port} je zatvorena zbog neaktivnosti.");
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
                                    Console.WriteLine("Greška prilikom slanja obaveštenja korisniku.");
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
                Console.WriteLine($"Doslo je do greske {ex}");
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
                        Console.WriteLine($"Ugašen device proces {p.Id}");
                    }
                }
                catch { }
            }
            serverSocket.Close();
            Console.WriteLine("Server zavrsava sa radom");
            Console.ReadKey();
        }

    }
}
