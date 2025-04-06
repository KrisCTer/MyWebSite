using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWebSite.Models
{
    public class OrderDetail
    {
        [Key]
        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; } = 0;
        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}
