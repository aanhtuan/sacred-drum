# Level 1 — Cánh đồng bị lãng quên

## Mục tiêu và phạm vi

Prototype map top-down cho Guardian of the Sacred Drum. Minh xuất phát ở đường làng, khám phá hai bờ ruộng, tiếp cận ba đèn dầu, vượt cầu và đến cổng dẫn vào khu mảnh trống đầu tiên.

**Đã dựng layout, collision và marker. Chưa có Player Controller, chiến đấu, thắp đèn, tự mở cổng, thu thập vật phẩm hoặc Enemy AI.** Không làm Level 2/Boss.

## Thông số

- Unity 6000.5.10f1, URP 17.6.0 / Renderer2D.
- Scene: `Assets/Scenes/Level1_CanhDong.unity`.
- Map 88×56 tile = 4928 ô; tọa độ từ (0,0) đến (87,55), trục X sang phải, Y lên trên.
- Tile 16×16 pixel, PPU 16, 1 tile = 1 Unity unit.
- Metric thử nghiệm: collider tròn bán kính 0,35 unit. Cần kiểm tra lại khi Player thật có kích thước khác.
- Đường chính và các nhánh rộng khoảng 4 tile; cầu rộng 4 tile theo Y, dài 11 tile theo X.
- Camera overview tại (44,28,-10), Orthographic Size 30, thiết kế xem toàn map ở 16:9. Chưa có camera follow; viewport hẹp cần zoom/pan hoặc camera gameplay ở Sprint sau.

## Cấu trúc scene

```text
Level1_CanhDong
├── Main Camera
├── Global Light 2D
├── Grid
│   ├── Ground
│   ├── Paths
│   ├── Water
│   │   └── WaterCollision
│   ├── GroundDecoration
│   ├── Obstacles
│   └── Foreground
├── Environment
│   ├── Trees
│   ├── Rocks
│   ├── Fences
│   ├── Bridges [Grid]
│   │   └── WoodenBridge
│   │       └── BridgeRails
│   └── Buildings
├── Gameplay
│   ├── PlayerSpawn
│   ├── Lamp_01
│   ├── Lamp_02
│   ├── Lamp_03
│   ├── LockedGate
│   ├── DrumFragment_01
│   └── EnemySpawnPoints
│       ├── EnemySpawn_01
│       └── EnemySpawn_02
└── LevelBounds
    ├── West
    ├── East
    ├── South
    └── North
```

Hierarchy export từ Editor: [Hierarchy.json](Verification/Hierarchy.json), 88 GameObject. Foreground hiện để trống để dành cho canopy; Environment/Fences là group dự phòng, fence hiện nằm trên Obstacles Tilemap.

## Tilemap và thứ tự hiển thị

Dùng Sorting Layer Default cùng Sorting Order rõ ràng; không sửa TagManager của project.

| Tilemap / đối tượng | Sorting Order | Số ô có tile | Collision |
| --- | ---: | ---: | --- |
| Ground | 0 | 4928 | Không |
| Paths | 10 | 814 | Không |
| Water | 20 | 312 | Qua WaterCollision child |
| WoodenBridge | 25 | 44 | Deck đi qua được |
| GroundDecoration | 30 | 191 | Không |
| Obstacles | 40 | 641 | TilemapCollider2D + CompositeCollider2D |
| BridgeRails | 45 | 22 | TilemapCollider2D + CompositeCollider2D |
| Trees / Rocks / Buildings | 50 | SpriteRenderer | Collider thân cây, đá và footprint nhà |
| Gameplay markers | 55 | SpriteRenderer | Trigger hoặc gate solid tùy đối tượng |
| Foreground | 60 | 0 | Không |

Composite dùng Rigidbody2D Static, geometry Polygons và `compositeOperation=Merge`. WaterCollision gồm 288 ô chặn, không render; khoảng cầu được bỏ collision để đi qua nước. Ground không có collider phủ kín.

## Luồng di chuyển và vị trí

