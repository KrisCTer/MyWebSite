namespace MyWebSite.Models
{
    public class RecentOrder
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
    }
}
