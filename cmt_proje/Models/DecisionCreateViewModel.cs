using System;
using System.Collections.Generic;
using cmt_proje.Core.Enums;

namespace cmt_proje.Models
{
    public class ReviewSummaryItem
    {
        public string ReviewerEmail { get; set; } = string.Empty;
        public int ScoreOverall { get; set; }
        public int Confidence { get; set; }
        public DateTime SubmittedAt { get; set; }

        // Eski view'ler hata vermesin diye ekledik:
        public string Strengths { get; set; } = string.Empty;
        public string Weaknesses { get; set; } = string.Empty;
        public string CommentsToChair { get; set; } = string.Empty;
        // Eğer CommentsToAuthor vs. kullanan başka view'ler çıkarsa aynı şekilde eklenebilir.
    }

    public class DecisionCreateViewModel
    {
        // Submission bilgileri
        public int SubmissionId { get; set; }
        public string SubmissionTitle { get; set; } = string.Empty;
        public string TrackName { get; set; } = string.Empty;
        public string ConferenceName { get; set; } = string.Empty;
        public string AuthorEmail { get; set; } = string.Empty;
        public SubmissionStatus SubmissionStatus { get; set; }

        // Decision bilgileri
        public int? ExistingDecisionId { get; set; }   // null ise yeni karar, dolu ise edit
        public DecisionStatus DecisionStatus { get; set; }
        public string? Note { get; set; }

        // Review özetleri
        public List<ReviewSummaryItem> Reviews { get; set; } = new();
    }
}
