using System.ComponentModel.DataAnnotations;

namespace Inyama_Yethu.Models
{
    public class TaskCategory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        public bool IsDefault { get; set; }

        // Indicates if the category can be deleted
        public bool IsSystem { get; set; }
    }
} 