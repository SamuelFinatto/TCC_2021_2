using DataCommunicator.Context;
using DataCommunicator.DTOs;
using DataCommunicator.Models;
using DataCommunicator.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataCommunicator.Services
{
    public class DbService
    {
        private readonly ILogger<DbService> _logger;
        private readonly CentraldbContext _context;
        private readonly Cape _cape;

        public DbService(CentraldbContext context, ILogger<DbService> logger)
        {
            _context = context;
            _logger = logger;
            byte[] bytes = Encoding.ASCII.GetBytes("D#&*EH*H&$R*$&R89$#d8943#HWF*&Uy9F");
            _cape = new Cape(bytes, 0);
        }

        public string GetSystemConfigSSID()
        {
            var systemConfig = _context.SystemConfig.FirstOrDefault();
            return systemConfig != null ? systemConfig.Ssid : "";
        }

        public string GetSystemConfigPassword()
        {
            var systemConfig = _context.SystemConfig.FirstOrDefault();
            if (systemConfig != null)
            {
                var encryptedPassword = Encoding.UTF8.GetString(_cape.Hash(Encoding.UTF8.GetBytes(systemConfig.Password)));
                return encryptedPassword;
            }

            return "";
        }

        public async Task<IEnumerable<Modules>> GetModulesAsync(CancellationToken ct = default)
        {
            return await (from con in _context.Modules
                   select new Modules
                   {
                       IpAddress = con.IpAddress,
                       MacAddress = con.MacAddress,
                       Status = con.Status,
                       Type = con.Type,
                       Id = con.Id,
                       Name = con.Name
                   }).ToListAsync(ct);
        }

        public Modules GetModule(Func<Modules, bool> predicate)
        {
            return _context.Modules.FirstOrDefault(predicate);
        }

        public SystemConfig GetSystemConfiguration()
        {
            var system = _context.SystemConfig.FirstOrDefault();
            return system ?? new SystemConfig();
        }

        public bool UpdateSystemConfiguration(SystemConfig config)
        {
            try
            {
                var systemConfig = _context.SystemConfig.FirstOrDefault();
                if (systemConfig != null)
                {
                    if (config.HostName != null)
                    {
                        systemConfig.HostName = config.HostName;
                    }

                    if (config.Ip != null)
                    {
                        systemConfig.Ip = config.Ip;
                    }

                    if (config.Ssid != null)
                    {
                        systemConfig.Ssid = config.Ssid;
                    }

                    if (config.Password != null)
                    {
                        systemConfig.Password = config.Password;
                    }

                    _context.SystemConfig.Update(systemConfig);
                    _context.SaveChanges();
                    return true;
                }
                else
                {
                    systemConfig = new SystemConfig()
                    {
                        HostName = config.HostName ?? "Não Definido",
                        Ip = config.Ip ?? "Não definido",
                        Ssid = config.Ssid ?? "Não definido",
                        Password = config.Password ?? "Não definido"
                    };

                    _context.SystemConfig.Add(config);
                    _context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error when update System configuration.", ex);
                return false;
            }
        }

        public bool UpdateModuleFromId(Modules module)
        {
            try
            {
                var moduleToUpdate = _context.Modules.FirstOrDefault(m => m.Id == module.Id);

                if (moduleToUpdate != null)
                {
                    moduleToUpdate.Name = module.Name;
                    _context.Modules.Update(moduleToUpdate);
                    _context.SaveChanges();
                    return true;
                }
                else
                {
                    _logger.LogWarning($"Can't find Module to update. id: {module.Id}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error when UpdateModuleFromId", ex);
                return false;
            }
        }

        public async Task<bool> AddNewModuleAsync(ModulesDTO module, CancellationToken ct = default)
        {
            try
            {
                var duplicateModule = _context.Modules.FirstOrDefault(m => m.MacAddress == module.MacAddress);

                if (duplicateModule != null)
                {
                    _logger.LogInformation($"Found module {module.MacAddress} on database. updating it");

                    duplicateModule.IpAddress = module.IpAddress;
                    duplicateModule.Status = module.Status;
                    duplicateModule.Type = module.Type;
                    duplicateModule.Name = "test name";
                    _context.Modules.Update(duplicateModule);
                }
                else
                {
                    _logger.LogInformation($"Creating new module");

                    _context.Modules.Add(new Modules()
                    {
                        IpAddress = module.IpAddress,
                        MacAddress = module.MacAddress,
                        Status = module.Status,
                        Type = module.Type,
                        Name = "test name"
                    });
                }
                
                await _context.SaveChangesAsync(ct);

                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError("Error when AddNewModuleAsync", ex);
                return false;
            }
        }

        public async Task<bool> RemoveAllModulesAsync(CancellationToken ct = default)
        {
            var modules = _context.Modules.Select(x => x);
            _context.Modules.RemoveRange(modules);
            await _context.SaveChangesAsync(ct);
            _logger.LogWarning("Removed all Modules from database as requested");
            return true;
        }

        public async Task<bool> RemoveModuleAsync(int id, CancellationToken ct = default)
        {
            var module = _context.Modules.FirstOrDefault(x => x.Id == id);
            if (module != null)
            {
                _context.Modules.Remove(module);
                await _context.SaveChangesAsync(ct);
                return true;
            }
            else
            {
                _logger.LogWarning($"Module with ID ${id} not found on database");
                return false;
            }
        }
    }
}
