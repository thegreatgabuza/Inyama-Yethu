using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inyama_Yethu.Models
{
    public enum SaleType
    {
        DirectSale,
        AbattoirSale,
        BreedingStock
    }

    public class AnimalSale
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("Animal")]
        public int AnimalId { get; set; }

        [Required]
        [Display(Name = "Sale Date")]
        [DataType(DataType.Date)]
        public DateTime SaleDate { get; set; }

        [Required]
        [Display(Name = "Sale Type")]
        public SaleType SaleType { get; set; }

        [Required]
        [Display(Name = "Sale Price")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SalePrice { get; set; }

        [Display(Name = "Weight at Sale")]
        public double? WeightAtSale { get; set; }

        [Display(Name = "Price per Kg")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PricePerKg { get; set; }

        [Required]
        [Display(Name = "Buyer Name")]
        [StringLength(100)]
        public string BuyerName { get; set; }

        [Required]
        [Display(Name = "Invoice Number")]
        [StringLength(20)]
        public string InvoiceNumber { get; set; }

        [Display(Name = "Payment Received")]
        public bool PaymentReceived { get; set; }

        [Display(Name = "Payment Date")]
        [DataType(DataType.Date)]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "Notes")]
        [StringLength(500)]
        public string Notes { get; set; }

        // Navigation property
        public virtual Animal Animal { get; set; }
    }
} 