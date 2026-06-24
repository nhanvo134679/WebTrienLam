using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebTrienLam.Models
{
    public class NguoiDung
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập không quá 50 ký tự")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(255)]
        public string MatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên không quá 100 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string VaiTro { get; set; } = "User"; // "Admin" hoặc "User"

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation property
        public virtual ICollection<YeuThich> DanhSachYeuThich { get; set; } = new List<YeuThich>();
    }
}
