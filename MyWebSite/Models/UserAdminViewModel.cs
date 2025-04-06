namespace MyWebSite.Models
{
    public class UserAdminViewModel
    {
        public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();
        public int TotalUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int ActiveUsers { get; set; }
        public int AdminUsers { get; set; }
    }
}
