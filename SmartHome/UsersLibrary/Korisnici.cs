using System;
using System.Collections.Generic;

namespace UsersLibrary
{
    [Serializable]
    public class Korisnici
    {
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string KorisnickoIme { get; set; }
        public string Lozinka { get; set; }
        public StatusKorisnika StatusPrijave { get; set; }
        public int DodeljeniPort { get; set; }

        public Korisnici(string ime, string prezime, string korisnickoIme, string lozinka, StatusKorisnika statusPrijave, int dodeljeniPort)
        {
            Ime = ime;
            Prezime = prezime;
            KorisnickoIme = korisnickoIme;
            Lozinka = lozinka;
            StatusPrijave = statusPrijave;
            DodeljeniPort = dodeljeniPort;
        }

        public Korisnici()
        {
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
            Console.WriteLine("| Ime       | Prezime     | Korisničko ime | Prijavljen | Port  | Lozinka      |");
            Console.WriteLine("--------------------------------------------------------------------------------");

            foreach (var korisnik in lista)
            {
                Console.WriteLine($"| {korisnik.Ime.PadRight(10)} | {korisnik.Prezime.PadRight(10)} | {korisnik.KorisnickoIme.PadRight(15)} | {(korisnik.StatusPrijave==StatusKorisnika.AKTIVAN ? "DA " : "NE ").PadRight(9)} | {korisnik.DodeljeniPort.ToString().PadRight(5)} | {korisnik.Lozinka.PadRight(12)} |");
            }
            Console.WriteLine("--------------------------------------------------------------------------------");
        }

        public override string ToString()
        {
            return $"Korisnik: {Ime} {Prezime}, Korisničko ime: {KorisnickoIme}, Prijavljen: {StatusPrijave}, Port: {DodeljeniPort}";
        }
    }
}
