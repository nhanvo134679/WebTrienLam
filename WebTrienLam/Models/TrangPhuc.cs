using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebTrienLam.Models
{
    public class TrangPhuc
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên trang phục không được để trống")]
        [StringLength(100, ErrorMessage = "Tên trang phục không quá 100 ký tự")]
        public string TenTrangPhuc { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại trang phục không được để trống")]
        [StringLength(50)]
        public string LoaiTrangPhuc { get; set; } = string.Empty; // Triều phục, Bá quan, Trang phục lính, Dân gian, v.v.

        [Required(ErrorMessage = "Mô tả ngắn không được để trống")]
        [StringLength(500, ErrorMessage = "Mô tả ngắn không quá 500 ký tự")]
        public string MoTaNgan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung chi tiết không được để trống")]
        public string NoiDungChiTiet { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hình ảnh không được để trống")]
        [StringLength(255)]
        public string HinhAnh { get; set; } = string.Empty;

        [StringLength(50)]
        public string TrieuDai { get; set; } = "Nhà Nguyễn";

        [StringLength(50)]
        public string NienDai { get; set; } = "1802 - 1945";

        public int LuotXem { get; set; } = 0;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation property
        public virtual ICollection<YeuThich> DanhSachYeuThich { get; set; } = new List<YeuThich>();
    }
}
