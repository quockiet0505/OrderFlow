# Validation & Handler Review Checklist

## Mục tiêu
Rà soát validation đầu vào và chuẩn hóa cấu trúc Handlers theo Clean Architecture.

Cấu trúc chuẩn đã refactor:
- **`Application/Abstractions/`**: Chứa interfaces (`IOrdersDbContext`, `IInventoryDbContext`, `IPaymentsDbContext`, `IStockService`, `IOrderSagaHandler`, `IInventoryCommandHandler`, `IPaymentCommandHandler`).
- **`Application/Handlers/`**: Tách từng event handler ra 1 file riêng biệt:
  - `Orders.Application/Handlers/`: `ReservationSucceededHandler.cs`, `ReservationFailedHandler.cs`, `PaymentSucceededHandler.cs`, `PaymentFailedHandler.cs`
  - `Inventory.Application/Handlers/`: `OrderPlacedHandler.cs`, `PaymentSucceededHandler.cs`, `PaymentFailedHandler.cs`
  - `Payments.Application/Handlers/`: `ReservationSucceededHandler.cs`


## 1. Validation ở API / Controller

### Orders.Api
- [x] Kiểm tra các endpoint nhận request body hoặc query.
- [x] Kiểm tra trường bắt buộc, định dạng và giới hạn dữ liệu (CustomerId, Lines, Quantity > 0, UnitPrice >= 0).
- [x] Trả lỗi HTTP phù hợp khi request không hợp lệ (HTTP 400 Bad Request).
- [x] Không đặt toàn bộ nghiệp vụ vào Controller.

### Payments.Api
- [x] Kiểm tra các endpoint nhận request từ client.
- [x] Kiểm tra dữ liệu đầu vào trước khi gọi Application (Validation Guid.Empty).
- [x] Trả lỗi HTTP phù hợp (HTTP 400 cho Bad Guid, HTTP 404 khi không tìm thấy).

### Inventory.Api
- [x] Kiểm tra các endpoint xem hoặc điều chỉnh tồn kho.
- [x] Kiểm tra SKU và số lượng điều chỉnh (SKU non-empty, quantity != 0).
- [x] Xác định rõ quy tắc cho số lượng âm hoặc SKU chưa tồn tại.

## 2. Validation trong Application Handlers

### Orders.Application
- [x] Kiểm tra event đầu vào của từng Event Handler.
- [x] Xác định các chuyển trạng thái Order hợp lệ (State Machine guards).
- [x] Xử lý event đến trễ hoặc sai thứ tự.
- [x] Xác định cách xử lý khi không tìm thấy Order.

### Payments.Application
- [x] Kiểm tra ReservationSucceededEvent trước khi tính tiền.
- [x] Kiểm tra Lines, Quantity và UnitPrice.
- [x] Xác định quy tắc một Payment cho mỗi Order (Idempotency check).
- [x] Giữ nguyên quy tắc thanh toán giả lập hiện tại (Tổng tiền kết thúc bằng .99 -> PaymentFailed).

### Inventory.Application
- [x] Kiểm tra OrderPlacedEvent và Lines.
- [x] Kiểm tra SKU và Quantity.
- [x] Xử lý nhiều dòng hàng trùng SKU (Gom nhóm SKU `GroupBy` và cộng dồn số lượng trước khi giữ kho).
- [x] Xác định cách bảo vệ cập nhật tồn kho đồng thời (DB Transaction atomic update).
- [x] Kiểm tra trạng thái Reservation khi hoàn hoặc tiêu thụ kho (Active -> Consumed / Active -> Released).

## 3. Consumers

### Orders / Payments / Inventory
- [x] Kiểm tra JSON parse và deserialize.
- [x] Kiểm tra EventId và dữ liệu bắt buộc.
- [x] Xác nhận consumer nhận đúng loại event.
- [x] Xác định cách xử lý message không hợp lệ: log, retry hoặc dead-letter.
- [x] Kiểm tra Inbox chống xử lý trùng.
- [x] Kiểm tra transaction bao phủ xử lý event và ghi Inbox.

## 4. Outbox và tính nhất quán dữ liệu

- [x] Business data và Outbox được lưu cùng transaction.
- [x] Outbox processor xử lý lỗi publish phù hợp.
- [x] Event có EventId ổn định khi retry publish.
- [x] Kiểm tra trường hợp consumer nhận lại event.

## 5. DI và cấu trúc

- [x] Controller gọi đúng Application abstraction (`IOrdersDbContext`, `IPaymentsDbContext`, `IStockService`).
- [x] Handler và DbContext được đăng ký đúng lifetime (Scoped).
- [x] Consumer dùng đúng scope và DbContext.
- [x] Không còn đăng ký DI trùng hoặc thiếu.
- [x] Kiểm tra StockService có đang được API sử dụng.
- [x] Kiểm tra chức năng bị trùng giữa StockService và InventoryCommandHandler (Đã phân tách rõ nhiệm vụ: StockService phục vụ API điều chỉnh/xem tồn kho, Handlers phục vụ Saga event logic).

## 6. Tests

- [ ] Unit test cho validation hợp lệ và không hợp lệ.
- [ ] Unit test cho các nhánh nghiệp vụ của từng Handler.
- [ ] Test event trùng và event đến sai thứ tự.
- [ ] Test trường hợp thiếu dữ liệu hoặc entity không tồn tại.
- [ ] Test transaction / Inbox / Outbox ở mức integration khi phù hợp.

## Trạng thái
- [ ] Chưa bắt đầu
- [ ] Đang xử lý
- [x] Đã kiểm tra
- [ ] Đã test