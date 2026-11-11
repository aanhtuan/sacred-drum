> Superseded for Minh: see [Minh_Chibi_Integration.md](Minh_Chibi_Integration.md) for the current no-hat character and removed old art/animations. Historical validation below is retained.

> Update: five scarecrows now use the owner's straw-brown/red-eye design with twenty directional animation clips. See [Scarecrow_Straw_Integration.md](Scarecrow_Straw_Integration.md). Descriptions below record the earlier preview.

# Minh farmer and scarecrow preview

Scene: `Assets/Scenes/Prototype_LDtk_Import.unity`.
Source map: `Assets/LevelDesign/LDtkPrototype/Prototype_LDtk_Import.ldtk`.
Branch: `codex/minh-farmer-preview`.

## Controls and behavior

- WASD uses the existing PlayerController and existing Input System Move action, speed 4. Neither the original controller nor original Player prefab was edited.
- Minh is a Prefab Variant at `Assets/Art/Characters/PrototypeCast/Prefabs/Minh_Farmer.prefab`. Farmer wears a non la, brown shirt, indigo trousers, red sash and carries a wooden staff.
- Idle breathing, running and staff attack each have six independently drawn sprite frames for down, side and up. Left mirrors side: 54 frames, nine AnimationClips, one AnimatorController. Idle 5 fps, run 12 fps, attack 10 fps / 0.6 seconds. No root motion.
- J or left mouse plays a staff swing. This is a visual action only: no damage, attack hitbox, health system or enemy AI. Movement remains controlled by the original script even during the swing.
- `Assets/Scripts/PrototypeCast/MinhSpriteAnimation.cs` reads movement/facing and selects animation in LateUpdate. It does not write the player position or velocity.
- Ten Scarecrow entities are saved in LDtk and mapped to `Scarecrow_VisualOnly.prefab`. They have SpriteRenderer plus importer metadata after import, with no Rigidbody2D, collider or AI. They are static visual previews.
- PlayerSpawn is on clear ground at LDtk pixel (88,520), Unity (5.5,-32.5). Source entity positions survive reimport and use project-relative asset references.

## Camera and map

Camera orthographic size changed from 18 to 7.5 (2.4 times closer). The full camera viewport follows Minh using the existing PlayerFollowCamera, bounded to Rect(0,-36,48,36). At 16:9 it shows approximately 26.67 by 15 world units: the game fills the screen and scrolls through the map. Map tile and collision layout remain the existing haunted field prototype.

## Validation on Unity 6000.5.10f1

- Compilation PASS; EditorUtility.scriptCompilationFailed false.
- Actual Play Mode keyboard tests PASS: WASD movement, diagonal normalization, release stops, water/thorn collision, camera clamp at all four map corners. Original controller and camera script unchanged.
- Animation PASS: queued synthetic Input System WASD/J events; observed actual Update, LateUpdate and Animator playback across 7.3 seconds. All nine states showed multiple distinct SpriteRenderer frames. Each of the three attack directions displayed all six frames; left running used flipX. No manual Animator.Update or clip sampling used for this validation. Physical keyboard and mouse hardware timing were not automated.
- Evidence: `Docs/Evidence/PrototypeCast/AnimationValidation.json` contains time/state/sprite/position observations.
- Recorded real camera frames during a repeat Play Mode input sequence, encoded as H.264 in `Docs/Evidence/PrototypeCast/Minh_Animation_PlayMode.mp4` (81 frames at 10 fps). Recording playback timing is approximate; Editor sampling was not a constant game frame rate.
- LDtk source force-reimported, prototype prefab reverted through Unity to remove transient tile cache overrides, lit Tilemap materials retained, camera target rebound, scene saved and reopened. One player, ten scarecrows, ten Tilemaps, zero missing script references, nine animation clips retained.
- Console had intermittent UnityConnect Token Exchange network errors during work; final read returned zero errors. No new C# compilation or animation runtime errors found after fixes.
- SHA256 comparison of pre-existing Assets confirms only the prototype Scene, its .ldtk and .ldtk.meta changed. Level1_CanhDong, Prototype_Player, original Player prefab, PlayerController and PlayerFollowCamera were preserved.

## Art and rebuilding

Sources: `Assets/Art/Characters/PrototypeCast/Sources/`.
Animation assets: `Assets/Art/Characters/PrototypeCast/Animations/`.
Generation tool, prompt and art caveats: [Minh_Animation_Prompts.md](Minh_Animation_Prompts.md).

Editor authoring utilities:
1. `SacredDrum > Prototype Cast > Prepare Sprites and Prefabs` prepares the static concepts and variants.
2. Then `SacredDrum > Prototype Cast > Build Minh Animations` slices the selected sheet and restores the animated Minh variant. This order matters if regenerating the prefab from scratch.

This is a visual prototype. Frame alignment and pixel density can be refined further for production. Windows build was NOT RUN. Protected scenes were not redesigned.
