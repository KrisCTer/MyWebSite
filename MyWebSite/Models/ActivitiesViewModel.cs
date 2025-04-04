namespace MyWebSite.Models
{
    public class ActivitiesViewModel
    {
        public int ActivityId { get; set; }
        public string Type { get; set; } // "order", "product", "customer", "review", etc.
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
