using Common.Enums;
using System;
using System.Collections.Generic;

namespace Common.Models
{
    public class Function
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
    [Serializable]
    public class Device
    {
        public int Id { get; set; }
        public string Name { get; set; }                // Ime uređaja
        public int Port { get; set; }
        public RoomType Location { get; set; }                  // Port na kojem uređaj komunicira
        public Dictionary<int, Function> Functions { get; set; } // Funkcije uređaja i njihove vrednosti
        public List<Command> CommandRegister { get; set; } // Evidencija komandi sa vremenskim oznakama
        public DateTime LastChanged { get; private set; }      // Vremenska oznaka poslednje promene

        public List<Device> devices { get; set; }
        // Konstruktor
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
            //devices = new List<Device> {
            //            new Device("Light",60001,new Dictionary<int, Function>{ { 1, new Function { Name = "value", Value = "40" } },{ 2, new Function { Name = "state", Value = "OFF" } }, { 3,new Function { Name = "red color", Value = "120"  } } }),
            //            new Device("TV",60002,new Dictionary<int, Function>{ { 1, new Function { Name = "state", Value = "OFF" } },{ 2, new Function { Name = "temperature", Value = "23" } }, { 3,new Function { Name = "red color", Value = "120"  } } }),
            //            new Device("Climate",60003,new Dictionary<int, Function>{ { 1, new Function { Name = "state", Value = "OFF" } },{ 2, new Function { Name = "temperature", Value = "12" } } }),
            //            new Device("Door",60004,new Dictionary<int, Function>{ { 1, new Function { Name = "state", Value = "OFF" } } }),
            //};
        }

        //// Dodavanje ili ažuriranje funkcije uređaja
        //public void AzurirajFunkciju(string function, string value)
        //{
        //    if (Functions.ContainsKey(function))
        //    {
        //        Functions[function] = value;
        //    }
        //    else
        //    {
        //        Functions.Add(function, value);
        //    }

        //    // Ažuriraj vremensku oznaku
        //    LastChanged = DateTime.Now;

        //    // Evidentiraj promenu
        //    //EvidencijaKomandi.Add($"[{PoslednjaPromena}] {Ime}: {funkcija} promenjena na {vrednost}");
        //    CommandRegister.Add(new Command { ID = CommandRegister.Count + 1, CreationDate = LastChanged, Log = $"[{LastChanged}] {Name}: {function} promenjena na {value}" });
        //}

        public List<Device> SviUredjaji()
        {
            return devices;
        }
        /* public void AzurirajListu(List<Uredjaj> noviUredjaji)
         {
             uredjaji = noviUredjaji;
         }*/

        //public string IspisiSveUredjajeUTabeli(List<Device> list)
        //{
        //    // Zaglavlje tabele
        //    string table = string.Format("{0,-15} | {1,-10} | {2,-50}\n", "Ime Uređaja", "Port", "Funkcije");
        //    table += new string('-', 80) + "\n";

        //    // Ispis uređaja
        //    foreach (var device in list)
        //    {
        //        // Pretvaranje funkcija u format ključ: vrednost
        //        string functions = string.Join(", ", device.Functions.Select(f => $"{f.Value.Name}: {f.Value.Value}"));

        //        // Dodavanje uređaja u tabelu
        //        table += string.Format("{0,-15} | {1,-10} | {2,-50}\n", device.Name, device.Port, functions);
        //    }

        //    return table;
        //}
        //public string IspisiSveFunkcijeUredjaja()
        //{
        //    // Zaglavlje tabele
        //    string table = string.Format("{0,-15} | {1,-10} | {2,-50}\n", "Ime Uređaja", "Port", "Funkcije");
        //    table += new string('-', 80) + "\n";

        //    // Pretvaranje funkcija u format ključ: vrednost
        //    string funkcije = string.Join(", ", this.Functions.Select(f => $"{f.Value.Name}: {f.Value.Value}"));

        //    // Dodavanje uređaja u tabelu
        //    table += string.Format("{0,-15} | {1,-10} | {2,-50}\n", this.Name, this.Port, funkcije);


        //    return table;
        //}
    }
}
