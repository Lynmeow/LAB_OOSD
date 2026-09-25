# QUẢN LÝ KHÁCH SẠN

## 1. Tổ chức cấu trúc bài tập

Bài tập được xây dựng dưới dạng **Project C# WinForms** với cấu trúc chính như sau:

```text
QuanLyKhachSan/
│
├── Database/
│   └── QuanLyKhachSan.sql       # File cơ sở dữ liệu
│
├── bin/                         # File được tạo sau khi Build
├── obj/                         # File trung gian của Project
├── App.config                   # Cấu hình kết nối cơ sở dữ liệu
├── Program.cs                   # Mã nguồn chính của chương trình
└── QuanLyKhachSan.csproj        # File cấu hình Project
```

Trong đó:

- **`Database/`**: Chứa file `QuanLyKhachSan.sql` liên quan đến cơ sở dữ liệu.
- **`App.config`**: Chứa thông tin cấu hình kết nối giữa chương trình và cơ sở dữ liệu.
- **`Program.cs`**: Chứa mã nguồn chính và các Form chức năng của hệ thống.
- **`QuanLyKhachSan.csproj`**: File cấu hình và quản lý Project C#.

---

## 2. Cơ sở dữ liệu

Hệ thống sử dụng **Microsoft SQL Server** làm hệ quản trị cơ sở dữ liệu.

Cơ sở dữ liệu **`QuanLyKhachSan`** được sử dụng để lưu trữ và quản lý các thông tin của hệ thống quản lý khách sạn, bao gồm:

- Thông tin khách hàng
- Thông tin nhân viên
- Thông tin khu vực
- Thông tin phòng
- Thông tin tiện nghi
- Thông tin đặt phòng
- Thông tin người lưu trú
- Thông tin dịch vụ
- Thông tin sử dụng dịch vụ
- Thông tin hóa đơn và thanh toán
- Thông tin đền bù

File **`QuanLyKhachSan.sql`** chứa các câu lệnh tạo và thiết lập cơ sở dữ liệu.

File **`App.config`** được sử dụng để cấu hình chuỗi kết nối giữa chương trình C# và SQL Server.

---

## 3. Tổ chức mã nguồn trong Program.cs

Để thuận tiện cho việc tổ chức và **upload bài tập lên Git**, toàn bộ mã nguồn của chương trình được **gom chung trong một file `Program.cs`**.

Trong `Program.cs` bao gồm **6 Form chính** của hệ thống:

| STT | Form | Chức năng |
|:---:|---|---|
| 1 | `FrmDanhMuc` | Quản lý danh mục |
| 2 | `FrmPhongTienNghi` | Quản lý phòng và tiện nghi |
| 3 | `FrmDatPhong` | Đặt phòng và quản lý nhận phòng |
| 4 | `FrmDichVu` | Quản lý việc sử dụng dịch vụ |
| 5 | `FrmTraPhong` | Trả phòng và thanh toán |
| 6 | `FrmThongKe` | Thống kê và báo cáo |

### Ghi chú

> Do yêu cầu tổ chức và upload bài tập lên Git, nhóm sử dụng **một file code duy nhất là `Program.cs`** để chứa toàn bộ mã nguồn của 6 Form cùng các phần xử lý liên quan.
>
> Cách tổ chức này giúp mã nguồn được quản lý tập trung và thuận tiện cho việc upload, quản lý và nộp bài trên Git.
