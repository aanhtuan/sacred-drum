> Latest prototype update: Minh farmer animations, static scarecrows and closer follow camera are documented in [PrototypeCast_Preview.md](PrototypeCast_Preview.md). Camera/entity counts below describe the earlier map-only validation.

# Prototype cánh đồng bị bóng tối chiếm đóng

Theo lựa chọn của Owner, bản này thay đổi **Prototype_LDtk_Import**, không sửa Level1_CanhDong hoặc Prototype_Player. Bản map kiểm thử 24×18 trước đây được giữ trong commit `6437073`; scene hiện tại dùng bố cục mới 48×36.

## Không gian và mỹ thuật

- 48×36 ô, tile 16×16, 768×576 source pixels.
- Spawn ở góc tây nam, ô (5,32); mốc tế đàn bị chiếm ở đông bắc, ô (41,5).
- Ba tuyến bờ gai chia cánh đồng thành các dải mê cung, các đoạn hàng rào gai dọc tạo lối vòng/ngõ cụt. Trục chính rộng khoảng ba ô; các nhánh thăm dò có đoạn một ô.
- Chín thửa ruộng cạn với luống đất, gốc rạ vàng xỉn; cây trụi lá, đá vỡ, mương đen và đất nhiễm tím quanh tế đàn.
- Các đèn vàng sắp tắt tạo điểm định hướng; khu cuối có quầng tím. Đường nâu khô tương phản với đất xám và gai tối để vẫn đọc được lối đi dưới sương.
- 16 tile procedural-native được tác giả hóa bằng C# tại `Assets/Editor/DarkFields/DarkFieldsArt.cs`. Không tải thêm art, không sửa PNG/tileset cũ, không dùng asset trả phí. Chạy menu Generate Native Prototype Atlas sẽ ghi lại chính atlas mới này và giữ GUID của asset.
- Các tile là placeholder pixel art cho prototype. Tế đàn là mốc hình ảnh, chưa phải cơ chế boss/puzzle mới.

## Nguồn và scene

- `Assets/Scenes/Prototype_LDtk_Import.unity`
- `Assets/LevelDesign/LDtkPrototype/Prototype_LDtk_Import.ldtk`
- `Assets/LevelDesign/LDtkPrototype/Prototype_LDtk_Import/DarkFields.ldtkt`
- `Assets/LevelDesign/DarkFields/Tilesets/DarkFields.png`
- `Assets/LevelDesign/DarkFields/Materials/`, `Prefabs/`, `Shaders/`

Nguồn LDtk tham chiếu atlas bằng đường dẫn tương đối `../DarkFields/Tilesets/DarkFields.png`. Vẫn dùng importer 6.12.4, source `.ldtk`, `.ldtkt` và `.meta` được quản lý cùng repo. Các file tileset của prototype cũ được giữ nguyên.

Phân lớp từ trước ra sau: Entities → Collision → Landmarks → DeadTrees → ThornWalls → WitheredRice → Water → Dirt_Path → DriedFields → Corruption → Ground. Có 10 Tilemap (9 hiển thị và 1 Collision IntGrid).

Collision chỉ nằm ở bờ gai, nước, gốc cây và đá: 417 ô. Gốc rạ và nền ruộng không chặn. Khi chỉnh các vật cản trong LDtk, cập nhật Collision IntGrid tương ứng.

## Cue sương mù và ánh sáng

| Mốc | Mục đích | Thực hiện | Camera/âm thanh | Vòng đời |
| --- | --- | --- | --- | --- |
| Scene load → unload | Màn sương thấp trôi qua cánh đồng | Một quad 48×36, shader URP `SacredDrum/DarkFields/DriftingMist`, alpha tối đa dưới 0.34, `_Drift=0.025` | Không rung camera, không thêm âm thanh | Object thuộc scene, không spawn lặp |
| Scene load → unload | Mốc đường đi | Ba Point Light2D vàng nhạt, bán kính 3.3 | Camera prototype nhìn toàn map | Tĩnh, không nhấp nháy |
| Scene load → unload | Dấu vết chiếm đóng | Một Point Light2D tím, bán kính 5.5, ở tế đàn | Không đổi gameplay | Tĩnh |

Sương dùng biến thời gian shader, không sửa timeScale hoặc physics. Reduced motion: đặt `_Drift=0` trên material `DriftingMist`. Có thể tắt object `Drifting Field Mist` để review rõ art và đường đi. Không thêm VFX Graph, post-processing package, enemy AI hoặc player controller.

Camera prototype: orthographic size 18. PNG Point, RGBA32, mipmaps off, PPU16. Pixel có thể không đều khi Game View scale không nguyên; ảnh toàn map không thay thế việc review art ở zoom gần.

## Kiểm chứng

| Kiểm tra | Kết quả |
| --- | --- |
| Compile và shader URP | PASS |
| Collision source so với Physics2D tại toàn bộ 1.728 ô | PASS, 0 sai khác |
| Đường spawn → tế đàn | PASS, đường ngắn nhất 103 bước trên lưới ô |
| Không có khu đi được bị cô lập | PASS, toàn bộ 1.311 ô đi được nối nhau |
| Dynamic collision trong Play Mode | PASS cho nước, gai, cây khô, đá; vận tốc về 0, có tiếp xúc |
| Spawn | (5.5,-32.5), thông thoáng |
| Mở/lưu trong LDtk GUI và chạy AfterSave exporter | PASS, mask collision và kích thước giữ nguyên |
| Reimport và mở lại scene | PASS, 1 map root, 1 entity, 10 Tilemap, 10 TilemapCollider2D |
| Missing script/reference/sprite | PASS, 0 / 0 / 0 |
| Sương thay đổi theo thời gian | PASS, hai frame Play Mode khác nhau ở 718.958 pixel; đây là kiểm tra hình ảnh, không phải benchmark |
| Override cache Tilemap/Composite | Không giữ override m_Tiles hoặc m_CompositePaths trong scene cuối |
| Level1_CanhDong, Prototype_Player, Player Controller, asset cũ ngoài prototype | Giữ nguyên, đối chiếu SHA-256 |

Bằng chứng: [ảnh Game View](Evidence/DarkFields/DarkFields_GameView.png), [frame sương sau đó](Evidence/DarkFields/DarkFields_FogLater.png), [kiểm tra layout](Evidence/DarkFields/LayoutValidation.json), [Play Mode](Evidence/DarkFields/RuntimeValidation.json).

Console lượt Play Mode cuối không có Error. Khi recompile, Unity còn các warning obsolete API trong editor scripts Level1 có sẵn; Unity Connect token-exchange error cũng đã tồn tại từ trước. Không sửa các script/cấu hình ngoài phạm vi để dọn các thông báo này.

Chưa thử đi bằng Player Controller thật, chưa benchmark CPU/GPU và chưa build Windows. Không thay đổi build settings, package version, scene chính hoặc gameplay hiện tại.
