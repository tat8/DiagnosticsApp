using System;
using System.Collections.Generic;

namespace DiagnosticsApp.DatabaseModels
{
    public partial class Diagnostics
    {
        public long DiagnosticsId { get; set; }
        public long DoctorId { get; set; }
        public DateTime StartTime { get; set; }
        public long ClientId { get; set; }
        public long? ExaminationId { get; set; }

        public Client Client { get; set; }
        public User Doctor { get; set; }
    }
}
