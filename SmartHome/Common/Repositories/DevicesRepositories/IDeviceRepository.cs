using Common.DTOs;
using Common.Enums;
using Common.Models;
using System;
using System.Collections.Generic;

namespace Common.Repositories.DevicesRepositories
{
    public interface IDeviceRepository
    {
        Device GetDeviceById(int id);
        Dictionary<int, Function> GetDeviceFunctions(int id);
        List<Command> GetDeviceCommands(int id);
        void AddDevice(string name, int port, RoomType location, DateTime lastChange);
        void AddDeviceFunctions(string function, string value, int deviceId);
        void AddDeviceCommands(string message, DateTime creation_date, int deviceId, string username);
        void UpdateDeviceFunction(int deviceId, int id, string name, string value);
        void UpdateDevice(int deviceId);
        IEnumerable<Device> GetAllDevices();
        IEnumerable<Command> GetAllCommands();
        string PrintDeviceCommands(Device device);
        string PrintDeviceFunctions(Device device);
        void PrintAllDevices(string deviceName);
    }
}
