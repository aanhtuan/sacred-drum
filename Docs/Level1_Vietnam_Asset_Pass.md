> Báo cáo lịch sử của art pass trước khi đổi layout thành mê cung. Trạng thái map hiện tại: [Level1_Maze_Redesign.md](Level1_Maze_Redesign.md).

# Level 1 — licensed rural art pass

Ngày thực hiện: 08/11/2026. Nhánh: `codex/level1-vietnam-asset-pass`, dựa trên Sprint 1 `0cf90a6`. Các tệp skill/AGENTS chưa commit từ yêu cầu trước được giữ nguyên.

## Hướng mỹ thuật và phạm vi

Dùng `level-design`, `unity-tilemap-2d`, `pixel-art-sprites`, `unity-2d` và Unity MCP. Tham khảo [Blossom Tales](https://store.steampowered.com/app/446810/) về góc nhìn 2D top-down, silhouette đơn giản, bảng màu sáng và đường đi dễ đọc. Không trích xuất hoặc sử dụng asset của Blossom Tales.

Tạo cảm giác đồng quê qua nhà gỗ mái tranh, bụi tre, luống mạ, sân phơi, rơm, chum nước, giỏ đan, giếng, gà, bò, cầu gỗ và thuyền nhỏ. Cảnh quan được phối từ asset có sẵn của họa sĩ, không gọi ImageGen và không vẽ texture mới bằng code. PNG gốc giữ nguyên byte; chỉ slice sprite trong Unity, cấu hình import, tint renderer/tile và ghép prefab.

Đây là cách phối asset nông thôn theo hướng Việt Nam, không phải bộ art Việt Nam chuyên dụng hay phục dựng lịch sử. Luống mạ dùng sprite cây non/cỏ để biểu đạt ruộng mới cấy; sprite giỏ dùng hình đồ đan/nông trại. Traveller đội mũ là placeholder CC0, chưa phải Minh chính thức; token di vật dùng hình cốc vàng placeholder, chưa phải mảnh trống Đông Sơn khảo cứu. Động vật là trang trí tĩnh, không thêm AI hay hệ thống chăn nuôi.

## Research và asset được chọn

| Nguồn | Tác giả và giấy phép | Dùng trong map |
| --- | --- | --- |
| [Superpowers Ninja Adventure](https://github.com/sparklinlabs/superpowers-asset-packs/tree/e8674a03ab4456802f71f848c4df79eccca23f7a/ninja-adventure) | Pixel-boy / Sparklin Labs, CC0 | Tileset, cây, đồ vật, cầu, thuyền, Traveller #14 và token |
| [Ninja Adventure Godot source](https://github.com/pixel-boy/NinjaAdventure/tree/6ac78232d5aedcc85ce5f27d060ea92366f7c24a) | Pixel-boy, CC0 theo [trang tác giả](https://pixel-boy.itch.io/ninja-adventure-asset-pack) | Sheet nền/đất; trang chính thức liên kết repo và ghi không dùng generative AI |
| [LPC bamboo, cave rocks, misc. objects](https://opengameart.org/content/lpc-bamboo-cave-rocks-misc-objects) | tapatilorenzo, CC0, đăng 04/03/2022 | Hai sheet tre, đá và thực vật; chỉ chọn vùng phù hợp, bỏ đồ hang động/fantasy |
| [LPC Farm](https://opengameart.org/content/lpc-farm) | bluecarrot16, Wolthera van Hövell tot Westerflier (TheraHedwig), Ivan Voirol; CC BY 4.0 | Mái tranh, rơm, vật dụng đan, xô/máng và vật liệu ghép nhà |
| [Tiny Farm](https://kenney.nl/assets/tiny-farm) | Kenney, CC0 | Gà và bò; tái sử dụng sheet đã tải trước, đối chiếu lại với ZIP gốc |

Giữ license và credit tại `Docs/Licenses/Level1_Vietnam/` và `Assets/StreamingAssets/ThirdPartyArtCredits.txt`. Không lấy các bản preview làm texture gameplay. Không tải/chạy chương trình hoặc project Godot từ các pack.

Đã khảo sát thêm LPC Thatched-roof Cottage và LPC Crops, lưu bản gốc để research nhưng không import: cottage có khung nhà châu Âu không phù hợp lựa chọn lần này; pack crops không xác lập được sprite lúa đúng brief. Trang tải itch bị chặn xác minh tự động, nên lấy các tệp cần thiết từ repo công khai do tác giả liên kết; không tải toàn bộ ZIP 89 MB, không vượt paywall/xác minh.

## Nơi lưu

- Thư viện nguồn gốc: `/home/atuna/Documents/Game/AssetDownloads/Level1_Vietnam/Original/`.
- PNG đưa vào Unity: `Assets/Art/Environment/CanhDong/AssetPass/Sources/`.
- Manifest URL, tác giả, license, byte size và SHA-256: `Docs/Verification/VietnamAssetPass/AssetSources.json`.
- Rect/pivot/PPU của từng sprite: `Docs/Verification/VietnamAssetPass/SpriteSlices.json`.
- Scene trước thay art: `Assets/Scenes/Archive/Level1_CanhDong_PreAssetPass.unity`.

**8 PNG nguồn, 285.662 byte**, đều trùng byte với bản gốc. **67 sprite có tên được slice**, **30 Tile assets**, một thư viện tile mesh giữ hình đường/ruộng/sông cũ, và prefab `NhaTranh_ThreeBay`. Một số tên sprite là alias của cùng vùng nguồn; số sprite không đồng nghĩa số artwork nguyên bản độc lập.

## Tích hợp

- Scene đang dùng: `Assets/Scenes/Level1_CanhDong.unity`.
- Thêm ba cụm nhà, sân nhà/giếng/sân phơi, đồ nông trại, bụi tre, vườn cây, cụm thực vật/đá và bến thuyền. Tránh rải đồ theo hàng đều; giữ khoảng trống trước các objective và trên đường đi.
- `Level1LicensedArtPass.cs` là công cụ authoring trong Editor. Các bước import/apply/refresh/refine nằm dưới menu `SacredDrum > Level 1 > Licensed art`. `Apply` từ chối ghi đè nếu scene đã được dress; không tự chạy theo startup hoặc vào Play Mode.
- `RefinementMeshImporter.cs` được sửa để tile mesh kế thừa tint của Tile nguồn, giúp các mép cong khớp phần nền. Không đổi polygon/mask của layout.
- `Player.prefab` chỉ đổi tham chiếu sprite sang asset CC0. Không sửa PlayerController, input, Rigidbody2D hoặc collider của Player.
- Renderer dùng atlas AI cũ được tắt trong scene hoạt động; source và GUID cũ vẫn giữ để scene archive có thể mở lại. Marker EnemySpawn được ẩn hình, không xóa marker.
- **152/152 GameObject gốc giữ vị trí, scale và active state**. Các cấu hình vật lý gốc giữ nguyên; cache SpriteTilingProperty của BoxCollider không auto-tiling được tách khỏi phép so sánh hình học. Cây/đá/nhà mới có collider ở phần chân khi cần; đồ nhỏ và động vật tĩnh chỉ là trang trí.
- PlayerSpawn, ba đèn, cổng khóa, mảnh trống, đường chính và cầu giữ vị trí. Không thêm combat, skill, puzzle hoặc logic mở cổng.

## Kiểm chứng

- PASS: Unity MCP đọc đúng project và Editor 6000.5.10f1; compile/domain reload hoàn tất.
- PASS: scene validation không missing script/broken prefab.
- PASS: kiểm tra bảo toàn transform và cấu hình vật lý gốc (`ProtectedLayout.json`).
- PASS: flood fill Physics2D với radius 0,32 và lưới 0,5 unit nối spawn tới cả ba đèn, hai đầu cầu và trước cổng; di vật vẫn bị khóa sau cổng (`RouteConnectivity.json`). Đây là kiểm tra connectivity bằng overlap, không thay cho test di chuyển thực tế.
- PASS: 20 probe Play Mode dùng keyboard mô phỏng qua Input System, controller thật và Physics2D.Simulate; bao gồm WASD, tốc độ chéo, thả phím, collider mẫu, cầu và camera (`PlayerValidation.json`). Các probe không kiểm tra mọi góc tiếp cận của mọi collider mới.
- Ảnh tổng thể được chụp bằng camera tạm zoom 30 để nhìn toàn map; gameplay camera được trả lại size 7 và target Player. Ảnh gần được chụp trong Play Mode.
- Console trong lượt test cuối không có lỗi gameplay/compile. Trong quá trình authoring có lỗi dịch vụ `UnityConnectWebRequestException: Token Exchange failed`; đây là kết nối tài khoản Editor, không phải lỗi C# controller. Có warning obsolete API từ script map có sẵn.
- NOT RUN: bàn phím vật lý, đánh giá camera ở nhiều FPS, Windows executable. Windows x64 Build Support vẫn chưa có. Không đổi Editor, package hoặc build target.

Kiểm tra thủ công: mở Level 1, Play, click Game View, đi từ sân nhà theo đường đến ba đèn và qua cầu hai chiều; thử đi vào thân cây/đá/hàng rào/cổng, rồi quan sát góc khuất sau mái/tre và sát bờ sông. Traveller và động vật vẫn là hình tĩnh; chưa thêm animation gameplay.

Thay đổi đang ở máy, chưa commit/push cho yêu cầu thay art này. Không chuyển Sprint.
