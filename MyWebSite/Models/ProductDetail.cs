using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWebSite.Models
{
    public class ProductDetail
    {
        [Key]
        [ForeignKey("Product")]
        public Guid ProductId { get; set; }
        public string? Color { get; set; }
        public string? Material { get; set; }
        public int StockCount { get; set; } = 0;
        public decimal? Price { get; set; }
        public string? Dimensions { get; set; }
        public decimal Discount { get; set; } = 0;
        public bool? IsAvailable { get; set; } = true;
        public string? Warranty { get; set; }
        public Product? Product { get; set; }

    }
}
