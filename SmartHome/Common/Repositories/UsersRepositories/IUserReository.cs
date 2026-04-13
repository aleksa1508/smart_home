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
        void PretragaPorta(int port);
        bool PretragaNeaktivnosti();
        void UpdateData(int id, string firstName, string lastName, string username, string password);
        User GetKorisnik(string username, string password);
        IEnumerable<User> GetAllUsers();
        void UpdateStatus(int id, ActiveStatus status,int port);
        void IspisKorisnika();

    }
}
