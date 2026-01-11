using System.ComponentModel.DataAnnotations;

namespace cmt_proje.Models
{
    public class NavigationItemViewModel
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Label { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Url { get; set; }

        [StringLength(100)]
        public string? Controller { get; set; }

        [StringLength(100)]
        public string? Action { get; set; }

        [StringLength(100)]
        public string? Area { get; set; }

        [StringLength(50)]
        public string? Icon { get; set; }

        public int? ParentId { get; set; }

        public bool IsDropdown { get; set; } = false;
    }

    public class NavItemOrderViewModel
    {
        public int Id { get; set; }
        public int Order { get; set; }
    }
}

