using System;
using System.ComponentModel.DataAnnotations;

namespace cmt_proje.Core.Entities
{
    public class PageContentBlock : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string PageKey { get; set; } = string.Empty; // "Home", "About", "Contact", etc.

        [Required]
        [StringLength(50)]
        public string BlockType { get; set; } = string.Empty; // "Heading", "Text", "Image", "Video", "Button", "HTML"

        public int DisplayOrder { get; set; } = 0; // Sıralama için

        [StringLength(200)]
        public string? Title { get; set; } // Başlık (Heading, Button için)

        public string? Content { get; set; } // HTML/metin içerik

        [StringLength(500)]
        public string? ImageUrl { get; set; } // Resim URL'i

        [StringLength(500)]
        public string? LinkUrl { get; set; } // Link URL'i (Button için)

        [StringLength(200)]
        public string? LinkText { get; set; } // Link metni

        [StringLength(100)]
        public string? CssClass { get; set; } // Özel CSS class

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

