using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inyama_Yethu.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Product Name")]
        public string Name { get; set; }
        
        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }
        
        [Required]
        [Display(Name = "Price (ZAR)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [Display(Name = "Is Available")]
        public bool IsAvailable { get; set; } = true;
        
        [StringLength(100)]
        [Display(Name = "SKU")]
        public string SKU { get; set; }
        
        [Display(Name = "Creation Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }
        
        [Display(Name = "Minimum Stock Level")]
        public int MinimumStockLevel { get; set; } = 5;
        
        [Display(Name = "Image URL")]
        [StringLength(255)]
        public string ImageUrl { get; set; }
        
        // Navigation properties
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        
        public Product()
        {
            OrderItems = new HashSet<OrderItem>();
        }
    }
} 