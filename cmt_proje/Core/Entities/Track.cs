using System;
using System.Collections.Generic;

namespace cmt_proje.Core.Entities
{
    public class Track : BaseEntity
    {
        public int ConferenceId { get; set; }
        public Conference? Conference { get; set; }

        public string? Name { get; set; }
        public string? Description { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Submission>? Submissions { get; set; }
    }
}
