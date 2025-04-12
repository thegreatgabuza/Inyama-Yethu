using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inyama_Yethu.Models
{
    public class Birth
    {
        public int Id { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        public int MotherAnimalId { get; set; }

        [ForeignKey("MotherAnimalId")]
        public virtual Animal MotherAnimal { get; set; }

        public int? FatherAnimalId { get; set; }

        [ForeignKey("FatherAnimalId")]
        public virtual Animal FatherAnimal { get; set; }

        [Required]
        [Display(Name = "Litter Size")]
        public int LitterSize { get; set; }

        [Required]
        [Display(Name = "Live Born")]
        public int LiveBorn { get; set; }

        [Display(Name = "Average Birth Weight")]
        public double? AverageBirthWeight { get; set; }

        public bool WasAssisted { get; set; }

        [Required]
        public BirthStatus Status { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
    }

    public enum BirthStatus
    {
        Normal,
        Complicated,
        StillBorn
    }
} 