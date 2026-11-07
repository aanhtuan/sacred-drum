# Level 1 — Visual & Level Design Refinement

Scene: `Assets/Scenes/Level1_CanhDong.unity`. Branch: `feature/level1-map-prototype`. Unity 6000.5.10f1 / URP 2D. Đây là chỉnh sửa trực tiếp map 88×56 hiện có, không tạo project mới, không thêm gameplay.

## Các thay đổi

| Hạng mục | Trước | Sau |
| --- | --- | --- |
| Đường đất | Nhánh rộng, góc theo ô tile, hai sân vuông nối thành dải đất lớn | Đường chính rộng khoảng 2,6 unit; nhánh 1,9 unit; đường cong liên tục, mép bám lưới pixel 1/16 unit |
| Ruộng | Các khối gần chữ nhật, viền bậc/terrace thẳng | Sáu thửa có đường viền riêng, góc bo không đều, bờ đất đi được và rãnh chia hàng; giữ khoảng trống cạnh đường/đèn/sân |
| Sông | Các đoạn thẳng đổi X theo từng bậc | Đường bờ uốn nhẹ liên tục, bề rộng biến thiên khoảng 3,1–3,5 unit; lớp grass shoulder → bờ đất → nước |
| Cầu | Một cầu gỗ trên dòng nước thẳng | Giữ nguyên một cầu chính, deck và lan can; chỉ chừa đoạn Y=26..30 trong collision sông để đi qua |
| Cảnh vật | Cây, đá và cỏ rời rạc | Cây gom thành các cụm ven biên, quanh chòi và ven sông; đá thành cụm nhỏ, cỏ/hoa đặt gần cụm thay vì rải đều |
| Đèn | Marker ở đầu nhánh, bệ vuông | Đặt trên bệ tròn nhỏ, tăng nhẹ sprite visual, vùng tiếp cận rộng và không có vật cản gần trigger |
| Chiến đấu | Hai sân chữ nhật lớn | Hai bãi đất tròn pha màu với đường; mỗi sân kiểm tra có bán kính trống ít nhất 3 unit |

Các vị trí cây phát sinh thay đổi trong lúc làm việc được giữ theo scene đang lưu. Đã kiểm tra lại connectivity và clearance trên bản cuối; không chạy lại bước dựng tự động để ghi đè các vị trí đó. Snapshot tọa độ cây nằm trong FinalEditorState.json.

## Gameplay layout hiện tại

| Object | World XY |
| --- | --- |
| PlayerSpawn | (14.5, 41.0), cách vị trí chòi khoảng 2,06 unit |
| Lamp_01 | (23.5, 37.5) |
| Lamp_02 | (29.5, 13.5) |
| Lamp_03 | (62.5, 18.5) |
| EnemySpawn_01 | (34.5, 30.5) |
| EnemySpawn_02 | (58.5, 29.5) |
| LockedGate | (75.0, 37.5), giữ nguyên collider và trạng thái đóng |
| DrumFragment_01 | (75.5, 45.5), giữ trong khu rào kín |

Luồng chính: chòi → đường làng → nhánh đèn phía bắc/nam bờ tây → bãi chiến đấu tây → cầu → bãi chiến đấu đông → nhánh đèn phía nam → cổng → khu mảnh trống. Ba đèn đều ở ngoài cổng. Không có đường vòng qua rào khi gate collider đang bật.

## Tilemap, collision và sorting

Các Tilemap gốc được giữ. Thêm các lớp hiển thị mỏng để chuyển tiếp, không thêm collider trên Ground hoặc đường đi:

| Lớp | Sorting Order | Vai trò |
| --- | ---: | --- |
| Ground | 0 | Cỏ nền, giữ texture Point |
| FieldBanks / Farmland | 3 / 4 | Bờ ruộng và đất canh tác |
| CombatMargins / CombatClearings | 6 / 7 | Bãi chiến đấu có mép chuyển màu |
| PathShoulders / Paths | 9 / 10 | Viền đường và phần đất chính |
| ObjectivePads | 11 | Bệ đèn và lối vào/bệ khu trống |
| RiverShoulders / RiverBanks / Water | 17 / 18 / 20 | Bờ cỏ, bờ đất, mặt nước |
| Bridge / GroundDecoration | 25 / 30 | Cầu và lúa/cỏ/hoa |
| Obstacles / BridgeRails | 40 / 45 | Biên/rào và lan can |
| Props / gameplay visuals / Foreground | 50 / 55 / 60 | Giữ quy ước hiển thị cũ |

`Grid/Water/WaterCollision` vẫn tồn tại cùng Rigidbody2D Static. TilemapCollider2D/CompositeCollider2D cũ của nước được tắt, thay bằng PolygonCollider2D có **hai path** bám bờ cong: phần trên và phần dưới cầu. Cặp collider cũ không được xóa. Collider của rào, lan can, cây, đá, nhà và cổng được giữ/di chuyển cùng props. Không có collider vô hình còn sót lại trên dòng sông cũ.

## Bảo toàn asset và cách tạo mép pixel

