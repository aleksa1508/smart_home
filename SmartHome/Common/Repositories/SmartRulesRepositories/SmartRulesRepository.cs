using Common.Models;
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

                    lista.Add(new SmartRule { Name = reader["name"].ToString(), Description = reader["description"].ToString(), IsEnabled = bool.Parse(reader["isEnabled"].ToString()) });

                }

                return lista;
            }
        }

        public void UpdateSmartRule(SmartRule smartRule)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE smartRules SET isEnabled=@isEnabled where name = @name";
                SqlCommand sqlCommand = new SqlCommand(query, connection);

                sqlCommand.Parameters.AddWithValue("@isEnabled", smartRule.IsEnabled.ToString());
                sqlCommand.Parameters.AddWithValue("@name", smartRule.Name);

                sqlCommand.ExecuteNonQuery();
            }
        }
        public void ExistsSmartRules()
        {
            var list = GetAllSmartRules().ToList();
            if (list.Count == 0)
            {
                AddNewSmartRules();
            }
        }

        public void AddNewSmartRules()
        {
            List<SmartRule> rules1 = new List<SmartRule>
            {
                new SmartRule{ IsEnabled=false, Name="NightMode",Description="Limits temperature to 20°C, restricts lights and locks garage during night hours."},
                new SmartRule{ IsEnabled=false, Name="SecurityMode",Description="Lock all doors and vaults."},
                new SmartRule{ IsEnabled=false, Name="EnergySaving",Description="Limits brightness and reduces energy usage."},
            };
            foreach (var r in rules1)
            {
                AddSmartRule(r.Name, r.Description, r.IsEnabled);
            }
        }
    }
}
