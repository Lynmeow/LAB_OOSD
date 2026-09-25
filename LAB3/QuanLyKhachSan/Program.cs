using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace QuanLyKhachSan
{
    // =====================================================================
    // 0. CHƯƠNG TRÌNH CHÍNH
    // =====================================================================
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmMain());
        }
    }

    // =====================================================================
    // 1. DATA ACCESS
    // =====================================================================
    public static class Db
    {
        public static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["QuanLyKhachSanDB"].ConnectionString;

        public static SqlConnection OpenConnection()
        {
            var cn = new SqlConnection(ConnectionString);
            cn.Open();
            return cn;
        }

        public static DataTable Query(string sql, params SqlParameter[] ps)
        {
            using var cn = OpenConnection();
            using var cmd = new SqlCommand(sql, cn);
            if (ps != null) cmd.Parameters.AddRange(ps);
            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public static int Execute(string sql, params SqlParameter[] ps)
        {
            using var cn = OpenConnection();
            using var cmd = new SqlCommand(sql, cn);
            if (ps != null) cmd.Parameters.AddRange(ps);
            return cmd.ExecuteNonQuery();
        }

        public static object Scalar(string sql, params SqlParameter[] ps)
        {
            using var cn = OpenConnection();
            using var cmd = new SqlCommand(sql, cn);
            if (ps != null) cmd.Parameters.AddRange(ps);
            return cmd.ExecuteScalar();
        }
    }

    // =====================================================================
    // 2. MODELS DÙNG CHUNG
    // =====================================================================
    public class KetQuaXuLy
    {
        public bool ThanhCong { get; set; }
        public string ThongBao { get; set; }
        public static KetQuaXuLy Ok(string msg) => new KetQuaXuLy { ThanhCong = true, ThongBao = msg };
        public static KetQuaXuLy Fail(string msg) => new KetQuaXuLy { ThanhCong = false, ThongBao = msg };
    }

    public class PhongDatItem
    {
        public string SoPhong { get; set; }
        public int SoNguoi { get; set; }
        public decimal DonGiaNgay { get; set; }
    }

    public class DenBuItem
    {
        public string MaTienNghi { get; set; }
        public string TenLoaiTN { get; set; }
        public string MucDoThietHai { get; set; }
        public decimal SoTien { get; set; }
    }

    // =====================================================================
    // 3. SERVICES (nghiệp vụ + truy cập dữ liệu, Form không viết SQL)
    // =====================================================================
    public class DanhMucService
    {
        public DataTable LayKhuVuc() => Db.Query("SELECT * FROM KhuVuc ORDER BY MaKhuVuc");
        public DataTable LayNhanVien() => Db.Query("SELECT * FROM NhanVien ORDER BY MaNV");
        public DataTable LayLoaiTienNghi() => Db.Query("SELECT * FROM LoaiTienNghi ORDER BY MaLoaiTN");
        public DataTable LayDichVu() => Db.Query("SELECT * FROM DichVu ORDER BY MaDV");
        public DataTable LayQuyDinhDenBu() => Db.Query(
            "SELECT q.*, l.TenLoaiTN FROM QuyDinhDenBu q JOIN LoaiTienNghi l ON q.MaLoaiTN = l.MaLoaiTN ORDER BY q.MaQuyDinh");

        public KetQuaXuLy ThemKhu(string ma, string ten)
        {
            if (string.IsNullOrWhiteSpace(ma) || string.IsNullOrWhiteSpace(ten))
                return KetQuaXuLy.Fail("Mã khu vực và tên khu vực không được để trống.");
            try
            {
                Db.Execute("INSERT INTO KhuVuc VALUES(@m,@t)",
                    new SqlParameter("@m", ma), new SqlParameter("@t", ten));
                return KetQuaXuLy.Ok("Đã thêm khu vực.");
            }
            catch (Exception ex) { return KetQuaXuLy.Fail(ex.Message); }
        }

        public KetQuaXuLy ThemNhanVien(string ma, string ten, string vaiTro, string sdt)
        {
            if (string.IsNullOrWhiteSpace(ma) || string.IsNullOrWhiteSpace(ten) || string.IsNullOrWhiteSpace(vaiTro))
                return KetQuaXuLy.Fail("Thông tin nhân viên chưa đầy đủ.");
            try
            {
                Db.Execute("INSERT INTO NhanVien VALUES(@m,@t,@v,@s)",
                    new SqlParameter("@m", ma), new SqlParameter("@t", ten),
                    new SqlParameter("@v", vaiTro), new SqlParameter("@s", (object)sdt ?? DBNull.Value));
                return KetQuaXuLy.Ok("Đã thêm nhân viên.");
            }
            catch (Exception ex) { return KetQuaXuLy.Fail(ex.Message); }
        }

        public KetQuaXuLy ThemLoaiTN(string ma, string ten)
        {
            if (string.IsNullOrWhiteSpace(ma) || string.IsNullOrWhiteSpace(ten))
                return KetQuaXuLy.Fail("Thông tin loại tiện nghi chưa đủ.");
            try
            {
                Db.Execute("INSERT INTO LoaiTienNghi VALUES(@m,@t)",
                    new SqlParameter("@m", ma), new SqlParameter("@t", ten));
                return KetQuaXuLy.Ok("Đã thêm loại tiện nghi.");
            }
            catch (Exception ex) { return KetQuaXuLy.Fail(ex.Message); }
        }

        public KetQuaXuLy ThemDichVu(string ma, string ten, string dvt, decimal gia)
        {
            if (string.IsNullOrWhiteSpace(ma) || string.IsNullOrWhiteSpace(ten) || string.IsNullOrWhiteSpace(dvt) || gia < 0)
                return KetQuaXuLy.Fail("Thông tin dịch vụ không hợp lệ.");
            try
            {
                Db.Execute("INSERT INTO DichVu VALUES(@m,@t,@d,@g)",
                    new SqlParameter("@m", ma), new SqlParameter("@t", ten),
                    new SqlParameter("@d", dvt), new SqlParameter("@g", gia));
                return KetQuaXuLy.Ok("Đã thêm dịch vụ.");
            }
            catch (Exception ex) { return KetQuaXuLy.Fail(ex.Message); }
        }

        public KetQuaXuLy ThemQuyDinh(string ma, string loai, string muc, decimal tien)
        {
            if (string.IsNullOrWhiteSpace(ma) || string.IsNullOrWhiteSpace(loai) || string.IsNullOrWhiteSpace(muc) || tien < 0)
                return KetQuaXuLy.Fail("Quy định đền bù không hợp lệ.");
            try
            {
                Db.Execute("INSERT INTO QuyDinhDenBu VALUES(@m,@l,@u,@t)",
                    new SqlParameter("@m", ma), new SqlParameter("@l", loai),
                    new SqlParameter("@u", muc), new SqlParameter("@t", tien));
                return KetQuaXuLy.Ok("Đã thêm quy định đền bù.");
            }
            catch (Exception ex) { return KetQuaXuLy.Fail(ex.Message); }
        }
    }

    public class PhongTienNghiService
    {
        public DataTable LayPhong() => Db.Query(
            "SELECT p.*, k.TenKhuVuc FROM Phong p JOIN KhuVuc k ON p.MaKhuVuc = k.MaKhuVuc ORDER BY p.SoPhong");
        public DataTable LayTienNghi() => Db.Query(
            "SELECT t.*, l.TenLoaiTN FROM TienNghi t JOIN LoaiTienNghi l ON t.MaLoaiTN = l.MaLoaiTN ORDER BY t.MaTienNghi");
        public DataTable LayLapDat() => Db.Query(
            "SELECT p.*, l.TenLoaiTN FROM PhieuLapDat p JOIN TienNghi t ON p.MaTienNghi = t.MaTienNghi " +
            "JOIN LoaiTienNghi l ON t.MaLoaiTN = l.MaLoaiTN ORDER BY NgayLap DESC");

        public KetQuaXuLy ThemPhong(string so, string khu, int max, decimal gia)
        {
            if (string.IsNullOrWhiteSpace(so) || string.IsNullOrWhiteSpace(khu) || max <= 0 || gia < 0)
                return KetQuaXuLy.Fail("Thông tin phòng không hợp lệ.");
            try
            {
                Db.Execute("INSERT INTO Phong(SoPhong,MaKhuVuc,SoNguoiToiDa,DonGiaNgay,TrangThai) VALUES(@s,@k,@m,@g,N'Trống')",
                    new SqlParameter("@s", so), new SqlParameter("@k", khu),
                    new SqlParameter("@m", max), new SqlParameter("@g", gia));
                return KetQuaXuLy.Ok("Đã thêm phòng.");
            }
            catch (Exception ex) { return KetQuaXuLy.Fail(ex.Message); }
        }

        public KetQuaXuLy ThemTienNghi(string ma, string loai, int stt, string tt)
        {
            if (string.IsNullOrWhiteSpace(ma) || string.IsNullOrWhiteSpace(loai) || stt <= 0)
                return KetQuaXuLy.Fail("Thông tin tiện nghi không hợp lệ.");
            try
            {
                Db.Execute("INSERT INTO TienNghi VALUES(@m,@l,@s,@t)",
                    new SqlParameter("@m", ma), new SqlParameter("@l", loai),
                    new SqlParameter("@s", stt), new SqlParameter("@t", (object)tt ?? DBNull.Value));
                return KetQuaXuLy.Ok("Đã thêm tiện nghi.");
            }
            catch (Exception ex) { return KetQuaXuLy.Fail(ex.Message); }
        }

        public KetQuaXuLy LapDat(string soPhieu, string maTN, string soPhong, DateTime ngay, string tinhTrang, string maNV, string ghiChu)
        {
            if (string.IsNullOrWhiteSpace(soPhieu) || string.IsNullOrWhiteSpace(maTN) || string.IsNullOrWhiteSpace(soPhong)
                || string.IsNullOrWhiteSpace(tinhTrang) || string.IsNullOrWhiteSpace(maNV))
                return KetQuaXuLy.Fail("Phiếu lắp đặt chưa đủ thông tin.");
            try
            {
                Db.Execute("INSERT INTO PhieuLapDat VALUES(@p,@tn,@ph,@n,@tt,@nv,@g)",
                    new SqlParameter("@p", soPhieu), new SqlParameter("@tn", maTN), new SqlParameter("@ph", soPhong),
                    new SqlParameter("@n", ngay.Date), new SqlParameter("@tt", tinhTrang),
                    new SqlParameter("@nv", maNV), new SqlParameter("@g", (object)ghiChu ?? DBNull.Value));
                Db.Execute("UPDATE TienNghi SET TinhTrangHienTai=@tt WHERE MaTienNghi=@m",
                    new SqlParameter("@tt", tinhTrang), new SqlParameter("@m", maTN));
                return KetQuaXuLy.Ok("Đã lập phiếu lắp đặt.");
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                return KetQuaXuLy.Fail("Thiết bị này đã được lắp cho một phòng khác trong ngày đã chọn.");
            }
            catch (Exception ex) { return KetQuaXuLy.Fail(ex.Message); }
        }
    }

    public class DatPhongService
    {
        public DataTable LayKhach() => Db.Query("SELECT * FROM KhachHang ORDER BY HoTen");
        public DataTable LayPhong() => Db.Query(
            "SELECT p.*, k.TenKhuVuc FROM Phong p JOIN KhuVuc k ON p.MaKhuVuc = k.MaKhuVuc ORDER BY p.SoPhong");
        public DataTable LayPhieuDat() => Db.Query(
            "SELECT d.*, k.HoTen FROM PhieuDatPhong d JOIN KhachHang k ON d.MaKhach = k.MaKhach ORDER BY d.NgayLap DESC");
        public DataTable LayChiTiet(string so) => Db.Query(
            "SELECT c.*, p.SoNguoiToiDa, p.DonGiaNgay FROM ChiTietDatPhong c JOIN Phong p ON c.SoPhong = p.SoPhong WHERE c.SoPhieuDat=@s",
            new SqlParameter("@s", so));
        public DataTable LayNguoiLuuTru(string so) => Db.Query(
            "SELECT * FROM NguoiLuuTru WHERE SoPhieuDat=@s ORDER BY SoPhong,MaNguoiLT", new SqlParameter("@s", so));

        public KetQuaXuLy ThemKhach(string ma, string ten, string cmnd, string qt, string sdt)
        {
            if (string.IsNullOrWhiteSpace(ma) || string.IsNullOrWhiteSpace(ten) || string.IsNullOrWhiteSpace(cmnd) || string.IsNullOrWhiteSpace(qt))
                return KetQuaXuLy.Fail("Thông tin khách chưa đầy đủ.");
            try
            {
                Db.Execute("INSERT INTO KhachHang VALUES(@m,@t,@c,@q,@s)",
                    new SqlParameter("@m", ma), new SqlParameter("@t", ten), new SqlParameter("@c", cmnd),
                    new SqlParameter("@q", qt), new SqlParameter("@s", (object)sdt ?? DBNull.Value));
                return KetQuaXuLy.Ok("Đã lưu khách hàng.");
            }
            catch (Exception ex) { return KetQuaXuLy.Fail(ex.Message); }
        }

        private bool PhongTrungLich(SqlConnection cn, SqlTransaction tx, string phong, DateTime nhan, DateTime tra)
        {
            var cmd = new SqlCommand(@"SELECT COUNT(*) FROM ChiTietDatPhong c JOIN PhieuDatPhong d ON c.SoPhieuDat=d.SoPhieuDat
                WHERE c.SoPhong=@p AND d.TrangThai IN(N'Đã đặt',N'Đang ở') AND @nhan<=d.NgayTraDuKien AND @tra>=d.NgayNhan", cn, tx);
            cmd.Parameters.AddWithValue("@p", phong);
            cmd.Parameters.AddWithValue("@nhan", nhan.Date);
            cmd.Parameters.AddWithValue("@tra", tra.Date);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public KetQuaXuLy TaoDatPhong(string so, string maKhach, string maNV, DateTime ngayLap, DateTime nhan, DateTime tra,
            decimal coc, string kenh, List<PhongDatItem> ds)
        {
            if (string.IsNullOrWhiteSpace(so) || string.IsNullOrWhiteSpace(maKhach) || string.IsNullOrWhiteSpace(maNV) || ds == null || ds.Count == 0)
                return KetQuaXuLy.Fail("Phiếu đặt phòng chưa đủ thông tin.");
            if (tra.Date < nhan.Date) return KetQuaXuLy.Fail("Ngày trả dự kiến không được trước ngày nhận.");

            using var cn = Db.OpenConnection();
            using var tx = cn.BeginTransaction();
            try
            {
                foreach (var x in ds)
                {
                    var q = new SqlCommand("SELECT SoNguoiToiDa FROM Phong WHERE SoPhong=@p", cn, tx);
                    q.Parameters.AddWithValue("@p", x.SoPhong);
                    var o = q.ExecuteScalar();
                    if (o == null) return KetQuaXuLy.Fail("Không tìm thấy phòng " + x.SoPhong);
                    if (x.SoNguoi <= 0 || x.SoNguoi > Convert.ToInt32(o))
                        return KetQuaXuLy.Fail("Số người của phòng " + x.SoPhong + " vượt sức chứa.");
                    if (PhongTrungLich(cn, tx, x.SoPhong, nhan, tra))
                        return KetQuaXuLy.Fail("Phòng " + x.SoPhong + " bị trùng lịch đặt.");
                }

                var h = new SqlCommand(@"INSERT INTO PhieuDatPhong(SoPhieuDat,MaKhach,MaNVLeTan,NgayLap,NgayNhan,NgayTraDuKien,TienCoc,KenhDat,TrangThai)
                    VALUES(@s,@k,@nv,@lap,@nhan,@tra,@c,@kenh,N'Đã đặt')", cn, tx);
                h.Parameters.AddWithValue("@s", so);
                h.Parameters.AddWithValue("@k", maKhach);
                h.Parameters.AddWithValue("@nv", maNV);
                h.Parameters.AddWithValue("@lap", ngayLap);
                h.Parameters.AddWithValue("@nhan", nhan.Date);
                h.Parameters.AddWithValue("@tra", tra.Date);
                h.Parameters.AddWithValue("@c", coc);
                h.Parameters.AddWithValue("@kenh", kenh);
                h.ExecuteNonQuery();

                foreach (var x in ds)
                {
                    var c = new SqlCommand("INSERT INTO ChiTietDatPhong VALUES(@s,@p,@n)", cn, tx);
                    c.Parameters.AddWithValue("@s", so);
                    c.Parameters.AddWithValue("@p", x.SoPhong);
                    c.Parameters.AddWithValue("@n", x.SoNguoi);
                    c.ExecuteNonQuery();

                    var u = new SqlCommand("UPDATE Phong SET TrangThai=N'Đã đặt' WHERE SoPhong=@p", cn, tx);
                    u.Parameters.AddWithValue("@p", x.SoPhong);
                    u.ExecuteNonQuery();
                }

                tx.Commit();
                return KetQuaXuLy.Ok("Đã lập phiếu đặt phòng.");
            }
            catch (Exception ex)
            {
                try { tx.Rollback(); } catch { }
                return KetQuaXuLy.Fail(ex.Message);
            }
        }

        public KetQuaXuLy ThemNguoiLuuTru(string so, string phong, string ten, string cmnd, string qt)
        {
            if (string.IsNullOrWhiteSpace(so) || string.IsNullOrWhiteSpace(phong) || string.IsNullOrWhiteSpace(ten)
                || string.IsNullOrWhiteSpace(cmnd) || string.IsNullOrWhiteSpace(qt))
                return KetQuaXuLy.Fail("Thông tin người lưu trú chưa đầy đủ.");
            try
            {
                int max = Convert.ToInt32(Db.Scalar("SELECT SoNguoi FROM ChiTietDatPhong WHERE SoPhieuDat=@s AND SoPhong=@p",
                    new SqlParameter("@s", so), new SqlParameter("@p", phong)));
                int dem = Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM NguoiLuuTru WHERE SoPhieuDat=@s AND SoPhong=@p",
                    new SqlParameter("@s", so), new SqlParameter("@p", phong)));
                if (dem >= max) return KetQuaXuLy.Fail("Đã đủ số người đăng ký cho phòng này.");

                Db.Execute("INSERT INTO NguoiLuuTru(SoPhieuDat,SoPhong,HoTen,SoCMND,QuocTich) VALUES(@s,@p,@t,@c,@q)",
                    new SqlParameter("@s", so), new SqlParameter("@p", phong), new SqlParameter("@t", ten),
                    new SqlParameter("@c", cmnd), new SqlParameter("@q", qt));
                return KetQuaXuLy.Ok("Đã thêm người lưu trú.");
            }
            catch (Exception ex) { return KetQuaXuLy.Fail(ex.Message); }
        }

        public KetQuaXuLy NhanPhong(string so, DateTime thucTe)
        {
            using var cn = Db.OpenConnection();
            using var tx = cn.BeginTransaction();
            try
            {
                var c = new SqlCommand("UPDATE PhieuDatPhong SET TrangThai=N'Đang ở',NgayNhanThucTe=@n WHERE SoPhieuDat=@s AND TrangThai=N'Đã đặt'", cn, tx);
                c.Parameters.AddWithValue("@n", thucTe);
                c.Parameters.AddWithValue("@s", so);
                if (c.ExecuteNonQuery() == 0) return KetQuaXuLy.Fail("Phiếu không ở trạng thái có thể nhận phòng.");

                var u = new SqlCommand("UPDATE Phong SET TrangThai=N'Đang ở' WHERE SoPhong IN(SELECT SoPhong FROM ChiTietDatPhong WHERE SoPhieuDat=@s)", cn, tx);
                u.Parameters.AddWithValue("@s", so);
                u.ExecuteNonQuery();

                tx.Commit();
                return KetQuaXuLy.Ok("Đã nhận phòng.");
            }
            catch (Exception ex)
            {
                try { tx.Rollback(); } catch { }
                return KetQuaXuLy.Fail(ex.Message);
            }
        }

        public KetQuaXuLy DanhDauNoShow(string so)
        {
            try
            {
                Db.Execute("UPDATE PhieuDatPhong SET TrangThai=N'No-show' WHERE SoPhieuDat=@s AND TrangThai=N'Đã đặt'",
                    new SqlParameter("@s", so));
                Db.Execute("UPDATE Phong SET TrangThai=N'Trống' WHERE SoPhong IN(SELECT SoPhong FROM ChiTietDatPhong WHERE SoPhieuDat=@s)",
                    new SqlParameter("@s", so));
                return KetQuaXuLy.Ok("Đã đánh dấu không nhận phòng.");
            }
            catch (Exception ex) { return KetQuaXuLy.Fail(ex.Message); }
        }
    }

    public class DichVuService
    {
        public DataTable LayPhieuDangO() => Db.Query(
            "SELECT d.SoPhieuDat, k.HoTen, c.SoPhong FROM PhieuDatPhong d JOIN KhachHang k ON d.MaKhach=k.MaKhach " +
            "JOIN ChiTietDatPhong c ON d.SoPhieuDat=c.SoPhieuDat WHERE d.TrangThai=N'Đang ở' ORDER BY d.SoPhieuDat,c.SoPhong");
        public DataTable LayDichVu() => Db.Query("SELECT * FROM DichVu ORDER BY MaDV");
        public DataTable LayLichSu(string so) => Db.Query(
            "SELECT p.SoPhieuSDDV,p.SoPhong,p.NgaySuDung,d.TenDV,c.SoLuong,c.DonGia,c.ThanhTien FROM PhieuSuDungDV p " +
            "JOIN ChiTietPhieuSuDungDV c ON p.SoPhieuSDDV=c.SoPhieuSDDV JOIN DichVu d ON c.MaDV=d.MaDV " +
            "WHERE p.SoPhieuDat=@s ORDER BY p.NgaySuDung,p.SoPhong,d.TenDV", new SqlParameter("@s", so));

        public KetQuaXuLy GhiNhan(string soPhieuDat, string soPhong, DateTime ngay, string maNV, string maDV, int soLuong)
        {
            if (string.IsNullOrWhiteSpace(soPhieuDat) || string.IsNullOrWhiteSpace(soPhong) || string.IsNullOrWhiteSpace(maNV)
                || string.IsNullOrWhiteSpace(maDV) || soLuong <= 0)
                return KetQuaXuLy.Fail("Thông tin sử dụng dịch vụ không hợp lệ.");

            using var cn = Db.OpenConnection();
            using var tx = cn.BeginTransaction();
            try
            {
                var st = new SqlCommand("SELECT TrangThai FROM PhieuDatPhong WHERE SoPhieuDat=@s", cn, tx);
                st.Parameters.AddWithValue("@s", soPhieuDat);
                if (Convert.ToString(st.ExecuteScalar()) != "Đang ở")
                    return KetQuaXuLy.Fail("Chỉ ghi nhận dịch vụ cho phiếu đang lưu trú.");

                var g = new SqlCommand("SELECT DonGia FROM DichVu WHERE MaDV=@d", cn, tx);
                g.Parameters.AddWithValue("@d", maDV);
                object og = g.ExecuteScalar();
                if (og == null) return KetQuaXuLy.Fail("Không tìm thấy dịch vụ.");
                decimal gia = Convert.ToDecimal(og);

                var f = new SqlCommand("SELECT SoPhieuSDDV FROM PhieuSuDungDV WHERE SoPhieuDat=@s AND SoPhong=@p AND NgaySuDung=@n", cn, tx);
                f.Parameters.AddWithValue("@s", soPhieuDat);
                f.Parameters.AddWithValue("@p", soPhong);
                f.Parameters.AddWithValue("@n", ngay.Date);
                string so = Convert.ToString(f.ExecuteScalar());

                if (string.IsNullOrWhiteSpace(so))
                {
                    so = "SD" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    var h = new SqlCommand("INSERT INTO PhieuSuDungDV VALUES(@so,@s,@p,@n,@nv)", cn, tx);
                    h.Parameters.AddWithValue("@so", so);
                    h.Parameters.AddWithValue("@s", soPhieuDat);
                    h.Parameters.AddWithValue("@p", soPhong);
                    h.Parameters.AddWithValue("@n", ngay.Date);
                    h.Parameters.AddWithValue("@nv", maNV);
                    h.ExecuteNonQuery();
                }

                var chk = new SqlCommand("SELECT COUNT(*) FROM ChiTietPhieuSuDungDV WHERE SoPhieuSDDV=@so AND MaDV=@d", cn, tx);
                chk.Parameters.AddWithValue("@so", so);
                chk.Parameters.AddWithValue("@d", maDV);
                if (Convert.ToInt32(chk.ExecuteScalar()) > 0)
                {
                    var u = new SqlCommand("UPDATE ChiTietPhieuSuDungDV SET SoLuong=SoLuong+@sl,DonGia=@g WHERE SoPhieuSDDV=@so AND MaDV=@d", cn, tx);
                    u.Parameters.AddWithValue("@sl", soLuong);
                    u.Parameters.AddWithValue("@g", gia);
                    u.Parameters.AddWithValue("@so", so);
                    u.Parameters.AddWithValue("@d", maDV);
                    u.ExecuteNonQuery();
                }
                else
                {
                    var i = new SqlCommand("INSERT INTO ChiTietPhieuSuDungDV VALUES(@so,@d,@sl,@g)", cn, tx);
                    i.Parameters.AddWithValue("@so", so);
                    i.Parameters.AddWithValue("@d", maDV);
                    i.Parameters.AddWithValue("@sl", soLuong);
                    i.Parameters.AddWithValue("@g", gia);
                    i.ExecuteNonQuery();
                }

                tx.Commit();
                return KetQuaXuLy.Ok("Đã ghi nhận dịch vụ. Cùng dịch vụ trong ngày được cộng dồn vào cùng phiếu.");
            }
            catch (Exception ex)
            {
                try { tx.Rollback(); } catch { }
                return KetQuaXuLy.Fail(ex.Message);
            }
        }
    }

    public class TraPhongService
    {
        public DataTable LayPhieuDangO() => Db.Query(
            "SELECT d.SoPhieuDat,k.HoTen,d.NgayNhanThucTe,d.NgayTraDuKien FROM PhieuDatPhong d " +
            "JOIN KhachHang k ON d.MaKhach=k.MaKhach WHERE d.TrangThai=N'Đang ở' ORDER BY d.SoPhieuDat");
        public DataTable LayPhongTheoPhieu(string so) => Db.Query(
            "SELECT c.SoPhong,p.DonGiaNgay FROM ChiTietDatPhong c JOIN Phong p ON c.SoPhong=p.SoPhong WHERE c.SoPhieuDat=@s",
            new SqlParameter("@s", so));
        public DataTable LayTienNghiPhong(string phong) => Db.Query(@"
            SELECT TOP 100 p.MaTienNghi,l.TenLoaiTN,t.TinhTrangHienTai FROM PhieuLapDat p
            JOIN TienNghi t ON p.MaTienNghi=t.MaTienNghi JOIN LoaiTienNghi l ON t.MaLoaiTN=l.MaLoaiTN
            WHERE p.SoPhong=@p ORDER BY p.NgayLap DESC", new SqlParameter("@p", phong));
        public DataTable LayQuyDinh() => Db.Query(
            "SELECT q.*, l.TenLoaiTN FROM QuyDinhDenBu q JOIN LoaiTienNghi l ON q.MaLoaiTN=l.MaLoaiTN ORDER BY l.TenLoaiTN,q.MucDoThietHai");
        public DataTable LayHoaDon() => Db.Query(
            "SELECT h.*, k.HoTen FROM HoaDon h JOIN PhieuDatPhong d ON h.SoPhieuDat=d.SoPhieuDat " +
            "JOIN KhachHang k ON d.MaKhach=k.MaKhach ORDER BY h.NgayLap DESC");

        public KetQuaXuLy LapPhieuDenBu(string soDB, string soDat, string phong, DateTime ngay, string maNV, List<DenBuItem> ds)
        {
            if (string.IsNullOrWhiteSpace(soDB) || string.IsNullOrWhiteSpace(soDat) || string.IsNullOrWhiteSpace(phong)
                || string.IsNullOrWhiteSpace(maNV) || ds == null || ds.Count == 0)
                return KetQuaXuLy.Fail("Phiếu đền bù chưa đủ thông tin.");

            using var cn = Db.OpenConnection();
            using var tx = cn.BeginTransaction();
            try
            {
                decimal tong = 0;
                foreach (var x in ds)
                {
                    if (x.SoTien < 0) return KetQuaXuLy.Fail("Mức đền bù không hợp lệ.");
                    tong += x.SoTien;
                }

                var h = new SqlCommand("INSERT INTO PhieuDenBu VALUES(@so,@d,@p,@n,@nv,@t)", cn, tx);
                h.Parameters.AddWithValue("@so", soDB);
                h.Parameters.AddWithValue("@d", soDat);
                h.Parameters.AddWithValue("@p", phong);
                h.Parameters.AddWithValue("@n", ngay);
                h.Parameters.AddWithValue("@nv", maNV);
                h.Parameters.AddWithValue("@t", tong);
                h.ExecuteNonQuery();

                foreach (var x in ds)
                {
                    var c = new SqlCommand("INSERT INTO ChiTietPhieuDenBu VALUES(@so,@tn,@m,@t)", cn, tx);
                    c.Parameters.AddWithValue("@so", soDB);
                    c.Parameters.AddWithValue("@tn", x.MaTienNghi);
                    c.Parameters.AddWithValue("@m", x.MucDoThietHai);
                    c.Parameters.AddWithValue("@t", x.SoTien);
                    c.ExecuteNonQuery();
                }

                tx.Commit();
                return KetQuaXuLy.Ok("Đã lập phiếu đền bù.");
            }
            catch (Exception ex)
            {
                try { tx.Rollback(); } catch { }
                return KetQuaXuLy.Fail(ex.Message);
            }
        }

        public KetQuaXuLy LapHoaDon(string soHD, string soDat, DateTime ngay, string maNV, int soNgay)
        {
            if (string.IsNullOrWhiteSpace(soHD) || string.IsNullOrWhiteSpace(soDat) || string.IsNullOrWhiteSpace(maNV) || soNgay <= 0)
                return KetQuaXuLy.Fail("Thông tin hóa đơn chưa hợp lệ.");
            try
            {
                decimal phong = Convert.ToDecimal(Db.Scalar(
                    "SELECT ISNULL(SUM(p.DonGiaNgay),0) FROM ChiTietDatPhong c JOIN Phong p ON c.SoPhong=p.SoPhong WHERE c.SoPhieuDat=@s",
                    new SqlParameter("@s", soDat))) * soNgay;
                decimal dv = Convert.ToDecimal(Db.Scalar(
                    "SELECT ISNULL(SUM(c.ThanhTien),0) FROM PhieuSuDungDV h JOIN ChiTietPhieuSuDungDV c ON h.SoPhieuSDDV=c.SoPhieuSDDV WHERE h.SoPhieuDat=@s",
                    new SqlParameter("@s", soDat)));

                Db.Execute(@"INSERT INTO HoaDon(SoHoaDon,SoPhieuDat,NgayLap,MaNV,SoNgayTinhTien,TienPhong,TienDichVu,TrangThai)
                    VALUES(@h,@s,@n,@nv,@ng,@p,@d,N'Chưa thanh toán')",
                    new SqlParameter("@h", soHD), new SqlParameter("@s", soDat), new SqlParameter("@n", ngay),
                    new SqlParameter("@nv", maNV), new SqlParameter("@ng", soNgay),
                    new SqlParameter("@p", phong), new SqlParameter("@d", dv));

                return KetQuaXuLy.Ok("Đã lập hóa đơn tiền phòng và dịch vụ.");
            }
            catch (Exception ex) { return KetQuaXuLy.Fail(ex.Message); }
        }

        public KetQuaXuLy ThanhToan(string maTT, string soHD, DateTime ngay, string hinhThuc, decimal tien)
        {
            if (string.IsNullOrWhiteSpace(maTT) || string.IsNullOrWhiteSpace(soHD) || string.IsNullOrWhiteSpace(hinhThuc) || tien <= 0)
                return KetQuaXuLy.Fail("Thông tin thanh toán không hợp lệ.");

            using var cn = Db.OpenConnection();
            using var tx = cn.BeginTransaction();
            try
            {
                var q = new SqlCommand("SELECT TongTien FROM HoaDon WHERE SoHoaDon=@h", cn, tx);
                q.Parameters.AddWithValue("@h", soHD);
                object o = q.ExecuteScalar();
                if (o == null) return KetQuaXuLy.Fail("Không tìm thấy hóa đơn.");
                decimal tong = Convert.ToDecimal(o);

                var paid = new SqlCommand("SELECT ISNULL(SUM(SoTien),0) FROM ThanhToan WHERE SoHoaDon=@h", cn, tx);
                paid.Parameters.AddWithValue("@h", soHD);
                decimal da = Convert.ToDecimal(paid.ExecuteScalar());
                if (da + tien > tong) return KetQuaXuLy.Fail("Số tiền thanh toán vượt số còn phải trả.");

                var i = new SqlCommand("INSERT INTO ThanhToan VALUES(@m,@h,@n,@ht,@t)", cn, tx);
                i.Parameters.AddWithValue("@m", maTT);
                i.Parameters.AddWithValue("@h", soHD);
                i.Parameters.AddWithValue("@n", ngay);
                i.Parameters.AddWithValue("@ht", hinhThuc);
                i.Parameters.AddWithValue("@t", tien);
                i.ExecuteNonQuery();

                if (da + tien == tong)
                {
                    var u = new SqlCommand("UPDATE HoaDon SET TrangThai=N'Đã thanh toán' WHERE SoHoaDon=@h", cn, tx);
                    u.Parameters.AddWithValue("@h", soHD);
                    u.ExecuteNonQuery();
                }

                tx.Commit();
                return KetQuaXuLy.Ok("Đã ghi nhận thanh toán bằng " + hinhThuc + ".");
            }
            catch (Exception ex)
            {
                try { tx.Rollback(); } catch { }
                return KetQuaXuLy.Fail(ex.Message);
            }
        }

        public KetQuaXuLy TraPhong(string soDat, DateTime ngayTra)
        {
            using var cn = Db.OpenConnection();
            using var tx = cn.BeginTransaction();
            try
            {
                var q = new SqlCommand("SELECT h.SoHoaDon,h.TrangThai FROM HoaDon h WHERE h.SoPhieuDat=@s", cn, tx);
                q.Parameters.AddWithValue("@s", soDat);
                string tt;
                using (var rd = q.ExecuteReader())
                {
                    if (!rd.Read()) return KetQuaXuLy.Fail("Chưa lập hóa đơn cho phiếu đặt phòng.");
                    tt = Convert.ToString(rd[1]);
                }
                if (tt != "Đã thanh toán") return KetQuaXuLy.Fail("Hóa đơn chưa thanh toán đủ.");

                var u1 = new SqlCommand("UPDATE PhieuDatPhong SET TrangThai=N'Đã trả',NgayTraThucTe=@n WHERE SoPhieuDat=@s", cn, tx);
                u1.Parameters.AddWithValue("@n", ngayTra);
                u1.Parameters.AddWithValue("@s", soDat);
                u1.ExecuteNonQuery();

                var u2 = new SqlCommand("UPDATE Phong SET TrangThai=N'Trống' WHERE SoPhong IN(SELECT SoPhong FROM ChiTietDatPhong WHERE SoPhieuDat=@s)", cn, tx);
                u2.Parameters.AddWithValue("@s", soDat);
                u2.ExecuteNonQuery();

                tx.Commit();
                return KetQuaXuLy.Ok("Đã hoàn tất trả phòng.");
            }
            catch (Exception ex)
            {
                try { tx.Rollback(); } catch { }
                return KetQuaXuLy.Fail(ex.Message);
            }
        }
    }

    public class ThongKeService
    {
        public DataTable TongHop(DateTime tu, DateTime den) => Db.Query(@"
            SELECT
              (SELECT COUNT(*) FROM PhieuDatPhong WHERE CAST(NgayLap AS date) BETWEEN @tu AND @den) SoPhieuDat,
              (SELECT COUNT(*) FROM PhieuDatPhong WHERE TrangThai=N'Đang ở') DangO,
              (SELECT COUNT(*) FROM HoaDon WHERE CAST(NgayLap AS date) BETWEEN @tu AND @den) SoHoaDon,
              (SELECT ISNULL(SUM(TongTien),0) FROM HoaDon WHERE CAST(NgayLap AS date) BETWEEN @tu AND @den) DoanhThuHoaDon,
              (SELECT ISNULL(SUM(TongTien),0) FROM PhieuDenBu WHERE CAST(NgayLap AS date) BETWEEN @tu AND @den) TongDenBu",
            new SqlParameter("@tu", tu.Date), new SqlParameter("@den", den.Date));

        public DataTable DichVu(DateTime tu, DateTime den) => Db.Query(@"
            SELECT d.MaDV, d.TenDV, SUM(c.SoLuong) TongSoLuong, SUM(c.ThanhTien) TongTien
            FROM PhieuSuDungDV p JOIN ChiTietPhieuSuDungDV c ON p.SoPhieuSDDV=c.SoPhieuSDDV JOIN DichVu d ON c.MaDV=d.MaDV
            WHERE p.NgaySuDung BETWEEN @tu AND @den GROUP BY d.MaDV,d.TenDV ORDER BY TongTien DESC",
            new SqlParameter("@tu", tu.Date), new SqlParameter("@den", den.Date));
    }

    // =====================================================================
    // 4. HÀM TIỆN ÍCH TẠO GIAO DIỆN (thay cho Windows Forms Designer)
    // =====================================================================
    internal static class Ui
    {
        // ---- Bảng màu & font dùng chung cho toàn bộ ứng dụng ----
        public static readonly Color ClrBg = Color.FromArgb(244, 246, 250);      // nền form
        public static readonly Color ClrPanel = Color.White;                     // nền khối nội dung
        public static readonly Color ClrPrimary = Color.FromArgb(37, 99, 235);   // xanh dương chính
        public static readonly Color ClrPrimaryDark = Color.FromArgb(29, 78, 216);
        public static readonly Color ClrDanger = Color.FromArgb(220, 53, 69);    // đỏ (No-show, Xóa...)
        public static readonly Color ClrDangerDark = Color.FromArgb(185, 40, 55);
        public static readonly Color ClrSuccess = Color.FromArgb(22, 163, 74);   // xanh lá (Thanh toán, Hoàn tất...)
        public static readonly Color ClrSuccessDark = Color.FromArgb(16, 128, 58);
        public static readonly Color ClrNeutral = Color.FromArgb(108, 117, 125); // xám (Đóng, Thoát)
        public static readonly Color ClrNeutralDark = Color.FromArgb(84, 91, 98);
        public static readonly Color ClrText = Color.FromArgb(33, 37, 41);
        public static readonly Color ClrGridHeader = Color.FromArgb(37, 99, 235);
        public static readonly Color ClrGridAlt = Color.FromArgb(237, 242, 250);
        public static readonly Color ClrGridSel = Color.FromArgb(191, 219, 254);
        public static readonly Color ClrBorder = Color.FromArgb(206, 212, 218);

        public static readonly Font FontBase = new Font("Segoe UI", 9.5F);
        public static readonly Font FontBold = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        public static readonly Font FontTitle = new Font("Segoe UI Semibold", 13F, FontStyle.Bold);
        public static readonly Font FontGridHeader = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);

        /// Áp style chung (màu nền, font) cho một Form và mọi TabControl con của nó.
        public static void ApplyFormTheme(Form f)
        {
            f.BackColor = ClrBg;
            f.Font = FontBase;
            f.ForeColor = ClrText;
        }

        public static Label Label(string text, int x, int y, int w = 120)
            => new Label { Text = text, Location = new Point(x, y), Size = new Size(w, 20), TextAlign = ContentAlignment.MiddleLeft, Font = FontBase, ForeColor = ClrText, BackColor = Color.Transparent };

        /// Nhãn tiêu đề khu vực (in đậm, màu xanh) — dùng để chia nhóm nội dung trong form.
        public static Label SectionTitle(string text, int x, int y, int w = 300)
            => new Label { Text = text, Location = new Point(x, y), Size = new Size(w, 24), Font = FontBold, ForeColor = ClrPrimaryDark, BackColor = Color.Transparent };

        public static TextBox TextBox(int x, int y, int w = 150)
            => new TextBox { Location = new Point(x, y), Size = new Size(w, 24), Font = FontBase, BorderStyle = BorderStyle.FixedSingle };

        public static ComboBox ComboBox(int x, int y, int w = 150)
            => new ComboBox { Location = new Point(x, y), Size = new Size(w, 24), DropDownStyle = ComboBoxStyle.DropDownList, Font = FontBase, FlatStyle = FlatStyle.Flat };

        public static NumericUpDown Numeric(int x, int y, int w = 100, decimal max = 1000000000)
            => new NumericUpDown { Location = new Point(x, y), Size = new Size(w, 24), Maximum = max, DecimalPlaces = 0, Font = FontBase, BorderStyle = BorderStyle.FixedSingle };

        public static NumericUpDown NumericTien(int x, int y, int w = 120)
            => new NumericUpDown { Location = new Point(x, y), Size = new Size(w, 24), Maximum = 1000000000, DecimalPlaces = 0, Increment = 1000, Font = FontBase, BorderStyle = BorderStyle.FixedSingle };

        public static DateTimePicker DatePicker(int x, int y, int w = 130)
            => new DateTimePicker { Location = new Point(x, y), Size = new Size(w, 24), Format = DateTimePickerFormat.Short, Font = FontBase };

        /// Loại nút, quyết định màu sắc: Primary (xanh, mặc định), Success (xanh lá), Danger (đỏ), Neutral (xám - Đóng/Thoát).
        public enum ButtonKind { Primary, Success, Danger, Neutral }

        public static Button Button(string text, int x, int y, int w = 130, int h = 30, ButtonKind kind = ButtonKind.Primary)
        {
            // Tự nhận diện các nút thoát/hủy để tô màu xám cho nhất quán, kể cả khi gọi không truyền kind.
            var t = (text ?? "").ToLowerInvariant();
            if (kind == ButtonKind.Primary && (t.Contains("đóng") || t == "thoát" || t.Contains("hủy") || t.Contains("huỷ")))
                kind = ButtonKind.Neutral;
            if (kind == ButtonKind.Primary && (t.Contains("no-show") || t.Contains("xóa") || t.Contains("xoá")))
                kind = ButtonKind.Danger;
            if (kind == ButtonKind.Primary && (t.Contains("thanh toán") || t.Contains("hoàn tất")))
                kind = ButtonKind.Success;

            Color back, backHover;
            switch (kind)
            {
                case ButtonKind.Success: back = ClrSuccess; backHover = ClrSuccessDark; break;
                case ButtonKind.Danger: back = ClrDanger; backHover = ClrDangerDark; break;
                case ButtonKind.Neutral: back = ClrNeutral; backHover = ClrNeutralDark; break;
                default: back = ClrPrimary; backHover = ClrPrimaryDark; break;
            }

            var btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, h),
                Font = FontBold,
                ForeColor = Color.White,
                BackColor = back,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = backHover;
            btn.FlatAppearance.MouseDownBackColor = backHover;
            return btn;
        }

        public static DataGridView Grid(int x, int y, int w, int h)
        {
            var g = new DataGridView
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = ClrPanel,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = ClrBorder,
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 32,
                RowTemplate = { Height = 26 },
                Font = FontBase
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = ClrGridHeader;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font = FontGridHeader;
            g.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            g.DefaultCellStyle.BackColor = ClrPanel;
            g.DefaultCellStyle.ForeColor = ClrText;
            g.DefaultCellStyle.SelectionBackColor = ClrGridSel;
            g.DefaultCellStyle.SelectionForeColor = ClrText;
            g.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
            g.AlternatingRowsDefaultCellStyle.BackColor = ClrGridAlt;
            return g;
        }

        /// Khung viền nhẹ bao quanh một nhóm control (giả lập GroupBox nhưng không đổi tọa độ con bên trong).
        public static Panel Card(int x, int y, int w, int h)
            => new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = ClrPanel,
                BorderStyle = BorderStyle.FixedSingle
            };
    }

    // =====================================================================
    // 5. FRMMAIN - màn hình chính, điều hướng
    // =====================================================================
    public class FrmMain : Form
    {
        public FrmMain()
        {
            Text = "Hệ thống quản lý khách sạn";
            Size = new Size(420, 380);
            StartPosition = FormStartPosition.CenterScreen;

            var btnDanhMuc = Ui.Button("Danh mục", 40, 30, 320);
            var btnPhong = Ui.Button("Phòng - Tiện nghi", 40, 70, 320);
            var btnDatPhong = Ui.Button("Đặt / Nhận phòng", 40, 110, 320);
            var btnDichVu = Ui.Button("Sử dụng dịch vụ", 40, 150, 320);
            var btnTraPhong = Ui.Button("Trả phòng - Thanh toán", 40, 190, 320);
            var btnThongKe = Ui.Button("Thống kê", 40, 230, 320);
            var btnThoat = Ui.Button("Thoát", 40, 280, 320);

            btnDanhMuc.Click += (s, e) => { using var f = new FrmDanhMuc(); f.ShowDialog(this); };
            btnPhong.Click += (s, e) => { using var f = new FrmPhongTienNghi(); f.ShowDialog(this); };
            btnDatPhong.Click += (s, e) => { using var f = new FrmDatPhong(); f.ShowDialog(this); };
            btnDichVu.Click += (s, e) => { using var f = new FrmDichVu(); f.ShowDialog(this); };
            btnTraPhong.Click += (s, e) => { using var f = new FrmTraPhong(); f.ShowDialog(this); };
            btnThongKe.Click += (s, e) => { using var f = new FrmThongKe(); f.ShowDialog(this); };
            btnThoat.Click += (s, e) =>
            {
                if (MessageBox.Show("Bạn có thực sự muốn thoát?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Close();
            };

            Controls.AddRange(new Control[] { btnDanhMuc, btnPhong, btnDatPhong, btnDichVu, btnTraPhong, btnThongKe, btnThoat });
        }
    }

    // =====================================================================
    // 6. FRMDANHMUC - dữ liệu nền
    // =====================================================================
    public class FrmDanhMuc : Form
    {
        readonly DanhMucService s = new DanhMucService();

        TextBox txtKhuMa, txtKhuTen;
        DataGridView dgvKhu;
        TextBox txtNVMa, txtNVTen, txtNVVaiTro, txtNVSDT;
        DataGridView dgvNV;
        TextBox txtLoaiMa, txtLoaiTen;
        DataGridView dgvLoaiTN;
        TextBox txtDVMa, txtDVTen, txtDVDVT;
        NumericUpDown numDVGia;
        DataGridView dgvDV;
        TextBox txtQDMa, txtQDMucDo;
        ComboBox cboQDLoai;
        NumericUpDown numQDTien;
        DataGridView dgvQD;

        public FrmDanhMuc()
        {
            Text = "Danh mục";
            Size = new Size(760, 560);
            StartPosition = FormStartPosition.CenterParent;

            var tab = new TabControl { Dock = DockStyle.Top, Height = 460 };

            var tabKhu = new TabPage("Khu vực");
            txtKhuMa = Ui.TextBox(110, 20); txtKhuTen = Ui.TextBox(110, 55, 250);
            var btnThemKhu = Ui.Button("Thêm", 380, 35);
            dgvKhu = Ui.Grid(20, 90, 680, 320);
            tabKhu.Controls.AddRange(new Control[] { Ui.Label("Mã khu vực:", 20, 20), txtKhuMa, Ui.Label("Tên khu vực:", 20, 55), txtKhuTen, btnThemKhu, dgvKhu });
            btnThemKhu.Click += (a, e) => H(s.ThemKhu(txtKhuMa.Text.Trim(), txtKhuTen.Text.Trim()));

            var tabNV = new TabPage("Nhân viên");
            txtNVMa = Ui.TextBox(110, 20); txtNVTen = Ui.TextBox(110, 55, 250);
            txtNVVaiTro = Ui.TextBox(110, 90, 180); txtNVSDT = Ui.TextBox(110, 125, 150);
            var btnThemNV = Ui.Button("Thêm", 380, 90);
            dgvNV = Ui.Grid(20, 160, 680, 250);
            tabNV.Controls.AddRange(new Control[] {
                Ui.Label("Mã NV:", 20, 20), txtNVMa, Ui.Label("Họ tên:", 20, 55), txtNVTen,
                Ui.Label("Vai trò:", 20, 90), txtNVVaiTro, Ui.Label("SĐT:", 20, 125), txtNVSDT,
                btnThemNV, dgvNV
            });
            btnThemNV.Click += (a, e) => H(s.ThemNhanVien(txtNVMa.Text.Trim(), txtNVTen.Text.Trim(), txtNVVaiTro.Text.Trim(), txtNVSDT.Text.Trim()));

            var tabLoai = new TabPage("Loại tiện nghi");
            txtLoaiMa = Ui.TextBox(110, 20); txtLoaiTen = Ui.TextBox(110, 55, 250);
            var btnThemLoaiTN = Ui.Button("Thêm", 380, 35);
            dgvLoaiTN = Ui.Grid(20, 90, 680, 320);
            tabLoai.Controls.AddRange(new Control[] { Ui.Label("Mã loại:", 20, 20), txtLoaiMa, Ui.Label("Tên loại:", 20, 55), txtLoaiTen, btnThemLoaiTN, dgvLoaiTN });
            btnThemLoaiTN.Click += (a, e) => H(s.ThemLoaiTN(txtLoaiMa.Text.Trim(), txtLoaiTen.Text.Trim()));

            var tabDV = new TabPage("Dịch vụ");
            txtDVMa = Ui.TextBox(110, 20); txtDVTen = Ui.TextBox(110, 55, 250);
            txtDVDVT = Ui.TextBox(110, 90, 120); numDVGia = Ui.NumericTien(280, 90);
            var btnThemDV = Ui.Button("Thêm", 420, 90);
            dgvDV = Ui.Grid(20, 130, 680, 280);
            tabDV.Controls.AddRange(new Control[] {
                Ui.Label("Mã DV:", 20, 20), txtDVMa, Ui.Label("Tên DV:", 20, 55), txtDVTen,
                Ui.Label("Đơn vị tính:", 20, 90), txtDVDVT, Ui.Label("Đơn giá:", 240, 90, 40), numDVGia,
                btnThemDV, dgvDV
            });
            btnThemDV.Click += (a, e) => H(s.ThemDichVu(txtDVMa.Text.Trim(), txtDVTen.Text.Trim(), txtDVDVT.Text.Trim(), numDVGia.Value));

            var tabQD = new TabPage("Quy định đền bù");
            txtQDMa = Ui.TextBox(110, 20); cboQDLoai = Ui.ComboBox(110, 55, 200);
            txtQDMucDo = Ui.TextBox(340, 55, 150); numQDTien = Ui.NumericTien(110, 90);
            var btnThemQD = Ui.Button("Thêm", 280, 90);
            dgvQD = Ui.Grid(20, 130, 680, 280);
            tabQD.Controls.AddRange(new Control[] {
                Ui.Label("Mã quy định:", 20, 20), txtQDMa, Ui.Label("Loại tiện nghi:", 20, 55, 90), cboQDLoai,
                Ui.Label("Mức độ:", 300, 55, 40), txtQDMucDo, Ui.Label("Mức đền bù:", 20, 90, 90), numQDTien,
                btnThemQD, dgvQD
            });
            btnThemQD.Click += (a, e) => H(s.ThemQuyDinh(txtQDMa.Text.Trim(),
                cboQDLoai.SelectedValue == null ? "" : cboQDLoai.SelectedValue.ToString(),
                txtQDMucDo.Text.Trim(), numQDTien.Value));

            tab.TabPages.AddRange(new[] { tabKhu, tabNV, tabLoai, tabDV, tabQD });

            var btnDong = Ui.Button("Đóng", 620, 475);
            btnDong.Click += (a, e) => Close();

            Controls.Add(tab);
            Controls.Add(btnDong);

            Load += (a, e) => Tai();
        }

        void Tai()
        {
            dgvKhu.DataSource = s.LayKhuVuc();
            dgvNV.DataSource = s.LayNhanVien();
            dgvLoaiTN.DataSource = s.LayLoaiTienNghi();
            dgvDV.DataSource = s.LayDichVu();
            dgvQD.DataSource = s.LayQuyDinhDenBu();

            cboQDLoai.DataSource = s.LayLoaiTienNghi();
            cboQDLoai.DisplayMember = "TenLoaiTN";
            cboQDLoai.ValueMember = "MaLoaiTN";
        }

        void H(KetQuaXuLy k)
        {
            MessageBox.Show(k.ThongBao);
            if (k.ThanhCong) Tai();
        }
    }

    // =====================================================================
    // 7. FRMPHONGTIENNGHI - phòng, tiện nghi, lắp đặt/luân chuyển
    // =====================================================================
    public class FrmPhongTienNghi : Form
    {
        readonly PhongTienNghiService s = new PhongTienNghiService();
        readonly DanhMucService dm = new DanhMucService();

        TextBox txtPhong; ComboBox cboKhu; NumericUpDown numMax; NumericUpDown numGia; DataGridView dgvPhong;
        TextBox txtMaTN; ComboBox cboLoai; NumericUpDown numSTT; TextBox txtTinhTrang; DataGridView dgvTN;
        TextBox txtSoLD; ComboBox cboTN, cboPhong, cboNV; DateTimePicker dtNgay; TextBox txtTTLD, txtGhiChu; DataGridView dgvLD;

        public FrmPhongTienNghi()
        {
            Text = "Phòng - Tiện nghi";
            Size = new Size(780, 560);
            StartPosition = FormStartPosition.CenterParent;

            var tab = new TabControl { Dock = DockStyle.Top, Height = 460 };

            var tabPhong = new TabPage("Phòng");
            txtPhong = Ui.TextBox(110, 20, 120);
            cboKhu = Ui.ComboBox(110, 55, 120);
            numMax = Ui.Numeric(280, 55, 60, 50);
            numGia = Ui.NumericTien(110, 90);
            var btnThemPhong = Ui.Button("Thêm phòng", 280, 90);
            dgvPhong = Ui.Grid(20, 130, 700, 280);
            tabPhong.Controls.AddRange(new Control[] {
                Ui.Label("Số phòng:", 20, 20), txtPhong,
                Ui.Label("Khu vực:", 20, 55), cboKhu, Ui.Label("Sức chứa:", 240, 55, 60), numMax,
                Ui.Label("Đơn giá/ngày:", 20, 90, 90), numGia,
                btnThemPhong, dgvPhong
            });
            btnThemPhong.Click += (a, e) => H(s.ThemPhong(txtPhong.Text.Trim(), V(cboKhu), (int)numMax.Value, numGia.Value));

            var tabTN = new TabPage("Tiện nghi");
            txtMaTN = Ui.TextBox(110, 20, 120);
            cboLoai = Ui.ComboBox(110, 55, 120);
            numSTT = Ui.Numeric(280, 55, 60, 999);
            txtTinhTrang = Ui.TextBox(110, 90, 200);
            var btnThemTN = Ui.Button("Thêm tiện nghi", 320, 90);
            dgvTN = Ui.Grid(20, 130, 700, 280);
            tabTN.Controls.AddRange(new Control[] {
                Ui.Label("Mã tiện nghi:", 20, 20), txtMaTN,
                Ui.Label("Loại:", 240, 55, 40), cboLoai, Ui.Label("Số thứ tự:", 20, 55, 90), numSTT,
                Ui.Label("Tình trạng:", 20, 90, 90), txtTinhTrang,
                btnThemTN, dgvTN
            });
            btnThemTN.Click += (a, e) => H(s.ThemTienNghi(txtMaTN.Text.Trim(), V(cboLoai), (int)numSTT.Value, txtTinhTrang.Text.Trim()));

            var tabLD = new TabPage("Lắp đặt / luân chuyển");
            txtSoLD = Ui.TextBox(110, 20, 140);
            cboTN = Ui.ComboBox(360, 20, 130);
            cboPhong = Ui.ComboBox(110, 55, 130);
            dtNgay = Ui.DatePicker(360, 55, 130);
            txtTTLD = Ui.TextBox(110, 90, 200);
            cboNV = Ui.ComboBox(110, 125, 200);
            txtGhiChu = Ui.TextBox(360, 125, 220);
            var btnLapDat = Ui.Button("Lập phiếu", 590, 90);
            dgvLD = Ui.Grid(20, 165, 700, 245);
            tabLD.Controls.AddRange(new Control[] {
                Ui.Label("Số phiếu:", 20, 20), txtSoLD, Ui.Label("Thiết bị:", 300, 20, 60), cboTN,
                Ui.Label("Phòng:", 20, 55), cboPhong, Ui.Label("Ngày lắp:", 300, 55, 60), dtNgay,
                Ui.Label("Tình trạng:", 20, 90, 90), txtTTLD,
                Ui.Label("Nhân viên:", 20, 125, 90), cboNV, Ui.Label("Ghi chú:", 300, 125, 60), txtGhiChu,
                btnLapDat, dgvLD
            });
            btnLapDat.Click += (a, e) => H(s.LapDat(txtSoLD.Text.Trim(), V(cboTN), V(cboPhong), dtNgay.Value, txtTTLD.Text.Trim(), V(cboNV), txtGhiChu.Text.Trim()));

            tab.TabPages.AddRange(new[] { tabPhong, tabTN, tabLD });

            var btnDong = Ui.Button("Đóng", 640, 475);
            btnDong.Click += (a, e) => Close();

            Controls.Add(tab);
            Controls.Add(btnDong);

            Load += (a, e) => KhoiTao();
        }

        void KhoiTao()
        {
            cboKhu.DataSource = dm.LayKhuVuc(); cboKhu.DisplayMember = "TenKhuVuc"; cboKhu.ValueMember = "MaKhuVuc";
            cboLoai.DataSource = dm.LayLoaiTienNghi(); cboLoai.DisplayMember = "TenLoaiTN"; cboLoai.ValueMember = "MaLoaiTN";
            cboTN.DataSource = s.LayTienNghi(); cboTN.DisplayMember = "MaTienNghi"; cboTN.ValueMember = "MaTienNghi";
            cboPhong.DataSource = s.LayPhong(); cboPhong.DisplayMember = "SoPhong"; cboPhong.ValueMember = "SoPhong";
            cboNV.DataSource = dm.LayNhanVien(); cboNV.DisplayMember = "HoTen"; cboNV.ValueMember = "MaNV";
            Tai();
        }

        void Tai()
        {
            dgvPhong.DataSource = s.LayPhong();
            dgvTN.DataSource = s.LayTienNghi();
            dgvLD.DataSource = s.LayLapDat();
        }

        string V(ComboBox c) => c.SelectedValue == null ? "" : c.SelectedValue.ToString();
        void H(KetQuaXuLy k) { MessageBox.Show(k.ThongBao); if (k.ThanhCong) Tai(); }
    }

    // =====================================================================
    // 8. FRMDATPHONG - khách hàng, đặt phòng, nhận phòng
    // =====================================================================
    public class FrmDatPhong : Form
    {
        readonly DatPhongService s = new DatPhongService();
        readonly DanhMucService dm = new DanhMucService();
        readonly BindingList<PhongDatItem> chon = new BindingList<PhongDatItem>();

        TextBox txtMaKH, txtTenKH, txtCMND, txtQT, txtSDT; DataGridView dgvKhach;
        TextBox txtSoPhieu; ComboBox cboKhach, cboNV, cboKenh; DateTimePicker dtLap, dtNhan, dtTra; NumericUpDown numCoc;
        DataGridView dgvPhong; NumericUpDown numSoNguoi; DataGridView dgvChon; DataGridView dgvPhieu;
        TextBox txtPhieuChon, txtNguoiPhong, txtNguoiTen, txtNguoiCMND, txtNguoiQT; DataGridView dgvCT, dgvNguoi;

        public FrmDatPhong()
        {
            Text = "Đặt / Nhận phòng";
            Size = new Size(900, 700);
            StartPosition = FormStartPosition.CenterParent;

            var tab = new TabControl { Dock = DockStyle.Top, Height = 600 };

            var tabKhach = new TabPage("Khách hàng");
            txtMaKH = Ui.TextBox(110, 20, 120); txtTenKH = Ui.TextBox(330, 20, 200);
            txtCMND = Ui.TextBox(110, 55, 150); txtQT = Ui.TextBox(330, 55, 150); txtSDT = Ui.TextBox(600, 55, 130);
            var btnThemKhach = Ui.Button("Lưu khách hàng", 600, 20);
            dgvKhach = Ui.Grid(20, 100, 800, 380);
            tabKhach.Controls.AddRange(new Control[] {
                Ui.Label("Mã khách:", 20, 20), txtMaKH, Ui.Label("Họ tên:", 260, 20, 70), txtTenKH,
                Ui.Label("CCCD:", 20, 55), txtCMND, Ui.Label("Quốc tịch:", 260, 55, 70), txtQT, Ui.Label("SĐT:", 540, 55, 60), txtSDT,
                btnThemKhach, dgvKhach
            });
            btnThemKhach.Click += (a, e) => H(s.ThemKhach(txtMaKH.Text.Trim(), txtTenKH.Text.Trim(), txtCMND.Text.Trim(), txtQT.Text.Trim(), txtSDT.Text.Trim()));

            var tabDat = new TabPage("Đặt phòng");
            txtSoPhieu = Ui.TextBox(90, 15, 120);
            cboKhach = Ui.ComboBox(300, 15, 180);
            cboNV = Ui.ComboBox(90, 45, 150);
            cboKenh = Ui.ComboBox(300, 45, 150);
            dtLap = Ui.DatePicker(90, 75, 120);
            dtNhan = Ui.DatePicker(300, 75, 120);
            dtTra = Ui.DatePicker(520, 75, 120);
            numCoc = Ui.NumericTien(90, 105);
            dgvPhong = Ui.Grid(20, 140, 380, 200);
            numSoNguoi = Ui.Numeric(420, 140, 60, 20);
            var btnThemPhongVaoPhieu = Ui.Button("Thêm phòng »", 420, 175, 140);
            dgvChon = Ui.Grid(420, 220, 360, 120);
            var btnLapPhieu = Ui.Button("Lập phiếu đặt phòng", 20, 350, 200);
            dgvPhieu = Ui.Grid(20, 390, 760, 190);

            tabDat.Controls.AddRange(new Control[] {
                Ui.Label("Số phiếu:", 20, 15, 65), txtSoPhieu, Ui.Label("Khách:", 260, 15, 40), cboKhach,
                Ui.Label("Lễ tân:", 20, 45, 65), cboNV, Ui.Label("Kênh đặt:", 260, 45, 40), cboKenh,
                Ui.Label("Ngày lập:", 20, 75, 65), dtLap, Ui.Label("Ngày nhận:", 230, 75, 65), dtNhan, Ui.Label("Ngày trả:", 450, 75, 65), dtTra,
                Ui.Label("Tiền cọc:", 20, 105, 65), numCoc,
                dgvPhong, Ui.Label("Số người:", 420, 145, 65), numSoNguoi, btnThemPhongVaoPhieu, dgvChon,
                btnLapPhieu, dgvPhieu
            });
            cboKenh.Items.AddRange(new object[] { "Điện thoại", "Website", "Trực tiếp" });
            btnThemPhongVaoPhieu.Click += (a, e) =>
            {
                if (dgvPhong.CurrentRow == null) return;
                string p = Convert.ToString(dgvPhong.CurrentRow.Cells["SoPhong"].Value);
                foreach (var x in chon) if (x.SoPhong == p) { MessageBox.Show("Phòng đã có trong phiếu."); return; }
                decimal g = Convert.ToDecimal(dgvPhong.CurrentRow.Cells["DonGiaNgay"].Value);
                chon.Add(new PhongDatItem { SoPhong = p, SoNguoi = (int)numSoNguoi.Value, DonGiaNgay = g });
            };
            btnLapPhieu.Click += (a, e) =>
            {
                H(s.TaoDatPhong(txtSoPhieu.Text.Trim(), V(cboKhach), V(cboNV), dtLap.Value, dtNhan.Value, dtTra.Value,
                    numCoc.Value, cboKenh.Text, new List<PhongDatItem>(chon)));
                chon.Clear();
            };

            var tabNhan = new TabPage("Nhận phòng");
            txtPhieuChon = new TextBox { Location = new Point(110, 20), Size = new Size(150, 22), ReadOnly = true };
            dgvCT = Ui.Grid(20, 55, 760, 130);
            txtNguoiPhong = Ui.TextBox(90, 200, 100);
            txtNguoiTen = Ui.TextBox(300, 200, 180);
            txtNguoiCMND = Ui.TextBox(90, 230, 150);
            txtNguoiQT = Ui.TextBox(300, 230, 150);
            var btnThemNguoi = Ui.Button("Thêm người lưu trú", 520, 200, 180);
            dgvNguoi = Ui.Grid(20, 265, 760, 130);
            var btnNhanPhong = Ui.Button("Nhận phòng", 20, 410, 150);
            var btnNoShow = Ui.Button("No-show", 200, 410, 150);
            tabNhan.Controls.AddRange(new Control[] {
                Ui.Label("Phiếu:", 20, 20), txtPhieuChon, dgvCT,
                Ui.Label("Phòng:", 20, 200, 60), txtNguoiPhong, Ui.Label("Họ tên:", 260, 200, 40), txtNguoiTen,
                Ui.Label("CCCD:", 20, 230, 60), txtNguoiCMND, Ui.Label("Quốc tịch:", 260, 230, 40), txtNguoiQT,
                btnThemNguoi, dgvNguoi, btnNhanPhong, btnNoShow
            });
            btnThemNguoi.Click += (a, e) => H(s.ThemNguoiLuuTru(txtPhieuChon.Text.Trim(), txtNguoiPhong.Text.Trim(), txtNguoiTen.Text.Trim(), txtNguoiCMND.Text.Trim(), txtNguoiQT.Text.Trim()));
            btnNhanPhong.Click += (a, e) => H(s.NhanPhong(txtPhieuChon.Text.Trim(), DateTime.Now));
            btnNoShow.Click += (a, e) => H(s.DanhDauNoShow(txtPhieuChon.Text.Trim()));

            tab.TabPages.AddRange(new[] { tabKhach, tabDat, tabNhan });

            var btnDong = Ui.Button("Đóng", 780, 615);
            btnDong.Click += (a, e) => Close();

            Controls.Add(tab);
            Controls.Add(btnDong);
            dgvChon.DataSource = chon;

            dgvPhieu.SelectionChanged += (a, e) =>
            {
                if (dgvPhieu.CurrentRow == null) return;
                string so = Convert.ToString(dgvPhieu.CurrentRow.Cells["SoPhieuDat"].Value);
                txtPhieuChon.Text = so;
                dgvCT.DataSource = s.LayChiTiet(so);
                dgvNguoi.DataSource = s.LayNguoiLuuTru(so);
            };

            Load += (a, e) => KhoiTao();
        }

        void KhoiTao()
        {
            cboKhach.DataSource = s.LayKhach(); cboKhach.DisplayMember = "HoTen"; cboKhach.ValueMember = "MaKhach";
            cboNV.DataSource = dm.LayNhanVien(); cboNV.DisplayMember = "HoTen"; cboNV.ValueMember = "MaNV";
            if (cboKenh.Items.Count > 0) cboKenh.SelectedIndex = 0;
            Tai();
        }

        void Tai()
        {
            dgvKhach.DataSource = s.LayKhach();
            dgvPhong.DataSource = s.LayPhong();
            dgvPhieu.DataSource = s.LayPhieuDat();
        }

        string V(ComboBox c) => c.SelectedValue == null ? "" : c.SelectedValue.ToString();
        void H(KetQuaXuLy k) { MessageBox.Show(k.ThongBao); if (k.ThanhCong) Tai(); }
    }

    // =====================================================================
    // 9. FRMDICHVU - ghi nhận dịch vụ
    // =====================================================================
    public class FrmDichVu : Form
    {
        readonly DichVuService s = new DichVuService();
        readonly DanhMucService dm = new DanhMucService();

        ComboBox cboLuot; TextBox txtPhong; ComboBox cboDV; DateTimePicker dtNgay; NumericUpDown numSL;
        ComboBox cboNV; DataGridView dgvLichSu;

        public FrmDichVu()
        {
            Text = "Sử dụng dịch vụ";
            Size = new Size(760, 520);
            StartPosition = FormStartPosition.CenterParent;

            cboLuot = Ui.ComboBox(140, 20, 220);
            txtPhong = new TextBox { Location = new Point(400, 20), Size = new Size(100, 22), ReadOnly = true };
            cboDV = Ui.ComboBox(140, 55, 220);
            dtNgay = Ui.DatePicker(400, 55, 130);
            numSL = Ui.Numeric(140, 90, 80, 1000);
            cboNV = Ui.ComboBox(400, 90, 200);
            var btnGhi = Ui.Button("Ghi nhận", 570, 55, 150);
            dgvLichSu = Ui.Grid(20, 130, 700, 300);

            Controls.AddRange(new Control[] {
                Ui.Label("Phiếu đang ở:", 20, 20, 110), cboLuot, Ui.Label("Phòng:", 350, 20, 50), txtPhong,
                Ui.Label("Dịch vụ:", 20, 55, 110), cboDV, Ui.Label("Ngày dùng:", 350, 55, 50), dtNgay,
                Ui.Label("Số lượng:", 20, 90, 110), numSL, Ui.Label("Nhân viên:", 350, 90, 50), cboNV,
                btnGhi, dgvLichSu
            });

            var btnDong = Ui.Button("Đóng", 600, 440);
            Controls.Add(btnDong);
            btnDong.Click += (a, e) => Close();

            cboLuot.SelectedIndexChanged += (a, e) =>
            {
                if (cboLuot.SelectedItem is DataRowView r) txtPhong.Text = Convert.ToString(r["SoPhong"]);
                Tai();
            };
            btnGhi.Click += (a, e) =>
            {
                var k = s.GhiNhan(V(cboLuot), txtPhong.Text.Trim(), dtNgay.Value, V(cboNV), V(cboDV), (int)numSL.Value);
                MessageBox.Show(k.ThongBao);
                if (k.ThanhCong) Tai();
            };

            Load += (a, e) => KhoiTao();
        }

        void KhoiTao()
        {
            cboLuot.DataSource = s.LayPhieuDangO(); cboLuot.DisplayMember = "SoPhieuDat"; cboLuot.ValueMember = "SoPhieuDat";
            cboDV.DataSource = s.LayDichVu(); cboDV.DisplayMember = "TenDV"; cboDV.ValueMember = "MaDV";
            cboNV.DataSource = dm.LayNhanVien(); cboNV.DisplayMember = "HoTen"; cboNV.ValueMember = "MaNV";
            Tai();
        }

        void Tai()
        {
            if (cboLuot.SelectedValue != null)
                dgvLichSu.DataSource = s.LayLichSu(cboLuot.SelectedValue.ToString());
        }

        string V(ComboBox c) => c.SelectedValue == null ? "" : c.SelectedValue.ToString();
    }

    // =====================================================================
    // 10. FRMTRAPHONG - đền bù, hóa đơn, thanh toán, trả phòng
    //     (bố cục theo đúng ảnh giao diện mẫu FrmTraPhong)
    // =====================================================================
    public class FrmTraPhong : Form
    {
        readonly TraPhongService s = new TraPhongService();
        readonly DanhMucService dm = new DanhMucService();
        readonly BindingList<DenBuItem> db = new BindingList<DenBuItem>();

        ComboBox cboDat; DataGridView dgvPhong, dgvTN, dgvDBChon;
        TextBox txtPhong, txtSoDB, txtMucDo; NumericUpDown numDenBu;
        TextBox txtSoHD; NumericUpDown numSoNgay; ComboBox cboNV; DataGridView dgvHD;
        TextBox txtHDChon; ComboBox cboHT; NumericUpDown numTienTT;

        public FrmTraPhong()
        {
            Text = "Trả phòng - Đền bù - Hóa đơn - Thanh toán";
            Size = new Size(1000, 780);
            StartPosition = FormStartPosition.CenterParent;

            cboDat = Ui.ComboBox(120, 15, 200);

            dgvPhong = Ui.Grid(20, 50, 300, 150);
            dgvTN = Ui.Grid(340, 50, 300, 150);
            dgvDBChon = Ui.Grid(660, 50, 300, 150);

            txtPhong = new TextBox { Visible = false };

            txtSoDB = Ui.TextBox(140, 215, 120);
            txtMucDo = Ui.TextBox(400, 215, 150);
            numDenBu = Ui.NumericTien(660, 215, 120);
            var btnThemDB = Ui.Button("Thêm vào phiếu", 140, 250, 160);
            var btnLapDB = Ui.Button("Lập phiếu đền bù", 790, 250, 170);

            txtSoHD = Ui.TextBox(140, 300, 120);
            numSoNgay = Ui.Numeric(400, 300, 60, 365);
            cboNV = Ui.ComboBox(660, 300, 180);
            var btnLapHD = Ui.Button("Lập hóa đơn", 490, 335, 140);

            dgvHD = Ui.Grid(20, 375, 940, 190);

            txtHDChon = new TextBox { Visible = false };
            cboHT = Ui.ComboBox(140, 580, 150);
            numTienTT = Ui.NumericTien(400, 580, 140);
            var btnThanhToan = Ui.Button("Thanh toán", 560, 580, 150);
            var btnTraPhong = Ui.Button("Hoàn tất trả phòng", 730, 580, 180);

            var btnDong = Ui.Button("Đóng", 880, 630);

            Controls.AddRange(new Control[] {
                Ui.Label("Phiếu đang ở:", 20, 15, 90), cboDat,
                dgvPhong, dgvTN, dgvDBChon, txtPhong,
                Ui.Label("Số phiếu đền bù:", 20, 215, 110), txtSoDB,
                Ui.Label("Mức độ:", 340, 215, 60), txtMucDo,
                Ui.Label("Số tiền:", 600, 215, 60), numDenBu,
                btnThemDB, btnLapDB,
                Ui.Label("Số hóa đơn:", 20, 300, 110), txtSoHD,
                Ui.Label("Số ngày tính tiền:", 340, 300, 60), numSoNgay,
                Ui.Label("Nhân viên:", 600, 300, 60), cboNV,
                btnLapHD,
                dgvHD, txtHDChon,
                Ui.Label("Hình thức:", 20, 580, 110), cboHT,
                Ui.Label("Số tiền:", 340, 580, 60), numTienTT,
                btnThanhToan, btnTraPhong,
                btnDong
            });

            cboHT.Items.AddRange(new object[] { "Tiền mặt", "Chuyển khoản", "Thẻ", "Ví điện tử" });

            cboDat.SelectedIndexChanged += (a, e) => TaiPhongTheoPhieu();
            dgvPhong.SelectionChanged += (a, e) =>
            {
                if (dgvPhong.CurrentRow == null) return;
                txtPhong.Text = Convert.ToString(dgvPhong.CurrentRow.Cells["SoPhong"].Value);
                dgvTN.DataSource = s.LayTienNghiPhong(txtPhong.Text);
            };
            btnThemDB.Click += (a, e) =>
            {
                if (dgvTN.CurrentRow == null) { MessageBox.Show("Chọn tiện nghi cần đền bù trước."); return; }
                string ma = Convert.ToString(dgvTN.CurrentRow.Cells["MaTienNghi"].Value);
                string ten = Convert.ToString(dgvTN.CurrentRow.Cells["TenLoaiTN"].Value);
                foreach (var x in db) if (x.MaTienNghi == ma) { MessageBox.Show("Tiện nghi đã có trong phiếu đền bù."); return; }
                db.Add(new DenBuItem { MaTienNghi = ma, TenLoaiTN = ten, MucDoThietHai = txtMucDo.Text.Trim(), SoTien = numDenBu.Value });
            };
            btnLapDB.Click += (a, e) =>
            {
                var k = s.LapPhieuDenBu(txtSoDB.Text.Trim(), V(cboDat), txtPhong.Text.Trim(), DateTime.Now, V(cboNV), new List<DenBuItem>(db));
                MessageBox.Show(k.ThongBao);
                if (k.ThanhCong) db.Clear();
            };
            btnLapHD.Click += (a, e) =>
            {
                var k = s.LapHoaDon(txtSoHD.Text.Trim(), V(cboDat), DateTime.Now, V(cboNV), (int)numSoNgay.Value);
                MessageBox.Show(k.ThongBao);
                Tai();
            };
            dgvHD.SelectionChanged += (a, e) =>
            {
                if (dgvHD.CurrentRow != null) txtHDChon.Text = Convert.ToString(dgvHD.CurrentRow.Cells["SoHoaDon"].Value);
            };
            btnThanhToan.Click += (a, e) =>
            {
                string maTT = "TT" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                var k = s.ThanhToan(maTT, txtHDChon.Text.Trim(), DateTime.Now, cboHT.Text, numTienTT.Value);
                MessageBox.Show(k.ThongBao);
                Tai();
            };
            btnTraPhong.Click += (a, e) =>
            {
                var k = s.TraPhong(V(cboDat), DateTime.Now);
                MessageBox.Show(k.ThongBao);
                Tai();
            };
            btnDong.Click += (a, e) => Close();

            dgvDBChon.DataSource = db;

            Load += (a, e) => KhoiTao();
        }

        void KhoiTao()
        {
            cboDat.DataSource = s.LayPhieuDangO(); cboDat.DisplayMember = "SoPhieuDat"; cboDat.ValueMember = "SoPhieuDat";
            cboNV.DataSource = dm.LayNhanVien(); cboNV.DisplayMember = "HoTen"; cboNV.ValueMember = "MaNV";
            Tai();
        }

        void TaiPhongTheoPhieu()
        {
            if (cboDat.SelectedValue == null) return;
            dgvPhong.DataSource = s.LayPhongTheoPhieu(cboDat.SelectedValue.ToString());
        }

        void Tai()
        {
            dgvHD.DataSource = s.LayHoaDon();
            TaiPhongTheoPhieu();
        }

        string V(ComboBox c) => c.SelectedValue == null ? "" : c.SelectedValue.ToString();
    }

    // =====================================================================
    // 11. FRMTHONGKE
    // =====================================================================
    public class FrmThongKe : Form
    {
        readonly ThongKeService s = new ThongKeService();
        DateTimePicker dtTu, dtDen;
        DataGridView dgvTongHop, dgvDV;

        public FrmThongKe()
        {
            Text = "Thống kê";
            Size = new Size(760, 560);
            StartPosition = FormStartPosition.CenterParent;

            dtTu = Ui.DatePicker(90, 20, 150);
            dtDen = Ui.DatePicker(330, 20, 150);
            var btnTK = Ui.Button("Thống kê", 550, 20, 150);
            dgvTongHop = Ui.Grid(20, 60, 700, 150);
            dgvDV = Ui.Grid(20, 230, 700, 220);
            var btnDong = Ui.Button("Đóng", 600, 470);

            Controls.AddRange(new Control[] {
                Ui.Label("Từ ngày:", 20, 20, 60), dtTu, Ui.Label("Đến ngày:", 260, 20, 60), dtDen,
                btnTK, dgvTongHop, dgvDV, btnDong
            });

            dtTu.Value = DateTime.Today.AddMonths(-1);
            dtDen.Value = DateTime.Today;

            btnTK.Click += (a, e) =>
            {
                if (dtDen.Value.Date < dtTu.Value.Date) { MessageBox.Show("Đến ngày không được trước từ ngày."); return; }
                dgvTongHop.DataSource = s.TongHop(dtTu.Value, dtDen.Value);
                dgvDV.DataSource = s.DichVu(dtTu.Value, dtDen.Value);
            };
            btnDong.Click += (a, e) => Close();
        }
    }
}