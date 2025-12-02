namespace cmt_proje.Models
{
    public class ConferenceSelectionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Acronym { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int TrackCount { get; set; }
        public string? CreatedByUserEmail { get; set; }
    }
}

