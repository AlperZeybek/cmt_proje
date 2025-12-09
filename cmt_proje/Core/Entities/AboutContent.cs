using System;
using System.ComponentModel.DataAnnotations;

namespace cmt_proje.Core.Entities
{
    public class AboutContent : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string PageKey { get; set; } = string.Empty; // "PastEvents", "AboutOrganizer", "Testimonials"

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty; // HTML/metin içerik

        [StringLength(500)]
        public string? ImageUrl { get; set; } // Resim URL'i

        [StringLength(500)]
        public string? LinkUrl { get; set; } // Link URL'i

        [StringLength(200)]
        public string? LinkText { get; set; } // Link metni

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}

