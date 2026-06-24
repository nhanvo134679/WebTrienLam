       using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTrienLam.Models;

namespace WebTrienLam.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Home/Index
        public async Task<IActionResult> Index(string? searchQuery, string? category)
        {
            ViewData["CurrentSearch"] = searchQuery;
            ViewData["CurrentCategory"] = category;

            // Truy vấn danh sách trang phục
            var query = _context.TrangPhucs.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(p => p.TenTrangPhuc.Contains(searchQuery));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.LoaiTrangPhuc == category);
            }

            var results = await query.OrderByDescending(p => p.NgayTao).ToListAsync();

            // Lấy danh sách ID các trang phục đã lưu của User hiện tại (nếu có)
            var savedCostumeIds = new HashSet<int>();
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int userId))
                {
                    savedCostumeIds = (await _context.YeuThichs
                        .Where(f => f.NguoiDungId == userId)
                        .Select(f => f.TrangPhucId)
                        .ToListAsync()).ToHashSet();
                }
            }

            ViewBag.SavedCostumeIds = savedCostumeIds;

            // Lấy danh sách danh mục để lọc ở View từ bảng LoaiTrangPhucs động
            ViewBag.Categories = await _context.LoaiTrangPhucs
                .Select(p => p.TenLoai)
                .ToListAsync();

            return View(results);
        }

        // POST: /Home/ToggleFavorite (API gọi bằng Ajax)
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int costumeId)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                // Trả về thông tin yêu cầu chuyển hướng đăng nhập
                string returnUrl = HttpContext.Request.Headers["Referer"].ToString() ?? "/";
                return Json(new { success = false, redirectUrl = Url.Action("Login", "Account", new { returnUrl = returnUrl }) });
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Json(new { success = false, message = "Lỗi xác thực người dùng." });
            }

            var costume = await _context.TrangPhucs.FindAsync(costumeId);
            if (costume == null)
            {
                return Json(new { success = false, message = "Không tìm thấy hiện vật/trang phục." });
            }

            var favorite = await _context.YeuThichs
                .FirstOrDefaultAsync(f => f.NguoiDungId == userId && f.TrangPhucId == costumeId);

            bool isSaved;
            if (favorite != null)
            {
                // Đã lưu -> Xóa khỏi danh sách yêu thích
                _context.YeuThichs.Remove(favorite);
                isSaved = false;
            }
            else
            {
                // Chưa lưu -> Thêm vào danh sách yêu thích
                var newFavorite = new YeuThich
                {
                    NguoiDungId = userId,
                    TrangPhucId = costumeId,
                    NgayLuu = DateTime.Now
                };
                _context.YeuThichs.Add(newFavorite);
                isSaved = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, isSaved = isSaved });
        }

        // GET: /Home/Favorites (Trang hiển thị bài viết/trang phục yêu thích của User)
        [Authorize]
        public async Task<IActionResult> Favorites()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var listFavorites = await _context.YeuThichs
                .Where(f => f.NguoiDungId == userId)
                .Include(f => f.TrangPhuc)
                .OrderByDescending(f => f.NgayLuu)
                .Select(f => f.TrangPhuc)
                .ToListAsync();

            return View(listFavorites);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
