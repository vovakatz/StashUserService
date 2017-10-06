using System;
using System.Collections.Generic;
using System.Text;
using StashUserService.Model;

namespace StashUserService.Business
{
    public interface IUserManager
    {
        List<User> GetUsers();
        void SaveUser(User users);
        bool ValidateUser(User user, out List<string> errors);
        User GetUser(string email);
    }
}
