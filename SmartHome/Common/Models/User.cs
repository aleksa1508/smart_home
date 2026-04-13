using Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [Serializable]
    public class User : INotifyPropertyChanged     //ako budem nekad u ui menjao objekat onda dodati notify property changed
    {
        private string firstName;
        public int ID {  get; set; }
        public string FirstName
        {
            get { return firstName; }
            set
            {
                firstName = value;
                OnPropertyChanged(nameof(FirstName));
            }
        }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";//samo za profile popup
        public string Username { get; set; }
        public string Password { get; set; }
        public ActiveStatus Status { get; set; }
        public int Port { get; set; }
        public UserRole Role { get; set; }
        //public static List<Korisnici> listaKorisnika = new List<Korisnici>
        //    {
        //        new Korisnici("Aleksa","Arsenic","user1","a",StatusKorisnika.NEAKTIVAN,0,UserRole.OWNER),
        //        new Korisnici("Uros","Milosevic","user2","b",StatusKorisnika.NEAKTIVAN,0, UserRole.USER)
        //    };

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public User(int id,string firstName, string lastName, string username, string password, ActiveStatus status, int port, UserRole role)
        {
            ID=id;
            FirstName = firstName;
            LastName = lastName;
            Username = username;
            Password = password;
            Status = status;
            Port = port;
            Role = role;
        }

        public User()
        {
        }

        
    }
}
