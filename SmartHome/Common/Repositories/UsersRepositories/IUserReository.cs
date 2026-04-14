using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Repositories.UsersRepositories
{
    public interface IUserReository
    {
        void AddUser(string firstName, string lastName, string username, string password, string role);
        void DeactivateByPort(int port);
        bool DetectInactiveUsers();
        void UpdateData(int id, string firstName, string lastName, string username);
        User GetKorisnik(string username, string password);
        User GetUserById(int id);
        IEnumerable<User> GetAllUsers();
        void UpdateStatus(int id, ActiveStatus status,int port);
        void UpdatePassword(int id, string password);
        void PrintAllUsers();

    }
}
