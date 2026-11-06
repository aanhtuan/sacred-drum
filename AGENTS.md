# SacredDrum — Quy tắc làm việc cho Codex

- Chỉ làm trong Sprint/phạm vi Project Owner đã giao. Sprint 0 không có gameplay; không tự động bắt đầu Sprint 1.
- Dùng C# và Unity 2D. Editor hiện tại: 6000.5.10f1, Universal 2D / URP 17.6.0. Không nâng cấp Editor/package ngoài yêu cầu.
- Kiểm tra checkout, git status và các thay đổi có sẵn trước khi sửa; bảo toàn công việc của người dùng.
- Ưu tiên Unity MCP để chỉnh Scene/Prefab. Đọc projectRoot, editor state, scene hierarchy trước; chọn đúng instance SacredDrum.
- Chỉ báo MCP CONNECTED khi đọc Unity Editor thực tế thành công; nếu chỉ phát hiện server, báo NOT VERIFIED.
- Không tự ý thay đổi scene quan trọng, scene gốc hoặc asset ngoài phạm vi. Không sửa Scene YAML khi có thể thao tác bằng Unity Editor/MCP.
- Giữ nguyên tài nguyên và GUID `.meta`; tạo/import thư mục và asset qua Unity khi có thể. Commit `.meta` cùng asset; giữ `.gitkeep` khi thư mục còn trống.
- Sau khi sửa script, đợi compile/domain reload hoàn tất và đọc Console. Báo rõ PASS/FAIL/NOT RUN; compile sạch không chứng minh gameplay hoặc Windows build chạy đúng.
- Không import asset chưa rõ giấy phép. Không thêm gameplay, Player Controller hay Enemy AI ngoài yêu cầu.
- Không commit Library, Temp, Obj, Logs, UserSettings, Build/Builds, token, mật khẩu, private key hoặc URI chứa credentials.
- Review file stage, diff --stat và diff --check trước commit; stage có chọn lọc. Giữ Assets, Packages và ProjectSettings được quản lý.
- Dùng Git LFS theo .gitattributes cho nguồn asset lớn; không migrate lịch sử tùy ý. Không force push, không xóa lịch sử, không reset --hard lên dữ liệu chưa sao lưu.
- Chỉ push khi yêu cầu người dùng cho phép và remote đồng bộ an toàn. Nếu có lịch sử khác nhau hoặc nguy cơ xung đột, báo trước; không ép ghi đè.
- Không thay đổi cấu hình bảo mật. Không lưu secrets vào tài liệu, log hoặc ảnh.
- Target Windows x64; kiểm tra Windows Build Support trước khi tuyên bố build sẵn sàng. Hiện module chưa cài.
- Báo cáo bằng tiếng Việt, gồm tệp đã sửa, kiểm chứng thực tế và các phần chưa chạy.
