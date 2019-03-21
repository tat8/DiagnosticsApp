using DiagnosticsApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiagnosticsApp.Services.Clients
{
    public interface IClientService
    {
        IEnumerable<ClientModel> Get();
        IEnumerable<ClientModel> FilterLastname(string lastname);
        ClientModel FilterSnils(string snils);
    }
}
