namespace cmt_proje.Core.Entities

{
    public class SubmissionAuthor : BaseEntity
    {
        public int SubmissionId { get; set; }
        public Submission Submission { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Affiliation { get; set; }
        public bool IsCorresponding { get; set; }
    }
}
