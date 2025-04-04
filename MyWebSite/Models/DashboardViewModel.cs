namespace MyWebSite.Models
{
    public class DashboardViewModel
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }

        // Growth Metrics
        public decimal OrdersGrowth { get; set; }
        public decimal RevenueGrowth { get; set; }
        public decimal CustomersGrowth { get; set; }

        // Recent Data
        public List<OrderViewModel> RecentOrders { get; set; }
        public List<ProductViewModel> TopProducts { get; set; }
        public List<ActivitiesViewModel> RecentActivities { get; set; }
    }
}
