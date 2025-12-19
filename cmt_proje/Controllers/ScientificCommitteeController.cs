using System;
using System.Linq;
using System.Threading.Tasks;
using cmt_proje.Core.Constants;
using cmt_proje.Core.Entities;
using cmt_proje.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cmt_proje.Controllers
{
    public class ScientificCommitteeController : Controller
    {
        private readonly ConferenceDbContext _context;

        public ScientificCommitteeController(ConferenceDbContext context)
        {
            _context = context;
        }

        // GET: /ScientificCommittee/Index (Public/Portal)
        [AllowAnonymous]
        public async Task<IActionResult> Index(int? conferenceId)
        {
            if (conferenceId == null)
            {
                return NotFound();
            }

            // Set Active Conference ID for Layout
            ViewBag.ActiveConferenceId = conferenceId;

            var conference = await _context.Conferences.FindAsync(conferenceId);
            ViewBag.ActiveConferenceAcronym = conference?.Acronym;
            ViewBag.Conference = conference; // Add Conference object to ViewBag

            var members = await _context.ScientificCommitteeMembers
                .Where(m => m.IsActive && m.ConferenceId == conferenceId)
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.FullName)
                .ToListAsync();


            ViewBag.ConferenceName = conference?.Name ?? "Conference";
            ViewBag.ConferenceYear = conference?.StartDate.Year ?? DateTime.Now.Year;

            return View(members);
        }

        // GET: /ScientificCommittee/Details/{id} (Public - gelecekte kullanılacak)
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.ScientificCommitteeMembers
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // ========== ADMIN ACTIONS (Chair only) ==========

        // GET: /ScientificCommittee/Admin
        [Authorize(Roles = AppRoles.Chair)]
        public async Task<IActionResult> Admin()
        {
            var members = await _context.ScientificCommitteeMembers
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.FullName)
                .ToListAsync();

            return View(members);
        }

        // GET: /ScientificCommittee/Create
        [Authorize(Roles = AppRoles.Chair)]
        public async Task<IActionResult> Create()
        {
            ViewBag.Conferences = await _context.Conferences
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View();
        }

        // POST: /ScientificCommittee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Chair)]
        public async Task<IActionResult> Create([Bind("FullName,Affiliation,Country,PhotoUrl,ShortBio,WebSiteUrl,IsActive,DisplayOrder,ConferenceId")] ScientificCommitteeMember member)
        {
            if (ModelState.IsValid)
            {
                member.CreatedAt = DateTime.UtcNow;
                _context.Add(member);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Scientific Committee member has been added successfully.";
                return RedirectToAction(nameof(Admin));
            }
            
            ViewBag.Conferences = await _context.Conferences.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return View(member);
        }

        // GET: /ScientificCommittee/Edit/{id}
        [Authorize(Roles = AppRoles.Chair)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.ScientificCommitteeMembers.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            ViewBag.Conferences = await _context.Conferences.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return View(member);
        }

        // POST: /ScientificCommittee/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Chair)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Affiliation,Country,PhotoUrl,ShortBio,WebSiteUrl,IsActive,DisplayOrder,CreatedAt,ConferenceId")] ScientificCommitteeMember member)
        {
            if (id != member.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(member);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Scientific Committee member has been updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScientificCommitteeMemberExists(member.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Admin));
            }
            ViewBag.Conferences = await _context.Conferences.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return View(member);
        }

        // GET: /ScientificCommittee/Delete/{id}
        [Authorize(Roles = AppRoles.Chair)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.ScientificCommitteeMembers
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // POST: /ScientificCommittee/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Chair)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var member = await _context.ScientificCommitteeMembers.FindAsync(id);
            if (member != null)
            {
                _context.ScientificCommitteeMembers.Remove(member);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Scientific Committee member has been deleted successfully.";
            }

            return RedirectToAction(nameof(Admin));
        }

        private bool ScientificCommitteeMemberExists(int id)
        {
            return _context.ScientificCommitteeMembers.Any(e => e.Id == id);
        }
    }
}

