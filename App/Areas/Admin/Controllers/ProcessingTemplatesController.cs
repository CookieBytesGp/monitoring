 using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.Data;
using App.Models;

namespace App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class ProcessingTemplatesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProcessingTemplatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/ProcessingTemplates
        public async Task<IActionResult> Index()
        {
            return View(await _context.ProcessingTemplates.ToListAsync());
        }

        // GET: Admin/ProcessingTemplates/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/ProcessingTemplates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProcessingTemplate template)
        {
            if (ModelState.IsValid)
            {
                _context.Add(template);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(template);
        }

        // GET: Admin/ProcessingTemplates/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var template = await _context.ProcessingTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }
            return View(template);
        }

        // POST: Admin/ProcessingTemplates/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProcessingTemplate template)
        {
            if (id != template.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(template);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TemplateExists(template.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(template);
        }

        // POST: Admin/ProcessingTemplates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var template = await _context.ProcessingTemplates.FindAsync(id);
            if (template != null)
            {
                _context.ProcessingTemplates.Remove(template);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/ProcessingTemplates/GetTemplate/5
        [HttpGet]
        public async Task<IActionResult> GetTemplate(int id)
        {
            var template = await _context.ProcessingTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }
            return Json(template);
        }

        private bool TemplateExists(int id)
        {
            return _context.ProcessingTemplates.Any(e => e.Id == id);
        }
    }
}