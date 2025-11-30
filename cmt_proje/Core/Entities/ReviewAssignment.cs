using Microsoft.AspNetCore.Mvc.ViewEngines;
namespace cmt_proje.Core.Entities

{
    public class ReviewAssignment : BaseEntity
    {
        public int SubmissionId { get; set; }
        public Submission Submission { get; set; }

        public string ReviewerId { get; set; }
        public ApplicationUser Reviewer { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public Review Review { get; set; }
    }
}
