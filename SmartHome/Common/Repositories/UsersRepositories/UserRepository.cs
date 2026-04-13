using Common.Enums;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Repositories.UsersRepositories
{
    public class UserRepository : IUserReository
    {
        private string connectionString = "Server=localhost\\SQLEXPRESS;Database=users_db;Trusted_Connection=True;";
        public IEnumerable<User> GetAllUsers()
        {
            List<User> lista = new List<User>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * from Users";
                SqlCommand command = new SqlCommand(query, connection);

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {

                    lista.Add(new User { ID = Int32.Parse(reader["id"].ToString()), FirstName = reader["firstName"].ToString(), LastName = reader["lastName"].ToString(), Username = reader["username"].ToString(), Password = reader["password"].ToString(), Role = (UserRole)Enum.Parse(typeof(UserRole), reader["role"].ToString()), Status = (ActiveStatus)Enum.Parse(typeof(ActiveStatus), reader["status"].ToString()), Port = Int32.Parse(reader["port"].ToString()) });

                }

                return lista;
            }
        }

        public User GetKorisnik(string username, string password)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * from Users where username = @username and password = @password";
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    if (username == reader["username"].ToString() && password == reader["password"].ToString())
                    {
                        return new User { ID = Int32.Parse(reader["id"].ToString()), FirstName = reader["firstName"].ToString(), LastName = reader["lastName"].ToString(), Username = reader["username"].ToString(), Password = reader["password"].ToString(), Role = (UserRole)Enum.Parse(typeof(UserRole), reader["role"].ToString()) };

                    }
                }

            }
            return null;
        }

        public void PretragaPorta(int port)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Users SET status=@status, port=0 WHERE port=@port";
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                sqlCommand.Parameters.AddWithValue("@status", ActiveStatus.INACTIVE.ToString());
                sqlCommand.Parameters.AddWithValue("@port", port);

                sqlCommand.ExecuteNonQuery();
            }

        }

        public bool PretragaNeaktivnosti()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Users WHERE status=@status";
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                sqlCommand.Parameters.AddWithValue("@status", ActiveStatus.INACTIVE.ToString());

                int count = (int)sqlCommand.ExecuteScalar();

                return count > 0;
            }
        }

        public void UpdateData(int id, string firstName, string lastName, string username, string password)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE users SET firstName=@firstName,lastName=@lastName , username=@username , password=@password WHERE id=@id";
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                sqlCommand.Parameters.AddWithValue("@firstName", firstName);
                sqlCommand.Parameters.AddWithValue("@lastName", lastName);
                sqlCommand.Parameters.AddWithValue("@username", username);
                sqlCommand.Parameters.AddWithValue("@password", password);
                sqlCommand.Parameters.AddWithValue("@id", id);

                sqlCommand.ExecuteNonQuery();
            }
        }
        public void UpdateStatus(int id,ActiveStatus status,int port)
        {
            List<User> list = GetAllUsers().ToList();
            foreach (var user in list)
            {
                if (user.ID.Equals(id))
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string query = "UPDATE users SET status=@status,port=@port WHERE id=@id";
                        SqlCommand sqlCommand = new SqlCommand(query, connection);
                        sqlCommand.Parameters.AddWithValue("@status", status.ToString());
                        sqlCommand.Parameters.AddWithValue("@port", port.ToString());
                        sqlCommand.Parameters.AddWithValue("@id", id);

                        sqlCommand.ExecuteNonQuery();
                    }
                }
            }
        }
        public void IspisKorisnika()
        {
            List<User> list = GetAllUsers().ToList();
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine("| FirstName       | LastName     | Username    | Status   | Port   | Role      |");
            Console.WriteLine("--------------------------------------------------------------------------------");

            foreach (var user in list)
            {
                Console.WriteLine($"| {user.FirstName.PadRight(10)} | {user.LastName.PadRight(10)} | {user.Username.PadRight(15)} | {(user.Status == ActiveStatus.ACTIVE ? "YES " : "NO ").PadRight(9)} | {user.Port.ToString().PadRight(5)} | {user.Role.ToString().PadRight(12)} |");
            }
            Console.WriteLine("--------------------------------------------------------------------------------");
        }
    }
}
