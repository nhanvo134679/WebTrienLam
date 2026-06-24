using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTrienLam.Models;

namespace WebTrienLam.Controllers
{
    public class TrangPhucController : Controller
    {
        private readonly AppDbContext _context;

        public TrangPhucController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /TrangPhuc/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trangPhuc = await _context.TrangPhucs
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trangPhuc == null)
            {
                return NotFound();
            }

            // Kiểm tra xem người dùng hiện tại đã lưu trang phục này chưa
            bool isFavorite = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int userId))
                {
                    isFavorite = await _context.YeuThichs
                        .AnyAsync(f => f.NguoiDungId == userId && f.TrangPhucId == trangPhuc.Id);
                }
            }

            ViewBag.IsFavorite = isFavorite;

            return View(trangPhuc);
        }
    }
}
