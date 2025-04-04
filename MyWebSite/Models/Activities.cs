using System.ComponentModel.DataAnnotations;

namespace MyWebSite.Models
{
    public class Activities
    {
        [Key]
        public int ActivityId { get; set; }

        [Required]
        [StringLength(20)]
        public string Type { get; set; } // "order", "product", "customer", "review", etc.

        [Required]
        [StringLength(255)]
        public string Description { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string UserId { get; set; }
    }
}
