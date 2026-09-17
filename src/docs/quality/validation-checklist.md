
# Validation & Handler Review Checklist

## Mục tiêu
Rà soát validation đầu vào

if validation ít có thể check trong file, hoặc nếu logic phức tạp hoặc có điểm chung tạo file riêng.
Maybe tách file handlers ra, như tách handlers ra
Inventory.Application/
├── Abstractions/
│   └── IInventoryCommandHandler.cs
└── Handlers/
    ├── OrderPlacedHandler.cs
    ├── PaymentSucceededHandler.cs
    └── PaymentFailedHandler.cs


## 1. Validation ở API / Controller

### Orders.Api
- [ ] Kiểm tra các endpoint nhận request body hoặc query.
- [ ] Kiểm tra trường bắt buộc, định dạng và giới hạn dữ liệu.
- [ ] Trả lỗi HTTP phù hợp khi request không hợp lệ.
- [ ] Không đặt toàn bộ nghiệp vụ vào Controller.

### Payments.Api
- [ ] Kiểm tra các endpoint nhận request từ client.
- [ ] Kiểm tra dữ liệu đầu vào trước khi gọi Application.
- [ ] Trả lỗi HTTP phù hợp.

### Inventory.Api
- [ ] Kiểm tra các endpoint xem hoặc điều chỉnh tồn kho.
- [ ] Kiểm tra SKU và số lượng điều chỉnh.
- [ ] Xác định rõ quy tắc cho số lượng âm hoặc SKU chưa tồn tại.

## 2. Validation trong Application Handlers

### Orders.Application
- [ ] Kiểm tra event đầu vào của OrderSagaHandler.
- [ ] Xác định các chuyển trạng thái Order hợp lệ.
- [ ] Xử lý event đến trễ hoặc sai thứ tự.
- [ ] Xác định cách xử lý khi không tìm thấy Order.

### Payments.Application
- [ ] Kiểm tra ReservationSucceededEvent trước khi tính tiền.
- [ ] Kiểm tra Lines, Quantity và UnitPrice.
- [ ] Xác định quy tắc một Payment cho mỗi Order.
- [ ] Giữ nguyên quy tắc thanh toán giả lập hiện tại.

### Inventory.Application
- [ ] Kiểm tra OrderPlacedEvent và Lines.
- [ ] Kiểm tra SKU và Quantity.
- [ ] Xử lý nhiều dòng hàng trùng SKU.
- [ ] Xác định cách bảo vệ cập nhật tồn kho đồng thời.
- [ ] Kiểm tra trạng thái Reservation khi hoàn hoặc tiêu thụ kho.

## 3. Consumers

### Orders / Payments / Inventory
- [ ] Kiểm tra JSON parse và deserialize.
- [ ] Kiểm tra EventId và dữ liệu bắt buộc.
- [ ] Xác nhận consumer nhận đúng loại event.
- [ ] Xác định cách xử lý message không hợp lệ: log, retry hoặc dead-letter.
- [ ] Kiểm tra Inbox chống xử lý trùng.
- [ ] Kiểm tra transaction bao phủ xử lý event và ghi Inbox.

## 4. Outbox và tính nhất quán dữ liệu

- [ ] Business data và Outbox được lưu cùng transaction.
- [ ] Outbox processor xử lý lỗi publish phù hợp.
- [ ] Event có EventId ổn định khi retry publish.
- [ ] Kiểm tra trường hợp consumer nhận lại event.

## 5. DI và cấu trúc

- [ ] Controller gọi đúng Application abstraction.
- [ ] Handler và DbContext được đăng ký đúng lifetime.
- [ ] Consumer dùng đúng scope và DbContext.
- [ ] Không còn đăng ký DI trùng hoặc thiếu.
- [ ] Kiểm tra StockService có đang được API sử dụng.
- [ ] Kiểm tra chức năng bị trùng giữa StockService và InventoryCommandHandler.

## 6. Tests

- [ ] Unit test cho validation hợp lệ và không hợp lệ.
- [ ] Unit test cho các nhánh nghiệp vụ của từng Handler.
- [ ] Test event trùng và event đến sai thứ tự.
- [ ] Test trường hợp thiếu dữ liệu hoặc entity không tồn tại.
- [ ] Test transaction / Inbox / Outbox ở mức integration khi phù hợp.

## Trạng thái
- [ ] Chưa bắt đầu
- [ ] Đang xử lý
- [ ] Đã kiểm tra
- [ ] Đã test