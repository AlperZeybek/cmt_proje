using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cmt_proje.Core.Entities
{
    public class ScientificCommitteeMember : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Affiliation { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [StringLength(500)]
        [Url]
        public string? PhotoUrl { get; set; }

        [StringLength(1000)]
        public string? ShortBio { get; set; }

        [StringLength(500)]
        [Url]
        public string? WebSiteUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0; // Sıralama için

        // Foreign Key for Conference
        [Required]
        public int ConferenceId { get; set; }
        
        [ForeignKey("ConferenceId")]
        public virtual Conference? Conference { get; set; }
    }
}

