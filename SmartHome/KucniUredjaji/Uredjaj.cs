using System;
using System.Collections.Generic;
using System.Linq;

namespace KucniUredjaji
{
    public class ResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public Uredjaj Uredjaj { get; set; }
        public string Function { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public List<Uredjaj> Uredjaji { get; set; }
    }
    public class Komanda
    {
        public int ID { get; set; }
        public string Log { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; }
        public Komanda() { }

        public override string ToString()
        {
            return $"{Log}";
        }
    }
    [Serializable]
    public class Uredjaj
    {
        public string Ime { get; set; }                // Ime uređaja
        public int Port { get; set; }                  // Port na kojem uređaj komunicira
        public Dictionary<string, string> Funkcije { get; set; } // Funkcije uređaja i njihove vrednosti
        public List<Komanda> EvidencijaKomandi { get; set; } // Evidencija komandi sa vremenskim oznakama
        public DateTime PoslednjaPromena { get; private set; }      // Vremenska oznaka poslednje promene

        public List<Uredjaj> uredjaji { get; set; }
        // Konstruktor
        public Uredjaj(string ime, int port)
        {
            Ime = ime;
            Port = port;
            Funkcije = new Dictionary<string, string>();
            EvidencijaKomandi = new List<Komanda>();
            PoslednjaPromena = DateTime.Now;
        }
        public Uredjaj(string ime, int port, Dictionary<string, string> funkcije)
        {
            Ime = ime;
            Port = port;
            Funkcije = funkcije;
            EvidencijaKomandi = new List<Komanda>();
            PoslednjaPromena = DateTime.Now;
        }

        public Uredjaj()
        {
            uredjaji = new List<Uredjaj> {
                        new Uredjaj("Svetlo",60001,new Dictionary<string, string>{{ "intenzitet", "70" }, { "stanje", "OFF" }, { "boja crvena", "110" }}),
                        new Uredjaj("TV",60002,new Dictionary<string, string>{{ "stanje", "OFF" },{ "temperatura", "15" }}),
                        new Uredjaj("Klima",60003,new Dictionary<string, string>{{ "stanje", "OFF" },{ "temperatura", "15" }}),
                        new Uredjaj("Door",60004,new Dictionary<string, string>{{ "stanje", "OFF" }})
            };
        }

        // Dodavanje ili ažuriranje funkcije uređaja
        public void AzurirajFunkciju(string funkcija, string vrednost)
        {
            if (Funkcije.ContainsKey(funkcija))
            {
                Funkcije[funkcija] = vrednost;
            }
            else
            {
                Funkcije.Add(funkcija, vrednost);
            }

            // Ažuriraj vremensku oznaku
            PoslednjaPromena = DateTime.Now;

            // Evidentiraj promenu
            //EvidencijaKomandi.Add($"[{PoslednjaPromena}] {Ime}: {funkcija} promenjena na {vrednost}");
            EvidencijaKomandi.Add(new Komanda { ID = EvidencijaKomandi.Count + 1, CreationDate = PoslednjaPromena, Log = $"[{PoslednjaPromena}] {Ime}: {funkcija} promenjena na {vrednost}" });
        }
       
        public List<Uredjaj> SviUredjaji()
        {
            return uredjaji;
        }
       /* public void AzurirajListu(List<Uredjaj> noviUredjaji)
        {
            uredjaji = noviUredjaji;
        }*/

        public string IspisiSveUredjajeUTabeli(List<Uredjaj> lista)
        {
            // Zaglavlje tabele
            string tabela = string.Format("{0,-15} | {1,-10} | {2,-50}\n", "Ime Uređaja", "Port", "Funkcije");
            tabela += new string('-', 80) + "\n";

            // Ispis uređaja
            foreach (var uredjaj in lista)
            {
                // Pretvaranje funkcija u format ključ: vrednost
                string funkcije = string.Join(", ", uredjaj.Funkcije.Select(f => $"{f.Key}: {f.Value}"));

                // Dodavanje uređaja u tabelu
                tabela += string.Format("{0,-15} | {1,-10} | {2,-50}\n", uredjaj.Ime, uredjaj.Port, funkcije);
            }

            return tabela;
        }
        public string IspisiSveFunkcijeUredjaja()
        {
            // Zaglavlje tabele
            string tabela = string.Format("{0,-15} | {1,-10} | {2,-50}\n", "Ime Uređaja", "Port", "Funkcije");
            tabela += new string('-', 80) + "\n";

            // Pretvaranje funkcija u format ključ: vrednost
            string funkcije = string.Join(", ", this.Funkcije.Select(f => $"{f.Key}: {f.Value}"));

            // Dodavanje uređaja u tabelu
            tabela += string.Format("{0,-15} | {1,-10} | {2,-50}\n", this.Ime, this.Port, funkcije);


            return tabela;
        }
       

    }
}
