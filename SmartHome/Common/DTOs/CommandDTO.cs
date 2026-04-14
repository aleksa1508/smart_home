using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTOs
{
    public class CommandDTO
    {
        public Device SelectedDevice { get; set; }
        public string Function { get; set; } = string.Empty;
        public string Value { get; set; }= string.Empty;
    }
}
