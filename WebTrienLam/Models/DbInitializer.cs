using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace WebTrienLam.Models
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Tự động tạo cơ sở dữ liệu nếu chưa tồn tại
            context.Database.EnsureCreated();

            // Tự động tạo bảng LoaiTrangPhucs nếu chưa tồn tại (cho trường hợp CSDL đã tồn tại trước đó)
            context.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LoaiTrangPhucs' AND xtype='U')
                BEGIN
                    CREATE TABLE LoaiTrangPhucs (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        TenLoai NVARCHAR(100) NOT NULL
                    );
                END
            ");

            // Seed các loại trang phục mẫu nếu trống
            if (!context.LoaiTrangPhucs.Any())
            {
                context.LoaiTrangPhucs.AddRange(
                    new LoaiTrangPhuc { TenLoai = "Triều phục" },
                    new LoaiTrangPhuc { TenLoai = "Bá quan" },
                    new LoaiTrangPhuc { TenLoai = "Trang phục lính" },
                    new LoaiTrangPhuc { TenLoai = "Dân gian" },
                    new LoaiTrangPhuc { TenLoai = "Lễ phục" },
                    new LoaiTrangPhuc { TenLoai = "Phụ kiện" }
                );
                context.SaveChanges();
            }

            // Tự động sửa đường dẫn ảnh và cập nhật lại loại trang phục (nếu CSDL đã được tạo trước đó)
            var currentDbItems = context.TrangPhucs.ToList();
            if (currentDbItems.Any())
            {
                bool changed = false;
                foreach (var item in currentDbItems)
                {
                    // Chuyển ảnh đuôi .jpg thành .png
                    if (item.HinhAnh.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                    {
                        item.HinhAnh = item.HinhAnh.Replace(".jpg", ".png", StringComparison.OrdinalIgnoreCase);
                        changed = true;
                    }

                    // Cập nhật phân loại theo yêu cầu mới:
                    // 1. Hoàng tộc / Trang phục trong cung -> Triều phục
                    if (item.LoaiTrangPhuc == "Hoàng tộc" || item.LoaiTrangPhuc == "Trang phục trong cung")
                    {
                        item.LoaiTrangPhuc = "Triều phục";
                        changed = true;
                    }
                    // 2. Quan lại -> Bá quan
                    else if (item.LoaiTrangPhuc == "Quan lại")
                    {
                        item.LoaiTrangPhuc = "Bá quan";
                        changed = true;
                    }
                    // 3. Thường dân -> Trang phục lính
                    else if (item.LoaiTrangPhuc == "Thường dân")
                    {
                        item.LoaiTrangPhuc = "Trang phục lính";
                        changed = true;
                    }
                }
                if (changed)
                {
                    context.SaveChanges();
                }
            }

            // 1. Seed người dùng nếu chưa có
            if (!context.NguoiDungs.Any())
            {
                var admin = new NguoiDung
                {
                    TenDangNhap = "admin",
                    MatKhau = PasswordHelper.HashPassword("admin123"),
                    HoTen = "Nguyễn Hoàng Nam (Admin)",
                    VaiTro = "Admin",
                    NgayTao = DateTime.Now
                };

                var user = new NguoiDung
                {
                    TenDangNhap = "user",
                    MatKhau = PasswordHelper.HashPassword("user123"),
                    HoTen = "Trần Thị Lan (Cổ Nhân)",
                    VaiTro = "User",
                    NgayTao = DateTime.Now
                };

                context.NguoiDungs.AddRange(admin, user);
                context.SaveChanges();
            }

            // 2. Seed trang phục nếu chưa có
            if (!context.TrangPhucs.Any())
            {
                var dsTrangPhuc = new TrangPhuc[]
                {
                    new TrangPhuc
                    {
                        TenTrangPhuc = "Áo Nhật Bình",
                        LoaiTrangPhuc = "Triều phục",
                        MoTaNgan = "Trang phục trang trọng dành cho Hoàng thái hậu, Hoàng hậu, Công chúa và các phi tần triều Nguyễn.",
                        NoiDungChiTiet = @"<p><strong>Áo Nhật Bình</strong> là triều phục dành cho các cung tần nhất, nhị, tam, tứ giai và là thường phục của Hoàng thái hậu, Hoàng hậu, Công chúa triều Nguyễn. Tên gọi 'Nhật Bình' xuất phát từ dạng thiết kế của cổ áo. Cổ áo to bản hình chữ nhật đối xứng chạy dọc từ ngực xuống, khi mặc hai vạt được cố định bằng một chiếc cúc hoặc dây thắt tạo thành một hình chữ nhật đặc trưng trước ngực.</p>
                                          <p>Áo Nhật Bình được may bằng các chất liệu sa, nhiễu, gấm quý phái thêu dệt nhiều họa tiết hoa văn như chim phượng, hoa lá, chữ Thọ, chữ Hỷ bằng chỉ vàng, chỉ bạc vô cùng tinh xảo. Điểm độc đáo khác là ở tay áo có thêu dệt năm dải màu sắc (lục, vàng, xanh, trắng, đỏ) tượng trưng cho ngũ hành (Mộc, Hỏa, Thổ, Kim, Thủy), biểu thị cho sự hòa hợp của vũ trụ và vị thế tôn quý của người mặc.</p>
                                          <p>Quy chế triều Nguyễn quy định rất nghiêm ngặt về màu sắc áo Nhật Bình tùy theo cấp bậc: Hoàng hậu dùng màu vàng chính sắc (hoàng kim), Công chúa dùng màu đỏ thẫm (đại hồng), các phi tần tùy theo giai phẩm mà dùng màu tím, màu lục hoặc xanh da trời. Đây được xem là một trong những đỉnh cao nghệ thuật may mặc và thêu thùa của Việt Nam thời kỳ phong kiến cận đại.</p>",
                        HinhAnh = "/images/nhat_binh.png",
                        TrieuDai = "Triều Nguyễn",
                        NienDai = "1802 - 1945",
                        LuotXem = 120
                    },
                    new TrangPhuc
                    {
                        TenTrangPhuc = "Áo Tấc (Áo Thụng Ngũ Thân)",
                        LoaiTrangPhuc = "Triều phục",
                        MoTaNgan = "Trang phục truyền thống nghi lễ trang trọng dành cho cả nam và nữ triều Nguyễn.",
                        NoiDungChiTiet = @"<p><strong>Áo Tấc</strong> (hay còn gọi là áo thụng, áo rộng, áo ngũ thân tay chẽn rộng vạt) là một loại lễ phục phổ biến dưới thời nhà Nguyễn. Cái tên 'Áo Tấc' bắt nguồn từ phần viền cổ áo rộng đúng một tấc (khoảng 4cm) theo thước đo cổ.</p>
                                          <p>Áo có thiết kế cổ đứng, năm thân áo (tượng trưng cho tứ thân phụ mẫu và bản thân người mặc), khuy cài bên phải. Điểm đặc trưng nhất của áo Tấc là phần ống tay áo được may rất rộng và dài, khi buông tay xuống ống tay áo dài quá đầu ngón tay khoảng một tấc. Chính thiết kế thụng rộng này tạo nên vẻ ngoài uy nghi, trang trọng và điềm đạm cho người mặc trong các dịp đại lễ.</p>
                                          <p>Áo Tấc thường được mặc kết hợp với quần trắng rộng và đội khăn xếp (khăn đóng) đối với nam giới, hoặc vấn tóc đối với nữ giới. Chất liệu may áo tấc rất đa dạng tùy thuộc vào thời tiết và địa vị xã hội của người mặc, phổ biến là các loại vải sa, gấm, đoạn hay tơ tằm thượng hạng. Đây là bộ lễ phục quốc dân thời Nguyễn, được sử dụng trong các dịp quan trọng như cưới hỏi, cúng bái tế tự, lễ Tết và các nghi thức bang giao.</p>",
                        HinhAnh = "/images/ao_tac.png",
                        TrieuDai = "Triều Nguyễn",
                        NienDai = "1802 - 1945",
                        LuotXem = 95
                    },
                    new TrangPhuc
                    {
                        TenTrangPhuc = "Áo Giao Lĩnh",
                        LoaiTrangPhuc = "Bá quan",
                        MoTaNgan = "Triều phục giao lĩnh với thiết kế cổ chéo tinh tế dành cho hàng quan lại triều đình.",
                        NoiDungChiTiet = @"<p><strong>Áo Giao Lĩnh</strong> (còn gọi là áo đối khâm hay áo cổ chéo) là một trong những kiểu áo có lịch sử lâu đời bậc nhất của người Việt, tồn tại qua nhiều triều đại Lý, Trần, Lê và tiếp tục được sử dụng trang trọng ở thời kỳ đầu nhà Nguyễn dưới dạng triều phục chính thức của quan lại.</p>
                                          <p>Thiết kế đặc trưng của Giao Lĩnh là hai vạt cổ áo bắt chéo nhau (vạt trái đè lên vạt phải) khi mặc, tạo thành hình chữ Y ở cổ. Thân áo rộng rãi, tay áo thụng dài. Đối với triều phục của quan lại (mãng bào), áo được thêu các hình vẽ rồng, mãng xà, mây, sóng nước bằng kỹ thuật thêu kim tuyến nổi bật để biểu thị phẩm cấp trong triều đình.</p>
                                          <p>Mặc dù sau cải cách trang phục của vua Minh Mạng, áo ngũ thân cổ đứng dần thay thế vị trí thường phục, áo Giao Lĩnh cổ chéo vẫn giữ vai trò đặc biệt quan trọng trong các nghi lễ thờ cúng tổ tiên tâm linh, đại tế triều đình. Nó thể hiện tính kết nối văn hóa sâu sắc giữa các triều đại tự chủ trước đó và triều đại nhà Nguyễn.</p>",
                        HinhAnh = "/images/ao_giao_linh.png",
                        TrieuDai = "Triều Nguyễn",
                        NienDai = "Khởi thủy từ thế kỷ 19",
                        LuotXem = 84
                    },
                    new TrangPhuc
                    {
                        TenTrangPhuc = "Áo Ngũ Thân Tay Chẽn",
                        LoaiTrangPhuc = "Trang phục lính",
                        MoTaNgan = "Trang phục hàng ngày và công sở tiện lợi của người dân cũng như quan viên thời Nguyễn.",
                        NoiDungChiTiet = @"<p><strong>Áo Ngũ Thân Tay Chẽn</strong> là tiền thân trực tiếp của chiếc áo dài truyền thống Việt Nam ngày nay. Trang phục này ra đời từ cuộc cải cách trang phục quy mô lớn dưới thời Chúa Nguyễn Phúc Khoát ở Đàng Trong và sau đó được Vua Minh Mạng chuẩn hóa trên toàn quốc từ năm 1836.</p>
                                          <p>Áo có cổ đứng cao, cài 5 khuy bằng đồng, đá quý hoặc gỗ ở sườn bên phải. Phần tay áo từ khuỷu tay đến cổ tay được may bó sát (chẽn) để tạo sự gọn gàng, thuận tiện cho các hoạt động làm việc, di chuyển hàng ngày. Tên gọi ngũ thân tượng trưng cho: hai vạt trước, hai vạt sau và một vạt con (vạt lót nằm bên trong vạt phải) tượng trưng cho người mặc đứng giữa sự đùm bọc của cha mẹ đẻ và cha mẹ vợ/chồng.</p>
                                          <p>Áo Ngũ Thân thể hiện triết lý sống nhân văn và kín đáo của người Việt cổ. Tà áo không bó sát sạt mà suông nhẹ, che khuyết điểm cơ thể và mang lại vẻ thanh lịch tôn nghiêm. Trải qua hơn 100 năm triều Nguyễn, loại áo này đã len lỏi vào mọi ngõ ngách đời sống từ chốn đô thị đến nông thôn Việt Nam.</p>",
                        HinhAnh = "/images/ao_ngu_than.png",
                        TrieuDai = "Triều Nguyễn",
                        NienDai = "1836 - nay",
                        LuotXem = 110
                    },
                    new TrangPhuc
                    {
                        TenTrangPhuc = "Mũ Phượng Hoàng Triều Nguyễn",
                        LoaiTrangPhuc = "Triều phục",
                        MoTaNgan = "Mũ miện cao quý thêu chim phượng hoàng lấp lánh dành cho Hoàng hậu và các Công chúa.",
                        NoiDungChiTiet = @"<p><strong>Mũ Phượng</strong> (Phoenix Crown) là loại mũ lễ phục cao quý nhất dành cho Hoàng thái hậu, Hoàng hậu và Công chúa triều Nguyễn khi tham dự các đại lễ triều nghi như Tế Nam Giao, Lễ vạn thọ, Lễ sắc phong.</p>
                                          <p>Chiếc mũ là một kiệt tác của nghệ thuật kim hoàn cung đình Huế. Khung mũ được làm từ tre hoặc kim loại phủ các lớp vải lụa đen bóng. Toàn bộ mũ được trang trí bằng hàng chục hình chim phượng hoàng chế tác từ vàng ròng, bạc, kết hợp cùng hàng nghìn hạt ngọc trai, san hô và đá quý lấp lánh. Đuôi chim phượng được chạm khắc tinh xảo uốn lượn xung quanh mũ.</p>
                                          <p>Số lượng chim phượng hoàng trên mũ quy định chặt chẽ cấp bậc của người đội: Mũ Phượng của Hoàng hậu thêu 9 con phượng hoàng vàng, mũ của Công chúa thêu 7 con phượng hoàng. Mỗi bước đi của người đội mũ phượng đều toát lên thần thái uy nghi, lộng lẫy và uyển chuyển của hoàng tộc.</p>",
                        HinhAnh = "/images/mu_phuong.png",
                        TrieuDai = "Triều Nguyễn",
                        NienDai = "Thế kỷ 19 - 20",
                        LuotXem = 145
                    }
                };

                context.TrangPhucs.AddRange(dsTrangPhuc);
                context.SaveChanges();
            }
        }
    }
}
