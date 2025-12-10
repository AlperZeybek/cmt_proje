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
using cmt_proje.Core.Constants;
using Microsoft.AspNetCore.Identity;
using cmt_proje.Services.Interfaces;

namespace cmt_proje.Controllers
{
    [Authorize]
    public class SubmissionsController : Controller
    {
        private readonly ConferenceDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public SubmissionsController(
            ConferenceDbContext context,
            IWebHostEnvironment env,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
            _emailService = emailService;
        }

        // =====================================================================
        //  EDIT: /Submissions/Edit/{id}
        // =====================================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var submission = await _context.Submissions
                .Include(s => s.Conference)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null)
                return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isChair = User.IsInRole(AppRoles.Chair);
            if (!isChair && submission.SubmittedByUserId != userId)
                return Forbid();

            var conference = await _context.Conferences
                .Include(c => c.Tracks)
                .FirstOrDefaultAsync(c => c.Id == submission.ConferenceId);

            if (conference == null)
                return NotFound();

            ViewBag.Conference = conference;
            ViewBag.TrackList = new SelectList(conference.Tracks.Where(t => t.IsActive), "Id", "Name", submission.TrackId);

            var vm = new SubmissionEditViewModel
            {
                Id = submission.Id,
                ConferenceId = submission.ConferenceId,
                TrackId = submission.TrackId,
                Title = submission.Title,
                Abstract = submission.Abstract,
                PresentationType = submission.PresentationType
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubmissionEditViewModel model)
        {
            var submission = await _context.Submissions
                .Include(s => s.Conference)
                .FirstOrDefaultAsync(s => s.Id == model.Id);

            if (submission == null)
                return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isChair = User.IsInRole(AppRoles.Chair);
            if (!isChair && submission.SubmittedByUserId != userId)
                return Forbid();

            var conference = await _context.Conferences
                .Include(c => c.Tracks)
                .FirstOrDefaultAsync(c => c.Id == submission.ConferenceId);

            if (conference == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Conference = conference;
                ViewBag.TrackList = new SelectList(conference.Tracks.Where(t => t.IsActive), "Id", "Name", model.TrackId);
                return View(model);
            }

            submission.TrackId = model.TrackId;
            submission.Title = model.Title;
            submission.Abstract = model.Abstract;
            submission.PresentationType = model.PresentationType!.Value;

            await _context.SaveChangesAsync();

            if (isChair)
                return RedirectToAction(nameof(Index), new { conferenceId = submission.ConferenceId });

            return RedirectToAction(nameof(My));
        }

        // =====================================================================
        //  TÜM SUBMISSION'LAR (CHAIR İÇİN)  /Submissions?conferenceId=1
        // Yetki: Chair
        // =====================================================================
        [Authorize(Roles = AppRoles.Chair)]
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
        //  SUBMISSION İÇİN KONFERANS SEÇİMİ  /Submissions/SelectConference
        // =====================================================================
        public async Task<IActionResult> SelectConference()
        {
            var conferences = await _context.Conferences
                .Include(c => c.CreatedByUser)
                .Include(c => c.Tracks)
                .Where(c => c.IsActive && c.Tracks != null && c.Tracks.Any(t => t.IsActive))
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(conferences);
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
                ModelState.AddModelError(nameof(model.PdfFile), "Please upload a Word file (.doc or .docx).");
                ViewBag.Conference = conference;
                ViewBag.TrackList = new SelectList(conference.Tracks.Where(t => t.IsActive), "Id", "Name");
                return View(model);
            }

