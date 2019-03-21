using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DiagnosticsApp.Models
{
    public class DiagnosticsModel
    {
        public long? DiagnosticsId { get; set; }

        [Required(ErrorMessage = "Не указано id врача")]
        public long? DoctorId { get; set; }

        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Не указано id клиента")]
        public long? ClientId { get; set; }

        [Required(ErrorMessage = "Не были выбраны снимки")]
        public IEnumerable<IFormFile> OriginalImagesFiles { get; set; }
        
        public IEnumerable<ImageModel> ImageModels { get; set; }

        [Required(ErrorMessage = "Не указано, был ли осмотр пациента")]
        public bool? HasExamination { get; set; }

        //проводить ли сразу при создании диагностики поиск
        public bool? SearchNow { get; set; }
        public long? ExaminationId { get; set; }
        public double? Temperature { get; set; }
        public string Pressure { get; set; }
        public int? Height { get; set; }
        public int? Weight { get; set; }
        public string Breath { get; set; }
        public string Complaint { get; set; }
        public string Other { get; set; }
    }
}
