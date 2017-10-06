using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace StashUserService.Business
{
    // Reused and Modified Code - https://github.com/aspnet/Microsoft.Data.Sqlite/blob/dev/src/Microsoft.Data.Sqlite/Utilities/DbConnectionExtensions.cs
    public static class DbConnectionExtensions
    {
        public static int ExecuteNonQuery(this DbConnection connection, string commandText, int timeout = 30)
        {
            var command = connection.CreateCommand();
            command.CommandTimeout = timeout;
            command.CommandText = commandText;
            return command.ExecuteNonQuery();
        }

        public static T ExecuteScalar<T>(this DbConnection connection, string commandText, int timeout = 30) =>
            (T)connection.ExecuteScalar(commandText, timeout);

        private static object ExecuteScalar(this DbConnection connection, string commandText, int timeout)
        {
            var command = connection.CreateCommand();
            command.CommandTimeout = timeout;
            command.CommandText = commandText;
            return command.ExecuteScalar();
        }

        public static DbDataReader ExecuteReader(this DbConnection connection, string commandText)
        {
            var command = connection.CreateCommand();
            command.CommandText = commandText;
            return command.ExecuteReader();
        }
    }
}
