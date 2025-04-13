using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inyama_Yethu.Models
{
    /// <summary>
    /// Represents a financial transaction in the system.
    /// This model centralizes all financial activities for better reporting and analysis.
    /// </summary>
    public class FinancialTransaction
    {
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// Type of transaction (Income, Expense, Transfer, Adjustment)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string TransactionType { get; set; }
        
        /// <summary>
        /// Category of transaction (Feed, Healthcare, Transport, DirectSale, AbattoirSale, etc.)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Category { get; set; }
        
        /// <summary>
        /// Transaction amount (positive number)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        
        /// <summary>
        /// Description of the transaction
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }
        
        /// <summary>
        /// Reference number (invoice, receipt, order number, etc.)
        /// </summary>
        [StringLength(100)]
        public string ReferenceNumber { get; set; }
        
        /// <summary>
        /// Date when the financial transaction occurred
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; }
        
        /// <summary>
        /// Date when the transaction was recorded in the system
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }
        
        /// <summary>
        /// Optional ID of related entity (AnimalId, FeedingId, HealthRecordId, etc.)
        /// </summary>
        public int? RelatedEntityId { get; set; }
        
        /// <summary>
        /// Name of the related entity table (Animal, Feeding, HealthRecord, etc.)
        /// </summary>
        [StringLength(100)]
        [Required]
        public string RelatedEntityType { get; set; } = "None";
        
        /// <summary>
        /// Payment method used (Cash, Bank Transfer, Credit Card, etc.)
        /// </summary>
        [StringLength(50)]
        public string PaymentMethod { get; set; }
        
        /// <summary>
        /// Whether this transaction has been reconciled with banking records
        /// </summary>
        public bool IsReconciled { get; set; }
        
        /// <summary>
        /// Username of the person who recorded the transaction
        /// </summary>
        [StringLength(256)]
        public string RecordedBy { get; set; }
        
        /// <summary>
        /// Additional notes about the transaction
        /// </summary>
        [StringLength(1000)]
        public string Notes { get; set; }
    }
} 