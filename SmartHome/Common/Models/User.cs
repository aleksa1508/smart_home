using Common.Enums;
using System;
using System.ComponentModel;

namespace Common
{
    [Serializable]
    public class User : INotifyPropertyChanged     //ako budem nekad u ui menjao objekat onda dodati notify property changed
    {
        private string firstName;
        public int ID { get; set; }
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
        public string FullName => $"{FirstName} {LastName}";  //for profile popup
        public string Username { get; set; }
        public string Password { get; set; }
        public ActiveStatus Status { get; set; }
        public int Port { get; set; }
        public UserRole Role { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public User(int id, string firstName, string lastName, string username, string password, ActiveStatus status, int port, UserRole role)
        {
            ID = id;
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
