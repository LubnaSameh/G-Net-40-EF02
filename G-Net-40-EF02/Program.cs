using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Emit;

namespace G_Net_40_EF02
{
    #region Enums
    public enum BadgeTier
    {
        Standard,
        VIP
    }
    #endregion

    #region Domain Models (Data Annotations for 1-to-1)
    public class Organizer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string CompanyName { get; set; }

        public bool IsVerified { get; set; }

        public virtual OrganizerProfile Profile { get; set; }
    }

    public class OrganizerProfile
    {
        [Key]
        public int Id { get; set; }

        public string Biography { get; set; }

        public string WebsiteLink { get; set; }

        public string LogoUrl { get; set; }

        [Required]
        [ForeignKey("Organizer")]
        public int OrganizerId { get; set; }
        public virtual Organizer Organizer { get; set; }
    }
    #endregion

    #region Domain Models (Fluent API for 1-to-1)
    public class Attendee
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }

        public virtual Badge Badge { get; set; }
    }

    public class Badge
    {
        public int Id { get; set; }
        public string BadgeNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public BadgeTier Tier { get; set; }

        public int AttendeeId { get; set; }
        public virtual Attendee Attendee { get; set; }
    }
    #endregion

    #region DbContext & Configuration
    public class EventHubDbContext : DbContext
    {
        public DbSet<Organizer> Organizers { get; set; }
        public DbSet<OrganizerProfile> OrganizerProfiles { get; set; }
        public DbSet<Attendee> Attendees { get; set; }
        public DbSet<Badge> Badges { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.;Database=EventHubDb;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API configuration for 1-to-1 relationship between Attendee and Badge
            modelBuilder.Entity<Attendee>()
                .HasOne(a => a.Badge)
                .WithOne(b => b.Attendee)
                .HasForeignKey<Badge>(b => b.AttendeeId);
        }
    }
    #endregion

    #region Application Entry Point
    internal class Program
    {
        static void Main(string[] args)
        {

            using (EventHubDbContext context = new EventHubDbContext())
            {
                Console.WriteLine("\nInitializing EventHub Database...");

                bool isCreated = context.Database.EnsureCreated();

                if (isCreated)
                {
                    Console.WriteLine("SUCCESS: Database 'EventHubDb' created successfully.");
                }
                else
                {
                    Console.WriteLine("INFO: Database already exists.");
                }
            }

          
        }
    }
    #endregion
}