| Object | Vị trí world XY | Vai trò / collider |
| --- | --- | --- |
| PlayerSpawn | (8.5, 27.5) | Marker xanh trên đường, không Controller |
| Lamp_01 | (23.5, 39.5) | Nhánh phía bắc bờ tây; CircleCollider2D trigger radius 0.85 |
| Lamp_02 | (28.5, 10.5) | Nhánh phía nam bờ tây; trigger radius 0.85 |
| Lamp_03 | (61.5, 15.5) | Nhánh phía nam bờ đông; trigger radius 0.85 |
| LockedGate | (75, 37.5) | BoxCollider2D solid 4×1, đóng mặc định |
| DrumFragment_01 | (75.5, 45.5) | Trong sân rào kín; trigger radius 0.85 |
| EnemySpawn_01 | (35.5, 33.5) | Sân chiến đấu bờ tây; marker Bóng Rơm |
| EnemySpawn_02 | (58.5, 33.5) | Sân chiến đấu bờ đông; marker Bóng Rơm |

1. Xuất phát ở phía tây, theo đường chính X=7..76, Y=26..29.
2. Hai nhánh ngắn dẫn đến Lamp_01 phía bắc và Lamp_02 phía nam. Mỗi nhánh có đích rõ ràng, không có đường cụt trang trí vô nghĩa.
3. Sân chiến đấu thứ nhất ở X=31..39, Y=30..36, giữ ít vật cản.
4. Cầu gỗ X=40..50, Y=26..29 vượt dòng suối chạy dọc map; hai lan can ngăn rời cầu sang nước.
5. Từ bờ đông, nhánh phía nam dẫn đến Lamp_03, sân thứ hai tại X=53..64, Y=30..36.
6. Nhánh phía bắc bên phải dẫn đến LockedGate. Sân mảnh trống được rào kín X=67..83, Y=37..51; lỗ mở duy nhất X=73..76 nằm dưới gate collider.

Cả ba đèn ở ngoài khu cổng. Khi cổng đóng, không có đường vòng vào mảnh trống. Test mở cổng chỉ tạm tắt collider để kiểm chứng layout rồi khôi phục; chưa có logic liên kết ba đèn với cổng.

## Art direction và palette

Cỏ xanh, đường đất nâu cam, hàng cây trồng vàng đại diện ruộng lúa, nước xanh dịu. Marker đèn màu cam vàng đặt trên bệ đá; icon mảnh trống màu đồng. Nhà nhỏ, cây thưa, fence và ruộng tạo bối cảnh prototype nông thôn; đây chưa phải art Việt cổ hoàn thiện.

Tile Palette: `Assets/Art/Environment/CanhDong/Palettes/Level1_CanhDong_Palette.prefab`. Sáu Tilemap con xếp từ trên xuống: Ground, Paths, Water, Vegetation, Structures, Decorations; số tile mẫu lần lượt 5, 10, 4, 9, 10, 8. Trong Tile Palette window chọn palette này; Active Tilemap chọn `Ground` hoặc map muốn vẽ. Palette được chọn không tự quyết định nơi vẽ. Có thể cuộn/pan hoặc Frame All để xem các nhóm dưới viewport.

Nguồn và các art cần thay thế: [AssetLicenses.md](AssetLicenses.md).

## Kiểm chứng thực tế

| Kiểm tra | Kết quả | Bằng chứng |
| --- | --- | --- |
| MCP đọc đúng SacredDrum và scene | PASS | Project root, editor state, hierarchy đọc trực tiếp |
| Import PPU/Point/Uncompressed/no mipmaps, slice 16×16 | PASS | [ImportSettings.json](Verification/ImportSettings.json), 14 PNG |
| Palette hợp lệ và đủ sáu nhóm | PASS | ImportSettings.json; prefab có GridPalette subasset |
| Scene lưu và mở lại | PASS | MCP load và scene path được xác nhận khi bàn giao |
| Ba đèn tiếp cận được khi cổng đóng | PASS | [EditModeGeometry.json](Verification/EditModeGeometry.json) và [PlayModeGeometry.json](Verification/PlayModeGeometry.json) |
| Cổng chặn mảnh trống; tạm mở thì có đường vào | PASS | BFS trên collider thực tế, kiểm tra clearance tại ô và dọc cạnh |
| Qua cầu, chặn nước, cổng đóng, biên map | PASS | [PlayModePhysicsProbe.json](Verification/PlayModePhysicsProbe.json) |
| Console sau thử / compile | PASS | Không có error/warning trong lần đọc sau Play Mode |
| Tệp gốc bảo toàn | PASS | [Preservation.json](Verification/Preservation.json): 121 tệp gốc đối chiếu SHA-256 |
| Input/di chuyển bằng Player thật | MANUAL TEST REQUIRED | Chưa có Player Controller |
| Thắp đèn → mở cổng → nhặt mảnh trống | NOT IMPLEMENTED | Ngoài phạm vi map placeholder |
| Build và chạy Windows | NOT RUN | Windows Build Support chưa cài |

