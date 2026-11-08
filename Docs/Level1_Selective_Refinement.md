# Level 1 — selective refinement

Ngày: 08/11/2026. Scene: `Assets/Scenes/Level1_CanhDong.unity`.
Branch: `feature/level1-selective-refinement`. Baseline: `a6b5ea7`. Commit triển khai: `00fcb00`; tài liệu và bằng chứng được commit riêng.

## Phạm vi và skills

Chỉnh trực tiếp trên map đã lưu, giữ kích thước **88×56**. Không chạy lại `Level1MazeAuthoring.Build()`, không xóa root/map cũ, không thay Input/Player Controller/Combat, không thêm Enemy AI hoặc logic phong ấn/mở cổng.

Đã kiểm tra danh sách skill trên đĩa và đọc các hướng dẫn phù hợp:

- `unity-mcp-orchestrator`: `/home/atuna/.codex/skills/unity-mcp-skill/SKILL.md` — đọc instance/project/Editor/scene trước khi thao tác, chờ compile, kiểm tra Console và ảnh.
- `level-design`: `/home/atuna/.agents/skills/level-design/SKILL.md` — dùng kích thước Player, khoảng chiến đấu, nhánh đi và kiểm tra gating trước khi hoàn thiện art.
- `unity-tilemap-2d`: `.agents/skills/unity-tilemap-2d/SKILL.md` — chỉnh Tilemap theo lớp, collider composite và save/reopen.
- `pixel-art-sprites`: `.agents/skills/pixel-art-sprites/SKILL.md`, cùng `references/patterns.md`, `sharp_edges.md`, `validations.md` — đọc silhouette ở zoom chơi, native pixel scale, Point filtering và dùng đúng vùng sprite.

Không có skill riêng về Git/scene validation trong các skill liên quan đã kiểm tra; phần này dùng quy tắc `AGENTS.md`, Git và công cụ Unity MCP thực tế. Không cài skill/package mới, không tải asset ngoài.

## Thay đổi môi trường

- Giữ các cụm cây và đường đi chính; thay một phần hình ảnh cây đều bằng bụi tự nhiên, đá và các mặt vách đất tại biên vật cản đã có. Giữ 495 object cây/tre gốc; còn **296 renderer cây/tre gốc được bật**, phần còn lại được tắt hình ảnh để thay vật liệu hoặc giải phóng sân. Không xóa các object đó.
- Đưa vegetation mới/được chỉnh về mật độ 16 pixel/world unit: cây/tre nguồn 16 PPU dùng scale 1; đá nhỏ nguồn 32 PPU dùng scale 2. Nhà và các landmark cũ được giữ; không tuyên bố toàn bộ bộ art tổng hợp đã đồng nhất tuyệt đối.
- Thêm mặt vách đất ở 8 đoạn ngắn, 54 ô mặt và 54 ô đỉnh. Chỉ thay hình ảnh trên vùng đã có collision; không thêm cơ chế nhảy hoặc độ cao gameplay.
- Nền đi được và khối địa hình bị chặn có màu khác nhau. Màu được lưu trong Tile asset để không mất sau refresh/reopen. Thêm vài cụm grass variation ở vai đường/sân, tránh phủ kín combat floor.
- Bổ sung đường đất tới ba phong ấn và cổng. Mở một nối ngắn cạnh bến sông, giảm đường vòng không cần thiết. Tuyến ngắn nhất từ spawn đến trước cổng giảm **153 → 122 đơn vị**, khoảng 20%.
- Thửa ruộng dùng ô chữ nhật, bờ đất và hàng cây non; giữ lối đi và sân chiến đấu. Cây non là sprite thực vật có sẵn dùng làm placeholder ruộng lúa, chưa phải art lúa Việt Nam chuyên biệt.
- Bờ sông chỉnh trên hai đoạn y=5–15 và y=39–48, tối đa 1 ô mỗi bờ; bề rộng cục bộ 4–5 ô. Nước tăng 208 → 218 ô. Vùng cầu và 16 ô cầu gỗ x=43–46, y=25–28 giữ nguyên.
- Chuyển CompositeCollider2D của sông và terrain từ `Outlines` sang `Polygons`, để vùng bị chặn có thể tích đặc thay vì chỉ có đường viền. Native Tile collider vẫn là Grid; biên đi lại được kiểm tra lại bằng Player thật.

