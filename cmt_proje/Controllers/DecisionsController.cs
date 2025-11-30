using System;
using System.Linq;
using System.Threading.Tasks;
using cmt_proje.Core.Entities;
using cmt_proje.Core.Enums;
using cmt_proje.Infrastructure.Data;
using cmt_proje.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cmt_proje.Core.Constants;

namespace cmt_proje.Controllers
{
    [Authorize(Roles = AppRoles.Chair)]
    public class DecisionsController : Controller
    {
        private readonly ConferenceDbContext _context;

        public DecisionsController(ConferenceDbContext context)
        {
            _context = context;
        }

        // GET: /Decisions/Details?submissionId=5
        public async Task<IActionResult> Details(int submissionId)
        {
            var submission = await _context.Submissions
                .Include(s => s.Conference)
                .Include(s => s.Track)
                .Include(s => s.SubmittedByUser)
                .Include(s => s.ReviewAssignments)
                    .ThenInclude(ra => ra.Review)
                .Include(s => s.ReviewAssignments)
                    .ThenInclude(ra => ra.Reviewer)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null)
                return NotFound();

            var decision = await _context.Decisions
                .Include(d => d.DecidedByUser)
                .FirstOrDefaultAsync(d => d.SubmissionId == submissionId);

            var vm = new DecisionCreateViewModel
            {
                SubmissionId = submission.Id,
                SubmissionTitle = submission.Title,
                TrackName = submission.Track?.Name ?? string.Empty,
                ConferenceName = submission.Conference.Name,
                AuthorEmail = submission.SubmittedByUser?.Email ?? string.Empty,
                SubmissionStatus = submission.Status,
                ExistingDecisionId = decision?.Id,
                DecisionStatus = decision?.DecisionStatus ?? default,

                Note = decision?.Note ?? string.Empty,
                Reviews = submission.ReviewAssignments
                    .Where(ra => ra.Review != null)
                    .Select(ra => new ReviewSummaryItem
                    {
                        ReviewerEmail = ra.Reviewer?.Email ?? string.Empty,
                        ScoreOverall = ra.Review!.ScoreOverall,
                        Confidence = ra.Review.Confidence,
                        SubmittedAt = ra.Review.SubmittedAt
                    })
                    .ToList()
            };

            ViewBag.ConferenceId = submission.ConferenceId;

            return View(vm);   // 🔴 BURASI ARTIK HER ZAMAN VIEWMODEL GÖNDERİYOR
        }

        // POST: /Decisions/Details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(DecisionCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reviews vs doldurmak için submission tekrar çekilir
                var submissionReload = await _context.Submissions
                    .Include(s => s.Conference)
                    .Include(s => s.Track)
                    .Include(s => s.SubmittedByUser)
                    .Include(s => s.ReviewAssignments)
                        .ThenInclude(ra => ra.Review)
                    .Include(s => s.ReviewAssignments)
                        .ThenInclude(ra => ra.Reviewer)
                    .FirstOrDefaultAsync(s => s.Id == model.SubmissionId);

                if (submissionReload != null)
                {
                    model.Reviews = submissionReload.ReviewAssignments
                        .Where(ra => ra.Review != null)
                        .Select(ra => new ReviewSummaryItem
                        {
                            ReviewerEmail = ra.Reviewer?.Email ?? string.Empty,
                            ScoreOverall = ra.Review!.ScoreOverall,
                            Confidence = ra.Review.Confidence,
                            SubmittedAt = ra.Review.SubmittedAt
                        })
                        .ToList();

                    ViewBag.ConferenceId = submissionReload.ConferenceId;
                }

                return View(model);
            }

            var submission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.Id == model.SubmissionId);

            if (submission == null)
                return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var decision = await _context.Decisions
                .FirstOrDefaultAsync(d => d.SubmissionId == model.SubmissionId);

            if (decision == null)
            {
                decision = new Decision
                {
                    SubmissionId = model.SubmissionId,
                    CreatedAt = DateTime.Now
                };
                _context.Decisions.Add(decision);
            }

            decision.DecisionStatus = model.DecisionStatus;
            decision.Note = model.Note ?? string.Empty;
            decision.DecidedAt = DateTime.Now;
            decision.DecidedByUserId = userId!;

            // İsteğe bağlı: submission durumunu da güncelle
            if (model.DecisionStatus == DecisionStatus.Accepted)
                submission.Status = SubmissionStatus.Accepted;
            else if (model.DecisionStatus == DecisionStatus.Rejected)
                submission.Status = SubmissionStatus.Rejected;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Submissions",
                new { conferenceId = submission.ConferenceId });
        }
    }
}
