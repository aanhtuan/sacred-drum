# Guardian of the Sacred Drum — Người Giữ Hồn Trống

SacredDrum là game 2D top-down action adventure, đồ họa pixel art, lấy cảm hứng từ văn hóa Việt cổ và trống đồng Đông Sơn. Ngôn ngữ lập trình: C#. Môi trường phát triển: Linux. Nền tảng đích: Windows x64.

## Level 1 map prototype

Scene `Assets/Scenes/Level1_CanhDong.unity`: map top-down 88×56 tile theo ảnh tham chiếu Level 1, đường uốn qua ruộng vàng, nhà tranh, tre, cây tán lớn, suối/cầu và bệ trống; có collision cùng marker cho ba đèn/cổng/mảnh trống. Terrain 16×16, prop atlas PPU64. Bản blockout giữ trong `Assets/Scenes/Archive/Level1_CanhDong_Blockout.unity`. Xem [thiết kế và kiểm chứng](Docs/Level1_Map_Design.md), [nguồn asset và giấy phép](Docs/AssetLicenses.md). Chưa có Player Controller hoặc gameplay thắp đèn/mở cổng.

## Thiết lập nền tảng Sprint 0

Đã thiết lập cấu trúc, Git/Git LFS, GitHub và kiểm thử Unity MCP thực tế. Chưa triển khai gameplay. Xem [báo cáo kiểm chứng](docs/SPRINT_0_REPORT.md).

- Unity Editor: **6000.5.10f1** (revision `3bd4f66ad299`).
- URP **17.6.0**, renderer thực tế: `Renderer2D`.
- Force Text, Visible Meta Files, Editor Mode 2D đã được bật sẵn.
- Scene chuẩn bị: `Assets/Scenes/Prototype_Player.unity`.
- Scene gốc `Assets/Scenes/SampleScene.unity` và scene template được bảo toàn.
- Prototype chỉ có Main Camera (Orthographic, Base) và Global Light 2D (Global, intensity 1).
- Windows Build Support **chưa được cài**; chưa build hay chạy bản Windows.

## Mở dự án

1. Cài Unity Hub và Editor đúng phiên bản **6000.5.10f1**.
2. Cài Git LFS trước khi clone (`git lfs install`), rồi clone repository:

```sh
git clone https://github.com/aanhtuan/sacred-drum.git
cd sacred-drum
git lfs pull
```

3. Unity Hub → Add project from disk → chọn thư mục có `Assets`, `Packages`, `ProjectSettings`. Checkout hiện tại: `/home/atuna/Documents/Game/SacredDrum`.
4. Chờ Package Manager resolve và import xong; kiểm tra Console.
5. Mở `Assets/Scenes/Prototype_Player.unity`.
6. Trước khi build Windows, thêm Windows Build Support qua Unity Hub cho đúng Editor, rồi chọn Windows x64 trong Build Profiles. Hiện Build Settings vẫn giữ SampleScene; chỉ cập nhật danh sách scene khi bắt đầu công việc build được giao.

Không mở project bằng phiên bản khác rồi lưu nâng cấp tùy ý. Commit `Packages/manifest.json` và `Packages/packages-lock.json`; package MCP dùng Git URL, lockfile giữ revision đã resolve. Chưa nâng cấp package ở Sprint 0.

## Cấu trúc

```text
Assets/
Animations/
  Boss/
  Enemies/
  Player/
Art/
  Characters/
    CoTrang/
    Enemies/
    Minh/
  DongSon/
    BronzeDrum/
    Patterns/
    Symbols/
  Environment/
    CanhDong/
    DinhLang/
    GiengLang/
  VFX/
    DrumShockwave/
    StaffAttack/
    TigerUltimate/
Audio/
  Music/
  SFX/
Materials/
Prefabs/
  Characters/
  Enemies/
  Environment/
Scenes/
ScriptableObjects/
Scripts/
  Combat/
  Core/
  Enemies/
  Interactions/
  Player/
  Skills/
  UI/
Settings/
  Scenes/
UI/
```

Giữ cả thư mục Settings/template Unity tạo sẵn. `.gitkeep` giữ thư mục trống khi clone; `.meta` được Unity tạo và phải commit cùng tài nguyên. Chưa có gameplay scripts.

