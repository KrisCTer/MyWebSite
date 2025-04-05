namespace MyWebSite.Models
{
    public class UserDiscountCode
    {
        public string UserId { get; set; }  // Liên kết với IdentityUser
        public int DiscountCodeId { get; set; }  // Liên kết với DiscountCode

        // Thêm các thuộc tính cần thiết cho bảng trung gian nếu cần
        public ApplicationUser User { get; set; }
        public DiscountCode DiscountCode { get; set; }
    }
}
