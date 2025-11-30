using cmt_proje.Core.Entities;
using cmt_proje.Core.Enums;
using cmt_proje.Infrastructure.Data;
using cmt_proje.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace cmt_proje.Controllers
{
    [Authorize]
    public class SubmissionsController : Controller
    {
        private readonly ConferenceDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SubmissionsController(ConferenceDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // =====================================================================
        //  TÜM SUBMISSION'LAR (CHAIR İÇİN)  /Submissions?conferenceId=1
        // =====================================================================
        public async Task<IActionResult> Index(int conferenceId)
        {
            var conference = await _context.Conferences.FindAsync(conferenceId);
            if (conference == null)
                return NotFound();

            ViewBag.Conference = conference;

            var submissions = await _context.Submissions
                .Where(s => s.ConferenceId == conferenceId)
                .Include(s => s.Track)
                .Include(s => s.SubmittedByUser)
                .Include(s => s.Decision) // >>> Decision da yüklensin
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            return View(submissions);
        }

        // =====================================================================
        //  SADECE GİRİŞ YAPAN KULLANICININ GÖNDERDİKLERİ  /Submissions/My
        // =====================================================================
        public async Task<IActionResult> My()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var submissions = await _context.Submissions
                .Where(s => s.SubmittedByUserId == userId)
                .Include(s => s.Conference)
                .Include(s => s.Track)
                .Include(s => s.Decision) // >>> Burada da Decision'ı Include et
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            return View(submissions);
        }

        // =====================================================================
        //  GET: /Submissions/Create?conferenceId=1
        // =====================================================================
        public async Task<IActionResult> Create(int conferenceId)
        {
            var conference = await _context.Conferences
                .Include(c => c.Tracks)
                .FirstOrDefaultAsync(c => c.Id == conferenceId);

            if (conference == null)
                return NotFound();

            var vm = new SubmissionCreateViewModel
            {
                ConferenceId = conferenceId
            };

            ViewBag.Conference = conference;
            ViewBag.TrackList = new SelectList(conference.Tracks.Where(t => t.IsActive), "Id", "Name");

            return View(vm);
        }

        // =====================================================================
        //  POST: /Submissions/Create
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubmissionCreateViewModel model)
        {
            var conference = await _context.Conferences
                .Include(c => c.Tracks)
                .FirstOrDefaultAsync(c => c.Id == model.ConferenceId);

            if (conference == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Conference = conference;
                ViewBag.TrackList = new SelectList(conference.Tracks.Where(t => t.IsActive), "Id", "Name");
                return View(model);
            }

            if (model.PdfFile == null || model.PdfFile.Length == 0)
            {
                ModelState.AddModelError(nameof(model.PdfFile), "Please upload a PDF file.");
                ViewBag.Conference = conference;
                ViewBag.TrackList = new SelectList(conference.Tracks.Where(t => t.IsActive), "Id", "Name");
                return View(model);
            }

            // PDF KAYDET
            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "submissions");
            Directory.CreateDirectory(uploadsRoot);

            var extension = Path.GetExtension(model.PdfFile.FileName);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsRoot, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.PdfFile.CopyToAsync(stream);
            }

            var relativePath = Path.Combine("uploads", "submissions", fileName).Replace("\\", "/");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var submission = new Submission
            {
                ConferenceId = model.ConferenceId,
                TrackId = model.TrackId,
                Title = model.Title,
                Abstract = model.Abstract,
                PdfFilePath = relativePath,
                Status = SubmissionStatus.Submitted,
                SubmittedByUserId = userId!,
                SubmittedAt = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { conferenceId = model.ConferenceId });
        }

        // =====================================================================
        //  DETAY
        // =====================================================================
        public async Task<IActionResult> Details(int id)
        {
            var submission = await _context.Submissions
                .Include(s => s.Conference)
                .Include(s => s.Track)
                .Include(s => s.SubmittedByUser)
                .Include(s => s.Authors)
                .Include(s => s.Decision)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null)
                return NotFound();

            return View(submission);
        }

        // =====================================================================
        //  PDF DOWNLOAD
        // =====================================================================
        public async Task<IActionResult> Download(int id)
        {
            var submission = await _context.Submissions.FirstOrDefaultAsync(s => s.Id == id);
            if (submission == null || string.IsNullOrEmpty(submission.PdfFilePath))
                return NotFound();

            var fullPath = Path.Combine(_env.WebRootPath, submission.PdfFilePath.TrimStart('/', '\\'));
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var fileName = Path.GetFileName(fullPath);
            return PhysicalFile(fullPath, "application/pdf", fileName);
        }

        // =====================================================================
        //  DELETE GET
        // =====================================================================
        public async Task<IActionResult> Delete(int id)
        {
            var submission = await _context.Submissions
                .Include(s => s.Conference)
                .Include(s => s.Track)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null)
                return NotFound();

            return View(submission);
        }

        // =====================================================================
        //  DELETE POST
        // =====================================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var submission = await _context.Submissions.FindAsync(id);
            if (submission == null)
                return NotFound();

            var conferenceId = submission.ConferenceId;

            if (!string.IsNullOrEmpty(submission.PdfFilePath))
            {
                var fullPath = Path.Combine(_env.WebRootPath, submission.PdfFilePath.TrimStart('/', '\\'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }

            _context.Submissions.Remove(submission);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { conferenceId });
        }
    }
}
