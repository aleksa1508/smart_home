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

        public void PrintAllDevices(string deviceName)
        {
            List<Device> list = GetAllDevices().ToList();
            var table = new ConsoleTable("Device name", "Port", "Functions");
            foreach (var device in list)
            {
                string functions = string.Join(", ", device.Functions.Select(f => $"{f.Value.Name}: {f.Value.Value}"));
                // Pretvaranje funkcija u format ključ: vrednost
                table.AddRow(device.Name, device.Port, functions);
            }
            string[] linije = table.ToMarkDownString().Split('\n');
            if (!deviceName.Equals(""))
            {
                foreach (string linija in linije)
                {
                    // Bojenje po statusu
                    if (linija.Contains(deviceName))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(linija);
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine(linija);
                    }

                }
                // Obavezno resetuj boju na kraju!
                Console.ResetColor();
                return;
            }
            table.Write(Format.Alternative);
            
        }

        public string PrintDeviceFunctions(Device device)
        {
            string funkcije = string.Join(", ", device.Functions.Select(f => $"{f.Value.Name}: {f.Value.Value}"));

            var tablee = new ConsoleTable("Device name", "Port", "Functions");
          
            tablee.AddRow(device.Name,device.Port,funkcije);
                
            return tablee.ToStringAlternative();
        }
        public string PrintDeviceCommands(Device device)
        {
            var tablee = new ConsoleTable("Date and Time", "Log", "Username");
            foreach(var c in device.CommandRegister.OrderByDescending(x=>x.CreationDate).ToList())
            {
                tablee.AddRow(c.CreationDate, c.Log, c.Username);
            }
                
            return tablee.ToStringAlternative();
        }
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
