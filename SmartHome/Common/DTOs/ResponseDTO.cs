using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    public class ResponseDTO
    {
        public string Message { get; set; } = string.Empty;
        public Device Device { get; set; }
        public string Function { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public List<Device> Devices { get; set; }
    }
}
