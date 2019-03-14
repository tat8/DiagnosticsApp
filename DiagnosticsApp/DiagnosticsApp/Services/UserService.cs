using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiagnosticsApp.DatabaseModels;
using DiagnosticsApp.Models;

namespace DiagnosticsApp.Services
{
    public class UserService : IUserService
    {
        private DiagnosticsDBContext diagnosticsDBContext;

        public UserService(DiagnosticsDBContext diagnosticsDBContext)
        {
            this.diagnosticsDBContext = diagnosticsDBContext;
        }

        public UserModel AddUser(UserModel userModel)
        {
            if (userModel.RoleId == null)
            {
                if (userModel.RoleName == null)
                {
                    throw new Exception("Не выбрана роль пользователя");
                }
                var role = diagnosticsDBContext.Role.SingleOrDefault(o => o.RoleName == userModel.RoleName);
                if (role == null)
                {
                    throw new Exception("Заданной роли не существует");
                }
                userModel.RoleId = role.RoleId;
            }
            var user = new User()
            {
                Inn = userModel.Inn,
                RoleId = (long)userModel.RoleId,
                FirstName = userModel.FirstName,
                FatherName = userModel.FatherName,
                LastName = userModel.LastName
            };
            diagnosticsDBContext.User.Add(user);
            diagnosticsDBContext.SaveChanges();

            //пароль
            string password = GeneratePassword(user.Inn);
            string encryptedPassword = EncryptPassword(password);
            var userPassword = new UserPassword()
            {
                UserId = user.UserId,
                Password = encryptedPassword
            };
            diagnosticsDBContext.UserPassword.Add(userPassword);
            diagnosticsDBContext.SaveChanges();

            userModel.Password = password;
            return userModel;
        }

        public void EditUser(UserModel userModel)
        {
            User user;

            if (userModel.UserId == null)
            {
                // находим user по ИНН (так как ИНН является уникальным индексом)
                if (userModel.Inn == null)
                {
                    throw new Exception("Неодназначный запрос. Необходимо ИНН или id пользователя");
                }

                user = diagnosticsDBContext.User.SingleOrDefault(o => o.Inn == userModel.Inn);
            }
            else
            {
                user = diagnosticsDBContext.User.Find(userModel.UserId);
            }
            if (user == null)
            {
                throw new Exception("Пользователя с заданным id или ИНН не найдено.");
            }

            // изменяем доступные для редактирования значения
            user.FirstName = userModel.FirstName;
            user.LastName = userModel.LastName;
            user.FatherName = userModel.FatherName;

            diagnosticsDBContext.Update(user);
            diagnosticsDBContext.SaveChanges();
        }

        public IEnumerable<UserModel> GetUsers()
        {
            var users = diagnosticsDBContext.User.ToList();

            var userModels = new List<UserModel>();
            foreach (var user in users)
            {
                var userModel = BuildModel(user);
                userModels.Add(userModel);
            }

            return userModels;
        }

        public void LogIn(UserModel userModel)
        {
            throw new NotImplementedException();
        }

        public void LogOut(UserModel userModel)
        {
            throw new NotImplementedException();
        }

        public UserModel GetUsersById(UserModel userModel)
        {
            var user = diagnosticsDBContext.User.Find(userModel.UserId);
            if(user == null)
            {
                throw new Exception("Пользователя с заданным id не найдено");
            }
            var resultUserModel = BuildModel(user);
            return resultUserModel;
        }
        public UserModel GetUsersByInn(UserModel userModel)
        {
            var user = diagnosticsDBContext.User.SingleOrDefault(o => o.Inn == userModel.Inn);
            if (user == null)
            {
                throw new Exception("Пользователя с заданным ИНН не найдено");
            }
            var resultUserModel = BuildModel(user);
            return resultUserModel;
        }

