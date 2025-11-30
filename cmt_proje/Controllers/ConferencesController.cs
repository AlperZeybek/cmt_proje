using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cmt_proje.Core.Entities;
using cmt_proje.Infrastructure.Data;

namespace cmt_proje.Controllers
{
    [Authorize]
    public class ConferencesController : Controller
    {
        private readonly ConferenceDbContext _context;

        public ConferencesController(ConferenceDbContext context)
        {
            _context = context;
        }

        // GET: /Conferences
        public async Task<IActionResult> Index()
        {
            var conferences = await _context.Conferences
                .Include(c => c.CreatedByUser)
                .ToListAsync();

            return View(conferences);
        }

        // GET: /Conferences/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var conf = await _context.Conferences
                .Include(c => c.Tracks)
                .Include(c => c.Submissions)
                .Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conf == null)
                return NotFound();

            return View(conf);
        }

        // GET: /Conferences/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Conferences/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Conference conference)
        {
            if (!ModelState.IsValid)
            {
                return View(conference);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            conference.CreatedAt = DateTime.Now;
            conference.CreatedByUserId = userId;
            conference.IsActive = true;

            _context.Conferences.Add(conference);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Conferences/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var conf = await _context.Conferences.FindAsync(id);
            if (conf == null)
                return NotFound();

            return View(conf);
        }

        // POST: /Conferences/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Conference conference)
        {
            if (id != conference.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(conference);

            try
            {
                _context.Update(conference);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Conferences.Any(c => c.Id == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Conferences/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var conf = await _context.Conferences.FindAsync(id);
            if (conf == null)
                return NotFound();

            return View(conf);
        }

        // POST: /Conferences/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var conf = await _context.Conferences.FindAsync(id);
            if (conf != null)
            {
                _context.Conferences.Remove(conf);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