- SHA-256 đối chiếu **454 tệp có sẵn** trong Assets/Packages/ProjectSettings: chỉ `Level1_CanhDong.unity` thay đổi. Mọi texture, asset, importer và `.meta` cũ đều nguyên byte.
- Giữ đủ **143 GameObject gốc**, gồm toàn bộ gameplay objects; thêm tám GameObject Tilemap overlay, tổng 151. Scene GUID giữ nguyên.
- Không tạo PNG mới, không đổi Filter Mode, compression hoặc mipmap của asset hiện có. Tất cả texture CanhDong vẫn dùng Point.
- Mesh mép địa hình chỉ cắt phần hiển thị của texture cũ theo lưới 16 pixel/tile. Không làm mờ texture. Thư viện `RefinementMeshes/RefinedTiles.sdmesh` chứa 1615 Tile/Sprite subasset được bake, dùng lại texture gốc.
- Hai script mới đều thuộc `Assets/Editor/Level1`: `Level1LayoutRefinement.cs` và `RefinementMeshImporter.cs`. Không sửa code gameplay hoặc code core khác.
- Importer dùng API dữ liệu vertex/index đã xác minh trực tiếp trong Editor: [SpriteDataAccessExtensions.SetVertexCount](https://docs.unity3d.com/ScriptReference/U2D.SpriteDataAccessExtensions.SetVertexCount.html), [SetIndices](https://docs.unity3d.com/ScriptReference/U2D.SpriteDataAccessExtensions.SetIndices.html). Sample importer kiểm tra đúng 64 đỉnh trước khi bake toàn bộ.
- Công cụ Apply từ chối scene khác, Play Mode, scene chưa lưu hoặc scene đã có các lớp refinement. Không chạy lại generator trên scene đã chỉnh tay.

Bản scene trước sửa được sao lưu ngoài project tại `/home/atuna/Documents/Game/AssetDownloads/Level1/Backups/PreRefinement/` và vẫn có trong lịch sử commit `f405a9b`.

## Kiểm chứng

| Kiểm tra | Kết quả / bằng chứng |
| --- | --- |
| Scene lưu, mở lại qua MCP, gate đóng, compile hoàn tất | PASS — [FinalEditorState.json](Verification/Refinement/FinalEditorState.json) |
| Ba đèn tiếp cận được từ spawn khi gate đóng | PASS — [EditModeGeometry.json](Verification/EditModeGeometry.json), [PlayModeGeometry.json](Verification/PlayModeGeometry.json) |
| Gate đóng chặn fragment; tạm mở thì có đường vào | PASS — hai báo cáo geometry trên |
| Không vật cản trong các hướng tương tác đèn | PASS — 16/16 vị trí mẫu quanh mỗi đèn, probe radius 0.35, khoảng cách tới đèn 1.25 unit |
| Không vật cản trong hai sân chiến đấu | PASS — OverlapCircle bán kính 3 unit tại hai EnemySpawn |
| Rigidbody2D qua cầu, dừng ở nước/cổng/biên | PASS — [PhysicsProbe.json](Verification/Refinement/PhysicsProbe.json), chạy Play Mode với timestep 0.02s |
| Bảo toàn asset/meta và gameplay GameObject | PASS — [Preservation.json](Verification/Refinement/Preservation.json) |
| Console cuối | 0 error; còn warning Unity AI Account API timeout như baseline; có warning MCP duplicate-command trong các lượt trước — [Console.json](Verification/Refinement/Console.json) |

Probe vật lý bán kính 0.35 unit dừng ở nước tại X≈40.353; qua cầu tới X≈52.500; cổng đóng dừng Y≈36.645; cổng tạm mở tới Y≈42.000; biên trái dừng X≈2.355. Probe được xóa, gate và simulation mode FixedUpdate được khôi phục. Các vị trí cây thay đổi sau lượt Play Mode được giữ lại và đã kiểm tra lại geometry/interaction clearance trên bản lưu cuối.

Trong lượt authoring đầu có lỗi `OverrideGeometry` không được phép trong ngữ cảnh Editor. Mẫu lỗi lưu tại [AuthoringErrorSamples.json](Verification/Refinement/AuthoringErrorSamples.json); các asset authoring lỗi được chuyển ra ngoài project trong AssetDownloads/Level1/Debug/FailedRefinementMeshes. Sau khi ghi bằng chứng, Console đã được xóa để kiểm tra lượt sửa. Pipeline cuối chuyển sang SpriteDataAccessExtensions và importer mẫu đã PASS; không đưa các asset lỗi vào Git. MCP có một số timeout/duplicate-command, nên kết quả lưu và scene được đọc lại để xác nhận thay vì coi phản hồi timeout là thành công.

## Hạn chế còn lại

- Texture lúa hiện tại vẫn lặp theo tile; thay đổi hình thửa, bờ và rãnh làm giảm cảm giác ô vuông nhưng không thay được một bộ rice/autotile chuyên dụng.
- Asset props được vẽ ở độ phân giải khác terrain 16×16; một số alpha highlight và tỷ lệ pixel vốn có chưa hoàn toàn đồng nhất. Giữ nguyên source theo yêu cầu, không dùng blur để che khác biệt.
- Chưa có Y-sort theo Player hoặc canopy fade. Các cụm cây được đặt xa tuyến chính để giảm che đường; cần đánh giá lại khi có Player thật.
- **MANUAL TEST REQUIRED:** cảm giác điều khiển, giao chiến với Bóng Rơm và tương tác bằng input thật. Các phép thử hiện dùng probe collider, không phải Player Controller.
- Chưa lập trình thắp đèn, mở cổng theo điều kiện, nhặt mảnh trống hoặc AI. Windows build NOT RUN.

## So sánh ảnh Unity thực tế

| View | Trước | Sau, bản lưu cuối |
| --- | --- | --- |
| Game/camera View | [Before](Screenshots/Refinement/Refinement_Before_GameView.png) | [After](Screenshots/Refinement/Refinement_Final_GameView.png) |
| Scene View trong cửa sổ Unity | [Before](Screenshots/Refinement/Refinement_Before_SceneView.png) | [After](Screenshots/Refinement/Refinement_Final_SceneView.png) |

[Camera trong Play Mode](Screenshots/Refinement/Refinement_After_PlayMode.png). Các screenshot Scene View được chụp bằng Spectacle trên KDE vì capture viewport của MCP có lỗi lật ảnh ở môi trường này. Ảnh camera được chụp qua Unity MCP.
