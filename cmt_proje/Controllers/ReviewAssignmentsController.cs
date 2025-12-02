using System;
using System.Linq;
using System.Threading.Tasks;
using cmt_proje.Core.Entities;
using cmt_proje.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cmt_proje.Core.Constants;

namespace cmt_proje.Controllers
{
    [Authorize(Roles = AppRoles.Chair)]
    public class ReviewAssignmentsController : Controller
    {
        private readonly ConferenceDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewAssignmentsController(ConferenceDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// CHAIR görünümü – bir konferanstaki tüm submission’lar ve onlara atanmış reviewer’lar.
        /// /ReviewAssignments?conferenceId=1
        /// </summary>
        public async Task<IActionResult> Index(int conferenceId)
        {
            var conference = await _context.Conferences
                .FirstOrDefaultAsync(c => c.Id == conferenceId);

            if (conference == null)
                return NotFound();

            var submissions = await _context.Submissions
                .Where(s => s.ConferenceId == conferenceId)
                .Include(s => s.Track)
                .Include(s => s.SubmittedByUser)
                .Include(s => s.ReviewAssignments)
                    .ThenInclude(ra => ra.Reviewer)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            ViewBag.Conference = conference;
            return View(submissions);
        }

        /// <summary>
        /// ESKİ ROUTE: /ReviewAssignments/MyAssignments
        /// Artık tüm reviewer görev listesi ReviewsController.MyAssignments altında
        /// toplandığı için burada sadece redirect yapıyoruz.
        /// Böylece eski linkler de bozulmamış oluyor.
        /// </summary>
        [HttpGet]
        public IActionResult MyAssignments()
        {
            // /Reviews/MyAssignments'a yönlendir
            return RedirectToAction("MyAssignments", "Reviews");
        }

        /// <summary>
        /// CHAIR – belirli bir submission’a hakem atama ekranı.
        /// /ReviewAssignments/Assign?submissionId=5
        /// </summary>
        public async Task<IActionResult> Assign(int submissionId)
        {
            var submission = await _context.Submissions
                .Include(s => s.Conference)
                .Include(s => s.Track)
                .Include(s => s.ReviewAssignments)
                    .ThenInclude(ra => ra.Reviewer)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                return NotFound();

            // Sadece Chair rolündeki kullanıcıları göster
            var chairUsers = await _userManager.GetUsersInRoleAsync(AppRoles.Chair);
            var users = chairUsers.OrderBy(u => u.Email).ToList();

            ViewBag.Users = users;
            return View(submission);
        }

        /// <summary>
        /// CHAIR – hakemi kaydet.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int submissionId, string reviewerId)
        {
            var submission = await _context.Submissions
                .Include(s => s.ReviewAssignments)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(reviewerId))
            {
                ModelState.AddModelError(string.Empty, "Please select a reviewer.");
            }

            // Atanan kullanıcının Chair rolünde olduğunu kontrol et
            var assignedUser = await _userManager.FindByIdAsync(reviewerId);
            if (assignedUser != null)
            {
                var isChair = await _userManager.IsInRoleAsync(assignedUser, AppRoles.Chair);
                if (!isChair)
                {
                    ModelState.AddModelError(string.Empty, "Only users with Chair role can be assigned as reviewers.");
                }
            }

            if (!ModelState.IsValid)
            {
                var chairUsers = await _userManager.GetUsersInRoleAsync(AppRoles.Chair);
                var users = chairUsers.OrderBy(u => u.Email).ToList();
                ViewBag.Users = users;
                return View(submission);
            }

            // Aynı kullanıcıya aynı submission için ikinci kez assignment verme
            var exists = submission.ReviewAssignments
                .Any(ra => ra.ReviewerId == reviewerId);

            if (!exists)
            {
                var assignment = new ReviewAssignment
                {
                    SubmissionId = submissionId,
                    ReviewerId = reviewerId,
                    AssignedAt = DateTime.UtcNow
                };

                _context.ReviewAssignments.Add(assignment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { conferenceId = submission.ConferenceId });
        }
    }
}
