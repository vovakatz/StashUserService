using Microsoft.Data.Sqlite;
using System.Data.Common;

namespace StashUserService.Business
{
    public enum DbConnetionType
    {
        Sqlite
    }

    public class ConnectionHelper
    {
        public DbConnection GetDbConnection(string connectionString, DbConnetionType type)
        {
            switch (type)
            {
                case DbConnetionType.Sqlite:
                    return new SqliteConnection(connectionString);
                default:
                    return null;
            }
        }
    }
}
