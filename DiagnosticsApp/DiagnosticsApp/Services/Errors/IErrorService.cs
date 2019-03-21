using DiagnosticsApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiagnosticsApp.Services.Errors
{
    public interface IErrorService
    {
        ErrorModel BuildError(Exception ex);
    }
}
