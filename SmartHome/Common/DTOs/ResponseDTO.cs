using Common.Models;
using System;
using System.Collections.Generic;

namespace Common.DTOs
{
    public class ResponseDTO
    {
        public string Message { get; set; } = string.Empty;
        public Device Device { get; set; }
        public string Function { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public List<Device> Devices { get; set; }
        public List<Command> Commands { get; set; }
        public List<User> Users { get; set; }
    }
}
