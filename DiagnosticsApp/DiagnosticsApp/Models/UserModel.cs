using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DiagnosticsApp.Models
{
    public class UserModel
    {
        public long? UserId { get; set; }

        [Required(ErrorMessage = "Не указано имя")]
        public string FirstName { get; set; }

        public string FatherName { get; set; }

        [Required(ErrorMessage = "Не указана фамилия")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Не указано ИНН")]
        public string Inn { get; set; }
        public long? RoleId { get; set; }
        public string RoleName { get; set; }
        public string Password { get; set; }

        [Required(ErrorMessage = "Не указан номер телефона")]
        public string PhoneNumber { get; set; }
    }
}
