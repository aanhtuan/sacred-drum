# Minh readability on dirt paths

The owner requested a slightly larger Minh and clothing that separates from the brown path. All seven animation atlases now use an indigo blue-grey peasant tunic, pale rolled cuffs/patches and an earthy red sash. Skin, brown hair, straw basket/rice and wooden staff retain their original palette. No hat or new armor is added.

Visible first-frame standing height is1.45world units (410pixels /282.7586PPU), up11.5% from1.3. Each sheet is normalized by its own first standing frame. Collider, movement speed, input, camera configuration and map source are unchanged.

Sources: `Assets/Art/Characters/PrototypeCast/Sources/MinhChibi/`. Prefab: `Assets/Art/Characters/PrototypeCast/Prefabs/Minh_Farmer.prefab`. Source/meta GUIDs are retained; the authoring tool rebuilt the eleven clips/controller states with numeric frame order, including the originally preferred Run sheet for side running and separate back idle/run.

## Actual verification

- Unity6000.5.10f1 compilation/import succeeded; final Console returned zero errors.
- Seven PNGs retain their original canvas dimensions and transparent alpha; all72 cells are nonempty. Generated edits show minor edge changes: thresholded silhouette IoU97–98%, rather than exact pixel preservation. Every atlas was visually inspected.
- Eleven runtime clips have no missing sprite references. Standing height measured from actual visible alpha is1.45units.
- Play Mode: isolated synthetic J and left-mouse input both entered StaffAttack, displayed all eight attack frames and returned to IdleSide. Actual Animator playback was used. Native keyboard/mouse and Dynamic Input System updates were restored after the check.
- [On-path color comparison](Evidence/MinhReadability/Minh_Indigo_On_Dirt_Path.png) temporarily placed Minh on a safe brown path and selected Idle for capture. Original runtime position, controller/visual enable states, Rigidbody velocity/interpolation and camera follow were restored immediately afterwards. This screenshot is a visual comparison, not a movement test.
- All scene files, original Player prefab and PlayerController match their pre-change hashes. Five scarecrows and ten sparse vegetation decorations are retained.
- [Atlas validation](Evidence/MinhReadability/AtlasValidation.json), [silhouette comparison](Evidence/MinhReadability/SilhouetteValidation.json), [runtime/input observations](Evidence/MinhReadability/RuntimeValidation.json).
- No complete level traversal or Windows build was performed.

The selected images were produced with built-in image_gen and copied into the project. [Exact prompts](Minh_Readability_Art_Prompts.md).
