using Common.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace SmartHomeDevices

{
    public class DeviceClient
    {
        static void Main(string[] args)
        {
            int number = 0;
            foreach (var arg in args)
            {
                Console.WriteLine($"\nRecived argument: {arg}");
                number = Int32.Parse(arg);
            }
            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint destinationEP = new IPEndPoint(IPAddress.Any, number);
            udpSocket.Bind(destinationEP);
            EndPoint senderEP = new IPEndPoint(IPAddress.Any, 0);
            Device d = new Device();
            List<Device> devices = d.SviUredjaji();
            bool end = false;
            while (!end)
            {
                byte[] buffer = new byte[1024];
                try
                {

                    int numberOfBytes = udpSocket.ReceiveFrom(buffer, ref senderEP);

                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, numberOfBytes);
                    Console.WriteLine("\nRecieved message " + receivedMessage);
                    if (receivedMessage == "Server je zavrsio sa radom")
                    {
                        end = true;
                        break;
                    }

                    Console.WriteLine($"Response received from the server, length {numberOfBytes}->:{receivedMessage}"); // 4

                    string[] parts = receivedMessage.Split(':');
                    Console.WriteLine(parts.Length + " " + parts[0] + " " + parts[1] + " " + parts[2]);
                    foreach (var device in devices)
                    {
                        if (device.Name == parts[0])
                        {
                            device.AzurirajFunkciju(parts[1], parts[2]);
                            Console.WriteLine(device.IspisiSveFunkcijeUredjaja());
                            break;
                        }
                    }


                    foreach (var device in devices)
                    {

                        foreach (var dev in device.CommandRegister)
                        {
                            Console.WriteLine(dev.ToString());
                        }
                    }



                }

                catch (Exception ex)
                {
                    Console.WriteLine($"Problem while sending a message: \n{ex}");
                }
            }
            Console.WriteLine("Device has shut down");
            udpSocket.Close(); // Zatvaramo soket na kraju rada
            Console.ReadKey();
        }
    }
}