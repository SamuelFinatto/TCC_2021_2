using DataCommunicator.Models;
using DataCommunicator.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Central.Controllers
{
    public class WiFiController : Controller
    {
        private readonly ILogger<WiFiController> _logger;
        private readonly DbService _dbContext;

        public SystemConfig SystemInfo { get; set; }

        public WiFiController(ILogger<WiFiController> logger, DbService dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        public IActionResult Index()
        {
            SystemInfo = _dbContext.GetSystemConfiguration();
            return View(SystemInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(SystemConfig std)
        {
            _dbContext.UpdateSystemConfiguration(std);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public string SSID()
        {
            var ssid = _dbContext.GetSystemConfigSSID();
            return ssid;
        }

        [HttpGet]
        public string Password()
        {
            var password = _dbContext.GetSystemConfigPassword();
            return password;
        }
    }
}
