using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.Controllers
{
    public class ResourcesController : Controller
    {
        private readonly MasterDBContext _context;

        public ResourcesController(MasterDBContext context)
        {
            _context = context;    
        }

        // GET: Resources
        public async Task<IActionResult> Index()
        {
            var Resources = _context.Resources.Include(navigationPropertyPath: m => m.ResourceSkills);
            
            var mermaid = @"graph LR;
                p>Production Line]
                Finish((<img src='/images/Production/Dump-Truck.svg' width='20' ><br> Finish))
                Start((<img src='/images/Production/saw.svg' width='20'><img src='/images/Production/plate.svg' width='20'><br> Start))
                ";


            var machineGroups = _context.ResourceSkills.ToList();
            for (int g = 0; g < machineGroups.Count; g++)
            {
                var start = (g == 0) ? "Start-->" : "\r\n";
                var machine = Resources.Where(predicate: x => machineGroups[g].Name == x.ResourceSkills.SingleOrDefault().Name);
                var i = 1;
                var thisGroup = machineGroups[index: g].Name;
                var nextGroup = (g+1 < machineGroups.Count)? machineGroups[index: g+1].Name : "Finish";
                mermaid += start + thisGroup + "{ }";
                mermaid += "\r\n subgraph " + thisGroup;


                var end = "\r\n end ";
                foreach (var m in machine)
                {
                    var name = m.Name.Replace(oldValue: " ", newValue: "_").ToString();
                    mermaid += "\r\n" + thisGroup + "-->" + name + "(<img src='" + machineGroups[index: g] + "' width='40' height='40'>)";
                    end += "\r\n" + name + "-->" + nextGroup;
                    i++;
                }
                mermaid = mermaid + end;
            }
           
            ViewData[index: "Resources"] = mermaid;
            return View(model: await Resources.ToListAsync());
        }

        // GET: Resources/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _context.Resources
                .Include(navigationPropertyPath: m => m.ResourceSkills)
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
            ViewData[index: "MachineGroupId"] = new SelectList(items: _context.ResourceSkills, dataValueField: "Id", dataTextField: "Name");
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
            ViewData[index: "MachineGroupId"] = new SelectList(items: _context.ResourceSkills, dataValueField: "Id", dataTextField: "Name", selectedValue: machine.Name);
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
            ViewData[index: "MachineGroupId"] = new SelectList(items: _context.ResourceSkills, dataValueField: "Id", dataTextField: "Name", selectedValue: machine.Name);
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
            ViewData[index: "MachineGroupId"] = new SelectList(items: _context.ResourceSkills, dataValueField: "Id", dataTextField: "Name", selectedValue: machine.Name);
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
                .Include(navigationPropertyPath: m => m.ResourceSkills)
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
                    new M_Resource{Capacity=0, Name="Saw 3", Count = 1},
                    new M_Resource{Capacity=0, Name="Saw 4", Count = 1},
                    new M_Resource{Capacity=0, Name="Saw 5", Count = 1},
                    new M_Resource{Capacity=0, Name="Saw 6", Count = 1},
                    new M_Resource{Capacity=0, Name="Drill 2", Count = 1},
                    new M_Resource{Capacity=0, Name="Drill 3", Count = 1},
                    new M_Resource{Capacity=0, Name="AssemblyUnit 3", Count=1},
                    new M_Resource{Capacity=0, Name="AssemblyUnit 4", Count=1},
                    new M_Resource{Capacity=0, Name="AssemblyUnit 5", Count=1},
                    new M_Resource{Capacity=0, Name="AssemblyUnit 6", Count=1}
                };
                _context.Resources.AddRange(entities: Resources);

            }

            await _context.SaveChangesAsync();
            return RedirectToAction(actionName: "Index");
        }
    }
}
