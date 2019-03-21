using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiagnosticsApp.Models
{
    public class ImageModel
    {
        public long? ImageId { get; set; }
        public long DiagnosticsId { get; set; }
        public double? CalcinatesPercent { get; set; }
        public double? CalcinateBiggest { get; set; }
        public int? CalcinatesCount { get; set; }
        public string RefNotParsed { get; set; }
        public string RefParsed { get; set; }
    }
}
