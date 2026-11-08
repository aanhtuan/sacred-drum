# Mẫu kịch bản hiệu ứng — SacredDrum

Đây là mẫu trống để dùng khi Owner giao hiệu ứng cụ thể; không phải yêu cầu triển khai combat, skill hay puzzle.

## Brief

- Tên / ID hiệu ứng:
- Sprint và phạm vi được giao:
- Sự kiện kích hoạt, đối tượng sở hữu và vị trí phát:
- Thông tin người chơi cần hiểu:
- Style / palette / tài liệu art tham chiếu:
- Asset nguồn, tác giả, giấy phép:
- Điều kiện thành công / thất bại / hủy:

## Cue sheet

Dùng mốc mili giây hoặc giây, chỉ rõ mốc tương đối với trigger. Phân biệt thời lượng hiệu ứng và thời gian ảnh hưởng gameplay; không để hiệu ứng tự thay đổi gameplay nếu chưa được giao.

| Mốc bắt đầu–kết thúc | Pha / mục đích | Sprite / animation / particle | Âm thanh | Camera | Kết thúc / cleanup |
| --- | --- | --- | --- | --- | --- |
| Điền | Báo trước nếu cần | Điền | Điền | Điền hoặc không dùng | Điền |
| Điền | Sự kiện chính | Điền | Điền | Điền hoặc không dùng | Điền |
| Điền | Dư âm / hồi phục | Điền | Điền | Điền hoặc không dùng | Điền |

## Ràng buộc triển khai

- Phân biệt tín hiệu quan trọng với trang trí; kiểm tra silhouette ở zoom gameplay và trên nền Level 1.
- Ghi sorting layer/order, PPU, spawn offset và chính sách che Player/UI.
- Đặt giới hạn số instance/particle/light đồng thời, vòng đời, pooling và material dùng chung theo số đo thực tế.
- Mô tả hành vi khi pause, trigger liên tiếp, đối tượng bị disable/destroy, đổi scene hoặc reset.
- Xác định phương án reduced motion / giảm flash; không mặc định thêm rung camera.
- Giữ shader và render path phù hợp Universal 2D/URP đang dùng; không thêm package để chạy ví dụ của skill.

## Kiểm chứng cần ghi lại

| Kiểm tra | Kết quả PASS / FAIL / NOT RUN | Bằng chứng |
| --- | --- | --- |
| Trigger, timing, hướng và vị trí phát đúng | | |
| Tín hiệu rõ khi nhiều hiệu ứng chồng nhau | | |
| Không che Player, đường đi hoặc UI quan trọng | | |
| Không leak khi pause/reset/disable/đổi scene | | |
| Chi phí CPU/GPU, allocation và số instance | | |
| Reduced motion / giảm flash | | |
| Compile, Console, Play Mode và ảnh Game View | | |
