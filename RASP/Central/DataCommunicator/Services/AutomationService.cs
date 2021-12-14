using DataCommunicator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DataCommunicator.Services
{
    /// <summary>
    /// Inserir aqui toda a lógica de manipulação dos módulos. Pois por aqui
    /// temos toda a informação necessária para realizar os comandos solicitados.
    /// </summary>
    public class AutomationService
    {
        private readonly ILogger<DbService> _logger;
        private readonly DbService _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;


        public AutomationService(ILogger<DbService> logger, DbService dbContext, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _dbContext = dbContext;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public void UpdateModule(int id, bool data)
        {
            var module = _dbContext.GetModule(mod => mod.Id == id);
            if (module != null)
            {
                string moduleUrl;
                if (data)
                {
                    moduleUrl = module.IpAddress
                    + GetUrlToModuleType(module.Type ?? ModulesType.unknown)
                    + "status1=80"
                    + "&status2=0"
                    +"&time=7";
                }
                else
                {
                   moduleUrl = module.IpAddress
                   + GetUrlToModuleType(module.Type ?? ModulesType.unknown)
                   + "status1=0"
                   + "&status20=80"
                   + "&time=7";
                }
                
                var request = new HttpRequestMessage(HttpMethod.Get, moduleUrl);
                using var scope = _serviceScopeFactory.CreateScope();
                var myScopedService = scope.ServiceProvider.GetService<IHttpClientFactory>();
                var client = myScopedService.CreateClient();
                client.Send(request);
            }
        }

        public async Task InvertModule(int id)
        {
            var module = _dbContext.GetModule(mod => mod.Id == id);
            if (module != null)
            {
                var moduleUrl = "http://" + module.IpAddress + GetUrlToModuleType(module.Type ?? ModulesType.unknown) + (module.Status ?? false ? "0" : "1");
                // Used throughout your application (i.e. app lifetime)
                //using var httpSocketHandler = new SocketsHttpHandler();
                using var httpClient = new HttpClient();
                await httpClient.GetAsync(moduleUrl);
            }
        }

        private string GetUrlToModuleType(ModulesType type)
        {
            return type switch
            {
                ModulesType.assistive => "/assistive?",
                ModulesType.relay => "/relay?switch=",
                ModulesType.rural => "/rural?switch=",
                _ => "/",
            };
        }

    }
}
