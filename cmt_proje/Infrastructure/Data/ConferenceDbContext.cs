using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using cmt_proje.Core.Entities;

namespace cmt_proje.Infrastructure.Data
{
    public class ConferenceDbContext : IdentityDbContext<ApplicationUser>
    {
        public ConferenceDbContext(DbContextOptions<ConferenceDbContext> options)
            : base(options)
        {
        }

        public DbSet<Conference> Conferences { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<SubmissionAuthor> SubmissionAuthors { get; set; }
        public DbSet<ReviewAssignment> ReviewAssignments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Decision> Decisions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Conference - CreatedByUser
            builder.Entity<Conference>()
                .HasOne(c => c.CreatedByUser)
                .WithMany(u => u.ConferencesCreated)
                .HasForeignKey(c => c.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Submission - Conference
            builder.Entity<Submission>()
                .HasOne(s => s.Conference)
                .WithMany(c => c.Submissions)
                .HasForeignKey(s => s.ConferenceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Track - Conference
            builder.Entity<Track>()
                .HasOne(t => t.Conference)
                .WithMany(c => c.Tracks)
                .HasForeignKey(t => t.ConferenceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Submission - Track
            builder.Entity<Submission>()
                .HasOne(s => s.Track)
                .WithMany(t => t.Submissions)
                .HasForeignKey(s => s.TrackId)
                .OnDelete(DeleteBehavior.Cascade);

            // Submission - SubmittedByUser
            builder.Entity<Submission>()
                .HasOne(s => s.SubmittedByUser)
                .WithMany(u => u.Submissions)
                .HasForeignKey(s => s.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ReviewAssignment - Submission
            builder.Entity<ReviewAssignment>()
                .HasOne(ra => ra.Submission)
                .WithMany(s => s.ReviewAssignments)
                .HasForeignKey(ra => ra.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // ReviewAssignment - Reviewer
            builder.Entity<ReviewAssignment>()
                .HasOne(ra => ra.Reviewer)
                .WithMany(u => u.ReviewAssignments)
                .HasForeignKey(ra => ra.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Submission - Decision (1-0/1)
            builder.Entity<Submission>()
                .HasOne(s => s.Decision)
                .WithOne(d => d.Submission)
                .HasForeignKey<Decision>(d => d.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // ReviewAssignment - Review (1-0/1)
            builder.Entity<ReviewAssignment>()
                .HasOne(ra => ra.Review)
                .WithOne(r => r.ReviewAssignment)
                .HasForeignKey<Review>(r => r.ReviewAssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
