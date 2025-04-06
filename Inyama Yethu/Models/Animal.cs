using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inyama_Yethu.Models
{
    public enum Gender
    {
        Male,
        Female
    }
    
    public enum AnimalType
    {
        Boar,
        Sow,
        Piglet,
        Weaner,
        Grower,
        Finisher
    }
    
    public enum AnimalStatus
    {
        Active,
        Mating,
        Pregnant,
        Farrowing,
        Sold,
        Deceased
    }
    
    public class Animal
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        [Display(Name = "Tag Number")]
        public string TagNumber { get; set; }
        
        [Required]
        public AnimalType Type { get; set; }
        
        [Required]
        public Gender Gender { get; set; }
        
        public DateTime? BirthDate { get; set; }
        
        [Required]
        public AnimalStatus Status { get; set; } = AnimalStatus.Active;
        
        public double? Weight { get; set; }
        
        [StringLength(500)]
        public string Notes { get; set; }
        
        // Parent information (for genetic tracking)
        public int? MotherAnimalId { get; set; }
        public int? FatherAnimalId { get; set; }
        
        // Tracking timestamps
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual ICollection<HealthRecord> HealthRecords { get; set; }
        public virtual ICollection<Mating> MatingsAsMother { get; set; }
        public virtual ICollection<Mating> MatingsAsFather { get; set; }
        public virtual ICollection<Feeding> Feedings { get; set; }
        public virtual ICollection<TaskAssignment> Tasks { get; set; }
        public virtual ICollection<WeightRecord> WeightRecords { get; set; }
        public virtual ICollection<Birth> Births { get; set; }
        
        // Navigate to parent animals
        public virtual Animal Mother { get; set; }
        public virtual Animal Father { get; set; }
        
        // Navigate to child animals
        public virtual ICollection<Animal> Offspring { get; set; }
        
        public Animal()
        {
            HealthRecords = new HashSet<HealthRecord>();
            MatingsAsMother = new HashSet<Mating>();
            MatingsAsFather = new HashSet<Mating>();
            Feedings = new HashSet<Feeding>();
            Tasks = new HashSet<TaskAssignment>();
            WeightRecords = new HashSet<WeightRecord>();
            Offspring = new HashSet<Animal>();
        }
    }
} 