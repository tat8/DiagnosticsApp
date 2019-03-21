using DiagnosticsApp.DatabaseModels;
using DiagnosticsApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiagnosticsApp.Services.Users
{
    public class UserService : IUserService
    {
        private DiagnosticsDBContext diagnosticsDBContext;

        public UserService(DiagnosticsDBContext diagnosticsDbContext)
        {
            this.diagnosticsDBContext = diagnosticsDbContext;
        }

        public UserModel AddUser(UserModel userModel)
        {
            if (userModel.RoleId == null)
            {
                if (userModel.RoleName == null)
                {
                    throw new Exception("Не выбрана роль пользователя");
                }

                userModel.RoleName = userModel.RoleName.Trim().ToLower();
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
                LastName = userModel.LastName,
                PhoneNumber = userModel.PhoneNumber
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
                RoleName = role.RoleName,
                PhoneNumber = user.PhoneNumber               
            };
            return userModel;
        }

        private string GeneratePassword(string inn)
        {
            var password = "";
            var innNumber = (int)(Convert.ToInt64(inn) % sizeof(int));
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
    }
}
