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
        private readonly MasterDBContext _context;
        private readonly IProcessMrp _processMrp;
        public MrpController(MasterDBContext context, IProcessMrp processMrp)
        {
            _context = context;
            _processMrp = processMrp;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MrpProcessing()
        {
            ViewData["Message"] = "Your application description page.";
            _processMrp.Process();
            return RedirectToAction("Index");
        }
       
        public IActionResult Error()
        {
            return View();
        }
    }
}
