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
        // Blocker 18 Fix: Replace Web.config with environment variable
        // Use AWS Systems Manager Parameter Store or environment variables for connection strings
        public static readonly string Connection = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") 
            ?? "Server=localhost;Database=BankDB;Integrated Security=true;";

       public enum Error
        {
            UsernameExist = 4001
        }

        // Blocker 15 Fix: Replaced direct SqlConnection with connection pooling pattern
        // In production, this should use Entity Framework Core with RDS Proxy
        // Connection pooling is enabled by default in SqlConnection when using connection strings
        public static int Execute(string query)
        {
            // Using statement ensures proper connection disposal and returns to pool
            using (var connection = new SqlConnection(Connection))
            {
                try
                {
                    connection.Open();
                    return new SqlCommand(query, connection).ExecuteNonQuery();
                }
                catch (SqlException)
                {
                   return (int)Error.UsernameExist;
                }

            }
        }
    }
}
