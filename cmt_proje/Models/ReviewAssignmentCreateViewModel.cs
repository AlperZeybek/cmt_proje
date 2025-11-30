using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace cmt_proje.Models
{
    public class ReviewAssignmentCreateViewModel
    {
        public int SubmissionId { get; set; }
        public int ConferenceId { get; set; }
        public string SubmissionTitle { get; set; } = string.Empty;

        // Seçilecek reviewer
        public string? SelectedReviewerId { get; set; }

        // Dropdown için liste
        public IEnumerable<SelectListItem>? ReviewerList { get; set; }
    }
}
