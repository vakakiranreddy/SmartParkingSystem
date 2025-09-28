using Microsoft.EntityFrameworkCore;
using SmartParkingSystem.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SmartParkingSystem.Data
{
    public class ParkingDbContext : DbContext
    {
        public ParkingDbContext(DbContextOptions<ParkingDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<ParkingSlot> ParkingSlots { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<ParkingSession> ParkingSessions { get; set; }
        public DbSet<EmailNotification> EmailNotifications { get; set; }
        public DbSet<BroadcastNotification> BroadcastNotifications { get; set; }
        public DbSet<ParkingRate> ParkingRates { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<SlotFeature> SlotFeatures { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure SlotFeature composite key for Many-to-Many
            modelBuilder.Entity<SlotFeature>()
                .HasKey(sf => new { sf.SlotId, sf.FeatureId });

            // Configure relationships and delete behavior
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Owner)
                .WithMany(u => u.Vehicles)
                .HasForeignKey(v => v.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ParkingSession>()
                .HasOne(ps => ps.User)
                .WithMany(u => u.ParkingSessions)
                .HasForeignKey(ps => ps.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ParkingSession>()
                .HasOne(ps => ps.Vehicle)
                .WithMany(v => v.ParkingSessions)
                .HasForeignKey(ps => ps.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ParkingSession>()
                .HasOne(ps => ps.ParkingSlot)
                .WithMany(slot => slot.ParkingSessions)
                .HasForeignKey(ps => ps.SlotId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmailNotification>()
                .HasOne(email => email.User)
                .WithMany(u => u.EmailNotifications)
                .HasForeignKey(email => email.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmailNotification>()
                .HasOne(email => email.ParkingSession)
                .WithMany(ps => ps.EmailNotifications)
                .HasForeignKey(email => email.ParkingSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SlotFeature>()
                .HasOne(sf => sf.ParkingSlot)
                .WithMany(ps => ps.SlotFeatures)
                .HasForeignKey(sf => sf.SlotId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SlotFeature>()
                .HasOne(sf => sf.Feature)
                .WithMany(f => f.SlotFeatures)
                .HasForeignKey(sf => sf.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}