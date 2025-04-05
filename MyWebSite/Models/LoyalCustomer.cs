using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MyWebSite.Models
{
    public class LoyalCustomer
    {
        [Key]
        public int Id { get; set; }

        // Foreign key đến ApplicationUser
        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public DateTime JoinedDate { get; set; } = DateTime.Now;

        public int RewardPoints { get; set; } = 0;
    }
}
