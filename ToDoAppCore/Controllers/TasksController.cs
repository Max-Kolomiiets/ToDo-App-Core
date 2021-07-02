using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ToDoAppCore.Data;
using ToDoAppCore.Helpers;
using ToDoAppCore.Models;
using static ToDoAppCore.Helpers.Helper;

namespace ToDoAppCore.Controllers
{
    public class TasksController : Controller
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Tasks
        public async Task<IActionResult> Index()
        {
            var tasks = await _context.Tasks.ToListAsync();

            foreach (var task in tasks)
            {
                if (task.Status != true)
                {
                    var res = DateTime.Compare(task.DateEnd, DateTime.Now);
                    if (res <= 0)
                        task.Status = null;
                    else
                        task.Status = false;
                    _context.Tasks.Update(task);
                    await _context.SaveChangesAsync();
                }
            }

            return View(tasks);
        }

        // GET: Tasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskModel = await _context.Tasks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskModel == null)
            {
                return NotFound();
            }

            return View(taskModel);
        }

        // GET: Tasks/Create
        [NoDirectAccess]
        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            if (id == 0)
                return View(new TaskModel());

            var taskModel = await _context.Tasks.FindAsync(id);
            if (taskModel == null)
            {
                return NotFound();
            }
            return View(taskModel);
        }

        // POST: Tasks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int id, [Bind("Id,Title,Description,DateIssue,DateEnd,Status")] TaskModel taskModel)
        {
            if (ModelState.IsValid)
            {
                //Insert
                if (id == 0)
                {
                    taskModel.Status = false;
                    _context.Add(taskModel);
                    await _context.SaveChangesAsync();
                }
                //Update
                else
                {
                    try
                    {
                        _context.Update(taskModel);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!TaskModelExists(taskModel.Id))
                        { return NotFound(); }
                        else
                        { throw; }
                    }
                }
                return Json(new { isValid = true, html = Helper.RenderRazorViewToString(this, "_ViewAll", _context.Tasks.ToList()) });
            }
            return Json(new { isValid = false, html = Helper.RenderRazorViewToString(this, "AddOrEdit", taskModel) });
        }

        // GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskModel = await _context.Tasks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskModel == null)
            {
                return NotFound();
            }

            return View(taskModel);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taskModel = await _context.Tasks.FindAsync(id);
            _context.Tasks.Remove(taskModel);
            await _context.SaveChangesAsync();
            return Json(new { html = Helper.RenderRazorViewToString(this, "_ViewAll", _context.Tasks.ToList()) });
        }

        [HttpGet]
        [Route("{Controller}/{Action}/{id}/{status}")]
        public IActionResult ChangeStatus(int? id, bool? status)
        {
            var task = _context.Tasks.FirstOrDefault(p => p.Id == id);
            if (task != null)
            {
                task.Status = status;
                _context.Tasks.Update(task);
                _context.SaveChanges();
            }
            return Json(new { html = Helper.RenderRazorViewToString(this, "_ViewAll", _context.Tasks.ToList()) });
        }

        private bool TaskModelExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}
