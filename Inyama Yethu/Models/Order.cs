using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Inyama_Yethu.Models
{
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Processing,
        ReadyForDelivery,
        InTransit,
        Delivered,
        Cancelled
    }
    
    public class Order
    {
        public int Id { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } = DateTime.Now;
        
        [Required]
        [Display(Name = "Delivery Date")]
        public DateTime DeliveryDate { get; set; }
        
        [Required]
        [Display(Name = "Status")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        
        [Required]
        [Display(Name = "Total Amount (ZAR)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        
        [Display(Name = "Payment Date")]
        public DateTime? PaymentDate { get; set; }
        
        [Display(Name = "Payment Received")]
        public bool PaymentReceived { get; set; } = false;
        
        [StringLength(500)]
        [Display(Name = "Notes")]
        public string Notes { get; set; }
        
        [Display(Name = "Delivery Address")]
        [StringLength(200)]
        public string DeliveryAddress { get; set; }
        
        [Display(Name = "Transportation Cost")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TransportationCost { get; set; }
        
        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }
        
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        
        // Not mapped properties
        [NotMapped]
        [Display(Name = "Total Items")]
        public int TotalItems => OrderItems?.Sum(item => item.Quantity) ?? 0;
        
        [NotMapped]
        [Display(Name = "Subtotal")]
        public decimal Subtotal => OrderItems?.Sum(item => item.ItemTotal) ?? 0;
        
        public Order()
        {
            OrderItems = new HashSet<OrderItem>();
        }
    }
} 