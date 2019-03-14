using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiagnosticsApp.DatabaseModels
{
    public partial class AppointmentDiagnosis
    {
        [Key, Column(Order = 0)]
        public long AppointmentId { get; set; }
        [Key, Column(Order = 1)]
        public long DiagnosisId { get; set; }
    }
}
