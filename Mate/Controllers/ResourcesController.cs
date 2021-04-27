using System;
using System.Linq;
using System.Threading.Tasks;
using Mate.DataCore.Data.Context;
using Mate.DataCore.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Mate.Controllers
{
    public class ResourcesController : Controller
    {
        private readonly MateDb _context;

        public ResourcesController(MateDb context)
        {
            _context = context;    
        }

        // GET: Resources
        public async Task<IActionResult> Index()
        {
            var resources = _context.Resources.Include(navigationPropertyPath: m => m.ResourceSetups)
                                                .ThenInclude(navigationPropertyPath: m => m.ResourceCapabilityProvider)
                                                .Where(x => x.IsPhysical);
            var oddRows = Math.Round(resources.Count() / 3.0 + 0.49 , MidpointRounding.ToPositiveInfinity);
            var evenRows = oddRows - 1;
            var width =  99 / oddRows;
            var mermaid = @"<style>
                                        #hexGrid {
                                            padding-bottom: " + (width / 4).ToString().Replace(",", ".") + @"%
                                        }
                                        .hex {
                                            width: "+ width.ToString().Replace(",",".") + @"%; /* = 100 / 5 */
                                            
                                        }
                                        .hex:nth-child(" + (evenRows + oddRows) + @"n+" + (oddRows +1) + @") { /* first hexagon of even rows */
                                            margin-left: " + (width / 2).ToString().Replace(",", ".") + @"%; /* = width of .hex / 2  to indent even rows */
                                        }
                                        .hexLink {
                                            background-size: 70%;
                                            background-position: center;
                                            background-repeat: no-repeat;
                                        }
                                        .imgDrill {
                                            background-image: url(/images/Production/drill.svg);
                                            background-color: rgb(108, 117, 125, 0.2);
                                           
                                        }
                                        .imgSaw {
                                            background-image: url(/images/Production/saw.svg);
                                            background-color: rgb(108, 117, 125, 0.4);
                                           
                                        }
                                        .imgAssembly {
                                            background-image: url(/images/Production/assemblys.svg);
                                            background-color: rgb(108, 117, 125, 0.6);
                                           
                                        }
                                        .imgOperator {
                                            background-image: url(/images/Production/operator.svg);
                                            background-color: rgb(108, 117, 125, 0.3);
                                           
                                        }
                                        .imgWorker {
                                            background-image: url(/images/Production/worker.svg);
                                            background-color: rgb(108, 117, 125, 0.5);
                                           
                                        }
                            </style>";
                                                        // = @"graph LR;
                // p>Production Line]
                // Finish((<img src='/images/Production/Dump-Truck.svg' width='20' ><br> Finish))
                // Start((<img src='/images/Production/saw.svg' width='20'><img src='/images/Production/plate.svg' width='20'><br> Start))
                // ";

            foreach (var resource in resources)
            {
                var name = resource.Name.Contains("Operator") ? "imgOperator"
                            : resource.Name.Contains("Assembl") ? "imgAssembly"
                                : resource.Name.Contains("Cut") ?  "imgSaw"
                                    : resource.Name.Contains("Drill") ? "imgDrill"
                                        : resource.Name.Contains("Worker") ? "imgWorker"
                                            : "imgWaterJet";
                    ;

                mermaid += @"<li class='hex'>
                    <div class='hexIn'>
                    <a class='hexLink " + name + @"' href='#'>
                    <h1>" + resource.Name.Replace("Resource ", "") + @"</h1>
                    <p> with " + resource.ResourceSetups.Count() +
                    @" possible setups</p>
                    </a>
                    </div>
                    </li>";
            }
            ViewData[index: "Resources"] = mermaid;
            return View(model: await resources.ToListAsync());
        }

        // GET: Resources/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _context.Resources
                .Include(navigationPropertyPath: m => m.ResourceSetups)
                .SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (machine == null)
            {
                return NotFound();
            }

            return View(model: machine);
        }

        // GET: Resources/Create
        public IActionResult Create()
        {
            ViewData[index: "MachineGroupId"] = new SelectList(items: _context.ResourceCapabilities, dataValueField: "Id", dataTextField: "Name");
            return View();
        }

        // POST: Resources/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Count,MachineGroupId,Capacity,Id")] M_Resource machine)
        {
            if (ModelState.IsValid)
            {
                _context.Add(entity: machine);
                await _context.SaveChangesAsync();
                return RedirectToAction(actionName: "Index");
            }
            ViewData[index: "MachineGroupId"] = new SelectList(items: _context.ResourceCapabilities, dataValueField: "Id", dataTextField: "Name", selectedValue: machine.Name);
            return View(model: machine);
        }

        // GET: Resources/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _context.Resources.SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (machine == null)
            {
                return NotFound();
            }
            ViewData[index: "MachineGroupId"] = new SelectList(items: _context.ResourceCapabilities, dataValueField: "Id", dataTextField: "Name", selectedValue: machine.Name);
            return View(model: machine);
        }

        // POST: Resources/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Count,MachineGroupId,Capacity,Id")] M_Resource machine)
        {
            if (id != machine.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(entity: machine);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MachineExists(id: machine.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(actionName: "Index");
            }
            ViewData[index: "MachineGroupId"] = new SelectList(items: _context.ResourceCapabilities, dataValueField: "Id", dataTextField: "Name", selectedValue: machine.Name);
            return View(model: machine);
        }

        // GET: Resources/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _context.Resources
                .Include(navigationPropertyPath: m => m.ResourceSetups)
                .SingleOrDefaultAsync(predicate: m => m.Id == id);
            if (machine == null)
            {
                return NotFound();
            }

            return View(model: machine);
        }

        // POST: Resources/Delete/5
        [HttpPost, ActionName(name: "Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var machine = await _context.Resources.SingleOrDefaultAsync(predicate: m => m.Id == id);
            _context.Resources.Remove(entity: machine);
            await _context.SaveChangesAsync();
            return RedirectToAction(actionName: "Index");
        }

        private bool MachineExists(int id)
        {
            return _context.Resources.Any(predicate: e => e.Id == id);
        }


        [HttpGet(template: "[Controller]/Setup/{setup}")]
        public async Task<IActionResult> Setup(string setup)
        {

            _context.Resources.RemoveRange(entities: _context.Resources.Where(predicate: x => x.Capacity == 0).ToList());
            if (setup == "Large")
            {
                var Resources = new M_Resource[] {
                    new M_Resource{Capacity=0, Name="Saw 3", IsPhysical = true},
                    new M_Resource{Capacity=0, Name="Saw 4", IsPhysical = true},
                    new M_Resource{Capacity=0, Name="Saw 5", IsPhysical = true},
                    new M_Resource{Capacity=0, Name="Saw 6", IsPhysical = true},
                    new M_Resource{Capacity=0, Name="Drill 2", IsPhysical = true},
                    new M_Resource{Capacity=0, Name="Drill 3", IsPhysical = true},
                    new M_Resource{Capacity=0, Name="AssemblyUnit 3", IsPhysical=true},
                    new M_Resource{Capacity=0, Name="AssemblyUnit 4", IsPhysical=true},
                    new M_Resource{Capacity=0, Name="AssemblyUnit 5", IsPhysical=true},
                    new M_Resource{Capacity=0, Name="AssemblyUnit 6", IsPhysical=true}
                };
                _context.Resources.AddRange(entities: Resources);

            }

            await _context.SaveChangesAsync();
            return RedirectToAction(actionName: "Index");
        }
    }
}
