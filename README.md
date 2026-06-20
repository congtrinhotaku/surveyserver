# Hệ thống Khảo sát Nhà ở Xã hội (Social Housing Survey System)

Dự án là một hệ thống backend được thiết kế theo kiến trúc Microservices, phục vụ cho nghiệp vụ khảo sát và quản lý thông tin nhà ở xã hội.

## 🏗️ Kiến trúc hệ thống (Architecture)

Hệ thống được chia thành nhiều dịch vụ nhỏ (microservices) để dễ dàng mở rộng và bảo trì:

- **APIGateway**: Cổng giao tiếp API, chịu trách nhiệm nhận các yêu cầu từ client và định tuyến đến các dịch vụ phù hợp bên trong. (Port: 5000)
- **authService**: Dịch vụ xử lý xác thực (Authentication) và phân quyền (Authorization) người dùng.
- **coreService**: Dịch vụ cốt lõi, xử lý các nghiệp vụ chung và quản lý dữ liệu danh mục của hệ thống.
- **SurveyService**: Dịch vụ chuyên biệt xử lý các nghiệp vụ liên quan đến khảo sát, thu thập biểu mẫu và thống kê dữ liệu nhà ở xã hội.

## 🚀 Công nghệ sử dụng

- **Framework**: .NET (C#)
- **Deployment**: Docker, Docker Compose

## 📁 Cấu trúc thư mục

```text
📦 khaosatnhaoxahoi
 ┣ 📂 APIGateway       # Source code API Gateway
 ┣ 📂 authService      # Source code dịch vụ xác thực
 ┣ 📂 coreService      # Source code dịch vụ cốt lõi
 ┣ 📂 SurveyService    # Source code dịch vụ khảo sát
 ┣ 📜 docker-compose.yml # Cấu hình chạy các container Docker
 ┗ 📜 khaosatnhaoxahoi.sln # Solution file của .NET
```

## 🛠️ Hướng dẫn cài đặt và chạy dự án

### Yêu cầu tiên quyết (Prerequisites)
- Đã cài đặt [.NET SDK](https://dotnet.microsoft.com/download) phù hợp với phiên bản của dự án.
- Đã cài đặt [Docker](https://www.docker.com/products/docker-desktop) và Docker Compose.

### Chạy bằng Docker Compose (Khuyên dùng)
Dự án đã được cấu hình sẵn `docker-compose.yml` để build và chạy tất cả các services.

1. Mở terminal tại thư mục gốc của dự án.
2. Chạy lệnh sau để build và khởi động các container:
   ```bash
   docker-compose up -d --build
   ```
3. API Gateway sẽ chạy ở địa chỉ: `http://localhost:5000`

Để dừng hệ thống, bạn có thể chạy:
```bash
docker-compose down
```

### Chạy thủ công từng Service (Development)
Nếu bạn muốn debug trực tiếp trong Visual Studio / Rider:
1. Mở file solution `khaosatnhaoxahoi.sln`.
2. Chọn multiple startup projects (chọn APIGateway và các service cần thiết).
3. Bấm **Start** hoặc `F5` để chạy.

## 📝 Giấy phép (License)
Dự án được phát triển nội bộ.
