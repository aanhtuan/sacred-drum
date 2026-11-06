# Sprint 0 – Completion Report

Ngày kiểm tra: 07/11/2026 (Asia/Ho_Chi_Minh).

## Unity Project

- Path: `/home/atuna/Documents/Game/SacredDrum`.
- Unity: `6000.5.10f1` (`3bd4f66ad299`).
- URP: `17.6.0`; đọc runtime Editor xác nhận `UniversalRenderPipelineAsset` và `Renderer2D`.
- ForceText / Visible Meta Files / Mode2D đã có trước Sprint 0.
- Scene đang mở sau kiểm tra: `Assets/Scenes/Prototype_Player.unity`, không dirty.
- Hierarchy đọc qua MCP: đúng 2 root, `Main Camera` và `Global Light 2D`.
- Camera: Orthographic, URP renderType Base. Light: Global, intensity 1.
- Prototype được tạo bằng EditorSceneManager.SaveScene(saveAsCopy=true), sau đó mở lại qua MCP; không sửa YAML thủ công.
- Không thêm gameplay scripts, Player hay Enemy.

## GitHub

- Repository: https://github.com/aanhtuan/sacred-drum
- Branch: `main`.
- Trước setup: chưa có Git tại project, remote không có refs.
- Git đã khởi tạo, fetch remote trước commit; không có lịch sử riêng cần merge/rebase.
- Commit đầu: `70d936c` — `chore: initialize SacredDrum Unity project`, đã push thành công.
- Commit cấu hình: `chore: configure Unity MCP and project structure`. Tra SHA bằng `git log -1`; báo cáo này được lưu trong chính commit đó.
- Origin fetch HTTPS, push SSH cùng repository; SSH đã xác thực tài khoản `aanhtuan`.
- Git LFS: 3.7.1; binary cục bộ từ RPM Fedora đã xác minh chữ ký, cấu hình hook/filter repo-local.
- LFS patterns: psd, aseprite, blend, fbx, wav; chưa có file LFS.
- Không commit cache. Git attributes chỉ bỏ kiểm tra trailing whitespace cho định dạng Unity serialize để bảo toàn tệp gốc; vẫn kiểm tra whitespace tài liệu/code.

## Unity MCP

- Package: `com.coplaydev.unity-mcp` 10.3.1-beta.11, có sẵn trước Sprint 0.
- Server: mcpforunityserver 10.3.0 qua uvx/stdin-stdout, cấu hình enabled có sẵn.
- Codex connection: **CONNECTED**, xác nhận bằng đọc trực tiếp Editor, không chỉ từ danh sách server.
- Instance được chọn: `SacredDrum@778c756a`.
- Đã đọc project info, editor state, cameras, hierarchy SampleScene và Prototype_Player, Console.
- Đã gọi thành công `set_active_instance`, `manage_scene`, `read_console`, `execute_code`.
- Ban đầu lệnh hierarchy chờ không trả kết quả; sau khi chọn rõ instance, thử lại thành công. Console có cảnh báo duplicate command bị chặn, chưa đủ bằng chứng quy nguyên nhân timeout duy nhất.
- Cuối kiểm tra: is_compiling=false, domain reload không pending, ready_for_tools=true; Console error trả 0 bản ghi. Không xóa Console để che lỗi.
- Có warning Unity AI Account API timeout và duplicate MCP command. Chưa xác nhận Unity Cloud/AI account; không thay đổi bảo mật hoặc liên kết tài khoản.

## Completed Tasks

- Kiểm tra project, packages, scene, URP, serialization và version control mode.
- Tạo đủ cấu trúc yêu cầu qua AssetDatabase: 39 thư mục mới và meta do Unity sinh; giữ thư mục template có sẵn.
- Thêm .gitkeep ở các thư mục lá trống để clone giữ cấu trúc.
- Cấu hình Git ignore, LFS, origin; commit và push khởi tạo.
- Kiểm chứng MCP đọc/ghi Editor và tạo prototype chỉ gồm camera/light.
- Tạo README, AGENTS và báo cáo này.
- SHA-256 baseline bao phủ 49 tệp gốc; mọi Assets, Packages và meta gốc được giữ nguyên.
- Unity tự thêm APP_UI_EDITOR_ONLY vào Standalone define và cấu hình com.unity.dt.app-ui vào EditorBuildSettings khi Editor/domain reload hoạt động. Đã review: trỏ tới asset App UI Settings của package com.unity.ai.inference đã có, không đổi danh sách scene build. Giữ các thay đổi do package sinh.
- SceneTemplateSettings.json do Unity tạo khi làm việc với scene; không tạo gameplay.

## Remaining Tasks / Limits

- **Windows build: NOT RUN**. Module Windows Build Support chưa cài; active build target hiện là StandaloneLinux64. Đây là dependency còn thiếu cho phát hành Windows, không phải bằng chứng Windows build PASS.
- **Play Mode/gameplay: NOT RUN**; Sprint 0 không có gameplay để nghiệm thu.
- Chưa nâng cấp hay đồng bộ phiên bản server 10.3.0 và package beta 10.3.1-beta.11; tổ hợp hiện tại đã qua các thao tác MCP nêu trên.
- Khả năng upload/download asset LFS thực tế chưa thử do project chưa có asset LFS; chỉ xác nhận cài đặt, pattern và hook.
- Kiểm tra push cuối, SHA remote và working tree được thực hiện sau commit, báo trong phản hồi bàn giao (không ghi trước một kết quả chưa thực hiện vào báo cáo).

## Next Sprint

Sprint 1: Player Controller. Chờ Project Owner giao việc; không tự động triển khai.