            // Word dosyası kontrolü
            var allowedExtensions = new[] { ".doc", ".docx" };
            var extension = Path.GetExtension(model.PdfFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(model.PdfFile), "Please upload a Word file (.doc or .docx).");
                ViewBag.Conference = conference;
                ViewBag.TrackList = new SelectList(conference.Tracks.Where(t => t.IsActive), "Id", "Name");
                return View(model);
            }

            // Orijinal dosya adını al (uzantı olmadan)
            var originalFileNameWithoutExtension = Path.GetFileNameWithoutExtension(model.PdfFile.FileName);
            var originalFileName = model.PdfFile.FileName;

            // Bu konferans için son submission numarasını bul (sayısal olarak en büyük)
            var submissions = await _context.Submissions
                .Where(s => s.ConferenceId == model.ConferenceId && !string.IsNullOrEmpty(s.SubmissionNumber))
                .Select(s => s.SubmissionNumber)
                .ToListAsync();

            // Yeni submission numarasını oluştur
            string submissionNumber;
            if (submissions == null || !submissions.Any())
            {
                submissionNumber = "001P";
            }
            else
            {
                // Tüm numaraları sayısal olarak parse et ve en büyüğünü bul
                int maxNumber = 0;
                foreach (var numStr in submissions)
                {
                    var numberStr = numStr?.Replace("P", "");
                    if (int.TryParse(numberStr, out int num) && num > maxNumber)
                    {
                        maxNumber = num;
                    }
                }
                var nextNumber = maxNumber + 1;
                submissionNumber = $"{nextNumber:D3}P";
            }

            // Word dosyası kaydet - orijinal_ad_001P.doc formatında
            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "submissions");
            Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{originalFileNameWithoutExtension}_{submissionNumber}{extension}";
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
                OriginalFileName = originalFileName,
                SubmissionNumber = submissionNumber,
                Status = SubmissionStatus.Submitted,
                PresentationType = model.PresentationType!.Value,
                SubmittedByUserId = userId!,
                SubmittedAt = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            // Bilgilendirme maili
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && !string.IsNullOrEmpty(currentUser.Email))
            {
                var fullName = string.IsNullOrWhiteSpace(currentUser.FullName)
                    ? currentUser.Email
                    : currentUser.FullName;

                var subject = "Başvurunuz Başarıyla İletilmiştir";
                var body = $@"
Merhaba Sayın {fullName},

İletiniz tarafımıza başarı ile iletilmiştir.

İyi günler dileriz.";

                await _emailService.SendEmailAsync(currentUser.Email, subject, body, isHtml: false);
            }

            // Chair ise Index'e, Author ise My'ye yönlendir
            var isChair = User.IsInRole(AppRoles.Chair);
            if (isChair)
            {
                return RedirectToAction(nameof(Index), new { conferenceId = model.ConferenceId });
            }
            else
            {
                return RedirectToAction(nameof(My));
            }
        }

        // =====================================================================
        //  DETAY
        // Yetki: Chair (tümü) + Author (sadece kendi)
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

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isChair = User.IsInRole(AppRoles.Chair);

            // Author sadece kendi submission'ını görebilir
            if (!isChair && submission.SubmittedByUserId != userId)
            {
                return Forbid();
            }

            return View(submission);
        }

        // =====================================================================
        //  PDF DOWNLOAD
        // Yetki: Chair (tümü) + Author (sadece kendi)
        // =====================================================================
        public async Task<IActionResult> Download(int id)
        {
            var submission = await _context.Submissions.FirstOrDefaultAsync(s => s.Id == id);
            if (submission == null || string.IsNullOrEmpty(submission.PdfFilePath))
                return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isChair = User.IsInRole(AppRoles.Chair);

            // Author sadece kendi PDF'ini indirebilir
            if (!isChair && submission.SubmittedByUserId != userId)
            {
                return Forbid();
            }

            var fullPath = Path.Combine(_env.WebRootPath, submission.PdfFilePath.TrimStart('/', '\\'));
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            // Orijinal dosya adına Submission Number ekle
            string downloadFileName;
            if (!string.IsNullOrEmpty(submission.OriginalFileName) && !string.IsNullOrEmpty(submission.SubmissionNumber))
            {
                // Orijinal dosya adını al (uzantı olmadan)
                var originalNameWithoutExtension = Path.GetFileNameWithoutExtension(submission.OriginalFileName);
                var extension = Path.GetExtension(submission.OriginalFileName);
                // Orijinal ad + "_" + Submission Number + uzantı
                downloadFileName = $"{originalNameWithoutExtension}_{submission.SubmissionNumber}{extension}";
            }
            else if (!string.IsNullOrEmpty(submission.OriginalFileName))
            {
                downloadFileName = submission.OriginalFileName;
            }
            else
            {
                downloadFileName = Path.GetFileName(fullPath);
            }

            var contentType = downloadFileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) 
                ? "application/vnd.openxmlformats-officedocument.wordprocessingml.document" 
                : "application/msword";
            return PhysicalFile(fullPath, contentType, downloadFileName);
        }

        // =====================================================================
        //  DELETE GET
        // Yetki: Chair (tümü) + Author (sadece kendi)
        // =====================================================================
        public async Task<IActionResult> Delete(int id)
        {
            var submission = await _context.Submissions
                .Include(s => s.Conference)
                .Include(s => s.Track)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null)
                return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isChair = User.IsInRole(AppRoles.Chair);

            // Author sadece kendi submission'ını silebilir
            if (!isChair && submission.SubmittedByUserId != userId)
            {
                return Forbid();
            }

            return View(submission);
        }

        // =====================================================================
        //  DELETE POST
        // Yetki: Chair (tümü) + Author (sadece kendi)
        // =====================================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var submission = await _context.Submissions.FindAsync(id);
            if (submission == null)
                return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isChair = User.IsInRole(AppRoles.Chair);

            // Author sadece kendi submission'ını silebilir
            if (!isChair && submission.SubmittedByUserId != userId)
            {
                return Forbid();
            }

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

            // Chair ise Index'e, Author ise My'ye yönlendir
            if (isChair)
            {
                return RedirectToAction(nameof(Index), new { conferenceId });
            }
            else
            {
                return RedirectToAction(nameof(My));
            }
        }
    }
}
