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
                Process clientProcess = new Process();
                clientProcess.StartInfo.FileName = clientPath;
                clientProcess.StartInfo.Arguments = $"{i + 60001}"; // argument udp port
                clientProcess.Start(); // runing device
                proccess.Add(clientProcess);

                Console.WriteLine($"Start device at #{clientProcess.Id}");
            }
        }

    }
}
