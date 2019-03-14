using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiagnosticsApp.Models
{
    public class UserModel
    {
        public long? UserId { get; set; }
        public string FirstName { get; set; }
        public string FatherName { get; set; }
        public string LastName { get; set; }
        public string Inn { get; set; }
        public long? RoleId { get; set; }
        public string RoleName { get; set; }
        public string Password { get; set; }
    }
}
