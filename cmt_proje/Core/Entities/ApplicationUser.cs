using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace cmt_proje.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        // NULL kabul etsin diye ?
        public string? FullName { get; set; }
        public string? Affiliation { get; set; }
        public string? Department { get; set; }
        public string? Faculty { get; set; }

        public ICollection<Conference>? ConferencesCreated { get; set; }
        public ICollection<Submission>? Submissions { get; set; }
        public ICollection<ReviewAssignment>? ReviewAssignments { get; set; }
    }
}
