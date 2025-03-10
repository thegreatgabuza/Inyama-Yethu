using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inyama_Yethu.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }
        
        [Required]
        [Display(Name = "Unit Price (ZAR)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        
        [NotMapped]
        [Display(Name = "Item Total (ZAR)")]
        public decimal ItemTotal => Quantity * UnitPrice;
        
        [StringLength(100)]
        [Display(Name = "Special Instructions")]
        public string SpecialInstructions { get; set; }
        
        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
        
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
} 