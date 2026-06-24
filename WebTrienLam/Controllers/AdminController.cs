using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTrienLam.Models;

namespace WebTrienLam.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Lấy số liệu thống kê đơn giản
            ViewBag.TotalCostumes = await _context.TrangPhucs.CountAsync();
            ViewBag.TotalUsers = await _context.NguoiDungs.CountAsync();
            ViewBag.TotalFavorites = await _context.YeuThichs.CountAsync();

            var listCostumes = await _context.TrangPhucs
                .OrderByDescending(p => p.NgayTao)
                .ToListAsync();

            return View(listCostumes);
        }

        // GET: /Admin/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.LoaiTrangPhucs = await _context.LoaiTrangPhucs.ToListAsync();
            return View();
        }

        // POST: /Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenTrangPhuc,LoaiTrangPhuc,MoTaNgan,NoiDungChiTiet,TrieuDai,NienDai")] TrangPhuc trangPhuc, IFormFile? AnhUp)
        {
            ModelState.Remove("HinhAnh"); // Bỏ qua kiểm tra bắt buộc của chuỗi HinhAnh từ form

            if (AnhUp == null || AnhUp.Length == 0)
            {
                ModelState.AddModelError("HinhAnh", "Vui lòng tải lên tệp tin hình ảnh cho hiện vật.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý tải và lưu file ảnh
                    string webRoot = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string uploadsFolder = Path.Combine(webRoot, "images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(AnhUp!.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await AnhUp.CopyToAsync(fileStream);
                    }

                    trangPhuc.HinhAnh = "/images/" + uniqueFileName;
                    trangPhuc.NgayTao = DateTime.Now;
                    trangPhuc.LuotXem = 0;
                    
                    _context.Add(trangPhuc);
                    await _context.SaveChangesAsync();
                    
                    TempData["AdminSuccess"] = "Thêm hiện vật trang phục thành công!";
                    return RedirectToAction(nameof(Dashboard));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi thêm hiện vật: " + ex.Message);
                }
            }
            ViewBag.LoaiTrangPhucs = await _context.LoaiTrangPhucs.ToListAsync();
            return View(trangPhuc);
        }

        // GET: /Admin/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trangPhuc = await _context.TrangPhucs.FindAsync(id);
            if (trangPhuc == null)
            {
                return NotFound();
            }
            ViewBag.LoaiTrangPhucs = await _context.LoaiTrangPhucs.ToListAsync();
            return View(trangPhuc);
        }

        // POST: /Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenTrangPhuc,LoaiTrangPhuc,MoTaNgan,NoiDungChiTiet,HinhAnh,TrieuDai,NienDai,LuotXem,NgayTao")] TrangPhuc trangPhuc, IFormFile? AnhUp)
        {
            if (id != trangPhuc.Id)
            {
                return NotFound();
            }

            ModelState.Remove("HinhAnh"); // Bỏ qua kiểm tra dạng chuỗi vì tự xử lý

            if (ModelState.IsValid)
            {
                try
                {
                    if (AnhUp != null && AnhUp.Length > 0)
                    {
                        // Xử lý lưu ảnh mới
                        string webRoot = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        string uploadsFolder = Path.Combine(webRoot, "images");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(AnhUp.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await AnhUp.CopyToAsync(fileStream);
                        }

                        trangPhuc.HinhAnh = "/images/" + uniqueFileName;
                    }

                    if (string.IsNullOrEmpty(trangPhuc.HinhAnh))
                    {
                        ModelState.AddModelError("HinhAnh", "Hiện vật bắt buộc phải có hình ảnh.");
                        ViewBag.LoaiTrangPhucs = await _context.LoaiTrangPhucs.ToListAsync();
                        return View(trangPhuc);
                    }

                    _context.Update(trangPhuc);
                    await _context.SaveChangesAsync();
                    TempData["AdminSuccess"] = "Cập nhật thông tin hiện vật thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrangPhucExists(trangPhuc.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Lỗi xung đột đồng thời khi cập nhật dữ liệu. Vui lòng tải lại trang.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật hiện vật: " + ex.Message);
                }
                return RedirectToAction(nameof(Dashboard));
            }
            ViewBag.LoaiTrangPhucs = await _context.LoaiTrangPhucs.ToListAsync();
            return View(trangPhuc);
        }

        // POST: /Admin/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var trangPhuc = await _context.TrangPhucs.FindAsync(id);
            if (trangPhuc != null)
            {
                _context.TrangPhucs.Remove(trangPhuc);
                await _context.SaveChangesAsync();
                TempData["AdminSuccess"] = "Đã xóa hiện vật thành công khỏi kho dữ liệu!";
            }
            return RedirectToAction(nameof(Dashboard));
        }

        // GET: /Admin/LoaiTrangPhucList
        [HttpGet]
        public async Task<IActionResult> LoaiTrangPhucList()
        {
            var list = await _context.LoaiTrangPhucs.ToListAsync();
            return View(list);
        }

        // GET: /Admin/CreateLoaiTrangPhuc
        [HttpGet]
        public IActionResult CreateLoaiTrangPhuc()
        {
            return View();
        }

        // POST: /Admin/CreateLoaiTrangPhuc
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLoaiTrangPhuc([Bind("TenLoai")] LoaiTrangPhuc model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                TempData["AdminSuccess"] = "Thêm loại trang phục mới thành công!";
                return RedirectToAction(nameof(LoaiTrangPhucList));
            }
            return View(model);
        }

        // GET: /Admin/EditLoaiTrangPhuc/5
        [HttpGet]
        public async Task<IActionResult> EditLoaiTrangPhuc(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.LoaiTrangPhucs.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        // POST: /Admin/EditLoaiTrangPhuc/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLoaiTrangPhuc(int id, [Bind("Id,TenLoai")] LoaiTrangPhuc model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    TempData["AdminSuccess"] = "Cập nhật loại trang phục thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.LoaiTrangPhucs.Any(e => e.Id == model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(LoaiTrangPhucList));
            }
            return View(model);
        }

        // POST: /Admin/DeleteLoaiTrangPhuc/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLoaiTrangPhuc(int id)
        {
            var item = await _context.LoaiTrangPhucs.FindAsync(id);
            if (item != null)
            {
                _context.LoaiTrangPhucs.Remove(item);
                await _context.SaveChangesAsync();
                TempData["AdminSuccess"] = "Đã xóa loại trang phục thành công!";
            }
            return RedirectToAction(nameof(LoaiTrangPhucList));
        }

        private bool TrangPhucExists(int id)
        {
            return _context.TrangPhucs.Any(e => e.Id == id);
        }
    }
}
