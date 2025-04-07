using Inyama_Yethu.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Inyama_Yethu.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Employee Management
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<TaskAssignment> TaskAssignments { get; set; }
        public DbSet<TaskCategory> TaskCategories { get; set; }

        // Livestock Management
        public DbSet<Animal> Animals { get; set; }
        public DbSet<Mating> Matings { get; set; }
        public DbSet<PigletProcessing> PigletProcessings { get; set; }
        public DbSet<Feeding> Feedings { get; set; }
        public DbSet<HealthRecord> HealthRecords { get; set; }
        public DbSet<WeightRecord> WeightRecords { get; set; }
        public DbSet<AbattoirShipment> AbattoirShipments { get; set; }
        public DbSet<Birth> Births { get; set; }
        public DbSet<AnimalSale> AnimalSales { get; set; }

        // Customer Management
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Product> Products { get; set; }
        
        // Activity Logging
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal properties
            modelBuilder.Entity<AbattoirShipment>()
                .Property(a => a.ActualPayment)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<AbattoirShipment>()
                .Property(a => a.EstimatedValue)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<AbattoirShipment>()
                .Property(a => a.TransportationCost)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Feeding>()
                .Property(f => f.CostPerKg)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<HealthRecord>()
                .Property(h => h.Cost)
                .HasColumnType("decimal(18,2)");

            // Configure relationships
            
            // Configure Animal self-referencing relationships (parent-child)
            modelBuilder.Entity<Animal>()
                .HasOne(a => a.Mother)
                .WithMany(a => a.Offspring)
                .HasForeignKey(a => a.MotherAnimalId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<Animal>()
                .HasOne(a => a.Father)
                .WithMany()
                .HasForeignKey(a => a.FatherAnimalId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Configure Mating relationships
            modelBuilder.Entity<Mating>()
                .HasOne(m => m.Mother)
                .WithMany(a => a.MatingsAsMother)
                .HasForeignKey(m => m.MotherAnimalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Mating>()
                .HasOne(m => m.Father)
                .WithMany(a => a.MatingsAsFather)
                .HasForeignKey(m => m.FatherAnimalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Birth entity
            modelBuilder.Entity<Birth>()
                .HasOne(b => b.Animal)
                .WithMany(a => a.Births)
                .HasForeignKey(b => b.AnimalId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Animal-AnimalSale relationship
            modelBuilder.Entity<Animal>()
                .HasOne(a => a.Sale)
                .WithOne(s => s.Animal)
                .HasForeignKey<AnimalSale>(s => s.AnimalId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed 2 employees as mentioned in the requirements
            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@inyamayethu.co.za",
                    PhoneNumber = "072-123-4567",
                    JobTitle = "Farm Worker",
                    HireDate = new DateTime(2023, 1, 15),
                    IsActive = true,
                    DateOfBirth = new DateTime(1990, 1, 1)
                },
                new Employee
                {
                    Id = 2,
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@inyamayethu.co.za",
                    PhoneNumber = "073-987-6543",
                    JobTitle = "Farm Manager",
                    HireDate = new DateTime(2022, 8, 10),
                    IsActive = true,
                    DateOfBirth = new DateTime(1985, 6, 15)
                }
            );
        }
    }
}
