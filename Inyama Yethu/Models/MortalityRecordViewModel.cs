using System;

namespace Inyama_Yethu.Models
{
    public class MortalityRecordViewModel
    {
        public int AnimalId { get; set; }
        public DateTime DeathDate { get; set; }
        public string Cause { get; set; }
        public double? Weight { get; set; }
        public string Notes { get; set; }
    }
} 