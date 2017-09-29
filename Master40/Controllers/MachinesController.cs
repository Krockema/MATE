using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Master40.DB.Data.Context;
using Master40.DB.Models;

namespace Master40.Controllers
{
    public class MachinesController : Controller
    {
        private readonly MasterDBContext _context;

        public MachinesController(MasterDBContext context)
        {
            _context = context;    
        }

        // GET: Machines
        public async Task<IActionResult> Index()
        {
            var machines = _context.Machines.Include(m => m.MachineGroup);
            
            var mermaid = @"graph LR;
                p>Production Line]
                Finish((<img src='/images/Production/Dump-Truck.svg' width='20' ><br> Finish))
                Start((<img src='/images/Production/saw.svg' width='20'><img src='/images/Production/plate.svg' width='20'><br> Start))
                ";


            var machineGroups = _context.MachineGroups.OrderBy(x => x.Stage).ToList();
            for (int g = 0; g < machineGroups.Count; g++)
            {
                var start = (g == 0) ? "Start-->" : "\r\n";
                var machine = machines.Where(x => machineGroups[g].Name == x.MachineGroup.Name);
                var i = 1;
                var thisGroup = machineGroups[g].Name;
                var nextGroup = (g+1 < machineGroups.Count)? machineGroups[g+1].Name : "Finish";
                mermaid += start + thisGroup + "{ }";
                mermaid += "\r\n subgraph " + thisGroup;


                var end = "\r\n end ";
                foreach (var m in machine)
                {
                    var name = m.Name.Replace(" ", "_").ToString();
                    mermaid += "\r\n" + thisGroup + "-->" + name + "(<img src='" + machineGroups[g].ImageUrl + "' width='40'>)";
                    end += "\r\n" + name + "-->" + nextGroup;
                    i++;
                }
                mermaid = mermaid + end;
            }
           
            ViewData["Machines"] = mermaid;
            return View(await machines.ToListAsync());
        }

        // GET: Machines/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _context.Machines
                .Include(m => m.MachineGroup)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (machine == null)
            {
                return NotFound();
            }

            return View(machine);
        }

        // GET: Machines/Create
        public IActionResult Create()
        {
            ViewData["MachineGroupId"] = new SelectList(_context.MachineGroups, "Id", "Id");
            return View();
        }

        // POST: Machines/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Count,MachineGroupId,Capacity,Id")] Machine machine)
        {
            if (ModelState.IsValid)
            {
                _context.Add(machine);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["MachineGroupId"] = new SelectList(_context.MachineGroups, "Id", "Name", machine.MachineGroupId);
            return View(machine);
        }

        // GET: Machines/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _context.Machines.SingleOrDefaultAsync(m => m.Id == id);
            if (machine == null)
            {
                return NotFound();
            }
            ViewData["MachineGroupId"] = new SelectList(_context.MachineGroups, "Id", "Name", machine.MachineGroupId);
            return View(machine);
        }

        // POST: Machines/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Count,MachineGroupId,Capacity,Id")] Machine machine)
        {
            if (id != machine.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(machine);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MachineExists(machine.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            ViewData["MachineGroupId"] = new SelectList(_context.MachineGroups, "Id", "Name", machine.MachineGroupId);
            return View(machine);
        }

        // GET: Machines/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machine = await _context.Machines
                .Include(m => m.MachineGroup)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (machine == null)
            {
                return NotFound();
            }

            return View(machine);
        }

        // POST: Machines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var machine = await _context.Machines.SingleOrDefaultAsync(m => m.Id == id);
            _context.Machines.Remove(machine);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool MachineExists(int id)
        {
            return _context.Machines.Any(e => e.Id == id);
        }
    }
}
