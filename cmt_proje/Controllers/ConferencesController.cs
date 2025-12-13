using System;
using System.Linq;
using System.Threading.Tasks;
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
        // Silme başarı sayfası - 5 saniye sonra Dashboard'a yönlendirir
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
