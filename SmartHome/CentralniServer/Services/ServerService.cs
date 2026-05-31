using CentralniServer.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CentralniServer.Service
{
    public class ServerService : IServerService
    {
        public void RunDevices(List<Process> proccess, int deviceNumber)
        {
            for (int i = 0; i < deviceNumber; i++)
            {
                string clientPath = @"C:\Users\Dell 3520\Desktop\AA\DIPLOMSKI\SmartHome\SmartHome\SmartHomeDevices\bin\Debug\DeviceClient.exe";
                Process klijentProces = new Process();
                klijentProces.StartInfo.FileName = clientPath;
                klijentProces.StartInfo.Arguments = $"{i + 60001}"; // argument udp port
                klijentProces.Start(); // runing device
                proccess.Add(klijentProces);

                Console.WriteLine($"Start device at #{i + 1}");
            }
        }


    }
}
