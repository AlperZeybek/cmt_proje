using cmt_proje.Core.Entities;
using cmt_proje.Infrastructure.Data;
using cmt_proje.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using cmt_proje.Core.Constants;

namespace cmt_proje.Controllers
{
    [Authorize(Roles = AppRoles.Chair)]
    public class ReviewsController : Controller
    {
        private readonly ConferenceDbContext _context;

        public ReviewsController(ConferenceDbContext context)
        {
            _context = context;
        }

        // KULLANICIYA ATANAN TÜM REVIEW GÖREVLERİ
        // GET: /Reviews/MyAssignments
        public async Task<IActionResult> MyAssignments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var assignments = await _context.ReviewAssignments
                .Where(a => a.ReviewerId == userId)
                .Include(a => a.Submission)
                    .ThenInclude(s => s.Conference)
                .Include(a => a.Submission)
                    .ThenInclude(s => s.Track)
                .Include(a => a.Review)
                .OrderByDescending(a => a.AssignedAt)
                .ToListAsync();

            return View(assignments);
        }

        // REVIEW YAZ / DÜZENLE
        // Aşağıdaki action hem:
        //   /Reviews/Edit/5         (route param: id)
        //   /Reviews/Edit?id=5      (query string)
        //   /Reviews/Edit?submissionId=5  (submissionId'a göre)
        // gibi çağrıları destekler.
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, int? submissionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Challenge();

            IQueryable<ReviewAssignment> query = _context.ReviewAssignments
                .Include(a => a.Submission)
                    .ThenInclude(s => s.Conference)
                .Include(a => a.Submission)
                    .ThenInclude(s => s.Track)
                .Include(a => a.Review);

            ReviewAssignment assignment = null;

            if (submissionId.HasValue)
            {
                // Eski premium link: ?submissionId=...
                assignment = await query
                    .FirstOrDefaultAsync(a =>
                        a.SubmissionId == submissionId.Value &&
                        a.ReviewerId == userId);
            }
            else if (id.HasValue)
            {
                // Klasik route: /Reviews/Edit/5 veya ?id=5
                assignment = await query
                    .FirstOrDefaultAsync(a =>
                        a.Id == id.Value &&
                        a.ReviewerId == userId);
            }

            if (assignment == null)
                return NotFound();

            var vm = new ReviewEditViewModel
            {
                AssignmentId = assignment.Id,
                SubmissionId = assignment.SubmissionId,
                SubmissionTitle = assignment.Submission.Title,
                TrackName = assignment.Submission.Track?.Name ?? "-",
                ConferenceName = assignment.Submission.Conference.Name,
                IsSubmitted = assignment.Review != null
            };

            if (assignment.Review != null)
            {
                vm.ScoreOverall = assignment.Review.ScoreOverall;
                vm.Confidence = assignment.Review.Confidence;
                vm.Strengths = assignment.Review.Strengths;
                vm.Weaknesses = assignment.Review.Weaknesses;
                vm.CommentsToAuthor = assignment.Review.CommentsToAuthor;
                vm.CommentsToChair = assignment.Review.CommentsToChair;
            }

            return View(vm);
        }

        // POST: /Reviews/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ReviewEditViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var assignment = await _context.ReviewAssignments
                .Include(a => a.Submission)
                    .ThenInclude(s => s.Conference)
                .Include(a => a.Submission)
                    .ThenInclude(s => s.Track)
                .Include(a => a.Review)
                .FirstOrDefaultAsync(a => a.Id == model.AssignmentId);

            if (assignment == null)
                return NotFound();

            // Güvenlik: sadece kendi assignment'ını düzenleyebilsin
            if (assignment.ReviewerId != userId)
                return Forbid();

            if (!ModelState.IsValid)
            {
                // Görsel bilgiler dolu kalsın
                model.SubmissionTitle = assignment.Submission.Title;
                model.TrackName = assignment.Submission.Track?.Name ?? "-";
                model.ConferenceName = assignment.Submission.Conference.Name;
                model.IsSubmitted = assignment.Review != null;
                return View(model);
            }

            if (assignment.Review == null)
            {
                // Yeni review oluştur
                var review = new Review
                {
                    ReviewAssignmentId = assignment.Id,
                    ScoreOverall = model.ScoreOverall,
                    Confidence = model.Confidence,
                    Strengths = model.Strengths,
                    Weaknesses = model.Weaknesses,
                    CommentsToAuthor = model.CommentsToAuthor,
                    CommentsToChair = model.CommentsToChair,
                    SubmittedAt = DateTime.Now,
                    CreatedAt = DateTime.Now
                };

                assignment.Review = review;
                _context.Reviews.Add(review);
            }
            else
            {
                // Mevcut review'i güncelle
                assignment.Review.ScoreOverall = model.ScoreOverall;
                assignment.Review.Confidence = model.Confidence;
                assignment.Review.Strengths = model.Strengths;
                assignment.Review.Weaknesses = model.Weaknesses;
                assignment.Review.CommentsToAuthor = model.CommentsToAuthor;
                assignment.Review.CommentsToChair = model.CommentsToChair;
                assignment.Review.SubmittedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyAssignments));
        }
    }
}
