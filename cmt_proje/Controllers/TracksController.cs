using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cmt_proje.Core.Entities;
using cmt_proje.Infrastructure.Data;
using cmt_proje.Core.Constants;
using cmt_proje.Models;

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

        /// <summary>
        /// Track Yönetimi için Konferans Seçimi Sayfası
        /// </summary>
        public async Task<IActionResult> SelectConference()
        {
            var conferences = await _context.Conferences
                .Include(c => c.CreatedByUser)
                .Include(c => c.Tracks)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var viewModel = conferences.Select(c => new ConferenceSelectionViewModel
            {
                Id = c.Id,
                Name = c.Name ?? string.Empty,
                Acronym = c.Acronym,
                Description = c.Description,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                IsActive = c.IsActive,
                TrackCount = c.Tracks?.Count ?? 0,
                CreatedByUserEmail = c.CreatedByUser?.Email
            }).ToList();

            return View(viewModel);
        }

        /// <summary>
        /// Konferans Track Listesi Sayfası
        /// </summary>
        public async Task<IActionResult> Index(int conferenceId)
        {
            var conference = await _context.Conferences
                .Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(c => c.Id == conferenceId);

            if (conference == null)
                return NotFound();

            ViewBag.Conference = conference;
            ViewBag.ActiveConferenceId = conferenceId;
            ViewBag.ActiveConferenceAcronym = conference.Acronym;

            var tracks = await _context.Tracks
                .Where(t => t.ConferenceId == conferenceId)
                .Include(t => t.Submissions)
                .ToListAsync();

            return View(tracks);
        }

        // GET: /Tracks/Create?conferenceId=1
        // Yetki: Chair + Author
        [Authorize(Roles = AppRoles.Chair)]
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
        // Yetki: Chair + Author
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Chair)]
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
        // Yetki: Chair + Author
        [Authorize(Roles = AppRoles.Chair)]
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
        // Yetki: Chair + Author
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Chair)]
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
        // Yetki: Chair + Author
        [Authorize(Roles = AppRoles.Chair)]
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
        // Yetki: Chair + Author
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Chair)]
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
