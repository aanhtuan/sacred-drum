# Sparse vegetation pass

The owner requested fewer props, dry branches and weeds only, with no scattered rocks. The prototype now has ten small decorative placements: five fallen dry branches and five weed tufts, concentrated at a few field edges. All36 raised-terrain prop instances were removed, including boulders, rock clusters, slabs, mounds, embankments and logs. Seven native rock tiles (Landmarks tile8) and their seven Collision int-grid cells were also removed. The architectural shrine tiles, lanterns, original dry trees/rice, paths, water/thorns, player spawn and five scarecrows are preserved.

Current source map: Assets/LevelDesign/LDtkPrototype/Prototype_LDtk_Import.ldtk.
Current atlas: Assets/LevelDesign/DarkFields/TerrainRelief/VegetationProps.png.
Four prefab variants: Relief_BranchCrooked, Relief_BranchFork, Relief_WeedsDry, Relief_WeedsLow in Assets/LevelDesign/DarkFields/TerrainRelief/Prefabs/.
The LDtk definitions/importer mappings now reference only these four Relief_* variants. SparseVegetation.ldtkt provides their crop rectangles. Previous terrain artwork/prefabs remain unused in the asset library; there are no instances of them in the prototype.

Widths are1.1/.95/.65/.75world units respectively. Muted grey-brown/olive artwork uses Point filtering, RGBA32 and the current lit material. The props have no colliders and do not block movement. Existing prototype depth sorting remains active.

## Validation

- Unity6000.5.10f1 compilation succeeded and final isolated LDtk reimport/Console query returned zero errors.
- Scene saved/reopened and Play Mode checked:10 decorations,0 decoration colliders,0 previous relief prop instances,5 scarecrows.
- Physics2D.OverlapPoint at all seven removed rock centers returned no collider, confirming no invisible blocking at those locations.
- All layers except Entities, Landmarks and the seven Collision cells match the previous source; original player/scarecrow entities match exactly.
- Existing route cells were not edited. No manual traversal of the entire map or Windows build was performed.
- [Game View](Evidence/SparseVegetation/Sparse_Vegetation_GameView.png), [layout](Evidence/SparseVegetation/LayoutValidation.json), [runtime/removed-rock physics checks](Evidence/SparseVegetation/RuntimeValidation.json).

## Art provenance and exact prompt

Generated with the built-in image_gen tool, transparent_background=true. Selected atlas copied unchanged into the project and sliced/scaled through Unity import settings.

Use case: stylized-concept. Production top-down 2D pixel art vegetation prop atlas for a muted dark abandoned rice field. Exactly TWO columns by TWO rows, FOUR isolated small props, one centered per equal square cell, generous transparent margins. Row1 col1: thin crooked dry fallen branch with a few bare angular twigs, dark weathered grey-brown wood, horizontal low silhouette. Row1 col2: small forked dry fallen branch, 3 fine twigs, sparse broken bark, no leaves. Row2 col1: small sparse tuft of dry ochre-grey wild grass, 5 to 7 short uneven blades, slightly bent. Row2 col2: low sparse grey-olive weeds with several tiny dried seed stems, compact unobtrusive mound of grass, no lush foliage. Hand pixelled 16-bit RPG style, logical32x32pixel cells enlarged with square nearest-neighbor pixels, restrained 5-7 color palette, no smooth painting or outlines. Slight top-down view, light upper left, subtle narrow contact shadow under each. Deliberately understated and low to the ground. NO rocks, NO stones, NO boulders, NO logs or tree trunks, NO soil platforms, NO big trees, NO vibrant grass. Real transparent alpha background, no brown backdrop, no checkerboard pattern, no labels, no text, no borders, no extra decoration.
