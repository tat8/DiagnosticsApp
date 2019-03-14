using System;
using System.Collections.Generic;

namespace DiagnosticsApp.DatabaseModels
{
    public partial class Image
    {
        public long ImageId { get; set; }
        public long DiagnosticsId { get; set; }
        public double CalcinatesPercent { get; set; }
        public double CalcinateBiggest { get; set; }
        public int CalcinatesCount { get; set; }
        public string RefNotParsed { get; set; }
        public string RefParsed { get; set; }
    }
}
