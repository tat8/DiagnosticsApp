using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiagnosticsApp.Models;

namespace DiagnosticsApp.Services.Errors
{
    public class ErrorService : IErrorService
    {
        public ErrorModel BuildError(Exception ex)
        {
            var errorModel = new ErrorModel()
            {
                CustomMessage = "Некорректный запрос",
                Message = ex.Message,
                InnerException = ex.InnerException,
                Source = ex.Source
            };

            return errorModel;
        }
    }
}
