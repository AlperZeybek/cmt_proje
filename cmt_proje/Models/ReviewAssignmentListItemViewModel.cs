namespace cmt_proje.Models
{
    public class ReviewAssignmentListItemViewModel
    {
        public int SubmissionId { get; set; }

        public string PaperTitle { get; set; } = string.Empty;
        public string ConferenceName { get; set; } = string.Empty;
        public string TrackName { get; set; } = string.Empty;

        public DateTime AssignedAt { get; set; }

        // Pending / Reviewed gibi metin göstermek için
        public string Status { get; set; } = string.Empty;

        // Puan verilmemişse null kalabilir
        public int? Score { get; set; }
    }
}
