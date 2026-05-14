using Common.Models;
using System.Collections.Generic;
using System.Data.SqlClient;

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
    }
}
