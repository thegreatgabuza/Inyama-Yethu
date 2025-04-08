using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Inyama_Yethu.ViewModels
{
    public class MortalityRecordViewModel
    {
        [Required]
        [Display(Name = "Animal")]
        public int AnimalId { get; set; }

        [Required]
        [Display(Name = "Date of Death")]
        [DataType(DataType.DateTime)]
        public DateTime RecordDate { get; set; }

        [Required]
        [Display(Name = "Cause of Death")]
        public string CauseOfDeath { get; set; }

        [Display(Name = "Additional Notes")]
        public string Notes { get; set; }
    }

    public class HealthProtocolViewModel
    {
        [Required]
        [Display(Name = "Protocol Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Animal Type")]
        public string AnimalType { get; set; }

        [Display(Name = "Age Group")]
        public string AgeGroup { get; set; }

        [Required]
        [Display(Name = "Treatment Steps")]
        public List<string> TreatmentSteps { get; set; }

        [Display(Name = "Required Medications")]
        public List<string> RequiredMedications { get; set; }

        [Display(Name = "Frequency")]
        public string Frequency { get; set; }

        [Display(Name = "Duration (days)")]
        public int? DurationInDays { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; }
    }

    public class HealthCheckScheduleViewModel
    {
        [Required]
        [Display(Name = "Check Type")]
        public string CheckType { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Scheduled Date")]
        [DataType(DataType.DateTime)]
        public DateTime ScheduledDate { get; set; }

        [Required]
        [Display(Name = "Assigned Employee")]
        public int AssignedEmployeeId { get; set; }

        [Display(Name = "Animal IDs")]
        public List<int> AnimalIds { get; set; }

        [Display(Name = "Protocol")]
        public int? ProtocolId { get; set; }

        [Display(Name = "Priority")]
        public string Priority { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; }

        [Display(Name = "Expected Duration (hours)")]
        [Range(0.5, 8)]
        public double ExpectedDuration { get; set; }
    }
} 