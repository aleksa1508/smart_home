using Common.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Common.Repositories.SmartRulesRepositories
{
    public class SmartRulesRepository : ISmartRulesRepository
    {
        private readonly string connectionString = "Server=localhost\\SQLEXPRESS;Database=users_db;Trusted_Connection=True;";
        public void AddSmartRule(string name, string description, bool isEnabled)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
        INSERT INTO smartRules (name,description,isEnabled)
        VALUES (@name, @description, @isEnabled)";

                SqlCommand cmd = new SqlCommand(query, connection);

                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@description", description);
                cmd.Parameters.AddWithValue("@isEnabled", isEnabled.ToString());

                cmd.ExecuteNonQuery();
            }
        }

        public IEnumerable<SmartRule> GetAllSmartRules()
        {
            List<SmartRule> lista = new List<SmartRule>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * from smartRules";
                SqlCommand command = new SqlCommand(query, connection);

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {

                    lista.Add(new SmartRule { Id = int.Parse(reader["id"].ToString()), Name = reader["name"].ToString(), Description = reader["description"].ToString(), IsEnabled = bool.Parse(reader["isEnabled"].ToString()) });

                }

                return lista;
            }
        }
        public int GetSmartRuleByName(string name)
        {
            int result = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * from smartRules where name=@name";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@name", name);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    result = int.Parse(reader["id"].ToString());
                }

                return result;
            }
        }

        public void UpdateSmartRule(SmartRule smartRule)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE smartRules SET isEnabled=@isEnabled where id = @id";
                SqlCommand sqlCommand = new SqlCommand(query, connection);

                sqlCommand.Parameters.AddWithValue("@isEnabled", smartRule.IsEnabled.ToString());
                sqlCommand.Parameters.AddWithValue("@id", smartRule.Id);

                sqlCommand.ExecuteNonQuery();
            }
        }
        public void DeleteSmartRule(SmartRule smartRule)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM smartRules WHERE id = @id";
                SqlCommand sqlCommand = new SqlCommand(query, connection);

                sqlCommand.Parameters.AddWithValue("@id", smartRule.Id);

                sqlCommand.ExecuteNonQuery();
            }
        }

        private static string Center(string text, int width)
        {
            if (text.Length >= width) return text;
            int totalPadding = width - text.Length;
            int leftPad = totalPadding / 2;
            int rightPad = totalPadding - leftPad;
            return new string(' ', leftPad) + text + new string(' ', rightPad);
        }
        public void PrintAllSmartRules()
        {
            var rules = GetAllSmartRules().ToList();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ╔════════════════════════════════════════╗");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ║" + Center("SMART RULES", 40) + "║");

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ╠══════╦══════════════════════╦══════════╣");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  ║  #   ║ Rule Name            ║ Status   ║");

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ╠══════╬══════════════════════╬══════════╣");

            int i = 1;
            foreach (var rule in rules)
            {
                bool enabled = rule.IsEnabled;
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("  ║");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($" {i++,-4}");

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write(" ║ ");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{rule.Name,-21}");

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("║ ");

                Console.ForegroundColor = ConsoleColor.White;

                Console.Write($"{(rule.IsEnabled ? "ENABLED" : "DISABLED"),-9}");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("║");
            }
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ╚══════╩══════════════════════╩══════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}
