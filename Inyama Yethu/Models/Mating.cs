using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inyama_Yethu.Models
{
    public enum MatingStatus
    {
        Scheduled,
        Completed,
        PregnancyConfirmed,
        NotPregnant,
        Farrowed,
        Aborted
    }
    
    public class Mating
    {
        public int Id { get; set; }
        
        [Required]
        public int MotherAnimalId { get; set; }
        
        [Required]
        public int FatherAnimalId { get; set; }
        
        [Required]
        [Display(Name = "Mating Date")]
        public DateTime MatingDate { get; set; }
        
        [Required]
        public MatingStatus Status { get; set; } = MatingStatus.Scheduled;
        
        // Pregnancy check 1 (21 days)
        [Display(Name = "Expected Pregnancy Check 1")]
        public DateTime? ExpectedPregnancyCheck1 { get; set; }
        [Display(Name = "Pregnancy Check 1 Date")]
        public DateTime? PregnancyCheck1Date { get; set; }
        [Display(Name = "Pregnancy Check 1 Result")]
        public bool? PregnancyCheck1Result { get; set; }
        
        // Pregnancy check 2 (42 days)
        [Display(Name = "Expected Pregnancy Check 2")]
        public DateTime? ExpectedPregnancyCheck2 { get; set; }
        [Display(Name = "Pregnancy Check 2 Date")]
        public DateTime? PregnancyCheck2Date { get; set; }
        [Display(Name = "Pregnancy Check 2 Result")]
        public bool? PregnancyCheck2Result { get; set; }
        
        // Pregnancy check details
        [StringLength(500)]
        [Display(Name = "Pregnancy Check Notes")]
        public string PregnancyCheckNotes { get; set; }
        [StringLength(50)]
        [Display(Name = "Pregnancy Checked By")]
        public string PregnancyCheckBy { get; set; }
        
        [Display(Name = "Expected Farrowing Date")]
        public DateTime? ExpectedFarrowingDate { get; set; }
        
        [Display(Name = "Expected Vaccination Date 1")]
        public DateTime? ExpectedVaccinationDate1 { get; set; }
        [Display(Name = "Vaccination 1 Completed")]
        public bool Vaccination1Completed { get; set; } = false;
        
        [Display(Name = "Expected Vaccination Date 2")]
        public DateTime? ExpectedVaccinationDate2 { get; set; }
        [Display(Name = "Vaccination 2 Completed")]
        public bool Vaccination2Completed { get; set; } = false;
        
        [Display(Name = "Actual Farrowing Date")]
        public DateTime? ActualFarrowingDate { get; set; }
        
        [Display(Name = "Number of Piglets Born")]
        public int? NumberOfPigletsBorn { get; set; }
        
        [Display(Name = "Number of Piglets Born Alive")]
        public int? NumberOfPigletsBornAlive { get; set; }
        
        [StringLength(500)]
        public string Notes { get; set; }
        
        // New properties needed for views
        [Display(Name = "Total Piglets Born")]
        public int? TotalPigletsBorn => NumberOfPigletsBorn;
        
        [Display(Name = "Liveborn Piglets")]
        public int? PigletsLiveborn => NumberOfPigletsBornAlive;
        
        [Display(Name = "Stillborn Piglets")]
        public int? PigletsStillborn => NumberOfPigletsBorn.HasValue && NumberOfPigletsBornAlive.HasValue 
            ? NumberOfPigletsBorn.Value - NumberOfPigletsBornAlive.Value 
            : null;
            
        [Display(Name = "Mummified Piglets")]
        public int? PigletsMummified { get; set; }
        
        // Record keeping fields
        public int? RecordedById { get; set; }
        public DateTime? RecordedDate { get; set; }
        
        [ForeignKey("RecordedById")]
        public virtual Employee RecordedBy { get; set; }
        
        // Navigation properties
        [ForeignKey("MotherAnimalId")]
        public virtual Animal Mother { get; set; }
        
        [ForeignKey("FatherAnimalId")]
        public virtual Animal Father { get; set; }
        
        // Offspring (piglets born from this mating)
        public virtual ICollection<Animal> Offspring { get; set; }
        
        // Calculate gestational age in days
        [NotMapped]
        public int? GestationalAge => Status == MatingStatus.PregnancyConfirmed && !ActualFarrowingDate.HasValue ? (int)(DateTime.Now - MatingDate).TotalDays : null;
        
        public Mating()
        {
            Offspring = new HashSet<Animal>();
            
            // Set default expectation dates based on mating date
            ExpectedPregnancyCheck1 = MatingDate.AddDays(18); // Check between day 18-21
            ExpectedPregnancyCheck2 = MatingDate.AddDays(42); // Check at day 42
            ExpectedFarrowingDate = MatingDate.AddDays(115); // Expected between day 114-116
            ExpectedVaccinationDate1 = MatingDate.AddDays(100); // Vaccination 1 at day 100
            ExpectedVaccinationDate2 = MatingDate.AddDays(107); // Vaccination 2 at day 107
        }
    }
} 