# Straw scarecrow replacement

Scene: `Assets/Scenes/Prototype_LDtk_Import.unity`. Source map: `Assets/LevelDesign/LDtkPrototype/Prototype_LDtk_Import.ldtk`.

The owner's reference replaces the purple mutated scarecrow with a squat golden-straw creature, black face, red eyes, patched brown tunic and reddish scarf. Five existing LDtk entities remain; five were removed at the owner's request. Existing entity IDs and the prefab/controller/script GUIDs were retained. No duplicated enemy objects were added.

Runtime source sheets: `Assets/Art/Characters/PrototypeCast/Sources/ScarecrowStraw/`. There are 120 illustrated frames across Idle (6 per direction), Walk (8), Attack (6), Hurt (4) and Die (6), with Down/Right/Up/Left directions. Twenty clips live in `Assets/Art/Characters/PrototypeCast/Animations/Scarecrow_*.anim`, driven by the retained `Scarecrow.controller` and `Scarecrow_VisualOnly.prefab`.

Each sheet has transparent alpha, Bilinear filtering, no mipmaps and uncompressed import. First standing frame height is 1.55 world units, close to Minh's 1.65. Cell pivots share a baseline within each sheet; frame ordering is numeric. Sprite-Lit-Default keeps the existing 2D lighting and fog. A cool, darker renderer tint (0.70, 0.74, 0.80) reduces the bright golden cutout appearance against the field. The reference is illustrated rather than pixel art; this remains a style approximation, not a hand-pixelled tileset match.

Idle/Walk loop; Attack/Hurt return to Idle after playing; Die holds its last frame. The default map behavior is asynchronous idle breathing/sway. In Play Mode, select an enemy and use the ScarecrowIdleVariation component context menu `Visual Preview` to preview each action. The public Preview(action, Facing) method selects the direction. These are visual previews only: no enemy translation, pursuit, health, hitbox, damage or player combat logic was added.

After an external GUID-reference audit found no remaining consumers, Unity AssetDatabase deleted the old Scarecrow.png, Scarecrow_CorruptedIdle.png and Scarecrow_CorruptedIdle.anim together with their meta files. Historical preview evidence is retained outside Assets.

## Verification

- Unity 6000.5.10f1 compilation succeeded; final Console query returned zero errors.
- Five nonempty transparent atlases, 120 cells, twenty states/clips, no missing sprite references and no missing scripts.
- Natural Play Mode Animator playback observed for all twenty states. The main timed pass observed nineteen states; a separate HurtRight pass covered the state skipped during an Editor pause. Sampling is not a frame-perfect animation capture.
- No enemy root movement during the preview pass; no Rigidbody2D or Collider2D added.
- LDtk source reimported with exactly five enemies, scene saved/reopened, camera target rebound to the imported player, ten Tilemaps retained. Tile counts match the LDtk source layers. All scene files, original Player prefab and PlayerController hashes match the pre-change baseline.
- Final Game View: [Scarecrow_Straw_GameView-1.png](Evidence/ScarecrowStraw/Scarecrow_Straw_GameView-1.png). Raw observations and atlas/clip checks are in the same evidence folder.
- No Windows build or combat gameplay test performed.

Art was generated using the built-in image_gen tool from the supplied reference. Selected PNGs are copied into the project; no runtime dependency on private machine paths. [Exact generation and transparency-edit prompts](Scarecrow_Straw_Art_Prompts.md).
