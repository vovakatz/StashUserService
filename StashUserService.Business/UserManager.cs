using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StashUserService.Model;

namespace StashUserService.Business
{
    public class UserManager : IUserManager
    {
        private const int TOTAL_RETRY_COUNT = 5;
        private static string connectionString = "Data Source=stash_users.db;";
        private static ConnectionHelper connectionHelper = new ConnectionHelper();
        private static  DbConnection connection = connectionHelper.GetDbConnection(connectionString, DbConnetionType.Sqlite);

        public List<User> GetUsers()
        {
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            List<User> users = new List<User>();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM Users";
                var result = command.ExecuteReader();
                while (result.Read())
                {
                    users.Add(MapUser(result));
                }
            }
            return users;
        }

        public User GetUser(string email)
        {
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"SELECT * FROM Users WHERE email = '{email}'";
                var result = command.ExecuteReader();
                if (result.Read())
                {
                    return MapUser(result);
                }
            }
            return null;
        }

        public async void SaveUser(User user)
        {
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            using (var command = connection.CreateCommand())
            {
                int id = connection.ExecuteNonQuery($@"
                    INSERT INTO Users
                        (email,  phone_number, full_name, password, key, metadata)
                    VALUES
                        ('{user.Email}', '{user.PhoneNumber}', '{user.FullName}', '{user.Password}', '{user.Key}', '{user.Metadata}')");

                await FetchAccount(user.Email, user.Key);
                Console.WriteLine("inserted user");
            }  
        }

        private void UpdateAccountKey(string email, string accountKey)
        {
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            using (var command = connection.CreateCommand())
            {
                connection.ExecuteNonQuery($"UPDATE Users SET account_key = '{accountKey}' WHERE email = '{email}'");
            }
        }

        private async Task FetchAccount(string email, string key)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://example.com/");
            client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, " https://account-key-service.herokuapp.com/v1/account");
            request.Content = new StringContent("{\"email\":\"user@example.com\",\"key\":\"72ae25495a7981c40622d49f9a52e4f1565c90f048f59027bd9c8c8900d5c3d8\"}",
                                                Encoding.UTF8,
                                                "application/json");

            await client.SendAsync(request)
                  .ContinueWith(async responseTask =>
                  {
                      if (responseTask.Exception == null)
                      {
                          string str = await responseTask.Result.Content.ReadAsStringAsync();
                          var obj = JObject.Parse(str);
                          string akey = obj["account_key"].Value<string>();
                          UpdateAccountKey(email, akey);
                      }
                      else
                      {
                          await Task.Delay(3600000);
                          FetchAccount(email, key);
                      }
                  });
        }

        public bool ValidateUser(User user, out List<string> errors)
        {
            errors = new List<string>();
            if (user.Email == null || user.Email.Trim() == "" || user.Email.Length > 200)
                errors.Add("email is invalid");
            if (user.PhoneNumber == null || user.PhoneNumber.Trim() == "" || user.PhoneNumber.Length > 200)
                errors.Add("phone number is invalid");
            if (user.FullName != null && user.FullName.Length > 200)
                errors.Add("full name is too long");
            if (user.Password == null || user.Password.Trim() == "" || user.Password.Length > 100)
                errors.Add("password is invalid");
            if (user.Key == null || user.Key.Trim() == "" || user.Key.Length > 100)
                errors.Add("key is invalid");
            if (user.Metadata != null && user.Metadata.Length > 2000)
                errors.Add("metadata is too long");

            if (errors.Count > 0)
                return false;
            return true;
        }

        private User MapUser(DbDataReader result)
        {
            User user = new User();

            user.Id = result.GetInt32(0);
            user.Email = result.GetString(1);
            user.PhoneNumber = result.GetString(2);
            if (!result.IsDBNull(3))
                user.FullName = result.GetString(3);
            user.Password = result.GetString(4);
            user.Key = result.GetString(5);
            if (!result.IsDBNull(6))
                user.AccountKey = result.GetString(6);
            if (!result.IsDBNull(7))
                user.Metadata = result.GetString(7);

            return user;
        }
    }
}