ForestCollision giảm **2908 → 2708 ô** (giảm 200 ô, khoảng 6,9%) để tận dụng các khoảng trống cho sân và nối lối đi. Ground giữ nguyên 4928 ô. Cầu, nhà, cổng, mảnh trống, ba đèn và Camera giữ vị trí/cấu hình gameplay tương ứng; chỉ hai spawn marker cũ được chuyển về điểm an toàn và tắt nhóm cũ để tránh hai bộ spawn cùng hoạt động.

## Ba khu phong ấn

| Khu | Seal | Vị trí | Marker dự kiến | Khoảng chiến đấu đã xác minh |
|---|---|---|---|---|
| A — dưới trái | Seal_01 | (31.5, 16.5) | ScarecrowSpawn_01 (35.5,17.5); RatSpawn_01 (26.5,14.5) | 8×6 trống quanh Seal |
| B — dưới phải | Seal_02 | (65, 13.5) | ScarecrowSpawn_02 (68.5,15.5) | 8×6 trống quanh Seal |
| C — sân trước cổng | Seal_03 | (74, 33.5) | ScarecrowSpawn_03 (68.5,33) | 8×6 trống quanh Seal |

Các object mới nằm trong `Gameplay/SealAreas`. Spawn markers chỉ có Transform; tên/vòng gizmo hiện trong Scene View. Không có quái thật. Seal có trigger và pedestal/đèn/ô lát từ asset sẵn có; chưa có trạng thái kích hoạt.

`LockedGate` vẫn ở (75,37.5), `DrumFragment_01` ở (75.5,45.5). Seal_03 nằm trước cổng và tới được khi cổng khóa. Không thêm Puzzle/Gate System.

## Tệp triển khai và bảo toàn

- Mới: `Assets/Editor/Level1/Level1SelectiveRefinement.cs`: chỉnh các Tilemap đang tồn tại, đặt marker, thêm các lớp art và gizmo. Apply có guard Edit Mode/đúng scene/chưa áp dụng; không dựng lại map gốc.
- Mới: `Assets/Editor/Level1/Level1SelectiveValidation.cs`: kiểm tra trong Play Mode bằng Input System, Rigidbody2D và collision scene.
- Scene được cập nhật: `Assets/Scenes/Level1_CanhDong.unity`.
- 5 Tile asset mới: `RefineCliffLeft`, `RefineCliffMiddle`, `RefineCliffRight`, `RefineCliffTop`, `RefineTerrainMass`, kèm `.meta`.
- `NinjaAdventure_Tileset.png.meta` thêm 6 slice từ PNG đang có (4 cliff, 2 shrub/grass), giữ GUID và các slice cũ. Không thay pixel PNG. Cả 8 PNG nguồn giữ nguyên SHA-256.
- Player Prefab, scripts runtime, InputActions, Packages và ProjectSettings không thay đổi.
- Đối chiếu scene serialized: **871 GameObject ID gốc và cả 5 PrefabInstance giữ nguyên**; 5 record PrefabInstance còn giống baseline theo byte. 85 ID runtime suy ra từ con của prefab đổi khi reload, không phải 85 object bị xóa. Xem `PreservationAndScene.json`.
- Không tạo scene backup mới vì baseline `a6b5ea7` đã ở Git và working tree trước task sạch ngoài crash dump. Các scene Archive trước đó được giữ nguyên. Chỉ đọc YAML để audit ID; mọi sửa Scene thực hiện qua Unity Editor/MCP.

## Kiểm chứng

Bằng chứng nằm trong `Docs/Verification/SelectiveRefinement/`.

