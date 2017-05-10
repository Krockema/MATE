using System.Collections.Generic;
using Master40.BusinessLogic.MRP;
using Master40.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
        public async Task<IActionResult> MrpProcessing(int id)
        {
            //call to process MRP I and II
            await _processMrp.Process(id);

            await Task.Yield();

            return View("Index", _processMrp.Logger);
        }
       
        public IActionResult Error()
        {
            return View();
        }
    }
}
