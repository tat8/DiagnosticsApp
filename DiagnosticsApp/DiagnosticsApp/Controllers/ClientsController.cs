using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiagnosticsApp.Models;
using DiagnosticsApp.Services.Clients;
using DiagnosticsApp.Services.Errors;
using Microsoft.AspNetCore.Mvc;

namespace DiagnosticsApp.Controllers
{
    [Route("api/clients")]
    public class ClientsController : Controller
    {
        private IClientService clientService;
        private IErrorService errorService;

        public ClientsController(IClientService clientService, IErrorService errorService)
        {
            this.clientService = clientService;
            this.errorService = errorService;
        }

        // GET api/clients
        [HttpGet]
        public IActionResult Get()
        {
            JsonResult result;
            try
            {
                var clientModels = clientService.Get();
                result = Json(Ok(clientModels));
            }
            catch (Exception ex)
            {
                var errorModel = errorService.BuildError(ex);
                result = Json(BadRequest(errorModel));
            }
            return result;
        }

        // GET api/clients/filter
        [HttpPost("filter")]
        public IActionResult Filter(ClientModel clientModel)
        {
            JsonResult result;
            try
            {
                var clientModels = new List<ClientModel>();
                if(clientModel.Snils != null)
                {
                    var clientModelFull = clientService.FilterSnils(clientModel.Snils);
                    clientModels.Add(clientModelFull);
                }
                else if(clientModel.LastName != null)
                {
                    clientModels = (List<ClientModel>)clientService.FilterLastname(clientModel.LastName);
                }
                else
                {
                    throw new Exception("Необходимо указать СНИЛС или фамилию");
                }
                result = Json(Ok(clientModels));
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
