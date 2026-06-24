using System.ComponentModel.DataAnnotations;

namespace WebTrienLam.Models
{
    public class LoaiTrangPhuc
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên loại trang phục không được để trống")]
        [StringLength(100, ErrorMessage = "Tên loại trang phục không quá 100 ký tự")]
        public string TenLoai { get; set; } = string.Empty;
    }
}
