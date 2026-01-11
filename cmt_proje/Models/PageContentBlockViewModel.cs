using System.ComponentModel.DataAnnotations;

namespace cmt_proje.Models
{
    public class PageContentBlockViewModel
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(100)]
        public string PageKey { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string BlockType { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Title { get; set; }

        public string? Content { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [StringLength(500)]
        public string? LinkUrl { get; set; }

        [StringLength(200)]
        public string? LinkText { get; set; }

        [StringLength(100)]
        public string? CssClass { get; set; }
    }

    public class BlockOrderViewModel
    {
        public int Id { get; set; }
        public int Order { get; set; }
    }
}

