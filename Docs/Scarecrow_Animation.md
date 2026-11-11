# Corrupted scarecrow animation preview

> Historical preview, replaced by the owner's straw-brown four-direction design with five enemies. See [current integration](Scarecrow_Straw_Integration.md). Old runtime textures and the corrupted-idle clip have been removed.

Current prototype: `Assets/Scenes/Prototype_LDtk_Import.unity`.

## Result

Ten existing LDtk Scarecrow entities now share a mutated scarecrow animation: hunched torso, tilted stitched sack head, asymmetrical root claws, ragged hat and tunic. The accepted art has dusty purple/plum cloth, violet corruption accents and red eyes. Lower saturation and cool highlights keep it distinct from the grey-brown map without the earlier bright golden straw.

Source: `Assets/Art/Characters/PrototypeCast/Sources/Scarecrow_CorruptedIdle.png`.
Prefab: `Assets/Art/Characters/PrototypeCast/Prefabs/Scarecrow_VisualOnly.prefab` (existing GUID retained).
Clip: `Assets/Art/Characters/PrototypeCast/Animations/Scarecrow_CorruptedIdle.anim`.
Controller: `Assets/Art/Characters/PrototypeCast/Animations/Scarecrow.controller`.

Eight distinct poses animate shoulders, head, claw and ragged clothing in place. A 1.5-second loop uses uneven frame holds (ticks0,2,5,7,9,12,14,16,18 at 12 fps). ScarecrowIdleVariation sets a deterministic initial phase and slightly varied playback speed from each LDtk placement, so the crowd does not move together. It has no Update movement logic, targeting, hitbox or damage. No Rigidbody2D or Collider2D added; root motion disabled. The prefab retains Sprite-Lit-Default and existing fog/2D lighting.

The animation sheet is sliced through Unity Sprite Editor data providers. Alpha bounds are inspected per cell, feet are aligned to each frame pivot, Point filtering/no mipmaps/uncompressed sprite import is used. One shared PPU preserves the drawn size across poses, average height 2.25 world units.

Rebuild via `SacredDrum > Prototype Cast > Build Scarecrow Animation`. If using the older Prepare Sprites and Prefabs menu, run this animation builder afterwards because that menu recreates the static preview.

## Actual verification

- Unity 6000.5.10f1, compilation PASS.
- Source PNG imported and LDtk source force-reimported: ten existing entities retained their animation prefab.
- Real Play Mode observed for 4.5 seconds on the final art: all ten showed all 8 distinct SpriteRenderer frames. All ten root positions remained fixed; all ten had no Rigidbody2D/Collider2D. Speeds/phases varied.
- Final Console read: zero Error entries. Intermittent UnityConnect token-exchange network errors occurred before validation, unrelated to the animation.
- Evidence: [Validation.json](Evidence/ScarecrowAnimation/Validation.json), [Play Mode video](Evidence/ScarecrowAnimation/Purple_RedEyes_PlayMode.mp4), [camera screenshot](Evidence/ScarecrowAnimation/Purple_RedEyes_PlayMode.png).
- Video encodes 32 captured camera frames at 8 fps; playback timing approximates the Editor sampling rate.
- No AI pursuit, locomotion or combat implemented. This is a looping visual preview.
- Original static Scarecrow.png remains available. Minh, his movement/animation, protected scenes and the LDtk map layout were preserved.

## Art provenance

Built-in image_gen with transparent_background=true. Exact final generation/edit prompts below. Reference was the existing original Scarecrow.png; the dark intermediate sheet was recolored to the user's requested purple/red eyes. Selected PNG copied into the project without pixel editing; original outputs remain in Codex generated_images. Discarded brighter/intermediate designs are not runtime dependencies.

### Dark mutated sheet prompt

