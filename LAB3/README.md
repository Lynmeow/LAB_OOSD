## 1. Tổ chức cấu trúc bài tập
Bài tập được tổ chức dưới dạng một Project C# WinForms với cấu trúc chính như sau:

```text
QuanLyKhachSan/
│
├── Database/                    # Các thành phần liên quan đến cơ sở dữ liệu
├── App.config                   # Cấu hình kết nối cơ sở dữ liệu
├── Program.cs                   # Mã nguồn chính của chương trình
└── QuanLyKhachSan.csproj        # File cấu hình Project
Trong đó, Program.cs là file chứa phần mã nguồn chính của hệ thống. Các thành phần còn lại được sử dụng để cấu hình Project và kết nối với cơ sở dữ liệu.

2. Cơ sở dữ liệu
Hệ thống sử dụng Microsoft SQL Server làm hệ quản trị cơ sở dữ liệu.
Cơ sở dữ liệu được sử dụng để lưu trữ và quản lý các thông tin của hệ thống quản lý khách sạn, bao gồm:
Thông tin khách hàng
Thông tin nhân viên
Thông tin phòng
Thông tin khu vực
Thông tin tiện nghi
Thông tin đặt phòng
Thông tin người lưu trú
Thông tin dịch vụ
Thông tin sử dụng dịch vụ
Thông tin hóa đơn và thanh toán
Thông tin đền bù

File App.config được sử dụng để cấu hình chuỗi kết nối giữa chương trình C# và SQL Server.
3. Tổ chức mã nguồn trong Program.cs
Để thuận tiện cho việc nộp và quản lý bài tập trên Git, toàn bộ phần mã nguồn của chương trình được gom chung trong một file Program.cs.
Trong Program.cs bao gồm các Form chính của hệ thống:
FrmDanhMuc – Quản lý các danh mục.
FrmPhongTienNghi – Quản lý phòng và tiện nghi.
FrmDatPhong – Đặt phòng và quản lý nhận phòng.
FrmDichVu – Quản lý việc sử dụng dịch vụ.
FrmTraPhong – Trả phòng và thanh toán.
FrmThongKe – Thống kê và báo cáo.
Ghi chú
Do yêu cầu tổ chức và upload bài tập lên Git, nhóm sử dụng một file code duy nhất là Program.cs để chứa toàn bộ mã nguồn của các Form và các phần xử lý liên quan. Cách tổ chức này giúp bài tập dễ dàng quản lý và upload lên repository.
