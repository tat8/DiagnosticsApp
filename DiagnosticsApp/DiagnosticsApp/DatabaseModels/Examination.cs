using System;
using System.Collections.Generic;

namespace DiagnosticsApp.DatabaseModels
{
    public partial class Examination
    {
        public long ExaminationId { get; set; }
        public double Temperature { get; set; }
        public string Pressure { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public string Breath { get; set; }
        public string Complaint { get; set; }
        public string Other { get; set; }
    }
}