Probe vật lý là CircleCollider2D radius 0.35, Rigidbody2D Dynamic, gravityScale=0, Continuous; chạy trong Play Mode bằng Physics2D.Simulate với timestep 0.02s. Kết quả: nước dừng X≈42.645; qua cầu X≈52.500; cổng đóng dừng Y≈36.645; mở cổng đến Y≈42.000; biên trái dừng X≈2.355. Probe được xóa, trạng thái gate và simulation mode được khôi phục trong finally.

Khi tạo script Editor có một lỗi compile `BoundsInt.PositionEnumerator.Where` không được hỗ trợ; đã thay bằng foreach, recompile thành công rồi chạy các test trên. Một lệnh hỗ trợ mở rộng Hierarchy ban đầu dùng API ID cũ cũng được sửa sang GetEntityId. Đây là lỗi authoring đã xử lý, không phải gameplay runtime error còn tồn tại.

## Công cụ Editor và chỉnh sửa tiếp

- `Assets/Editor/Level1/Level1AssetSetup.cs`: cấu hình importer, slice, tạo Tile assets, palette, placeholder pixel.
- `Assets/Editor/Level1/Level1MapBuilder.cs`: layout và thao tác bổ sung collision/marker theo menu SacredDrum → Level 1.
- `Assets/Editor/Level1/Level1MapValidation.cs`: kiểm tra collider thực tế và probe Play Mode, xuất JSON.
- Các script nằm trong thư mục Editor, không thêm MonoBehaviour gameplay vào scene/build.
- BuildLayout từ chối ghi đè scene đã tồn tại hoặc bỏ scene hiện tại chưa lưu; AddCollisionsAndMarkers từ chối tạo marker trùng.
- Sau bàn giao, chỉnh scene bằng Tile Palette/Unity Editor. Không chạy lại generator để thay chỉnh sửa tay. `PolishLayout` là thao tác authoring có ghi dữ liệu, chỉ dùng khi chủ đích.
- Khi thay đổi kích thước nhân vật, vật cản hoặc gate, chạy lại geometry/probe với metric mới. Probe hiện chỉ xác minh hình tròn 0.35, không thay được playtest bằng input thật.

## Vấn đề còn lại và kiểm tra thủ công

- Cần Player thật để đánh giá cảm giác điều khiển, khoảng trống chiến đấu và tốc độ khám phá; không có nhân vật điều khiển trong scene này.
- Cây/props đang có sorting order cố định, chưa có Y-sort cho Player hoặc canopy che/hiện theo vị trí; Foreground dành sẵn cho bước đó.
- Chưa có animation nước, VFX đèn, âm thanh hay nhạc. Các icon đèn chỉ là marker, không thể hiện trạng thái gameplay đã thắp.
- Art lúa, mái tranh, tre, đèn và hoa văn Đông Sơn cần thiết kế riêng trước bản hoàn thiện.
- Không sửa Build Profiles/danh sách scene build gốc. Mở scene trực tiếp để xem map.

## Ảnh thực tế

- [Tổng thể camera](Screenshots/Level1_Overview.png).
- [Camera render trong Play Mode](Screenshots/Level1_PlayMode.png).
- [Unity Editor: Scene View, Hierarchy và Tile Palette](Screenshots/Level1_Editor_Hierarchy_Palette.png) — chụp cửa sổ thật bằng Spectacle trên KDE, palette viewport chỉ hiện phần trên; đủ nhóm được kiểm tra qua prefab/JSON.

Không dùng ảnh concept hoặc bản vẽ thay cho bằng chứng Unity. Capture Scene View qua MCP bị lật dọc và ReadScreenPixel trả ảnh đen trên máy này; đã giữ các ảnh lỗi ngoài repo trong AssetDownloads/Level1/Debug, dùng ảnh cửa sổ native thay thế.

## Git bàn giao

Branch: `feature/level1-map-prototype`, không merge main. Các nhóm commit: import licensed tileset; tilemap layout; collision/markers và validation; tài liệu và evidence. SHA, push và working tree cuối được báo trong phản hồi bàn giao sau khi đối chiếu remote.
