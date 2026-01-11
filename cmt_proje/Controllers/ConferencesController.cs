using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cmt_proje.Core.Entities;
using cmt_proje.Infrastructure.Data;
using cmt_proje.Core.Constants;

namespace cmt_proje.Controllers
{
    [AllowAnonymous]
    public class ConferencesController : Controller
    {
        private readonly ConferenceDbContext _context;

        public ConferencesController(ConferenceDbContext context)
        {
            _context = context;
        }

        // Helper method to generate slug from acronym
        private string GenerateSlug(string? acronym)
        {
            if (string.IsNullOrWhiteSpace(acronym))
                return string.Empty;
            
            // Convert to lowercase and replace spaces/dashes with nothing
            var slug = acronym.ToLowerInvariant()
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("_", "");
            
            // Remove any non-alphanumeric characters
            slug = Regex.Replace(slug, @"[^a-z0-9]", "");
            
            return slug;
        }

        // GET: /Conferences
        public async Task<IActionResult> Index()
        {
            var conferences = await _context.Conferences
                .Include(c => c.CreatedByUser)
                .ToListAsync();

            return View(conferences);
        }

        // GET: /Conferences/Details/5 (Legacy - redirects to slug-based route)
        public async Task<IActionResult> Details(int id)
        {
            var conf = await _context.Conferences
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conf == null)
                return NotFound();

            // Generate slug if not exists
            if (string.IsNullOrWhiteSpace(conf.Slug))
            {
                conf.Slug = GenerateSlug(conf.Acronym);
                conf.Slug = await EnsureUniqueSlug(conf.Slug, conf.Id);
                
                // Update slug in database
                var confToUpdate = await _context.Conferences.FindAsync(id);
                if (confToUpdate != null)
                {
                    confToUpdate.Slug = conf.Slug;
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("ConferenceHome", new { conferenceSlug = conf.Slug });
        }

        // GET: /{conferenceSlug}
        public async Task<IActionResult> ConferenceHome(string conferenceSlug)
        {
            var conf = await _context.Conferences
                .Include(c => c.Tracks)
                .Include(c => c.Submissions)
                .Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(c => c.Slug == conferenceSlug);

            if (conf == null)
                return NotFound();

            // Set Active Conference ID for Layout Navbar Switching
            ViewBag.ActiveConferenceId = conf.Id;
            ViewBag.ActiveConferenceAcronym = conf.Acronym;
            ViewBag.ActiveConferenceSlug = conf.Slug ?? GenerateSlug(conf.Acronym);

            return View("Details", conf);
        }

        // GET: /{conferenceSlug}/{sectionSlug}
        public async Task<IActionResult> ConferenceSection(string conferenceSlug, string sectionSlug)
        {
            var conf = await _context.Conferences
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Slug == conferenceSlug);

            if (conf == null)
                return NotFound();

            // Set Active Conference ID for Layout Navbar Switching
            ViewBag.ActiveConferenceId = conf.Id;
            ViewBag.ActiveConferenceAcronym = conf.Acronym;
            ViewBag.ActiveConferenceSlug = conf.Slug;

            // Map section slugs to controller actions
            switch (sectionSlug.ToLowerInvariant())
            {
                case "about":
                    return RedirectToAction("Index", "About", new { conferenceId = conf.Id });
                
                case "committees":
                case "scientific-committee":
                case "committee":
                    return RedirectToAction("Index", "ScientificCommittee", new { conferenceId = conf.Id });
                
                case "tracks":
                case "track":
                    return RedirectToAction("Index", "Tracks", new { conferenceId = conf.Id });
                
                case "submissions":
                case "my-submissions":
                    return RedirectToAction("Index", "Submissions", new { conferenceId = conf.Id });
                
                case "registration":
                case "call-for-papers":
                case "callforpapers":
                    // These sections don't have dedicated controllers yet, redirect to conference home
                    return RedirectToAction("ConferenceHome", new { conferenceSlug = conferenceSlug });
                
                default:
                    // Unknown section, redirect to conference home
                    return RedirectToAction("ConferenceHome", new { conferenceSlug = conferenceSlug });
            }
        }

        // Helper method to ensure unique slug
        private async Task<string> EnsureUniqueSlug(string baseSlug, int excludeId)
        {
            var slug = baseSlug;
            var counter = 1;
            
            while (await _context.Conferences.AnyAsync(c => c.Slug == slug && c.Id != excludeId))
            {
                slug = $"{baseSlug}{counter}";
                counter++;
            }
            
            return slug;
        }

