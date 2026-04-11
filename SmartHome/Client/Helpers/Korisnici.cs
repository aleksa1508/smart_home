using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Client.Helpers

{
    public enum StatusKorisnika
    {
        AKTIVAN, NEAKTIVAN
    }
    public enum UserRole
    {
        OWNER, USER
    }
    [Serializable]
    public class Korisnici : INotifyPropertyChanged     //ako budem nekad u ui menjao objekat onda dodati notify property changed
    {
        private string ime;
        private string prezime;
        private string korisnickoIme;
        private string lozinka;
        private int dodeljeniPort;
        public string Ime
        {
            get { return ime; }
            set
            {
                ime = value;
                OnPropertyChanged(nameof(Ime));
            }
        }
        public string Prezime { get; set; }
        public string FullName => $"{Ime} {Prezime}";//samo za profile popup
        public string KorisnickoIme { get; set; }
        public string Lozinka { get; set; }
        public StatusKorisnika StatusPrijave { get; set; }
        public int DodeljeniPort { get; set; }
        public UserRole Role { get; set; }
        public static List<Korisnici> listaKorisnika = new List<Korisnici>
            {
                new Korisnici("Aleksa","Arsenic","user1","a",StatusKorisnika.NEAKTIVAN,0,UserRole.OWNER),
                new Korisnici("Uros","Milosevic","user2","b",StatusKorisnika.NEAKTIVAN,0, UserRole.USER)
            };

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Korisnici(string ime, string prezime, string korisnickoIme, string lozinka, StatusKorisnika statusPrijave, int dodeljeniPort, UserRole role)
        {
            Ime = ime;
            Prezime = prezime;
            KorisnickoIme = korisnickoIme;
            Lozinka = lozinka;
            StatusPrijave = statusPrijave;
            DodeljeniPort = dodeljeniPort;
            Role = role;
        }

        public Korisnici()
        {
        }
        public void UpdateData(string ime, string prezime, string korisnickoIme, string lozinka)
        {
            foreach (var korisnik in listaKorisnika)
            {
                if (korisnik.KorisnickoIme.Equals(korisnickoIme))
                {
                    korisnik.Ime = ime;
                    korisnik.Prezime = prezime;
                    korisnik.KorisnickoIme = korisnickoIme;
                    korisnik.Lozinka = lozinka;
                }
            }
        }
        public Korisnici PretraziKorisnika(List<Korisnici> korisnici, string korisnickoIme, string lozinka)
        {
            foreach (var korisnik in korisnici)
            {
                if (korisnik.KorisnickoIme == korisnickoIme && korisnik.Lozinka == lozinka)
                {
                    korisnik.StatusPrijave = StatusKorisnika.AKTIVAN;
                    return korisnik;
                }
            }
            return null; // Ako korisnik nije pronađen
        }
        public static Korisnici PretragaKorisnika(string korisnickoIme, string lozinka)
        {
            foreach (var korisnik in listaKorisnika)
            {
                if (korisnik.KorisnickoIme == korisnickoIme && korisnik.Lozinka == lozinka)
                {
                    korisnik.StatusPrijave = StatusKorisnika.AKTIVAN;
                    return korisnik;
                }
            }
            return null; // Ako korisnik nije pronađen
        }

        public void PretragaPorta(List<Korisnici> korisnici, int port)
        {
            foreach (var korisnik in korisnici)
            {
                if (korisnik.DodeljeniPort == port)
                {
                    korisnik.StatusPrijave = StatusKorisnika.NEAKTIVAN;
                }
            }
        }

        public bool PretragaNeaktivnosti(List<Korisnici> korisnici)
        {
            foreach (var korisnik in korisnici)
            {
                if (korisnik.StatusPrijave == StatusKorisnika.AKTIVAN)
                {
                    return false;
                }
            }
            return true;
        }
        public void IspisKorisnika(List<Korisnici> lista)
        {
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine("| Ime       | Prezime     | Korisničko ime | Prijavljen | Port  | Role      |");
            Console.WriteLine("--------------------------------------------------------------------------------");

            foreach (var korisnik in lista)
            {
                Console.WriteLine($"| {korisnik.Ime.PadRight(10)} | {korisnik.Prezime.PadRight(10)} | {korisnik.KorisnickoIme.PadRight(15)} | {(korisnik.StatusPrijave == StatusKorisnika.AKTIVAN ? "DA " : "NE ").PadRight(9)} | {korisnik.DodeljeniPort.ToString().PadRight(5)} | {korisnik.Role.ToString().PadRight(12)} |");
            }
            Console.WriteLine("--------------------------------------------------------------------------------");
        }

        public override string ToString()
        {
            return $"Korisnik: {Ime} {Prezime}, Korisničko ime: {KorisnickoIme}, Prijavljen: {StatusPrijave}, Role: {DodeljeniPort}";
        }
    }
}
