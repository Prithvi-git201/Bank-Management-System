using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankDatabaseAccess
{
    public static class DatabaseConnection
    {
        // Blocker 18 Fix: Replace Web.config with environment variables
        // Use environment variable for connection string instead of ConfigurationManager
        public static readonly string Connection = 
            Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") 
            ?? "Server=localhost;Database=BankDB;Integrated Security=true;";

       public enum Error
        {
            UsernameExist = 4001
        }

        // Blocker 15 Fix: Use RDS Proxy with connection pooling
        // Modified to use connection pooling best practices for cloud environments
        public static int Execute(string query)
        {
            // Connection pooling is enabled by default in SqlConnection
            // RDS Proxy will handle connection multiplexing at infrastructure level
            using (var connection = new SqlConnection(Connection))
            {
                try
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        return command.ExecuteNonQuery();
                    }
                }
                catch (SqlException)
                {
                   return (int)Error.UsernameExist;
                }
            }
        }
    }
}
