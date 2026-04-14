using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Common.Models
{
    [Serializable]
    public class Device
    {
        public string Name { get; set; }                // Ime uređaja
        public int Port { get; set; }                  // Port na kojem uređaj komunicira
        public Dictionary<string, string> Functions { get; set; } // Funkcije uređaja i njihove vrednosti
        public List<Command> CommandRegister { get; set; } // Evidencija komandi sa vremenskim oznakama
        public DateTime LastChanged { get; private set; }      // Vremenska oznaka poslednje promene

        public List<Device> devices { get; set; }
        // Konstruktor
        public Device(string name, int port)
        {
            Name = name;
            Port = port;
            Functions = new Dictionary<string, string>();
            CommandRegister = new List<Command>();
            LastChanged = DateTime.Now;
        }
        public Device(string name, int port, Dictionary<string, string> functions)
        {
            Name = name;
            Port = port;
            Functions = functions;
            CommandRegister = new List<Command>();
            LastChanged = DateTime.Now;
        }

        public Device()
        {
            devices = new List<Device> {
                        new Device("Svetlo",60001,new Dictionary<string, string>{{ "intenzitet", "70" }, { "stanje", "OFF" }, { "boja crvena", "110" }}),
                        new Device("TV",60002,new Dictionary<string, string>{{ "stanje", "OFF" },{ "temperatura", "15" }}),
                        new Device("Klima",60003,new Dictionary<string, string>{{ "stanje", "OFF" },{ "temperatura", "15" }}),
                        new Device("Door",60004,new Dictionary<string, string>{{ "stanje", "OFF" }})
            };
        }

        // Dodavanje ili ažuriranje funkcije uređaja
        public void AzurirajFunkciju(string function, string value)
        {
            if (Functions.ContainsKey(function))
            {
                Functions[function] = value;
            }
            else
            {
                Functions.Add(function, value);
            }

            // Ažuriraj vremensku oznaku
            LastChanged = DateTime.Now;

            // Evidentiraj promenu
            //EvidencijaKomandi.Add($"[{PoslednjaPromena}] {Ime}: {funkcija} promenjena na {vrednost}");
            CommandRegister.Add(new Command { ID = CommandRegister.Count + 1, CreationDate = LastChanged, Log = $"[{LastChanged}] {Name}: {function} promenjena na {value}" });
        }

        public List<Device> SviUredjaji()
        {
            return devices;
        }
        /* public void AzurirajListu(List<Uredjaj> noviUredjaji)
         {
             uredjaji = noviUredjaji;
         }*/

        public string IspisiSveUredjajeUTabeli(List<Device> list)
        {
            // Zaglavlje tabele
            string table = string.Format("{0,-15} | {1,-10} | {2,-50}\n", "Ime Uređaja", "Port", "Funkcije");
            table += new string('-', 80) + "\n";

            // Ispis uređaja
            foreach (var device in list)
            {
                // Pretvaranje funkcija u format ključ: vrednost
                string functions = string.Join(", ", device.Functions.Select(f => $"{f.Key}: {f.Value}"));

                // Dodavanje uređaja u tabelu
                table += string.Format("{0,-15} | {1,-10} | {2,-50}\n", device.Name, device.Port, functions);
            }

            return table;
        }
        public string IspisiSveFunkcijeUredjaja()
        {
            // Zaglavlje tabele
            string table = string.Format("{0,-15} | {1,-10} | {2,-50}\n", "Ime Uređaja", "Port", "Funkcije");
            table += new string('-', 80) + "\n";

            // Pretvaranje funkcija u format ključ: vrednost
            string funkcije = string.Join(", ", this.Functions.Select(f => $"{f.Key}: {f.Value}"));

            // Dodavanje uređaja u tabelu
            table += string.Format("{0,-15} | {1,-10} | {2,-50}\n", this.Name, this.Port, funkcije);


            return table;
        }
    }
}
