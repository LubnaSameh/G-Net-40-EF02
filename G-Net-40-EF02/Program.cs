using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    #region Owned Entity Type (Q02 Fix)
    // Q02:
    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
    }
    #endregion

    #region Domain Models (Fluent API & Others)
    public class Attendee
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        // Address is now an Owned Type
        public virtual Address Address { get; set; }

        public virtual Badge Badge { get; set; }

        // Navigation property for Many-to-Many
        public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
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

    // Q01 & Q04: 
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } up
        public int MaxAttendees { get; set; }

        // Self-Referencing Relationship
        public int? ParentEventId { get; set; }
        public virtual Event ParentEvent { get; set; }
        public virtual ICollection<Event> Sessions { get; set; } = new List<Event>();

        // Navigation property for Many-to-Many
        public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    }

    // Q01: 
    public class Registration
    {
        public int AttendeeId { get; set; }
        public virtual Attendee Attendee { get; set; }

        public int EventId { get; set; }
        public virtual Event Event { get; set; }

        // Payload data
        public string Note { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
    #endregion

    #region DbContext & Configuration
    public class EventHubDbContext : DbContext
    {
        public DbSet<Organizer> Organizers { get; set; }
        public DbSet<OrganizerProfile> OrganizerProfiles { get; set; }
        public DbSet<Attendee> Attendees { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<Event> Events { get; set; } // Added
        public DbSet<Registration> Registrations { get; set; } // Added

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.;Database=EventHubDb;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Attendee to Badge (1-to-1)
            modelBuilder.Entity<Attendee>()
                .HasOne(a => a.Badge)
                .WithOne(b => b.Attendee)
                .HasForeignKey<Badge>(b => b.AttendeeId);

            // 2. Q02: 
            modelBuilder.Entity<Attendee>()
                .OwnsOne(a => a.Address);

            // 3. Q01: 
            modelBuilder.Entity<Registration>()
                .HasKey(r => new { r.AttendeeId, r.EventId });

            modelBuilder.Entity<Registration>()
                .HasOne(r => r.Attendee)
                .WithMany(a => a.Registrations)
                .HasForeignKey(r => r.AttendeeId);

            modelBuilder.Entity<Registration>()
                .HasOne(r => r.Event)
                .WithMany(e => e.Registrations)
                .HasForeignKey(r => r.EventId);

            // 4. Q04: 
            modelBuilder.Entity<Event>()
                .HasOne(e => e.ParentEvent)
                .WithMany(e => e.Sessions)
                .HasForeignKey(e => e.ParentEventId)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. Q03: 
            modelBuilder.Entity<Event>()
                .Property<DateTime>("CreatedAt");

            modelBuilder.Entity<Event>()
                .Property<DateTime>("ModifiedAt");
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

                // Drop and recreate to apply new schema cleanly
                context.Database.EnsureDeleted();
                bool isCreated = context.Database.EnsureCreated();

                if (isCreated)
                {
                    Console.WriteLine("SUCCESS: Database 'EventHubDb' created successfully with ALL requirements.");
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