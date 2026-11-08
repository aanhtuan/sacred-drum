# LDtk → Unity prototype

## Phạm vi và kết quả

Pipeline đã được kiểm chứng với LDtk **1.5.3**, LDtkToUnity **6.12.4**, Unity **6000.5.10f1**, URP **17.6.0**, Fedora 44 KDE/Wayland. Chỉ tạo prototype riêng; không sửa Level1_CanhDong, Prototype_Player, prefab Player, Player Controller hay Camera/Collision của các scene cũ.

- Scene: `Assets/Scenes/Prototype_LDtk_Import.unity`
- Nguồn: `Assets/LevelDesign/LDtkPrototype/Prototype_LDtk_Import.ldtk`
- Tileset PNG: `Assets/LevelDesign/LDtkPrototype/Tilesets/`
- Tileset definition đã export: `Assets/LevelDesign/LDtkPrototype/Prototype_LDtk_Import/*.ldtkt`
- Collision assets và spawn prefab: các thư mục `CollisionTiles/`, `Prefabs/` trong cùng prototype.
- Công cụ chuẩn bị exporter: `Assets/Editor/LDtkPrototype/LDtkPrototypeTools.cs`
- Ảnh Play Mode, từ Main Camera: [Game View](Evidence/LDtkPrototype/Prototype_LDtk_Import_GameView.png)
- Kết quả MCP thực tế: [Validation.json](Evidence/LDtkPrototype/Validation.json)

## Importer và khả năng tái lập

Package `com.cammin.ldtkunity` được ghim **6.12.4** qua scoped registry `OpenUPM`, URL `https://package.openupm.com`, scope `com.cammin.ldtkunity`. Registry và lockfile được quản lý trong Git. Không thay đổi phiên bản dependency hiện có.

