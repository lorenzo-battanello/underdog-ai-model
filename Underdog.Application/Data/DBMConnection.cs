using Microsoft.Data.SqlClient;
using Underdog.Application.Abstractions;
using System.Data;

namespace Underdog.Application.Data;
public class DBMConnection(string connectionString) : IDBMConnection
{
    private readonly string _connectionString = connectionString;

    public IDbConnection OpenConnection()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}