        public IEnumerable<UserModel> GetUsersByRoleFIO(UserModel userModel)
        {
            var userModels = new List<UserModel>();
            var users = diagnosticsDBContext.User.AsEnumerable();

            if ((userModel.RoleId == null) && (userModel.RoleName == null) && (userModel.LastName == null) && (userModel.FatherName == null) && (userModel.FirstName == null))
            {
                throw new Exception("Для поиска необходимо ввести роль, фамилию, имя или отчество (хотя бы одно из списка)");
            }

            if (userModel.RoleId == null)
            {
                if (userModel.RoleName != null)
                {
                    var role = diagnosticsDBContext.Role.SingleOrDefault(o => o.RoleName == userModel.RoleName);
                    if (role == null)
                    {
                        throw new Exception("Роли с требуемым названием не найдено");
                    }
                    users = users.Where(o => o.RoleId == role.RoleId);
                }
            }
            else
            {
                users = users.Where(o => o.RoleId == userModel.RoleId);
            }

            if (userModel.LastName != null)
            {
                users = users.Where(o => o.LastName == userModel.LastName);
            }
            if (userModel.FirstName != null)
            {
                users = users.Where(o => o.FirstName == userModel.FirstName);
            }

            if (userModel.FatherName != null)
            {
                users = users.Where(o => o.FatherName == userModel.FatherName);
            }

            foreach (var user in users)
            {
                var fullUserModel = BuildModel(user);
                userModels.Add(fullUserModel);
            }

            return userModels;
        }
        
        private UserModel BuildModel(User user)
        {
            var role = diagnosticsDBContext.Role.Find(user.RoleId);
            var userModel = new UserModel()
            {
                UserId = user.UserId,
                RoleId = user.RoleId,
                Inn = user.Inn,
                FatherName = user.FatherName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RoleName = role.RoleName
            };
            return userModel;
        }

        private string GeneratePassword(string Inn)
        {
            var password = "";
            var innNumber = (int)(Convert.ToInt64(Inn) % sizeof(int));
            var rnd = new Random(innNumber);
            for (int i = 0; i < 8; i++)
            {
                int type = rnd.Next(0, 2);
                if (type == 0)
                {
                    password += (char)('a' + rnd.Next(0, 26));
                }
                else
                {
                    password += (char)('0' + rnd.Next(0, 10));
                }
            }

            return password;
        }

        //используется шифрование по методу сдвига (t = (a*t+k)%m)
        private string EncryptPassword(string password)
        {
            string encryptedPassword = "";
            int aLetter = (int)Enums.PasswordEnumLetter.a;
            int kLetter = (int)Enums.PasswordEnumLetter.k;
            int mLetter = (int)Enums.PasswordEnumLetter.m;
            int aNumber = (int)Enums.PasswordEnumNumber.a;
            int kNumber = (int)Enums.PasswordEnumNumber.k;
            int mNumber = (int)Enums.PasswordEnumNumber.m;

            for (int i = 0; i < password.Length; i++)
            {
                if (password[i] >= '0' && password[i] <= '9')
                {
                    encryptedPassword += Convert.ToChar(((aNumber * (Convert.ToInt32(password[i]) - '0') + kNumber) % mNumber) + '0');
                }
                else
                {

                    encryptedPassword += Convert.ToChar(((aLetter * (Convert.ToInt32(password[i]) - 'a') + kLetter) % mLetter) + 'a');
                }
            }

            return encryptedPassword;
        }

        //private string DecryptPassword(string encryptedPassword)
        //{
        //    string password = "";
        //    int aLetter = (int)Enums.PasswordEnumLetter.a;
        //    int kLetter = (int)Enums.PasswordEnumLetter.k;
        //    int mLetter = (int)Enums.PasswordEnumLetter.m;
        //    int aNumber = (int)Enums.PasswordEnumNumber.a;
        //    int kNumber = (int)Enums.PasswordEnumNumber.k;
        //    int mNumber = (int)Enums.PasswordEnumNumber.m;

        //    for (int i = 0; i < encryptedPassword.Length; i++)
        //    {
        //        if (encryptedPassword[i] >= '0' && encryptedPassword[i] <= '9')
        //        {
        //            int reversedElement = GetReversedElement(Convert.ToInt32(encryptedPassword[i]) - '0', mNumber);
        //            password += Convert.ToChar((((reversedElement - kNumber + mNumber ) % mNumber) / aNumber) + '0');
        //        }
        //        else
        //        {

        //            password += "a";
        //        }
        //    }

        //    return password;
        //}

        //private int GetReversedElement(int a, int m)
        //{
        //    int x, y;
        //    int g = GCD(a, m, out x, out y);
        //    if (g != 1)
        //        throw new ArgumentException();
        //    return (x % m + m) % m;
        //}
        //private int GCD(int a, int b, out int x, out int y)
        //{
        //    if (a == 0)
        //    {
        //        x = 0;
        //        y = 1;
        //        return b;
        //    }
        //    int x1, y1;
        //    int d = GCD(b % a, a, out x1, out y1);
        //    x = y1 - (b / a) * x1;
        //    y = x1;
        //    return d;
        //}
    }
}
