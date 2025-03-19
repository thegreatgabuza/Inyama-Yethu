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
        public int AnimalId { get; set; }

        [ForeignKey("AnimalId")]
        public Animal Animal { get; set; }

        public string Notes { get; set; }

        [Required]
        public int? NumberOfOffspring { get; set; }

        public bool WasAssisted { get; set; }

        [Required]
        public BirthStatus Status { get; set; }
    }

    public enum BirthStatus
    {
        Normal,
        Complicated,
        StillBorn
    }
} 