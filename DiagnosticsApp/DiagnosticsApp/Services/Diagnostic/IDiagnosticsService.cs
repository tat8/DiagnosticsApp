using DiagnosticsApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiagnosticsApp.Services.Diagnostic
{
    public interface IDiagnosticsService
    {
        void AddDiagnostics(DiagnosticsModel diagnosticsModel);
        void FindCalcinatesRegions(DiagnosticsModel diagnosticsModel);

    }
}
