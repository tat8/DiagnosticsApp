using DiagnosticsApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiagnosticsApp.Services
{
    public interface IUserService
    {
        IEnumerable<UserModel> GetUsers();
        UserModel GetUsersByInn(UserModel userModel);
        UserModel GetUsersById(UserModel userModel);
        IEnumerable<UserModel> GetUsersByRoleFIO(UserModel userModel);
        UserModel AddUser(UserModel userModel);
        void EditUser(UserModel userModel);
        void LogIn(UserModel userModel);
        void LogOut(UserModel userModel);
    }
}
