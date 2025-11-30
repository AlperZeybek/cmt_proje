using System.ComponentModel.DataAnnotations;

namespace cmt_proje.Models
{
    public class ReviewEditViewModel
    {
        public int AssignmentId { get; set; }
        public int SubmissionId { get; set; }

        public string SubmissionTitle { get; set; } = string.Empty;
        public string TrackName { get; set; } = string.Empty;
        public string ConferenceName { get; set; } = string.Empty;

        [Display(Name = "Overall Score (1–10)")]
        [Range(1, 10, ErrorMessage = "Score must be between 1 and 10.")]
        public int ScoreOverall { get; set; }

        [Display(Name = "Confidence (1–5)")]
        [Range(1, 5, ErrorMessage = "Confidence must be between 1 and 5.")]
        public int Confidence { get; set; }

        [Display(Name = "Strengths")]
        [Required(ErrorMessage = "Please describe the strengths of the paper.")]
        public string Strengths { get; set; } = string.Empty;

        [Display(Name = "Weaknesses")]
        [Required(ErrorMessage = "Please describe the weaknesses of the paper.")]
        public string Weaknesses { get; set; } = string.Empty;

        [Display(Name = "Comments to Author")]
        public string CommentsToAuthor { get; set; } = string.Empty;

        [Display(Name = "Comments to Chair")]
        public string CommentsToChair { get; set; } = string.Empty;

        public bool IsSubmitted { get; set; }
    }
}
