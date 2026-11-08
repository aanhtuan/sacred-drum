# Skills cho SacredDrum

Tìm trên [skills.sh](https://skills.sh/) và kiểm tra nội dung upstream ngày 2026-11-08. Đây là hướng dẫn cho agent, không phải Unity package hay asset nhân vật. Không thay đổi Assets, Packages, ProjectSettings hoặc gameplay khi cài.

## Skill cài riêng trong dự án

| Công việc | Skill | Nguồn |
| --- | --- | --- |
| Tilemap, Tile Palette, Rule Tile, collider map | `unity-tilemap-2d` | [gamedev-skills](https://skills.sh/gamedev-skills/awesome-gamedev-agent-skills/unity-tilemap-2d) |
| Thiết kế Minh/NPC: silhouette, hình khối, trang phục, character sheet | `character-design` | [omer-metin](https://skills.sh/omer-metin/skills-for-antigravity/character-design) |
| Sprite pixel art, palette, spritesheet và animation frame | `pixel-art-sprites` | [omer-metin](https://skills.sh/omer-metin/skills-for-antigravity/pixel-art-sprites) |
| C# dễ đọc, trách nhiệm lớp/hàm, review và refactor có giới hạn | `clean-code` | [sickn33](https://skills.sh/sickn33/agentic-awesome-skills/clean-code) |
| Hướng dẫn VFX, độ rõ tín hiệu, chi phí và tích hợp hiệu ứng | `vfx-pipeline` | [mrcalderon3d](https://skills.sh/mrcalderon3d/everything-game-dev-code/vfx-pipeline) |

Các thư mục nằm tại `.agents/skills/<tên>/`, gồm `SKILL.md` và các reference upstream đi kèm. Nguồn được cố định bằng commit trong `.agents/skill-sources.json`; giấy phép được giữ trong `.agents/skill-licenses/`. File JSON là bản kê provenance/hash của dự án, không phải lockfile do Skills CLI tạo.

## Tái sử dụng các skill đã có ở cấp người dùng

Đã kiểm tra các thư mục hiện có: `level-design`, `camera-systems`, `game-feel`, `unity-2d`, `unity-animation`, `unity-lighting-vfx`, `unity-performance`. Không cài lại hay cập nhật chúng trong lần này. Trên máy khác, cần cài các skill tương ứng nếu muốn dùng cùng workflow.

- **Map:** `level-design` xác định critical path, pacing, metric; `unity-tilemap-2d` hỗ trợ thực hiện và collider. Level 1 hiện tại chỉ được sửa trong phạm vi Owner giao.
- **Nhân vật:** `character-design` → `pixel-art-sprites` → `unity-2d`/`unity-animation`. Kiểm tra giấy phép art, PPU, pivot, scale, sorting và hướng nhìn trước khi import. Skill không tự cung cấp asset Minh chính thức.
- **Clean code:** dùng `clean-code` với quy ước C# đang có; ưu tiên thay đổi nhỏ, bảo toàn serialized fields và kiểm chứng hành vi. Các mục TDD, số dòng, số tham số là heuristic, không phải lý do mở rộng scope.
- **Kịch bản hiệu ứng:** dùng `vfx-pipeline` với cue sheet bên cạnh. `game-feel` và `unity-lighting-vfx` hỗ trợ timing, camera, particle và URP khi sprint cho phép triển khai.

`vfx-pipeline` có lượng cài thấp hơn các lựa chọn còn lại (23 lượt trên trang đã xem), nên được chọn sau khi đọc trực tiếp nội dung, thay vì coi lượt cài là bằng chứng chất lượng. Đây là hướng dẫn pipeline tổng quát; chi tiết Unity phải được xác minh trong Editor. Những skill nhắm engine khác không được dùng để thay hướng dẫn Unity.

Các reference của `pixel-art-sprites` có ví dụ JavaScript/Phaser và regex validation cho web. Chỉ dùng nguyên tắc art, palette, silhouette, frame và export; không áp dụng regex web như phép kiểm tra C#/Unity. Các gợi ý thẩm mỹ trong `character-design` cần được điều chỉnh theo brief và văn hóa SacredDrum, không phải quy tắc bắt buộc về nhân vật.

## Ví dụ yêu cầu

- “Dùng level-design và unity-tilemap-2d đánh giá đường đi Level 1, chỉ đọc, giữ nguyên layout.”
- “Dùng character-design và pixel-art-sprites lên character sheet Minh theo brief tôi cung cấp; chưa thay placeholder.”
- “Dùng clean-code review PlayerController, nêu vấn đề trước; chưa refactor ngoài phạm vi.”
- “Dùng vfx-pipeline lập cue sheet cho hiệu ứng được giao theo VFX_CUE_TEMPLATE.md; chưa tạo gameplay.”

Skill cài mới có thể được chọn từ lượt tiếp theo. Có thể kiểm tra discovery bằng `npx --offline skills list --agent codex --json` tại thư mục dự án. Cài skill không chứng minh Unity compile, gameplay hay Windows build; các kiểm chứng đó cần chạy khi có thay đổi Unity thực tế.

## Kết quả kiểm tra cài đặt

- PASS: Skills CLI nhận diện đủ 5 skill ở scope `project`, agent `Codex`.
- PASS: 11 file trong các thư mục skill trùng byte với upstream tại commit đã ghim; SHA-256 được lưu trong manifest.
- PASS: giữ LICENSE của cả 4 repo nguồn và NOTICE của nguồn Apache có cung cấp NOTICE.
- PASS: các thư mục skill chỉ chứa tài liệu Markdown; không chạy code hay script từ skill tải về.
- Unity compile / Play Mode: NOT RUN trong lần cài này vì không thay đổi tệp Unity.
- Thay đổi được giữ cục bộ trên `codex/sprint1-player-controller`; chưa commit/push cho yêu cầu cài skill này.
