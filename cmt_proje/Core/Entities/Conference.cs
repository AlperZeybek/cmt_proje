using System;
using System.Collections.Generic;

namespace cmt_proje.Core.Entities
{
    public class Conference : BaseEntity
    {
        public string? Name { get; set; }

        // ⭐ EKLENEN ALAN — Premium görünüm tasarımları bunu kullanıyor
        public string? ShortName { get; set; }

        public string? Acronym { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime SubmissionDeadline { get; set; }

        public bool IsActive { get; set; }

        // Oluşturan kişi
        public string? CreatedByUserId { get; set; }
        public ApplicationUser? CreatedByUser { get; set; }

        public ICollection<Track>? Tracks { get; set; }
        public ICollection<Submission>? Submissions { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