Use case: stylized-concept. Reference image identifies the EXISTING scarecrow in SacredDrum, but user asks to REDESIGN it as a darker mutated monster that blends into a foggy dead field. Produce a PRODUCTION ANIMATION SPRITE SHEET, 8 frames, exactly 4 columns x 2 rows on wide2:1 canvas. Same creature throughout all frames.
ART DIRECTION: sinister, organic, rotten and malformed, less cute/cartoon, not bright yellow. A possessed scarecrow with hunched crooked spine, asymmetrical shoulders, narrow sackcloth head tilted at an unnatural angle, torn rotting straw hat with a broken brim, thin dim purple eyes sunk deep in the dark sack face. Charred grey-brown straw, mud-stained grey beige sackcloth, nearly black desaturated plum ragged tunic, frayed rope. One long knotted wooden arm ends in branching root claws; the other shorter arm bent close to chest. Twisted rootlike wooden legs rooted in place, a few corrupted black vine growths through torn sleeves and waist. Irregular silhouette, angular slender limbs rather than big cute round hands. Small dark-violet fissures in torso with restrained subtle light, not a neon outline. No blood, no gore, no skull head. Recognizably a corrupted agricultural scarecrow.
PALETTE: muted soot brown, ashy straw, charcoal, moss grey and deep bruised purple. LOW SATURATION and LOWER VALUE than reference. No bright gold/yellow/orange surfaces or candy purple. Fine cool-grey edge accents make silhouette readable in dark fog; do not solve visibility with a glow halo. Keep shadowy textures crisp and sparse.
ANIMATION: 8 actual independently drawn key poses in a seamless living idle loop, row-major. Feet remain EXACTLY planted at SAME horizontal and vertical coordinates in every cell, one scale. Body sways and breathes ominously with overlapping motion, head and cloth lag behind shoulders: neutral hunched -> shoulder slowly left, head lag -> deeper left lean with long root claw curling -> recover through center -> hunched right, head lag -> deeper right with shorter hand trembling, long claw unfolding -> recover -> head twitch then near-neutral. Strong readable bend at waist/neck/elbows, not whole-sprite translations/rotations. Smooth progression between neighboring poses, no sudden changes to clothing/body anatomy. NO walking or attack actions.
Style: crisp limited-palette pixel art matching top-down16bit game, clearly square pixel clusters, slight overhead RPG front view, not isometric, no smooth painting or excessive texture noise.
COMPOSITION: equal square cells, each figure centered with feet baseline at88% of cell height, at least8% transparent padding on every side INCLUDING hat and root claw tips. Exactly4 columns2rows eightframes. Each body same apparent size. Every hat, foot, claw inside cell. No text, labels, numbers, gridlines, shadows, floor, scene, border or watermark. Truly transparent RGBA, alpha0 outside hard silhouette, no background glow. This is meant for actual sprite frame animation, not a collage.

### Final purple/red-eyes edit prompt

Use case: precise-object-edit. This is a PRODUCTION animation sprite sheet of8 poses. User likes the dark mutated design but it blends into grey/brown field background too much. RECOLOR ONLY, PRESERVE EXACTLY all eight poses, silhouette, cropping, pixel geometry, feet alignment, size and 4cols2rows layout.
Make the ragged tunic DISTINCTLY DUSTY PURPLE / BRUISED VIOLET instead of nearly black brown, with medium-value muted purple highlights on folds and violet corrupted vine veins along shoulders and forearms. Use charcoal purple shadows, desaturated plum-violet midtones. The body needs enough purple hue and slightly brighter midtones to be readable against a dark desaturated brown-grey map without looking neon. Preserve ashy grey-brown rotting wood/straw hands, hat and legs; add restrained cool purple corruption accents. Keep darker, grim, desaturated mood, NO bright gold or orange straw.
IMPORTANT: Change both eyes in EVERY FRAME to RED, deep crimson with a tiny scarlet-red core, a subtle ominous red ember look. NO purple eyes, NO blue eyes. Restrained red luminance, no large halo outside face. Tiny torso fissures can remain dark violet, eyes must be clearly red.
Do not redraw anatomical features, change poses, add weapons or add effects. Keep all hats and claws entirely inside each own cell. Actual alpha0 transparent background preserved, no shadows, no checkerboard, no text, no palette swatches, no borders. Exact same sprite sheet structure and crisp pixel art.
