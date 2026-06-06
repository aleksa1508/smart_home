using Common.Enums;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

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

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                cmd.Parameters.AddWithValue("@password", hashedPassword);

                cmd.Parameters.AddWithValue("@role", role);

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
        public void DeleteUser(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM users WHERE id=@id";
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                sqlCommand.Parameters.AddWithValue("@id", id);

                sqlCommand.ExecuteNonQuery();
            }
        }
        public void UpdateStatus(int id, ActiveStatus status, int port)
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
            var users = GetAllUsers().ToList();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ╔═════════════════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ║" + Center("REGISTERED USERS", 69) + "║");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ╠══════╦══════════════════╦══════════════╦════════╦═══════╦═══════════╣");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ║  #   ║  Name            ║  Username    ║ Online ║  Port ║  Role     ║");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ╠══════╬══════════════════╬══════════════╬════════╬═══════╬═══════════╣");
            int i = 1;
            foreach (var u in users)
            {
                Console.ResetColor();
                bool online = u.Status == ActiveStatus.ACTIVE;
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("  ║");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($" {i++,-4}");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write(" ║ ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{$"{u.FirstName} {u.LastName}",-17}");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("║ ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{u.Username,-13}");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("║ ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{(online ? "YES" : "NO"),-7}");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("║ ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{u.Port,-6}");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("║ ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{u.Role,-10}");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("║");
            }
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ╚══════╩══════════════════╩══════════════╩════════╩═══════╩═══════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }
        private static string Center(string text, int width)
        {
            if (text.Length >= width) return text;
            int totalPadding = width - text.Length;
            int leftPad = totalPadding / 2;
            int rightPad = totalPadding - leftPad;
            return new string(' ', leftPad) + text + new string(' ', rightPad);
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
            return new User { ID = 0 };
        }

        public void UpdateData(int id, string firstName, string lastName, string username)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE users SET firstName=@firstName,lastName=@lastName,username=@username WHERE id=@id";
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                sqlCommand.Parameters.AddWithValue("@firstName", firstName);
                sqlCommand.Parameters.AddWithValue("@lastName", lastName);
                sqlCommand.Parameters.AddWithValue("@username", username);
                sqlCommand.Parameters.AddWithValue("@id", id);

                sqlCommand.ExecuteNonQuery();
            }
        }
        public void UpdatePassword(int id, string password)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE users SET password=@password where id = @id";
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                sqlCommand.Parameters.AddWithValue("@password", hashedPassword);
                sqlCommand.Parameters.AddWithValue("@id", id);

                sqlCommand.ExecuteNonQuery();
            }
        }

        public void UpdateUserRole(int id, UserRole role)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE users SET role=@role where id = @id";
                SqlCommand sqlCommand = new SqlCommand(query, connection);

                sqlCommand.Parameters.AddWithValue("@role", role.ToString());
                sqlCommand.Parameters.AddWithValue("@id", id);

                sqlCommand.ExecuteNonQuery();
            }
        }
    }
}
