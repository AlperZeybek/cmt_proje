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
    public class TracksController : Controller
    {
        private readonly ConferenceDbContext _context;

        public TracksController(ConferenceDbContext context)
        {
            _context = context;
        }

        // /Tracks?conferenceId=1
        public async Task<IActionResult> Index(int conferenceId)
        {
            var conference = await _context.Conferences
                .FirstOrDefaultAsync(c => c.Id == conferenceId);

            if (conference == null)
                return NotFound();

            ViewBag.Conference = conference;

            var tracks = await _context.Tracks
                .Where(t => t.ConferenceId == conferenceId)
                .ToListAsync();

            return View(tracks);
        }

        // GET: /Tracks/Create?conferenceId=1
        public async Task<IActionResult> Create(int conferenceId)
        {
            var conference = await _context.Conferences.FindAsync(conferenceId);
            if (conference == null)
                return NotFound();

            ViewBag.Conference = conference;

            var model = new Track
            {
                ConferenceId = conferenceId,
                IsActive = true
            };

            return View(model);
        }

        // POST: /Tracks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Track track)
        {
            var conference = await _context.Conferences.FindAsync(track.ConferenceId);
            if (conference == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Conference = conference;
                return View(track);
            }

            track.CreatedAt = DateTime.Now;

            _context.Tracks.Add(track);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { conferenceId = track.ConferenceId });
        }

        // GET: /Tracks/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var track = await _context.Tracks
                .Include(t => t.Conference)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (track == null)
                return NotFound();

            ViewBag.Conference = track.Conference;

            return View(track);
        }

        // POST: /Tracks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Track track)
        {
            if (id != track.Id)
                return NotFound();

            var conference = await _context.Conferences.FindAsync(track.ConferenceId);
            if (conference == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Conference = conference;
                return View(track);
            }

            _context.Update(track);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { conferenceId = track.ConferenceId });
        }

        // GET: /Tracks/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var track = await _context.Tracks
                .Include(t => t.Conference)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (track == null)
                return NotFound();

            ViewBag.Conference = track.Conference;

            return View(track);
        }

        // POST: /Tracks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var track = await _context.Tracks.FindAsync(id);
            if (track == null)
                return NotFound();

            var conferenceId = track.ConferenceId;

            _context.Tracks.Remove(track);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { conferenceId });
        }
    }
}
