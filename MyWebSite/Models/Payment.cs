using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWebSite.Models
{
    public class Payment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public int OrderId { get; set; }

        [Required]
        [StringLength(50)]
        public string Method { get; set; } // e.g., Credit Card, PayPal, etc.

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending"; // e.g., Pending, Completed, Failed

        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;
    }
}
