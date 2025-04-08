using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Inyama_Yethu.ViewModels
{
    public class FeedInventoryViewModel
    {
        [Required]
        [Display(Name = "Feed Type")]
        public string FeedType { get; set; }

        [Required]
        [Display(Name = "Current Stock (kg)")]
        [Range(0, double.MaxValue)]
        public double CurrentStock { get; set; }

        [Required]
        [Display(Name = "Minimum Stock Level (kg)")]
        [Range(0, double.MaxValue)]
        public double MinimumStockLevel { get; set; }

        [Required]
        [Display(Name = "Cost per Kg")]
        [DataType(DataType.Currency)]
        public decimal CostPerKg { get; set; }

        [Display(Name = "Supplier")]
        public string Supplier { get; set; }

        [Display(Name = "Last Restocked Date")]
        [DataType(DataType.Date)]
        public DateTime? LastRestockedDate { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; }
    }

    public class FeedScheduleViewModel
    {
        [Required]
        [Display(Name = "Animal Group")]
        public string AnimalGroup { get; set; }

        [Required]
        [Display(Name = "Feed Type")]
        public string FeedType { get; set; }

        [Required]
        [Display(Name = "Feeding Time")]
        [DataType(DataType.Time)]
        public TimeSpan FeedingTime { get; set; }

        [Required]
        [Display(Name = "Amount per Animal (kg)")]
        [Range(0, double.MaxValue)]
        public double AmountPerAnimal { get; set; }

        [Display(Name = "Special Instructions")]
        public string SpecialInstructions { get; set; }

        [Display(Name = "Assigned Employee")]
        public int? AssignedEmployeeId { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }

    public class FeedBudgetViewModel
    {
        [Required]
        [Display(Name = "Budget Period")]
        public DateTime BudgetPeriod { get; set; }

        [Required]
        [Display(Name = "Total Budget")]
        [DataType(DataType.Currency)]
        public decimal TotalBudget { get; set; }

        [Display(Name = "Feed Type Allocations")]
        public List<FeedTypeAllocation> FeedAllocations { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; }
    }

    public class FeedTypeAllocation
    {
        [Required]
        [Display(Name = "Feed Type")]
        public string FeedType { get; set; }

        [Required]
        [Display(Name = "Allocated Amount")]
        [DataType(DataType.Currency)]
        public decimal AllocatedAmount { get; set; }

        [Display(Name = "Estimated Quantity (kg)")]
        public double EstimatedQuantity { get; set; }
    }
} 