- **PASS:** MCP đọc đúng `/home/atuna/Documents/Game/SacredDrum`, Unity 6000.5.10f1; compile hoàn tất.
- **PASS:** save/reopen Scene, 0 missing scripts, 0 broken prefabs; không có sprite đang bật bị mất hoặc renderer đang bật dùng atlas AI cũ.
- **PASS 41/41:** `PlayModeValidation.json`. Bao gồm WASD, tốc độ chéo, dừng/hướng nhìn, vật cản, cầu, cổng, bốn tuyến đi thật tới ba Seal và trước cổng, spawn marker, diện tích sân và camera bounds.
- **PASS:** PlayerController đi tới Seal_01/02/03 theo các tuyến vật lý dài 55,5 / 102 / 119 đơn vị; tới trước cổng theo tuyến 122 đơn vị. `TraversedSealRoutes.json` lưu đường đi. Đây là input bàn phím mô phỏng, không phải kiểm thử bàn phím vật lý.
- **PASS:** mỗi sân kiểm tra bằng 63 mẫu có bán kính 0,45 và thêm OverlapBox toàn bộ 8×6; không có vật cản rắn bên trong.
- **PASS:** 4 spawn mới và 2 marker cũ ở điểm an toàn, ngoài collider. Marker cũ được giữ inactive.
- **PASS:** `RiverTopology.json`: tạm chặn cầu trong QA, Seal phía tây vẫn tới được, bờ đông và hai Seal phía đông không tới được. Collider tạm đã được gỡ; không tạo lối vượt sông khác.
- **PASS 218/218:** `WaterColliderCoverage.json` kiểm tra mỗi tâm ô nước nằm trong collider của sông sau khi đổi sang Polygons.
- **PASS:** cổng khóa chặn Player; không có đường vòng tới mảnh trống. Chỉ kiểm tra trạng thái khóa vì chưa có hệ thống mở cổng.
- Camera giữ Orthographic size 6, smoothTime 0,18 và bounds (0,0,88,56), target Player. Cấu hình overview tạm thời đã được trả về trước khi lưu.
- Lượt Play cuối: **0 errors, 0 warnings**. Đầu task có cảnh báo “referenced script Unknown is missing” và Unity Token Exchange từ trạng thái Editor trước đó; Scene validator trước/sau đều không tìm thấy missing component. Không xóa component hoặc sửa cấu hình dịch vụ để che cảnh báo.

Ảnh trước: `Before_GameView.png`, `Before_SceneView_Framed.png`. Ảnh cuối: `Final_AreaA.png`, `Final_AreaB.png`, `Final_AreaC.png`, `Final_Overview.png`, `Final_SceneView.png`. Tool Scene View lưu viewport với chiều dọc bị đảo (cả chữ toolbar); ảnh Game View đúng hướng được dùng để review art. Ảnh từng sân dùng teleport chỉ để chụp; bằng chứng đi bằng controller là QA riêng.

**NOT RUN:** combat thực tế với 1–2 quái, né đòn/gậy/skill, cân bằng độ khó với người chơi thật, bàn phím vật lý và Windows build. Task không triển khai các gameplay đó; kiểm tra khoảng trống không chứng minh combat cân bằng.

## Kiểm thử thủ công

1. Mở Scene Level1_CanhDong, Play, click Game View và dùng WASD.
2. Đi tới sân dưới trái, qua cầu tới sân dưới phải, rồi tới Seal_03 trước cổng. Quan sát việc nhận ra đường đi và khoảng né đòn.
3. Thử ép vào cây/đá, mặt vách đất, bờ sông và cổng. Cầu phải qua được, còn cổng vẫn khóa và mảnh trống chưa thể lấy.
4. Kiểm tra Camera ở mép map và các khúc rẽ. Đánh giá lại độ đọc đường và art ở độ phân giải chơi dự kiến.
5. Chạy lại QA: trong Play Mode dùng MCP execute_code gọi `SacredDrum.EditorTools.Level1SelectiveValidation.Run()`.
