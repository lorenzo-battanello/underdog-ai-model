namespace Underdog.Application.Abstractions;
using System.Data;
public interface IDBMConnection
{
    IDbConnection OpenConnection();
}