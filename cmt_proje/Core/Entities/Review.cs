namespace cmt_proje.Core.Entities

{
    public class Review : BaseEntity
    {
        public int ReviewAssignmentId { get; set; }
        public ReviewAssignment ReviewAssignment { get; set; }

        public int ScoreOverall { get; set; }      // 1–10
        public int Confidence { get; set; }        // 1–5

        public string Strengths { get; set; }
        public string Weaknesses { get; set; }
        public string CommentsToAuthor { get; set; }
        public string CommentsToChair { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
