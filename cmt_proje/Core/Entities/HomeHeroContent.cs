using System;
using System.ComponentModel.DataAnnotations;

namespace cmt_proje.Core.Entities
{
    public class HomeHeroContent : BaseEntity
    {
        [Required]
        public string HeroTitle { get; set; } = "The Meeting Point<br />of Scientific Advancements";

        [Required]
        public string HeroSubtitle { get; set; } = "Explore world-class academic conferences hosted by Sakarya University.";

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}

