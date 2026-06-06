using Common.Enums;
using System.Collections.Generic;

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
        void DeleteUser(int id);
        void UpdateStatus(int id, ActiveStatus status, int port);
        void UpdatePassword(int id, string password);
        void UpdateUserRole(int id, UserRole role);
        void PrintAllUsers();

    }
}
