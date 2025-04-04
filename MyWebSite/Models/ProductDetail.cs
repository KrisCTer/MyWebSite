﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWebSite.Models
{
    public class ProductDetail
    {
        [Key]
        [ForeignKey("Product")]
        public Guid ProductId { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? Material { get; set; }
        public int StockCount { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public bool IsAvailable { get; set; }
        public Product Product { get; set; } = null!;

    }
}
