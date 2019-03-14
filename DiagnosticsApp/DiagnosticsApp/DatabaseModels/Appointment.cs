using System;
using System.Collections.Generic;

namespace DiagnosticsApp.DatabaseModels
{
    public partial class Appointment
    {
        public long AppointmentId { get; set; }
        public long DoctorId { get; set; }
        public DateTime StartTime { get; set; }
        public long ClientId { get; set; }
        public string Prescription { get; set; }
        public long ExaminationId { get; set; }

    }
}
