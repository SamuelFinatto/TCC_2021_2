using Central.Models;
using DataCommunicator.Context;
using DataCommunicator.DTOs;
using DataCommunicator.Models;
using DataCommunicator.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Central.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DbService _dbContext;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly AutomationService _autoService;

        public HomeController(ILogger<HomeController> logger, DbService dbContext, AutomationService automationService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _autoService = automationService;
        }

        [BindProperty]
        public List<Modules> ModulesData { get; set; }

        public async Task<IActionResult> Index(CancellationToken ct = default)
        {
            //Response.Headers.Add("Refresh", "5");
            ModulesData = new List<Modules>(await _dbContext.GetModulesAsync());
            return View(ModulesData);
        }

        public async Task<IActionResult> Privacy(CancellationToken ct = default)
        {
            _logger.LogInformation("Creating new module into DB");
            await _dbContext.AddNewModuleAsync(new ModulesDTO
            {
                IpAddress = "10.10.20.4",
                MacAddress = "90dj39kdj",
                Status = true,
                Type = ModulesType.assistive
            }, ct);
            return View();
        }

        public async Task<IActionResult> Edit(int Id)
        {
            var list = await _dbContext.GetModulesAsync();
            var std = list.FirstOrDefault(s => s.Id == Id);
            return View(std);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Modules std)
        {
            _dbContext.UpdateModuleFromId(std);
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Delete(int Id)
        {
            await _dbContext.RemoveModuleAsync(Id);
            return Redirect(Request.Headers["Referer"].ToString());
        }

        public IActionResult Invert(int Id)
        {
            var module = _dbContext.GetModule(mod => mod.Id == Id);
            if (module != null)
            {
                _autoService.InvertModule(Id);
                module.Status = !module.Status;
                _dbContext.UpdateModuleFromId(module);
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpGet("/Modules")]
        public async Task<IActionResult> GetModules(CancellationToken ct = default)
        {
            _logger.LogInformation("Modules GET");
            return Json(await _dbContext.GetModulesAsync(ct));
        }

        [HttpGet("/Modules/Type/{type}")]
        public IActionResult GetModulesType(int type, CancellationToken ct = default)
        {
            _logger.LogInformation("Modules GET");
            return Json(_dbContext.GetModule(t => t.Type == (ModulesType)type));
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
