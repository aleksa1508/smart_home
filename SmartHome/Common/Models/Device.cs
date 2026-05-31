using Common.DTOs;
using Common.Enums;
using System;
using System.Collections.Generic;

namespace Common.Models
{
    [Serializable]
    public class Device
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Port { get; set; }
        public RoomType Location { get; set; }
        public Dictionary<int, Function> Functions { get; set; }
        public List<Command> CommandRegister { get; set; }
        public DateTime LastChanged { get; private set; }

        public List<Device> devices { get; set; }
        public Device(int id, string name, int port, RoomType location)
        {
            Id = id;
            Name = name;
            Port = port;
            Location = location;
            Functions = new Dictionary<int, Function>();
            CommandRegister = new List<Command>();
            LastChanged = DateTime.Now;
        }
        public Device(string name, int port, Dictionary<int, Function> functions)
        {
            Name = name;
            Port = port;
            Functions = functions;
            CommandRegister = new List<Command>();
            LastChanged = DateTime.Now;
        }
        public Device(int id, string name, int port, RoomType location, Dictionary<int, Function> functions, List<Command> commands, DateTime lastChange)
        {
            Id = id;
            Name = name;
            Port = port;
            Location = location;
            Functions = functions;
            CommandRegister = commands;
            LastChanged = lastChange;
        }
        public Device(string name, int port, DateTime lastChange)
        {
            Name = name;
            Port = port;
            Functions = new Dictionary<int, Function>();
            CommandRegister = new List<Command>();
            LastChanged = lastChange;
        }

        public Device()
        {
        }

    }
}