Nguồn package: [OpenUPM](https://openupm.com/packages/com.cammin.ldtkunity/), [release 6.12.4](https://github.com/Cammin/LDtkToUnity/releases/tag/6.12.4), [hướng dẫn importer](https://cammin.github.io/LDtkToUnity/documentation/Installation/topic_StartupGuide.html).

Các `.ldtkt` là dữ liệu đầu vào của importer và cần được commit cùng `.ldtk`, PNG và `.meta`. Chúng không phải cache. Một checkout mới có thể import scene từ các file này mà không cần chạy exporter trước. Không commit `Library/`, executable của exporter, cache hoặc backup của LDtk.

Nguồn `.ldtk` và `.ldtkt` chỉ dùng đường dẫn tương đối. Các đường dẫn tới Mono/Unity theo máy chỉ được tạo trong `Library/LDtkTilesetExporter/` và không nằm trong Git.

## Quy trình chỉnh map trên Linux

1. Mở project bằng đúng Unity 6000.5.10f1; chờ Package Manager và compilation hoàn tất.
2. Chạy **SacredDrum → LDtk → Prepare Local Tileset Exporter** một lần cho mỗi checkout, hoặc chạy lại sau khi xóa Library. Menu giải nén exporter đi kèm package đã ghim và tạo wrapper Linux bằng Mono của chính Editor hiện tại; không cần sudo hoặc cài Mono hệ thống.
3. Mở nguồn `.ldtk` bằng LDtk 1.5.3.
4. Chỉnh tile/entity và lưu bằng Ctrl+S. Project có lệnh AfterSave:

   ```sh
   ../../../Library/LDtkTilesetExporter/ExportTilesetDefinitionLinux.sh "Prototype_LDtk_Import" $1
   ```

   LDtk hỏi cho phép custom command ở lần lưu đầu tiên. Chỉ cho phép sau khi đối chiếu đúng lệnh trên: wrapper chạy exporter đi kèm package để sinh `.ldtkt` trong thư mục prototype. Đây là thao tác theo project, không yêu cầu thay đổi thiết lập bảo mật hệ thống.
5. Quay lại Unity, chờ asset refresh. Nếu auto refresh chưa chạy, dùng Assets → Refresh. Scene chứa prefab instance của asset `.ldtk`, nên importer cập nhật instance hiện có.
6. Kiểm tra Console, Game View và collision, rồi review/commit nguồn, PNG, `.ldtkt` và các `.meta` thay đổi.

Khi thêm tileset mới, cấu hình PNG **RGBA32 / Uncompressed, Point, PPU16, mipmaps off, NPOT None**, rồi lưu lại LDtk để tạo `.ldtkt`. Không sửa thiết lập của tileset dùng chung trong các scene cũ; các PNG của prototype là bản sao riêng.

Lệnh AfterSave hiện dành cho Linux. Import dữ liệu đã commit trên Windows chưa được thử; authoring trên Windows cần đổi command theo hướng dẫn chính thức và chuẩn bị exporter cho máy đó.

## Cấu trúc map

- Kích thước **24×18 tiles**, tile **16×16**, tương đương **384×288 px**, **24×18 Unity units** ở PPU16.
- Thứ tự LDtk từ trước ra sau: Entities, Collision, Rocks, Obstacles, Water, Dirt_Path, Ground.
- Có 5 Tilemap hiển thị và 1 Tilemap collision ẩn.
- Ground: 432 tile; Dirt_Path: 60; Water: 32; Obstacles: 10 sprite cây; Rocks: 85 tile gồm 80 ô biên và 5 đá phía trong.
- Collision IntGrid: **122 ô** = 32 nước + 90 vật cản. `Collision_1` và `Collision_2` gán Tile có Grid collider. Importer tạo TilemapCollider2D trực tiếp; CompositeCollider2D được tắt cho prototype nhỏ này để tránh scene giữ override hình học cũ sau reimport.
- Collision được vẽ riêng: khi đổi nước, cây, đá hoặc biên, phải cập nhật IntGrid tương ứng. Collision theo cả ô 16×16; tán cây cao hai ô chỉ chặn ô gốc. Đây là hình dạng prototype, chưa phải collider ôm theo sprite.
- `PlayerSpawn` là entity và prefab marker; không có Player Controller hoặc cơ chế di chuyển nhân vật.
- Camera riêng: orthographic size 9. Ảnh 1024×576 có tỉ lệ pixel nguyên 2× so với source 16 PPU. Không cài thêm package Pixel Perfect Camera; ở kích thước Game View khác cần dùng độ phân giải/tỉ lệ nguyên để tránh pixel có độ rộng không đều.

## Nguồn art

Không tải hoặc mua asset mới.

- `Kenney_TinyTown.png`, `Kenney_TinyFarm.png`: bản sao nguyên byte từ tileset hiện có của dự án, **Kenney CC0**; giữ provenance tại [AssetLicenses.md](AssetLicenses.md) và `Docs/Licenses/tiny-town-License.txt`, `tiny-farm-License.txt`.
- `Water.png`: bản sao texture 16×16 procedural-native của dự án tại `Assets/Art/Environment/CanhDong/ReferenceTiles/Water.png`, xem phần cập nhật art trong AssetLicenses.md.
- Marker spawn tham chiếu Sprite đã có tại `Assets/Art/Environment/CanhDong/Props/Spawn.png`, bằng GUID trong repo.

## Kiểm chứng thực tế

| Kiểm tra | Kết quả |
| --- | --- |
| MCP đọc projectRoot, Editor version và scene | PASS, đúng SacredDrum |
| Package được cài / compilation | PASS, 6.12.4; scriptCompilationFailed=false |
| Scene lưu và mở lại | PASS |
| Tilemap, layer order, sprite | PASS, 6 Tilemap; 0 tile hiển thị thiếu sprite/texture |
| Entity | PASS, 1 PlayerSpawn, vị trí cuối (10.5, -9.5), prefab marker hợp lệ |
| Missing scripts / object references | PASS, 0 / 0 trong cây object được import |
| Play Mode collision | PASS, 3 dynamic Rigidbody2D probe, radius 0.2, gravity 0, vận tốc ban đầu (4,0) |
| Spawn thông thoáng | PASS, OverlapCircle radius 0.2 không chạm collider |
| LDtk edit → save → exporter → Unity refresh | PASS |
| Reimport giữ dữ liệu và không tạo object trùng | PASS, 1 root / 1 entity / 17 transform / 6 Tilemap / 6 collider |
| Play Mode sau khi mở lại scene và reimport | PASS, cast chạm đủ nước/cây/đá; không thiếu sprite/reference |
| Scene/asset cũ | PASS, SHA-256 của toàn bộ file Assets có trước nhiệm vụ không đổi |
| Windows build | NOT RUN |

Collision probe chạy trong Play Mode và bị chặn như sau; vận tốc cuối đều (0,0), IsTouchingLayers=true:

| Đối tượng | Vị trí đầu | Vị trí dừng |
| --- | --- | --- |
| Nước | (17, -8.5) | (17.80, -8.50) |
| Cây | (3, -3.5) | (3.80, -3.50) |
| Đá | (7, -7.5) | (7.80, -7.50) |

Các probe chỉ tồn tại trong Play Mode, đã biến mất khi Stop, không nằm trong scene đã lưu. Chưa kiểm tra tương tác bằng Player Controller thật vì controller và các scene cũ nằm ngoài phạm vi nhiệm vụ.

Reimport được thử bằng thao tác chuột trong LDtk để chuyển PlayerSpawn từ ô (9,9) sang (10,9) trên cấu hình collider cuối, sau đó Ctrl+S. IID `779a8ceb-e717-4c64-88d0-b469ca8701a7` không đổi. Unity cập nhật vị trí từ (9.5,-9.5) sang (10.5,-9.5), số tile của từng layer không đổi. Các `.ldtkt` được LDtk AfterSave exporter ghi lại sau thời điểm lưu nguồn.

Đã thử thêm vòng xóa rồi vẽ lại ô Collision nước (18,8) ngay trong LDtk: sau lần xóa/lưu/reimport, Physics2D.OverlapPoint tại (18.5,-8.5) trả về không có collider, ô bên cạnh vẫn chặn; sau lần vẽ lại/lưu/reimport, cả hai ô đều chặn. Dữ liệu cuối giữ đủ 122 ô collision. Đây là kiểm tra hình học collision thực sự cập nhật, ngoài kiểm tra vị trí entity.

## Sự cố đã xử lý và giới hạn

- Lần import ban đầu báo PNG chưa RGBA32 và importer cố gọi ImportAsset trong quá trình import. Đã sửa setting của các bản sao PNG rồi reimport; lỗi này không còn trong các lượt xác minh cuối. Không sửa source package.
- Đã loại các override tile/collider phát sinh trong quá trình thử CompositeCollider. Cấu hình cuối dùng 6 TilemapCollider2D, không dùng CompositeCollider2D; scene không chứa override m_Tiles hoặc m_CompositePaths.
- Hộp thoại Missing Signature của package OpenUPM đã được đóng; một lần restart Editor khôi phục MCP sau khi domain reload bị treo. Không tắt kiểm tra chữ ký hoặc đổi cấu hình bảo mật.
- **UnityConnectWebRequestException: Token Exchange failed** có sẵn trước nhiệm vụ và vẫn xuất hiện lại sau đó. Đây không phải compile/import error của pipeline LDtk; không tuyên bố toàn bộ Console sạch tuyệt đối. Lượt collision đầu không có Error/Warning mới; lượt cuối chỉ ghi nhận lại lỗi Unity Connect này.
- LDtk test profile ghi cảnh báo `inotify_init ... Too many open files`; vòng mở/sửa/lưu/export đã thành công. Không đổi sysctl/quyền hệ thống trong nhiệm vụ này.
- Tương thích đã chứng minh cho prototype này trên Editor hiện tại; chưa chứng minh mọi tính năng importer, Windows authoring/build hoặc gameplay Level 1.
- Không push, không merge main. Crash dump và file ngoài phạm vi được loại khỏi commit.
