using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master40.BusinessLogic.MRP;
using Master40.Data;
using Master40.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Master40.Controllers
{
    public class MrpController : Controller
    {
        private readonly IDemandForecast _demandForecast;
        public MrpController(IDemandForecast demandForecast)
        {
            _demandForecast = demandForecast;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MrpProcessing()
        {
            ViewData["Message"] = "Your application description page.";
            //TODO: hand over dynamic orderId
            IProcessMrp processMrp = new ProcessMrp(_demandForecast);
            processMrp.Process(1);
            return RedirectToAction("Index");
        }
       
        public IActionResult Error()
        {
            return View();
        }
    }
}
