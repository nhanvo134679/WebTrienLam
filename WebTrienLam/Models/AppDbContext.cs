using Microsoft.EntityFrameworkCore;

namespace WebTrienLam.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<TrangPhuc> TrangPhucs { get; set; }
        public DbSet<YeuThich> YeuThichs { get; set; }
        public DbSet<LoaiTrangPhuc> LoaiTrangPhucs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure unique index for Username
            modelBuilder.Entity<NguoiDung>()
                .HasIndex(u => u.TenDangNhap)
                .IsUnique();

            // Configure cascade deletes or constraints if needed
            modelBuilder.Entity<YeuThich>()
                .HasOne(f => f.NguoiDung)
                .WithMany(u => u.DanhSachYeuThich)
                .HasForeignKey(f => f.NguoiDungId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<YeuThich>()
                .HasOne(f => f.TrangPhuc)
                .WithMany(p => p.DanhSachYeuThich)
                .HasForeignKey(f => f.TrangPhucId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
