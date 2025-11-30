using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

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
        [Display(Name = "PDF File")]
        public IFormFile PdfFile { get; set; } = default!;
    }
}
