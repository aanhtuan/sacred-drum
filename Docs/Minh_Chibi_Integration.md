# Minh chibi replacement in the LDtk prototype

Scene: `Assets/Scenes/Prototype_LDtk_Import.unity`.
Prefab: `Assets/Art/Characters/PrototypeCast/Prefabs/Minh_Farmer.prefab`.
Sources: `Assets/Art/Characters/PrototypeCast/Sources/MinhChibi/`.

## Result and controls

The approved no-hat farmer replaces the older conical-hat sprite: tied dark hair, earth-brown patched shirt, charcoal-indigo trousers, red-brown sash, wicker rice backpack and wooden staff. Standing sprite height is approximately 1.85 world units, reduced from 2.2. Illustrated art uses Bilinear filtering, no mipmaps and uncompressed textures.

- WASD: original PlayerController and input asset unchanged, speed4. W uses a true back-view RunUp/IdleUp, S uses the approved front three-quarter view, D uses right profile and A mirrors it.
- Space: one-shot visual jump, then idle/run.
- Hold C or left Ctrl: crouch down, stay low, release to stand back up.
- J or left mouse: one-shot two-handed martial staff sweep, then idle/run.
- Jump and attack are visual animation only. The feet collider remains on the ground; there is no obstacle-jump mechanic, hitbox or damage.
- Jump/crouch/attack currently use the approved front three-quarter sheets; separate directional art is available for idle/run only.

Seven source sheets contain72 drawn frames. Eleven clips cover the five action groups and directional idle/run variants; crouch is split into Down/Hold/Up. Clip timing follows the preview GIF timing, with Unity's final sample interval included. Runtime action timers read actual clip lengths, so one-shots do not get cut off by hard-coded durations.

Minh remains a variant of the original Player prefab. The prefab, controller and script GUIDs were retained. LDtk PlayerSpawn mapping continues using the same prefab GUID. The existing feet collider, movement, camera, scarecrows and map layout were preserved.

## Removed older assets

After auditing references outside the removal set, Unity AssetDatabase removed these assets together with their .meta files:

- `Sources/Minh_Farmer.png`
- `Sources/Minh_Animations.png`
- The9 earlier `Idle_*`, `Run_*`, `Attack_*` clips for Down/Side/Up.

The retained Minh.controller was rebuilt with new states and sprite references. The authoring menu now refreshes approved animation prefabs; it no longer regenerates the removed static Minh art. Original Player.prefab, PlayerController, PlayerFollowCamera, Level1_CanhDong, Prototype_Player and recovery files were preserved.

## Validation on Unity6000.5.10f1

- Compilation PASS; final Console read0 Error entries.
- LDtk source force-reimported after prefab replacement. Scene saved and reopened; new prefab sprite/controller persisted and camera target remained valid.
- Play Mode PASS:11 state/direction/input checks, including W back view, S front, A/D profile, jump, crouch down/hold/release, J attack and mouse attack. [Validation.json](Evidence/MinhChibi/Validation.json).
- Method: synthetic input isolated from hardware and explicitly processed using temporary Input System manual updates; actual PlayerController.Update and animation LateUpdate invoked, with real Animator playback and Play Mode FixedUpdate. No manual Animator.Update or SampleAnimation. Original update mode and real keyboard/mouse were restored; temporary devices removed. Physical keyboard/mouse timing was not automated.
- Physics PASS using original controller and actual Physics2D simulation: straight/diagonal0.5-second distances both approximately2units; release velocity zero; water stopped at(2.70,-11.50), thorns at(20.50,-28.53).
- One player, ten scarecrows, zero missing scripts, eleven clips, root motion disabled.
- Initial Console had an unrelated existing LicensedTiles.sdmesh import error; subsequent final reads returned zero errors. That asset was not changed.
- [Play Mode recording](Evidence/MinhChibi/Minh_Chibi_PlayMode.mp4):137 camera frames encoded at8fps, approximate capture timing.
- [Back-view running proof](Evidence/MinhChibi/Running_Up_Back_View.png).
- No Windows build performed.

## Art provenance

The five approved source sheets were copied unchanged from the preview folder. Two additional directional atlases were generated with the built-in image_gen tool using the approved four-view no-hat farmer as reference. [Exact prompts](Minh_Chibi_Art_Prompts.md). All runtime source textures and .meta files are inside the Unity project; no machine-specific file paths are needed at runtime.
