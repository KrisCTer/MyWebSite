namespace MyWebSite.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime RegistrationDate { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public int OrderCount { get; set; }
    }
}
