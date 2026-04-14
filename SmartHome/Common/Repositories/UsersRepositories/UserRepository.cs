using BCrypt.Net;
using Common.Enums;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Common.Repositories.UsersRepositories
{
    public class UserRepository : IUserReository
    {
        private readonly string connectionString = "Server=localhost\\SQLEXPRESS;Database=users_db;Trusted_Connection=True;";
        public void AddUser(string firstName, string lastName, string username, string password, string role)
        {
            using (SqlConnection connection = new SqlConnection("Server=localhost\\SQLEXPRESS;Database=users_db;Trusted_Connection=True;"))
            {
                connection.Open();

                string query = @"
        INSERT INTO users (firstName, lastName, username, password, role, port, status)
        VALUES (@firstName, @lastName, @username, @password, @role, @port, @status)";

                SqlCommand cmd = new SqlCommand(query, connection);

                cmd.Parameters.AddWithValue("@firstName", firstName);
                cmd.Parameters.AddWithValue("@lastName", lastName);
                cmd.Parameters.AddWithValue("@username", username);

                // 🔐 hash password (preporučeno)
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                cmd.Parameters.AddWithValue("@password", hashedPassword);

                cmd.Parameters.AddWithValue("@role", role);

                // početne vrijednosti
                cmd.Parameters.AddWithValue("@port", 0);
                cmd.Parameters.AddWithValue("@status", "INACTIVE");

                cmd.ExecuteNonQuery();
            }
        }
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
                string query = "SELECT * from Users where username = @username ";
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@username", username);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    if (BCrypt.Net.BCrypt.Verify(password, reader["password"].ToString()))
                    {

                        return new User { ID = Int32.Parse(reader["id"].ToString()), FirstName = reader["firstName"].ToString(), LastName = reader["lastName"].ToString(), Username = reader["username"].ToString(), Password = reader["password"].ToString(), Role = (UserRole)Enum.Parse(typeof(UserRole), reader["role"].ToString()), Port = Int32.Parse(reader["port"].ToString()) };
                    }

                }

            }
            return null;
        }

        public void DeactivateByPort(int port)
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

        public bool DetectInactiveUsers()
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
        public void PrintAllUsers()
        {
            List<User> list = GetAllUsers().ToList();

            Console.WriteLine("-------------------------------------------------------------------------------------------");
            Console.WriteLine("| {0,-15} | {1,-12} | {2,-15} | {3,-8} | {4,-6} | {5,-10} |",
                "FirstName", "LastName", "Username", "Status", "Port", "Role");
            Console.WriteLine("-------------------------------------------------------------------------------------------");

            foreach (var user in list)
            {
                Console.WriteLine("| {0,-15} | {1,-12} | {2,-15} | {3,-8} | {4,-6} | {5,-10} |",
                    user.FirstName,user.LastName,user.Username,user.Status == ActiveStatus.ACTIVE ? "YES" : "NO",user.Port,user.Role);
            }

            Console.WriteLine("-------------------------------------------------------------------------------------------");
        }

        public User GetUserById(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * from Users where id=@id";
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                        return new User { ID = Int32.Parse(reader["id"].ToString()), FirstName = reader["firstName"].ToString(), LastName = reader["lastName"].ToString(), Username = reader["username"].ToString(), Password = reader["password"].ToString(), Role = (UserRole)Enum.Parse(typeof(UserRole), reader["role"].ToString()), Port = Int32.Parse(reader["port"].ToString()) };
                }

            }
            return new User{ ID =0};
        }
    }
}
