# Sprint 1 — Player Controller in Level 1

Implemented on `codex/sprint1-player-controller`, based on map commit `b5ec971`.
Unity 6000.5.10f1; URP 17.6.0; Input System 1.20.0. Packages and Editor version unchanged.

## Implementation

- `Assets/Scripts/Player/PlayerController.cs`: clones and enables only `Player/Move` from the existing input asset. WASD (plus its existing arrow/gamepad bindings), clamped diagonal movement, Inspector `Move Speed` default 4 units/s, Rigidbody2D velocity in FixedUpdate, stop on disable, retained cardinal `LastFacingDirection`. No combat, skills, interactions or puzzles enabled.
- `Assets/Scripts/Core/PlayerFollowCamera.cs`: Orthographic follow in LateUpdate with SmoothDamp (0.18 seconds), viewport-aware bounds (0,0)-(88,56), snap on start. Size 7, z=-10. Bounds clamp tested at 16:9. A viewport larger than the map on either axis cannot fit fully inside it; that axis is centered.
- `Assets/Prefabs/Characters/Player.prefab`: SpriteRenderer, dynamic Rigidbody2D, gravity 0, frozen rotation, interpolation, continuous collision, non-trigger CircleCollider2D radius 0.3 at feet offset (0,-0.45). Zero-friction material `Player_NoFriction.physicsMaterial2D`.
- `Assets/Art/Characters/Minh/Minh_Placeholder.png`: original 16×24 code-drawn placeholder, 16 PPU, point filtering. No third-party asset imported. Replace this sprite when official Minh art is available. Facing is stored as state; directional animation is outside this sprint.
- `Assets/Scenes/Level1_CanhDong.unity`: prefab child of the existing `Gameplay/PlayerSpawn`, feet at (14.5,41); marker renderer hidden, marker position preserved. Camera now follows Player. Scene changes made through Unity MCP.
- `Assets/Editor/Level1/Sprint1PlayerValidation.cs`: repeatable, explicitly invoked Play Mode verification. No test component is shipped on Player.
- `.gitattributes`: apply the existing Unity serializer whitespace exception to `.prefab` files, preserving Unity-generated empty fields without manual YAML edits.

## Verification

Unity MCP connected to the actual SacredDrum Editor and verified project root, version, active scene and state.

**PASS:** compile/domain reload complete; no Console errors; prefab components and reference; scene validation (0 missing scripts, 0 broken prefabs); save/reload retained Player and camera target.

**PASS, 20 Play Mode probes:** four WASD directions; equal straight/diagonal distance (~2 units in 0.5 seconds); release stops and preserves facing; opposite keys cancel; disabling stops velocity; representative tree, rock, fence, river, locked gate and boundary block Player; wooden bridge crossing succeeds; four camera corners and one smooth follow step.

These probes queue a synthetic keyboard through the real Input System action, invoke the actual controller Update/FixedUpdate, and simulate the real scene physics at 0.02 seconds. Camera tests explicitly teleport the target and invoke LateUpdate. They do not establish physical keyboard focus behavior or subjective smoothness across frame rates. Collision probes sample representative routes, not every surface and approach angle.

**PASS:** comparison with the original scene: 534 of 539 existing serialized objects unchanged; only five objects changed for spawn child/marker and camera configuration. No original object removed. All map tilemaps, art references, obstacle colliders, environment, lamps, gate and fragment retained.

Existing map editor scripts still emit obsolete API warnings (`FindObjectsSortMode`); no such legacy scripts were edited. Initial MCP snippet compilation failures were tool snippets, not project script errors.

**NOT RUN:** human keyboard acceptance, visual camera QA at low/high FPS, Windows executable build/run. Windows x64 Build Support checked in Unity and is unavailable. Active Editor platform remains Linux; no platform switch or package/module installation performed.

Evidence:

- `Docs/Verification/Sprint1/PlayerValidation.json`
- `Docs/Verification/Sprint1/EditorValidation.json`
- `Docs/Verification/Sprint1/LayoutPreservation.json`
- `Docs/Verification/Sprint1/PlayerSpawn_PlayMode.png` (MCP Main Camera capture during Play Mode)

## Manual acceptance

1. Open `Assets/Scenes/Level1_CanhDong.unity`, press Play, then click Game View to focus it.
2. Walk with W/A/S/D. Hold W+D, then D alone: speed should remain equal. Release all keys: no drift. To inspect stored facing, use the debugger property `LastFacingDirection`; the placeholder has no direction animation.
3. Walk into tree trunks and rocks, then the shrine fence and locked gate. None should be passable. Walk around corners and hold diagonal movement along walls to check sliding.
4. Follow the road southeast from spawn to the wooden bridge near (45,27.5), cross both ways. Try entering the river above/below the bridge; it must block movement.
5. Walk toward map borders and observe camera bounds. Compare camera motion with Game View VSync on/off and at low/high frame rates; confirm no visible jitter.
6. Stop Play. Select Player prefab or instance, adjust `Move Speed` in Inspector, and Play again to check tuning. Revert local tuning if retaining the default is desired.
7. To repeat automated checks: while playing Level 1, use `SacredDrum > Sprint 1 > Validate player in Play Mode`. It returns PASS/FAIL and overwrites the JSON evidence. Temporary keyboard and simulation mode are restored after testing.

The pre-existing untracked `mono_crash.mem.5088.1.blob` is preserved and excluded from the commit. No main merge, history rewrite, or subsequent sprint work.
