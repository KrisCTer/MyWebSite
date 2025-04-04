using System.ComponentModel.DataAnnotations;

namespace MyWebSite.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Phone]
        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(100)]
        public string Address { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        [StringLength(50)]
        public string State { get; set; }

        [StringLength(20)]
        public string ZipCode { get; set; }

        [StringLength(50)]
        public string Country { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        public string UserId { get; set; } // Link to ASP.NET Identity User

        public List<Order> Orders { get; set; }
        public List<Review> Reviews { get; set; }
    }
}
