using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWebSite.Models
{
    public class OrderHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;
    }
}
