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
        public Uredjaj IzabraniUredjaj { get; set; }
        public string Funkcija { get; set; }
        public string Vrednost { get; set; }
    }
}
