using Common.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CentralniServer.Repositories.RuleActionRepository
{
    public class RuleActionRepository : IRuleActionRepository
    {
        private readonly string connectionString = "Server=localhost\\SQLEXPRESS;Database=users_db;Trusted_Connection=True;";
        public void AddRuleAction(int ruleId, string functionName, string value, string deviceGroup, int? functionId, int? deviceId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
        INSERT INTO ruleActions (ruleId,deviceId,deviceGroup,functionId,functionName,value)
        VALUES (@ruleId,@deviceId,@deviceGroup,@functionId,@functionName, @value)";

                SqlCommand cmd = new SqlCommand(query, connection);

                cmd.Parameters.AddWithValue("@ruleId", ruleId);
                cmd.Parameters.AddWithValue("@functionName", functionName);
                cmd.Parameters.AddWithValue("@value", value);
                cmd.Parameters.Add("@deviceId", SqlDbType.Int).Value = (object)deviceId ?? DBNull.Value;

                cmd.Parameters.Add("@deviceGroup", SqlDbType.VarChar).Value = (object)deviceGroup ?? DBNull.Value;

                cmd.Parameters.Add("@functionId", SqlDbType.Int).Value = (object)functionId ?? DBNull.Value;

                cmd.ExecuteNonQuery();
            }
        }

        public IEnumerable<RuleAction> GetAllActions()
        {
            List<RuleAction> lista = new List<RuleAction>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * from ruleActions";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {

                    lista.Add(new RuleAction
                    {
                        RuleId = int.Parse(reader["ruleId"].ToString()),
                        DeviceId = reader["deviceId"] == DBNull.Value
          ? (int?)null
          : Convert.ToInt32(reader["deviceId"]),

                        FunctionId = reader["functionId"] == DBNull.Value
          ? (int?)null
          : Convert.ToInt32(reader["functionId"]),
                        FunctionName = reader["functionName"].ToString(),
                        Value = reader["value"].ToString(),
                        DeviceGroup = reader["deviceGroup"] == DBNull.Value
          ? null
          : reader["deviceGroup"].ToString()
                    });

                }

                return lista;
            }
        }
        public IEnumerable<RuleAction> GetAllActionsByRuleId(int ruleId)
        {
            List<RuleAction> lista = new List<RuleAction>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * from ruleActions where ruleId=@ruleId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ruleId", ruleId);

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {

                    lista.Add(new RuleAction
                    {
                        RuleId = int.Parse(reader["ruleId"].ToString()),
                        DeviceId = reader["deviceId"] == DBNull.Value
                        ? (int?)null
                        : Convert.ToInt32(reader["deviceId"]),
                        FunctionId = reader["functionId"] == DBNull.Value
                        ? (int?)null
                        : Convert.ToInt32(reader["functionId"]),
                        FunctionName = reader["functionName"].ToString(),
                        Value = reader["value"].ToString(),
                        DeviceGroup = reader["deviceGroup"] == DBNull.Value
                        ? null
                        : reader["deviceGroup"].ToString()
                    });

                }

                return lista;
            }
        }
        public void DeleteRuleAction(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM ruleActions WHERE ruleId = @id";
                SqlCommand sqlCommand = new SqlCommand(query, connection);

                sqlCommand.Parameters.AddWithValue("@id", id);

                sqlCommand.ExecuteNonQuery();
            }
        }
    }
}
