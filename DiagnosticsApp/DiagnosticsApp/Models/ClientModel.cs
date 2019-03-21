using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiagnosticsApp.Models
{
    public class ClientModel
    {
        public long? ClientId { get; set; }
        public string FirstName { get; set; }
        public string FatherName { get; set; }
        public string LastName { get; set; }
        public DateTime? BirthDate { get; set; }
        public bool? IsMale { get; set; }
        public string Passport { get; set; }
        public string Snils { get; set; }
        public string PhoneNumber { get; set; }
        public IEnumerable<DiagnosticsModel> DiagnosticsModels { get; set; }
    }
}
