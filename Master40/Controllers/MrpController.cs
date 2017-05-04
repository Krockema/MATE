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
        private readonly IProcessMrp _processMrp;
        public MrpController(IProcessMrp processMrp)
        {
            _processMrp = processMrp;
        }
        public IActionResult Index()
        {
            if (_processMrp.Logger == null)
            {
                _processMrp.Logger = new List<LogMessage>() {
                    new LogMessage() { MessageType = MessageType.success, Message = "Nothing logged yet.", MessageNumber = 1 }
                };
            }
            return View(_processMrp.Logger);
        }

        [HttpGet("[Controller]/MrpProcessing/{id}")]
        public IActionResult MrpProcessing(int? id)
        {
            //TODO: hand over dynamic orderId
            _processMrp.Process(1);
            return RedirectToAction("Index");
        }
       
        public IActionResult Error()
        {
            return View();
        }
    }
}
