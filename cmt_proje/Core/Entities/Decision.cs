using cmt_proje.Core.Enums;

namespace cmt_proje.Core.Entities

{
    public class Decision : BaseEntity
    {
        public int SubmissionId { get; set; }
        public Submission Submission { get; set; }

        public DecisionStatus DecisionStatus { get; set; }

        public string DecidedByUserId { get; set; }
        public ApplicationUser DecidedByUser { get; set; }

        public DateTime DecidedAt { get; set; } = DateTime.UtcNow;

        public string Note { get; set; }
    }
}
