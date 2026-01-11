using System;
using System.ComponentModel.DataAnnotations;

namespace cmt_proje.Core.Entities
{
    public class NavigationItem : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Label { get; set; } = string.Empty; // Menü metni

        [StringLength(500)]
        public string? Url { get; set; } // Link URL'i

        [StringLength(100)]
        public string? Controller { get; set; } // MVC Controller

        [StringLength(100)]
        public string? Action { get; set; } // MVC Action

        [StringLength(100)]
        public string? Area { get; set; } // MVC Area

        [StringLength(50)]
        public string? Icon { get; set; } // Bootstrap icon class

        public int DisplayOrder { get; set; } = 0; // Sıralama için

        public bool IsActive { get; set; } = true;

        public bool IsDropdown { get; set; } = false; // Dropdown menü mü?

        public int? ParentId { get; set; } // Üst menü ID (dropdown için)

        public NavigationItem? Parent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

