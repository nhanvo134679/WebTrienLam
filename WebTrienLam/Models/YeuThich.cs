using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTrienLam.Models
{
    public class YeuThich
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NguoiDungId { get; set; }

        [ForeignKey("NguoiDungId")]
        public virtual NguoiDung? NguoiDung { get; set; }

        [Required]
        public int TrangPhucId { get; set; }

        [ForeignKey("TrangPhucId")]
        public virtual TrangPhuc? TrangPhuc { get; set; }

        public DateTime NgayLuu { get; set; } = DateTime.Now;
    }
}
