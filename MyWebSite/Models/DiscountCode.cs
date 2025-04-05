using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MyWebSite.Models
{
    public class DiscountCode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required(ErrorMessage = "Mã giảm giá là bắt buộc.")]
        public string Code { get; set; }

        [Range(1, 100, ErrorMessage = "Phần trăm giảm giá phải từ 1 đến 100.")]
        public int DiscountPercentage { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lần sử dụng phải lớn hơn 0.")]
        public int MaxUsage { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lần sử dụng thực tế phải không âm.")]
        public int UsageCount { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Ngày bắt đầu không hợp lệ.")]
        public DateTime ValidFrom { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Ngày kết thúc không hợp lệ.")]
        public DateTime ValidTo { get; set; }

        public bool Status { get; set; }
    }
}
