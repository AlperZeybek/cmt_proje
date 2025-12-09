using cmt_proje.Core.Enums;

namespace cmt_proje.Core.Entities

{
    public class Submission : BaseEntity
    {
        public int ConferenceId { get; set; }
        public Conference Conference { get; set; }

        public int? TrackId { get; set; }
        public Track Track { get; set; }

        public string Title { get; set; }
        public string Abstract { get; set; }
        public string PdfFilePath { get; set; }
        public string? OriginalFileName { get; set; }
        public string? SubmissionNumber { get; set; }

        public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;
        public PresentationType PresentationType { get; set; }

        public string SubmittedByUserId { get; set; }
        public ApplicationUser SubmittedByUser { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public ICollection<SubmissionAuthor> Authors { get; set; }
        public ICollection<ReviewAssignment> ReviewAssignments { get; set; }
        public Decision Decision { get; set; }
    }
}
