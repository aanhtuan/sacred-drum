# Level 1 — mê cung đồng quê, 08/11/2026

Scene hiện tại: `Assets/Scenes/Level1_CanhDong.unity`.
Nhánh: `codex/level1-vietnam-asset-pass`.

Yêu cầu mới cho phép làm lại layout, thay cho ràng buộc giữ nguyên bố cục của Sprint 1 ban đầu. Chỉ thay không gian và art trong Level 1; không thêm combat, AI, puzzle hay cơ chế mở cổng.

## Tham chiếu video

[Blossom Tales: The Sleeping King, Part 2 — DeltaShinyZeta](https://www.youtube.com/watch?v=F9zuB_IY0DQ), đoạn 00:00–10:00. Mở được video bằng trình duyệt, phát 2x đến 10:23 và xem lại mốc 03:00. Quan sát trực tiếp các khung hình ở đầu video, khoảng 00:37, 01:50, 03:00, 06:00, 07:31 và 10:13. Đây là tham chiếu thị giác; không trích xuất hay sử dụng asset của Blossom Tales.

- Đầu đoạn: đường rừng được chia khoang bằng cây lớn và bờ đất, có góc ngoặt và khoảng sân ở cổng.
- Khoảng 03:00: các lối vào gặp nhau ở sân; đèn/cổng là mốc nhớ đường, phần trung tâm còn trống.
- Khoảng 06:00: điểm gặp nhân vật được đặt trong khoảng nghỉ bên đường.
- Khoảng 07:31–10:00: hành lang hầm hẹp nối các phòng có hình khối rõ. Chỉ tham khảo nhịp hành lang → phòng; các câu đố trong video không được triển khai.

## Thay đổi layout và hình ảnh

Map 88×56 dùng một tuyến đi ngoặt qua hai bờ sông. Các nhánh phụ có ngõ cụt ở sân rơm, giếng, mỏm đá, bến thuyền và các hướng tiếp cận đền bị chắn. Có vòng nối cục bộ để người chơi nhận ra chỗ đã đi qua. Hai lối tắt ngoài ý muốn đã được phát hiện bằng tìm đường trên collider và tách lại trước khi chốt.

Hành lang danh nghĩa rộng 3,6 đơn vị, footprint Player bán kính 0,3. Tuyến ngắn nhất từ chân Player `(14.5,41)` đến trước cổng `(75,35.5)` đo trên lưới vật lý 0,5 đơn vị dài **153 đơn vị**, so với khoảng cách thẳng 60,75, tỷ lệ **2,52**. Đây là số đo đường đi, chưa phải bằng chứng người chơi thật thấy mê cung khó hay dễ.

Giảm họa tiết cỏ lặp và props rải đều. Tập trung đồ vật vào sân nhà, sân rơm, giếng, bến thuyền, khu nghỉ và sân đền. Cây/tre được dùng làm khối phân cách có va chạm liên tục. Giữ chất liệu nhà tranh, lu, giỏ, ruộng, tre và gà từ bộ asset đã có giấy phép. Đây là đồng quê Việt Nam lấy cảm hứng từ asset tổng hợp; chưa phải bộ art Việt Nam chuyên biệt. Nhân vật và mảnh trống vẫn là placeholder.

Ba đèn được chuyển sang `(24,34)`, `(29.5,13.5)`, `(62.5,19)` theo layout mới. Cổng và mảnh trống giữ tại khu đền. Không có logic thắp đèn hay mở cổng mới.

## Script, Prefab và asset

- Mới: `Assets/Editor/Level1/Level1MazeAuthoring.cs`: dữ liệu đường đi được tác giả định sẵn, dựng Tilemap và khối phân cách trong Editor; không sinh map ngẫu nhiên lúc chơi. Kiểm tra đúng scene/Edit Mode và yêu cầu bản sao trước khi chạy.
- Mới: `Assets/Editor/Level1/Level1MazeValidation.cs`: QA trong Play Mode bằng Input System, Rigidbody2D thật và collider trong scene.
- Sửa: `Level1LicensedArtPass.cs` giữ các sprite slice không thuộc pass cũ khi reimport, để các tile mới không mất GUID.
- Player tái sử dụng `Assets/Prefabs/Characters/Player.prefab` và PlayerController hiện có. Sprite người đội mũ là placeholder CC0 đã chọn ở art pass trước; không viết lại movement.
- 11 slice/tile nền và mép sân mới từ nguyên PNG `NinjaAdventure_Floor.png`, không sửa pixel nguồn. `MazeGround.spriteatlas` dùng Point, padding 4, RGBA32 không nén để tránh đường kẻ do texel bleed/nén DXT khi camera chuyển động.
- Nguồn, hash và giấy phép của 8 PNG: `Verification/VietnamAssetPass/AssetSources.json`; credits phát hành: `Assets/StreamingAssets/ThirdPartyArtCredits.txt`. Không dùng image generation.

## Bảo toàn và phục hồi

`Assets/Scenes/Archive/Level1_CanhDong_PreMaze.unity` là bản sao bằng Unity Editor của map ngay trước lần làm mê cung. Bản `Level1_CanhDong_PreAssetPass.unity` cũ cũng được giữ. Không ghi Scene YAML bằng tay.

Grid và Environment cũ còn trong scene, được tắt. Root `Level1_Maze` chứa map mới. Gameplay/Player, cổng, đèn, mảnh trống và LevelBounds được tái sử dụng. Không xóa asset nguồn hay thay GUID `.meta` đã có. Có thể mở scene archive để xem/phục hồi bố cục trước.

## Kết quả kiểm chứng

- **PASS:** Unity MCP đọc đúng projectRoot, Scene và Editor; compile xong, không có lỗi C# cuối lượt.
- **PASS:** save/reopen Scene; validator của MCP: 0 missing scripts, 0 broken prefabs; kiểm tra riêng: 0 sprite đang bật bị mất và 0 renderer đang bật dùng atlas AI cũ.
- **PASS — 29/29:** `Verification/Level1Maze/PlayModeValidation.json`. WASD, tốc độ chéo, nhả phím và hướng nhìn; tường cây, đá, hàng rào, sông ngoài cầu và cổng chặn Player; cầu gỗ đi qua được.
- **PASS:** 3 đèn, hai đầu cầu và các đầu nhánh được kiểm tra đều tới được; mảnh trống không tới được khi cổng còn khóa.
- **PASS:** PlayerController thực sự đi theo tuyến tìm được đến trước cổng bằng input bàn phím mô phỏng: 1.911 bước physics × 0,02 giây. Lưu tọa độ tuyến tại `TraversedPath.json`.
- **PASS:** Camera Orthographic, size 6, giữ smoothing 0,18 giây hiện có; 4 góc map được kiểm tra viewport không vượt bounds. Ảnh tổng quan dùng zoom tạm thời, đã trả camera về cấu hình chơi trước khi lưu.
- Console ở lần Play cuối: **0 errors, 0 warnings**. Trong quá trình compile/Editor có xuất hiện `UnityConnectWebRequestException: Token Exchange failed...` của dịch vụ Unity. Một lỗi C# trong helper đã được sửa trước các lượt PASS; không tính compile sạch là bằng chứng build Windows.
- **NOT RUN:** bàn phím vật lý, playtest độ khó với người chơi, Windows x64 build (module còn thiếu).

Ảnh Game View: `Verification/Level1Maze/Final_Gameplay_Spawn.png`, `Final_Gameplay_Junction.png`, `Final_Gameplay_Bridge.png`. `Final_Overview.png` có thể tiết lộ đường đi. Ảnh từng góc dùng teleport phục vụ quan sát; bằng chứng đi liên tục là phép QA và JSON riêng.

## Kiểm thử thủ công

1. Mở `Assets/Scenes/Level1_CanhDong.unity`, bấm Play và click Game View, dùng WASD.
2. Đi thử mọi nhánh từ sân nhà; tìm 3 đèn rồi tiếp cận cổng. Ghi lại chỗ khó nhận ra đường, bị kẹt hoặc thấy vật cản không khớp hình.
3. Thử ép vào cây, đá, hàng rào và cổng; thử đi xuống sông ở ngoài cầu, sau đó đi ngang cầu gỗ. Không vật cản nào ở danh sách này được cho đi xuyên; riêng cầu phải đi qua được.
4. Thả phím và thử đi chéo; kiểm tra không trượt và không tăng tốc. Quan sát camera ở các khúc ngoặt và mép map.
5. Cổng vẫn khóa là trạng thái chủ ý của phạm vi hiện tại. Muốn chạy lại QA bằng MCP: vào Play Mode, gọi `SacredDrum.EditorTools.Level1MazeValidation.Run()` qua execute_code.

Độ khó và khả năng nhớ đường cần được đánh giá bằng lượt chơi đầu tiên của người thật trước khi gọi map là hoàn tất về thiết kế.