        // GET: /Conferences/Create
        // Yetki: Chair
        [Authorize(Roles = AppRoles.Chair)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Conferences/Create
        // Yetki: Chair
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Chair)]
        public async Task<IActionResult> Create(Conference conference)
        {
            // Tarih validasyonları
            var today = DateTime.Today;
            
            // 1. StartDate bugünden önce olamaz
            if (conference.StartDate < today)
            {
                ModelState.AddModelError(nameof(conference.StartDate), "Konferans başlangıç tarihi bugünden önce olamaz.");
            }

            // 2. EndDate, StartDate'den sonra olmalı
            if (conference.EndDate < conference.StartDate)
            {
                ModelState.AddModelError(nameof(conference.EndDate), "Bitiş tarihi başlangıç tarihinden önce olamaz.");
            }

            // 3. Başlangıç ve bitiş tarihleri arasında maksimum 3 ay
            var maxEndDate = conference.StartDate.AddMonths(3);
            if (conference.EndDate > maxEndDate)
            {
                ModelState.AddModelError(nameof(conference.EndDate), "Konferans süresi maksimum 3 ay olabilir.");
            }

            // 4. SubmissionDeadline, StartDate'den en az 1 hafta önce olmalı
            var minDeadline = conference.StartDate.AddDays(-7);
            if (conference.SubmissionDeadline >= conference.StartDate || conference.SubmissionDeadline > minDeadline)
            {
                ModelState.AddModelError(nameof(conference.SubmissionDeadline), "Submission Deadline, konferans başlangıcından en az 1 hafta önce olmalıdır.");
            }

            // 5. SubmissionDeadline bugünden önce olamaz
            if (conference.SubmissionDeadline < today)
            {
                ModelState.AddModelError(nameof(conference.SubmissionDeadline), "Submission Deadline bugünden önce olamaz.");
            }

            if (!ModelState.IsValid)
            {
                return View(conference);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            conference.CreatedAt = DateTime.Now;
            conference.CreatedByUserId = userId;
            conference.IsActive = true;
            
            // Generate slug from acronym if not provided
            if (string.IsNullOrWhiteSpace(conference.Slug))
            {
                conference.Slug = GenerateSlug(conference.Acronym);
            }

            _context.Conferences.Add(conference);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Conferences/Edit/5
        // Yetki: Chair
        [Authorize(Roles = AppRoles.Chair)]
        public async Task<IActionResult> Edit(int id)
        {
            var conf = await _context.Conferences.FindAsync(id);
            if (conf == null)
                return NotFound();

            return View(conf);
        }

        // POST: /Conferences/Edit/5
        // Yetki: Chair
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Chair)]
        public async Task<IActionResult> Edit(int id, Conference conference)
        {
            if (id != conference.Id)
                return NotFound();

            // Tarih validasyonları
            var today = DateTime.Today;
            
            // 1. StartDate bugünden önce olamaz
            if (conference.StartDate < today)
            {
                ModelState.AddModelError(nameof(conference.StartDate), "Konferans başlangıç tarihi bugünden önce olamaz.");
            }

            // 2. EndDate, StartDate'den sonra olmalı
            if (conference.EndDate < conference.StartDate)
            {
                ModelState.AddModelError(nameof(conference.EndDate), "Bitiş tarihi başlangıç tarihinden önce olamaz.");
            }

            // 3. Başlangıç ve bitiş tarihleri arasında maksimum 3 ay
            var maxEndDate = conference.StartDate.AddMonths(3);
            if (conference.EndDate > maxEndDate)
            {
                ModelState.AddModelError(nameof(conference.EndDate), "Konferans süresi maksimum 3 ay olabilir.");
            }

            // 4. SubmissionDeadline, StartDate'den en az 1 hafta önce olmalı
            var minDeadline = conference.StartDate.AddDays(-7);
            if (conference.SubmissionDeadline >= conference.StartDate || conference.SubmissionDeadline > minDeadline)
            {
                ModelState.AddModelError(nameof(conference.SubmissionDeadline), "Submission Deadline, konferans başlangıcından en az 1 hafta önce olmalıdır.");
            }

            // 5. SubmissionDeadline bugünden önce olamaz
            if (conference.SubmissionDeadline < today)
            {
                ModelState.AddModelError(nameof(conference.SubmissionDeadline), "Submission Deadline bugünden önce olamaz.");
            }

            if (!ModelState.IsValid)
                return View(conference);

            // Generate slug from acronym if not provided
            if (string.IsNullOrWhiteSpace(conference.Slug))
            {
                conference.Slug = GenerateSlug(conference.Acronym);
            }

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
        // Yetki: Chair
        [Authorize(Roles = AppRoles.Chair)]
        public async Task<IActionResult> Delete(int id)
        {
            var conf = await _context.Conferences.FindAsync(id);
            if (conf == null)
                return NotFound();

            return View(conf);
        }

        // POST: /Conferences/Delete/5
        // Yetki: Chair
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Chair)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var conf = await _context.Conferences.FindAsync(id);
            if (conf != null)
            {
                var conferenceName = conf.Name;
                _context.Conferences.Remove(conf);
                await _context.SaveChangesAsync();
                
                // Başarı mesajı ile success sayfasına yönlendir
                ViewBag.ConferenceName = conferenceName;
                return View("DeleteSuccess");
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Conferences/DeleteSuccess
        // Silme başarı sayfası - 5 saniye sonra Home'a yönlendirir
        [Authorize(Roles = AppRoles.Chair)]
        [HttpGet]
        public IActionResult DeleteSuccess()
        {
            // TempData'dan konferans adını al (eğer GET ile çağrılırsa)
            var conferenceName = TempData["DeletedConferenceName"]?.ToString() ?? ViewBag.ConferenceName?.ToString() ?? "Konferans";
            ViewBag.ConferenceName = conferenceName;
            return View();
        }
    }
}
