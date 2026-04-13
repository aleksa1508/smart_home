using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class Command
    {
        public int ID { get; set; }
        public string Log { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }
        public Command() { }

        public override string ToString()
        {
            return $"{Log}";
        }
    }
}
