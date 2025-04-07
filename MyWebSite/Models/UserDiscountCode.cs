using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWebSite.Models
{
    public class UserDiscountCode
    {
        [Key]
        [ForeignKey("DiscountCodeId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string UserId { get; set; }  // Liên kết với IdentityUser
        public int DiscountCodeId { get; set; }  // Liên kết với DiscountCode

        // Thêm các thuộc tính cần thiết cho bảng trung gian nếu cần
        public DateTime UsedDate { get; set; }

        public ApplicationUser User { get; set; }
        public DiscountCode DiscountCode { get; set; }
    }
}
