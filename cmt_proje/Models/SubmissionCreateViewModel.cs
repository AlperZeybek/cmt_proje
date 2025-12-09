using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using cmt_proje.Core.Enums;

namespace cmt_proje.Models
{
    public class SubmissionCreateViewModel
    {
        [Required]
        public int ConferenceId { get; set; }

        [Display(Name = "Track")]
        [Required(ErrorMessage = "Please select a track.")]
        public int? TrackId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(4000)]
        [Display(Name = "Abstract")]
        public string Abstract { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Word File")]
        public IFormFile PdfFile { get; set; } = default!;

        [Required(ErrorMessage = "Please select a presentation type.")]
        [Display(Name = "Presentation Type")]
        public PresentationType? PresentationType { get; set; }
    }
}
