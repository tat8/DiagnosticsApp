using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiagnosticsApp.Models;
using DiagnosticsApp.Services.Diagnostic;
using DiagnosticsApp.Services.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DiagnosticsApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiagnosticsController : Controller
    {
        private IDiagnosticsService diagnosticsService;
        private IErrorService errorService;

        public DiagnosticsController(IDiagnosticsService diagnosticsService, IErrorService errorService)
        {
            this.diagnosticsService = diagnosticsService;
            this.errorService = errorService;
        }

        // POST api/diagnostics/add
        [HttpPost("add")]
        public IActionResult Add([FromForm]DiagnosticsModel diagnosticsModel)
        {
            JsonResult result;
            try
            {
                if(ModelState.IsValid)
                {
                    diagnosticsService.AddDiagnostics(diagnosticsModel);
                }
                result = Json(Ok());
            }
            catch (Exception ex)
            {
                var errorModel = errorService.BuildError(ex);
                result = Json(BadRequest(errorModel));
            }
            return result;
        }

        // POST api/diagnostics/fins
        [HttpGet("find")]
        public IActionResult Find()
        {
            var m = new DiagnosticsModel()
            { };

            JsonResult result;
            try
            {
                diagnosticsService.FindCalcinatesRegions(m);
                result = Json(Ok());
            }
            catch (Exception ex)
            {
                var errorModel = errorService.BuildError(ex);
                result = Json(BadRequest(errorModel));
            }
            return result;
        }
    }
}