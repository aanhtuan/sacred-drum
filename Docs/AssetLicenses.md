# Level 1 — Asset research và giấy phép

Kiểm tra ngày 07/11/2026. Tải trực tiếp từ Kenney, sử dụng nút công khai Continue without donating và endpoint download-link của website. Không đăng nhập, mua asset, vượt paywall hoặc chạy chương trình trong ZIP.

## So sánh ba bộ

| Pack / tác giả | Top-down và phong cách | Tile | Nội dung / import Unity | License | Quyết định |
| --- | --- | --- | --- | --- | --- |
| [Tiny Town — Kenney](https://kenney.nl/assets/tiny-town) | Overworld góc nhìn 3/4, mặt đất trên mặt phẳng XY; cùng pixel density với Tiny Farm | 16×16 | Cỏ, đường nối 3×3, cây, nhà, gỗ; packed PNG 192×176, slice thuận tiện | CC0 | **Bộ chính**, chỉ dùng thành phần trung tính; không dùng tháp thành, mái nhà kiểu châu Âu hoặc vũ khí |
| [Tiny Farm — Kenney](https://kenney.nl/assets/tiny-farm) | Nông trại, cùng nhóm Tiny, palette và góc nhìn tương thích | 16×16 | Cây trồng, đá, mái xanh; packed PNG 192×176 | CC0 | **Bộ phụ**, dùng cây trồng và mái placeholder; không dùng máy móc/nông trại hiện đại |
| [Tiny Dungeon — Kenney](https://kenney.nl/assets/tiny-dungeon) | Top-down RPG nhưng thiên về dungeon, sewer và đá tối | 16×16 | Có tường/đồ vật dungeon; PNG dễ slice | CC0 theo trang chính thức | Không tải/import: bối cảnh lệch cánh đồng và không cần bộ thứ ba |

Nhận xét về sự phù hợp là đánh giá thiết kế cho prototype, không phải chứng nhận tính chính xác văn hóa. Không bộ nào là tileset Việt cổ chuyên dụng.

## Các bộ thực sự sử dụng

### Tiny Town 1.1

- Tác giả/đơn vị phát hành: **Kenney**, https://kenney.nl.
- Nguồn: https://kenney.nl/assets/tiny-town.
- License gốc: [tiny-town-License.txt](Licenses/tiny-town-License.txt).
- License: [CC0 1.0 Universal](https://creativecommons.org/publicdomain/zero/1.0/).
- Cho phép sao chép, chỉnh sửa, sử dụng trong game thương mại và phân phối source PNG trong GitHub công khai. Không bắt buộc attribution; dự án vẫn ghi nguồn tự nguyện. Không ngụ ý Kenney bảo trợ dự án.
- ZIP gốc: `/home/atuna/Documents/Game/AssetDownloads/Level1/Original/kenney_tiny-town.zip`.
- Kích thước: **182243 bytes**. SHA-256: `9768692dccff1d706408a5aedd6ca4f6cd1409506cbc84cb2f862919764be977`.
- File import: `Assets/Art/Environment/CanhDong/Tilesets/Kenney_TinyTown.png` — bản sao nguyên byte của `Tilemap/tilemap_packed.png`.
- 45 Tile assets được chọn từ bộ này; không copy toàn bộ thư mục asset pack, preview, shortcut hoặc sample map.

### Tiny Farm 1.0

- Tác giả/đơn vị phát hành: **Kenney**.
- Nguồn: https://kenney.nl/assets/tiny-farm.
- License gốc: [tiny-farm-License.txt](Licenses/tiny-farm-License.txt).
- CC0 1.0 Universal; quyền sửa, dùng trong game và phân phối source công khai tương tự Tiny Town. Attribution không bắt buộc, vẫn ghi nguồn tự nguyện.
- ZIP gốc: `/home/atuna/Documents/Game/AssetDownloads/Level1/Original/kenney_tiny-farm.zip`.
- Kích thước: **191128 bytes**. SHA-256: `a06f75f312c27eff15a2288475612e6f6699411be7259d408323cd15a790decc`.
- File import: `Assets/Art/Environment/CanhDong/Tilesets/Kenney_TinyFarm.png` — bản sao nguyên byte của `Tilemap/tilemap_packed.png`.
- 23 Tile assets được chọn, chỉ một phần xuất hiện trong map.

Hai ZIP: **373371 bytes**; sau giải nén tổng **282 file**. Archive gốc, bản giải nén và bản HTML nguồn nằm ngoài repository. Manifest kiểm chứng: [Downloads.json](Verification/Downloads.json). Nội dung chữ của license giữ nguyên; bản trong Docs chỉ chuẩn hóa newline và bỏ khoảng trắng cuối dòng. License nguyên byte vẫn còn trong bản giải nén gốc.

## Các thay đổi import

- Không sửa pixel của hai sheet nguồn.
- Mỗi sheet packed 192×176 được slice bằng Unity Sprite Editor Data Provider: 12 cột × 11 hàng, mỗi ô 16×16, margin 0, spacing 0. Không dùng khoảng cách 1 px của sheet `tilemap.png` chưa packed.
- Tạo 132 sprite mỗi sheet, 264 sprite tổng cộng; sheet có các ô trống. Chỉ tạo Tile assets cho phần đã chọn.
- Unity tạo mọi asset và `.meta`; không sửa YAML/meta thủ công.
- Sprite PPU 16, Point filtering, Uncompressed, mipmaps tắt, Full Rect.
- Nền ruộng dùng sprite đất Tiny Town với tint nâu trong Tilemap; cây trồng dùng sprite Tiny Farm. Màu tint không sửa source PNG.

## Placeholder được tạo riêng cho SacredDrum

12 PNG 16×16 được tạo bằng API Texture2D trong công cụ Editor `Level1AssetSetup.cs`: Water, BankWest, BankEast, Bridge, Fence, StoneFloor, OilLamp, Gate, BronzeFragment, Spawn, EnemySpawn, Boundary.

Đây là placeholder pixel đơn giản tạo trong dự án, không tải từ bên thứ ba hoặc dùng concept image làm tileset. Việc đưa chúng vào repository được thực hiện theo yêu cầu xây map và push của Project Owner. Không có hạn chế phân phối từ nguồn asset ngoài dự án.

Tổng import: **14 PNG**, gồm hai sheet nguồn và 12 placeholder; **80 Tile assets**, một Tile Palette prefab. Không có file lớn cần thêm pattern Git LFS; file lớn nhất là scene text khoảng 2,2 MiB. Không thay đổi chính sách LFS hiện có.

## Art cần thay thế trước bản hoàn thiện

- Cây trồng vàng chỉ đại diện **ruộng lúa**, sprite hiện tại không mô tả chính xác cây lúa Việt Nam.
- Mái xanh/nhà nhỏ là placeholder trung tính, cần thiết kế lại mái tranh Việt cổ.
- Hàng rào gỗ cần thay bằng hàng rào tre có art thống nhất.
- Đèn dầu, cổng, bệ đá và mảnh trống cần sprite riêng; icon mảnh trống chưa tái hiện hoa văn Đông Sơn khảo cứu.
- Không sử dụng nhận diện thương hiệu, logo Kenney hoặc asset trả phí.
