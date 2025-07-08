using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cleanApi.Users;
using cleanApi.Bookings;
using cleanApi.Payments;
using cleanApi.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace cleanApi.Config
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Users Module
        public DbSet<User> Users { get; set; }
        
        // Bookings Module
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<CleaningService> CleaningServices { get; set; }
        
        // Payments Module
        public DbSet<Payment> Payments { get; set; }
        
        // Subscriptions Module
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure entities
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasConversion<string>();
                entity.HasOne<User>().WithMany().HasForeignKey(e => e.UserId);
                entity.HasOne(e => e.Service).WithMany().HasForeignKey(e => e.ServiceId);
            });

            modelBuilder.Entity<CleaningService>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Status).HasConversion<string>();
            });

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasConversion<string>();
                entity.HasOne<User>().WithMany().HasForeignKey(e => e.UserId);
                entity.HasOne(e => e.Plan).WithMany().HasForeignKey(e => e.PlanId);
            });

            modelBuilder.Entity<SubscriptionPlan>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Duration).HasConversion<string>();
            });
        }
}