> Superseded for Minh: see [Minh_Chibi_Integration.md](Minh_Chibi_Integration.md) for the current no-hat character and removed old art/animations. Historical validation below is retained.

# Minh art and animation provenance

Generated with the built-in `image_gen` tool, transparent_background=true. Original generated files remain under the Codex generated_images directory. Selected PNGs were copied unchanged into the Unity project; Unity Sprite Editor rectangles slice the animation sheet without modifying its pixels.

## Selected assets

- `Assets/Art/Characters/PrototypeCast/Sources/Minh_Farmer.png`: static farmer concept, conical hat, brown rolled sleeve shirt, indigo trousers, red sash, sandals, one wooden staff. Background cleanup retained the silhouette.
- `Assets/Art/Characters/PrototypeCast/Sources/Scarecrow.png`: hostile straw creature with patched plum tunic, stitched sack face, violet eyes, ragged straw hat and raised arms. Background cleanup retained the silhouette. Static visual only.
- `Assets/Art/Characters/PrototypeCast/Sources/Minh_Animations.png`: selected animation sheet; reference was Minh_Farmer.png. 6 columns, 9 rows, 54 frames. Left is mirrored from right. Rows have hand-audited transparent boundaries in MinhAnimationAssets.cs; do not assume an exact uniform vertical grid on replacement art.

## Animation prompt (exact)

Use case: stylized-concept. Production transparent pixel art animation sprite sheet for Unity top-down RPG. Reference image is Minh character identity: Vietnamese young male farmer wearing conical straw non la hat, brown rolled sleeve shirt, red waist sash, indigo trousers, sandals, holding ONE wooden staff. Preserve this identity and chunky pixel style. Create EXACTLY 6 COLUMNS x 9 ROWS =54 separate frames on a portrait canvas, each cell equal size, no gridlines or text. Transparent background alpha zero. Strict uniform grid spanning whole canvas. Every character centered horizontally in its cell, feet aligned at 85 percent cell height, same size in all frames with enough margin for staff swing. Each frame is independently drawn limb pose, not copies or rigid translations. RPG slight overhead perspective not isometric.
Rows counted from top:
1: DOWN-facing idle breathing 6 frames, subtle chest rise/fall and relaxed arm/hat motion.
2: DOWN-facing RUN 6 frames, pronounced alternating legs and counter-swinging arms, full stride, staff held alongside.
3: DOWN-facing STAFF ATTACK 6 frames: ready, windup, raised staff, sweeping strike, followthrough, recovery.
4: RIGHT-facing idle breathing 6 frames, profile.
5: RIGHT-facing RUN 6 frames, visibly alternating legs, full running cycle.
6: RIGHT-facing STAFF ATTACK 6 frames, windup strike recovery.
7: UP-facing idle breathing 6 frames, back of hat/clothing, NO face.
8: UP-facing RUN 6 frames from behind with alternating steps.
9: UP-facing STAFF ATTACK 6 frames from behind, overhead strike and recovery.
No left facing row needed. Clear distinct silhouettes in running and attacks. Crisp square pixel clusters, limited earthy palette dark outline. No soft glow, no shadows, no brown background halo, no gradients outside character, no scenery, no labels, no numbers, no gutters, no borders. Keep ALL hands, feet, hat and staff entirely inside each cell. Canvas aspect 2:3 to make 6x9 square cells.

## Art limitations

AI generated prototype artwork, manually inspected in Game View. Frame scale and outlines can still be refined by a pixel artist. No external paid asset was downloaded. The original static art remains available; Minh now displays distinct sprite frames rather than translating the single concept image.