## Git workflow

Repository: [aanhtuan/sacred-drum](https://github.com/aanhtuan/sacred-drum). Nhánh chính: `main`.

```sh
git status
git fetch origin
git switch -c feature/sprint-1-player-controller
# Thực hiện đúng phạm vi, kiểm tra Unity Console.
git add <cac-file-da-review>
git diff --cached --stat
git diff --cached --check
git commit -m "feat: describe the reviewed change"
git push -u origin feature/sprint-1-player-controller
```

Dùng PR để review và merge vào main cho các Sprint sau. Không force push, không reset dữ liệu chưa sao lưu, không commit cache hoặc thông tin nhạy cảm. Origin fetch URL giữ HTTPS; trên máy hiện tại, push URL dùng `git@github.com:aanhtuan/sacred-drum.git` vì SSH đã xác thực tài khoản `aanhtuan`. Push URL là cấu hình Git cục bộ, không tự truyền sang máy clone mới.

`.gitignore` loại Library, Temp, Obj, Build, Builds, Logs, UserSettings, file IDE sinh tự động và secret cục bộ. Assets, Packages, ProjectSettings và `.meta` vẫn được quản lý.

Git LFS theo dõi `*.psd`, `*.aseprite`, `*.blend`, `*.fbx`, `*.wav`; không đưa toàn bộ ảnh, scene hoặc file nhỏ vào LFS. Hiện chưa có asset LFS. Nếu muốn loại một file nhỏ thuộc năm định dạng này, review và thêm ngoại lệ theo đường dẫn trước khi stage. Không chạy migrate làm đổi lịch sử.

Git LFS trên máy hiện tại được cài ở `~/.local/bin/git-lfs` từ RPM Fedora `3.7.1-6.fc44`, đã kiểm tra chữ ký/digest. Hook và filter dùng phạm vi repository (`git lfs install --local`). Máy khác có thể cài bằng package manager Linux tương ứng.

## Unity MCP

Package đã có: `com.coplaydev.unity-mcp` **10.3.1-beta.11**. MCP server runtime hiện dùng **10.3.0**, stdio qua uvx; đã kiểm thử tương tác với package trên, chưa thay đổi phiên bản. Cấu hình Codex đã tồn tại và được giữ nguyên.

```sh
codex mcp list
```

Cấu hình server hiện tại tương đương:

```text
/usr/bin/uvx --offline --prerelease explicit --from mcpforunityserver>=0.0.0a0 mcp-for-unity --transport stdio
```

`--offline` yêu cầu package server đã có trong cache uv; đây là cấu hình của máy hiện tại, không phải trình cài cho máy mới. Khi thiết lập máy khác, chuẩn bị uv và phiên bản server tương thích trước, rồi cấu hình theo package MCP đang dùng. Không commit đường dẫn riêng, token hoặc cấu hình Codex toàn cục vào repo.

1. Mở đúng project trong Unity, chờ compile xong.
2. Trong Unity, mở cửa sổ MCP for Unity (menu Window), kiểm tra transport stdio và bridge đang chạy.
3. Trong Codex, đọc `mcpforunity://instances` và chọn instance của SacredDrum bằng `set_active_instance`; ID có thể thay đổi sau khi khởi động lại.
4. Đọc `mcpforunity://project/info` và `mcpforunity://editor/state`, xác nhận projectRoot trước mọi thao tác ghi.
5. Gọi `manage_scene(action="get_hierarchy")`, `read_console(action="get", types=["error"])`.
6. Chỉ đánh dấu CONNECTED sau thao tác đọc Editor thành công; nếu chỉ thấy tên server thì NOT VERIFIED.

Các công cụ đã gọi thành công: `set_active_instance`, `manage_scene`, `read_console`, `execute_code`; resource project info, editor state và cameras. Có thể dùng các tool asset, GameObject, component, prefab và script theo phạm vi Sprint. Luôn đọc scene/Console trước thay đổi. Nếu lệnh treo, chọn lại instance đúng, kiểm tra Editor idle rồi thử lại; không coi nhận diện server là đủ.

## Roadmap

- Sprint 0: môi trường, cấu trúc, GitHub, MCP, scene trống và tài liệu.
- **Sprint 1: Player Controller**, chỉ bắt đầu khi Project Owner giao việc.
- Các Sprint sau: animation, combat, skills, interactions, enemies, UI và level; thứ tự chi tiết do Project Owner phê duyệt.

## Package inventory tại Sprint 0

Danh sách khai báo trực tiếp; dependency bắc cầu và revision Git nằm trong `Packages/packages-lock.json`.

| Package | Version / source |
| --- | --- |
| `com.coplaydev.unity-mcp` | `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity` |
| `com.unity.2d.animation` | `15.1.0` |
| `com.unity.2d.aseprite` | `5.0.3` |
| `com.unity.2d.psdimporter` | `14.0.3` |
| `com.unity.2d.sprite` | `1.0.0` |
| `com.unity.2d.spriteshape` | `15.0.3` |
| `com.unity.2d.tilemap` | `1.0.0` |
| `com.unity.2d.tilemap.extras` | `8.0.3` |
| `com.unity.2d.tooling` | `3.0.1` |
| `com.unity.ai.assistant` | `2.21.0-pre.2` |
| `com.unity.ai.inference` | `2.6.1` |
| `com.unity.collab-proxy` | `2.13.6` |
| `com.unity.ide.rider` | `3.0.38` |
| `com.unity.ide.visualstudio` | `2.0.26` |
| `com.unity.inputsystem` | `1.20.0` |
| `com.unity.multiplayer.center` | `1.0.1` |
| `com.unity.pipeline` | `0.10.0-exp.1` |
| `com.unity.render-pipelines.universal` | `17.6.0` |
| `com.unity.test-framework` | `1.7.0` |
| `com.unity.timeline` | `1.8.13` |
| `com.unity.ugui` | `2.5.0` |
| `com.unity.visualscripting` | `1.9.12` |
| `com.unity.modules.accessibility` | `1.0.0` |
| `com.unity.modules.adaptiveperformance` | `1.0.0` |
| `com.unity.modules.ai` | `1.0.0` |
| `com.unity.modules.androidjni` | `1.0.0` |
| `com.unity.modules.animation` | `1.0.0` |
| `com.unity.modules.assetbundle` | `1.0.0` |
| `com.unity.modules.audio` | `1.0.0` |
| `com.unity.modules.cloth` | `1.0.0` |
| `com.unity.modules.director` | `1.0.0` |
| `com.unity.modules.imageconversion` | `1.0.0` |
| `com.unity.modules.imgui` | `1.0.0` |
| `com.unity.modules.jsonserialize` | `1.0.0` |
| `com.unity.modules.particlesystem` | `1.0.0` |
| `com.unity.modules.physics` | `1.0.0` |
| `com.unity.modules.physics2d` | `1.0.0` |
| `com.unity.modules.physicscore2d` | `1.0.0` |
| `com.unity.modules.screencapture` | `1.0.0` |
| `com.unity.modules.terrain` | `1.0.0` |
| `com.unity.modules.terrainphysics` | `1.0.0` |
| `com.unity.modules.tilemap` | `1.0.0` |
| `com.unity.modules.ui` | `1.0.0` |
| `com.unity.modules.uielements` | `1.0.0` |
| `com.unity.modules.umbra` | `1.0.0` |
| `com.unity.modules.unityanalytics` | `1.0.0` |
| `com.unity.modules.unitywebrequest` | `1.0.0` |
| `com.unity.modules.unitywebrequestassetbundle` | `1.0.0` |
| `com.unity.modules.unitywebrequestaudio` | `1.0.0` |
| `com.unity.modules.unitywebrequesttexture` | `1.0.0` |
| `com.unity.modules.unitywebrequestwww` | `1.0.0` |
| `com.unity.modules.vectorgraphics` | `1.0.0` |
| `com.unity.modules.vehicles` | `1.0.0` |
| `com.unity.modules.video` | `1.0.0` |
| `com.unity.modules.wind` | `1.0.0` |
| `com.unity.modules.xr` | `1.0.0` |
