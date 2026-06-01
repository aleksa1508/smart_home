using Common.DTOs;
using Common.Enums;
using Common.Models;
using ConsoleTables;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Common.Repositories.DevicesRepositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly string connectionString = "Server=localhost\\SQLEXPRESS;Database=devices_db;Trusted_Connection=True;";
        public void AddDevice(string name, int port, RoomType location, DateTime lastChange)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"INSERT INTO device(name,port,location,lastChanged) VALUES(@name,@port,@location,@date)";

                SqlCommand cmd = new SqlCommand(query, connection);

                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@port", port);
                cmd.Parameters.AddWithValue("@location", location.ToString());
                cmd.Parameters.AddWithValue("@date", lastChange);

                cmd.ExecuteNonQuery();
            }
        }
        public void AddDeviceFunctions(string function, string value, int deviceId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"INSERT INTO Functions(device_id,function_name,function_value)VALUES(@device_id,@function,@value)";

                SqlCommand cmd = new SqlCommand(query, connection);

                cmd.Parameters.AddWithValue("@device_id", deviceId);
                cmd.Parameters.AddWithValue("@function", function);
                cmd.Parameters.AddWithValue("@value", value);

                cmd.ExecuteNonQuery();
            }
        }
        public void AddDeviceCommands(string message, DateTime creation_date, int deviceId, string username)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"INSERT INTO Commands(device_id,message,creation_date,username)VALUES(@device_id,@message,@creation_date,@username)";

                SqlCommand cmd = new SqlCommand(query, connection);

                cmd.Parameters.AddWithValue("@device_id", deviceId);
                cmd.Parameters.AddWithValue("@message", message);
                cmd.Parameters.AddWithValue("@creation_date", creation_date);
                cmd.Parameters.AddWithValue("@username", username);

                cmd.ExecuteNonQuery();
            }
        }
        public IEnumerable<Device> GetAllDevices()
        {
            List<Device> devices = new List<Device>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Device";
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    int id = Int32.Parse(reader["id"].ToString());
                    Dictionary<int, Function> functions = GetDeviceFunctions(id);
                    List<Command> commands = GetDeviceCommands(id);
                    devices.Add(new Device(id, reader["name"].ToString(), Int32.Parse(reader["port"].ToString()), (RoomType)Enum.Parse(typeof(RoomType), reader["location"].ToString()), functions, commands, DateTime.Parse(reader["lastChanged"].ToString())));
                }
            }
            return devices;
        }
        public IEnumerable<Command> GetAllCommands()
        {
            List<Command> commands = new List<Command>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Commands";
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    commands.Add(new Command { ID = Int32.Parse(reader["id"].ToString()), Log = reader["message"].ToString(), CreationDate = DateTime.Parse(reader["creation_date"].ToString()), Username = reader["username"].ToString() });
                }
            }
            return commands;
        }
        public Device GetDeviceById(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Device WHERE id=@id";
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                sqlCommand.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    return new Device(id, reader["name"].ToString(), Int32.Parse(reader["port"].ToString()), (RoomType)Enum.Parse(typeof(RoomType), reader["location"].ToString()));
                }
            }
            return null;
        }
        public Dictionary<int, Function> GetDeviceFunctions(int id)
        {
            Dictionary<int, Function> functions = new Dictionary<int, Function>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Functions WHERE device_id=@id";
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                sqlCommand.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    functions.Add(Int32.Parse(reader["id"].ToString()), new Function { Name = reader["function_name"].ToString(), Value = reader["function_value"].ToString() });
                }
            }
            return functions;
        }
        public List<Command> GetDeviceCommands(int id)
        {
            List<Command> commands = new List<Command>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Commands WHERE device_id=@id";
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                sqlCommand.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    commands.Add(new Command { ID = Int32.Parse(reader["id"].ToString()), Log = reader["message"].ToString(), CreationDate = DateTime.Parse(reader["creation_date"].ToString()), Username = reader["username"].ToString() });
                }
            }
            return commands;
        }
        private static string Center(string text, int width)
        {
            if (text.Length >= width) return text;
            int totalPadding = width - text.Length;
            int leftPad = totalPadding / 2;
            int rightPad = totalPadding - leftPad;
            return new string(' ', leftPad) + text + new string(' ', rightPad);
        }
        public void PrintAllDevices(string deviceName = "")
        {
            var devices = GetAllDevices().ToList();

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ╔══════════════════════════════════════════════════════════════════════════════════╗");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ║" + Center("REGISTERED DEVICES", 82) + "║");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ╠══════╦══════════════════════╦════════╦═══════════════════════════════════════════╣");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ║  #   ║ Device Name          ║  Port  ║ Functions                                 ║");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ╠══════╬══════════════════════╬════════╬═══════════════════════════════════════════╣");

            int i = 1;

            foreach (var device in devices)
            {
                string functions = FormatFunctions(device);

                bool highlight =
                    !string.IsNullOrWhiteSpace(deviceName) &&
                    device.Name.Equals(deviceName, StringComparison.OrdinalIgnoreCase);

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("  ║");

                Console.ForegroundColor = highlight ? ConsoleColor.Green : ConsoleColor.White;
                Console.Write($" {i++,-4}");

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write(" ║ ");

                Console.ForegroundColor = highlight ? ConsoleColor.Green : ConsoleColor.White;
                Console.Write($"{device.Name,-21}");

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("║ ");

                Console.ForegroundColor = highlight ? ConsoleColor.Green : ConsoleColor.White;
                Console.Write($"{device.Port,-7}");

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("║ ");

                Console.ForegroundColor = highlight ? ConsoleColor.Green : ConsoleColor.White;

                Console.Write($"{functions,-42}");

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("║");
            }

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ╚══════╩══════════════════════╩════════╩═══════════════════════════════════════════╝");

            Console.ResetColor();
            Console.WriteLine();
        }

        public string PrintDeviceFunctions(Device device)
        {
            var table = new ConsoleTable("Device Name", "Port", "Functions");
            table.AddRow(device.Name, device.Port, FormatFunctions(device));
            return table.ToStringAlternative();
        }

        public string PrintDeviceCommands(Device device)
        {
            var table = new ConsoleTable("Date and Time", "Log", "Username");

            foreach (var cmd in device.CommandRegister.OrderByDescending(x => x.CreationDate))
                table.AddRow(cmd.CreationDate.ToString("yyyy-MM-dd HH:mm:ss"), cmd.Log, cmd.Username);

            return table.ToStringAlternative();
        }

        private string FormatFunctions(Device device) =>
            string.Join(", ", device.Functions.Select(f => $"{f.Value.Name}: {f.Value.Value}"));
        public void UpdateDeviceFunction(int deviceId, int id, string name, string value)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Functions SET function_name=@function_name,function_value=@function_value WHERE device_id=@deviceId and id=@id";
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                sqlCommand.Parameters.AddWithValue("@function_name", name);
                sqlCommand.Parameters.AddWithValue("@function_value", value);
                sqlCommand.Parameters.AddWithValue("@deviceId", deviceId);
                sqlCommand.Parameters.AddWithValue("@id", id);
                sqlCommand.ExecuteNonQuery();

            }
            UpdateDevice(deviceId);//update timestamp when device function changes
        }
        public void UpdateDevice(int deviceId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Device SET lastChanged=@lastChanged WHERE id=@deviceId";
                SqlCommand sqlCommand = new SqlCommand(query, connection);
                sqlCommand.Parameters.AddWithValue("@lastChanged", DateTime.Now);
                sqlCommand.Parameters.AddWithValue("@deviceId", deviceId);
                sqlCommand.ExecuteNonQuery();

            }
        }
    }
}
