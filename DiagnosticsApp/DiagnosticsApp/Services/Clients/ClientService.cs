using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiagnosticsApp.DatabaseModels;
using DiagnosticsApp.Models;

namespace DiagnosticsApp.Services.Clients
{
    public class ClientService : IClientService
    {
        private DiagnosticsDBContext diagnosticsDBContext;
        
        public ClientService(DiagnosticsDBContext diagnosticsDBContext)
        {
            this.diagnosticsDBContext = diagnosticsDBContext;
        }

        public IEnumerable<ClientModel> FilterLastname(string lastname)
        {
            if(lastname == null)
            {
                throw new Exception("Необходимо ввести фамилию");
            }

            lastname = lastname.Trim().ToLower();
            var clients = diagnosticsDBContext.Client.Where(o => o.LastName == lastname);
            if(clients == null)
            {
                throw new Exception("Пользователей с заданной фамилией не найдено");
            }

            var clientModels = BuildModels(clients);

            return clientModels;
        }

        public ClientModel FilterSnils(string snils)
        {
            if(snils == null)
            {
                throw new Exception("Необходимо ввести номер СНИЛС");
            }

            var client = diagnosticsDBContext.Client.SingleOrDefault(o => o.Snils == snils);
            if(client == null)
            {
                throw new Exception("Пациента с заданным СНИЛС не найдено");
            }

            var clientModel = BuildModel(client);

            return clientModel;
        }

        public IEnumerable<ClientModel> Get()
        {
            var clients = diagnosticsDBContext.Client.AsQueryable();
            var clientModels = BuildModels(clients);

            return clientModels;
        }

        private IEnumerable<ClientModel> BuildModels(IQueryable<Client> clients)
        {
            var clientModels = new List<ClientModel>();
            foreach (var client in clients)
            {
                var clientModel = BuildModel(client);
                clientModels.Add(clientModel);
            }

            return clientModels;
        }
        
        private ClientModel BuildModel(Client client)
        {
            var clientModel = new ClientModel()
            {
                BirthDate = client.BirthDate,
                ClientId = client.ClientId,
                FatherName = client.FatherName,
                FirstName = client.FirstName,
                IsMale = client.IsMale,
                LastName = client.LastName,
                Passport = client.Passport,
                PhoneNumber = client.PhoneNumber,
                Snils = client.Snils
            };

            return clientModel;
        }
    }
}
