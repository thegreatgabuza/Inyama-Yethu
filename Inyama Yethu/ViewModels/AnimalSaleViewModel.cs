using System;
using System.ComponentModel.DataAnnotations;
using Inyama_Yethu.Models;

namespace Inyama_Yethu.ViewModels
{
    public class AnimalSaleViewModel
    {
        [Required(ErrorMessage = "Animal is required")]
        [Display(Name = "Animal")]
        public int AnimalId { get; set; }

        [Required(ErrorMessage = "Sale date is required")]
        [Display(Name = "Sale Date")]
        [DataType(DataType.Date)]
        public DateTime SaleDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Sale type is required")]
        [Display(Name = "Sale Type")]
        public SaleType SaleType { get; set; }

        [Required(ErrorMessage = "Sale price is required")]
        [Display(Name = "Sale Price (ZAR)")]
        [Range(0.01, 1000000, ErrorMessage = "Price must be between 0.01 and 1,000,000 ZAR")]
        public decimal SalePrice { get; set; }

        [Display(Name = "Weight at Sale (kg)")]
        [Range(0.1, 1000, ErrorMessage = "Weight must be between 0.1 and 1000 kg")]
        public double? WeightAtSale { get; set; }

        [Display(Name = "Buyer Name")]
        [StringLength(100, ErrorMessage = "Buyer name cannot exceed 100 characters")]
        public string BuyerName { get; set; }

        [Display(Name = "Invoice Number")]
        [StringLength(100, ErrorMessage = "Invoice number cannot exceed 100 characters")]
        public string InvoiceNumber { get; set; }

        [Display(Name = "Payment Received")]
        public bool PaymentReceived { get; set; }

        [Display(Name = "Payment Date")]
        [DataType(DataType.Date)]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "Notes")]
        [DataType(DataType.MultilineText)]
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string Notes { get; set; }
    }
} 