using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace SacredDrum.EditorTools
{
    public static class TerrainReliefAssets
    {
        const string Root = "Assets/LevelDesign/DarkFields/TerrainRelief/";
        public static readonly string[] Names = { "Boulder", "RockCluster", "SplitSlab", "Mound", "BankLong", "BankCorner", "Reeds", "Log" };
        static readonly float[] Widths = { 2.1f, 1.8f, 1.65f, 1.9f, 2.6f, 2f, 1.1f, 2f };

        [MenuItem("SacredDrum/Dark Fields/Build Terrain Relief Prefabs")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play Mode first.");
            string atlas = Root + "ReliefProps.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(atlas);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 200;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.isReadable = true;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 2048;
            var platform = importer.GetDefaultPlatformTextureSettings();
            platform.format = TextureImporterFormat.RGBA32;
            platform.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SetPlatformTextureSettings(platform);
            importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(atlas);
            var pixels = texture.GetPixels32();
            var factories = new SpriteDataProviderFactories();
            factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            var old = provider.GetSpriteRects().ToDictionary(r => r.name);
            var rects = new System.Collections.Generic.List<SpriteRect>();
            for (int i = 0; i < 8; i++)
            {
                int left = Mathf.RoundToInt(i % 4 * texture.width / 4f);
                int right = Mathf.RoundToInt((i % 4 + 1) * texture.width / 4f);
                int bottom = Mathf.RoundToInt((1 - i / 4) * texture.height / 2f);
                int top = Mathf.RoundToInt((2 - i / 4) * texture.height / 2f);
                int x0 = right, x1 = left, y0 = top, y1 = bottom;
                for (int y = bottom; y < top; y++) for (int x = left; x < right; x++)
                    if (pixels[y * texture.width + x].a > 90)
                    { x0 = Mathf.Min(x0, x); x1 = Mathf.Max(x1, x + 1); y0 = Mathf.Min(y0, y); y1 = Mathf.Max(y1, y + 1); }
                if (x1 <= x0 || y1 <= y0) throw new System.InvalidOperationException("Empty relief sprite: " + Names[i]);
                rects.Add(new SpriteRect { name = Names[i], spriteID = old.ContainsKey(Names[i]) ? old[Names[i]].spriteID : GUID.Generate(),
                    rect = new Rect(x0, y0, x1 - x0, y1 - y0), alignment = SpriteAlignment.Custom, pivot = new Vector2(.5f, .015f) });
            }
            provider.SetSpriteRects(rects.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply();
            importer.SaveAndReimport();
            var sprites = AssetDatabase.LoadAllAssetsAtPath(atlas).OfType<Sprite>().ToDictionary(s => s.name);
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/LevelDesign/DarkFields/Materials/DarkFieldsLit.mat");
            for (int i = 0; i < Names.Length; i++)
            {
                string path = Root + "Prefabs/Relief_" + Names[i] + ".prefab";
                bool exists = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
                var root = exists ? PrefabUtility.LoadPrefabContents(path) : new GameObject("Relief_" + Names[i]);
                try
                {
                    var renderer = root.GetComponent<SpriteRenderer>();
                    if (renderer == null) renderer = root.AddComponent<SpriteRenderer>();
                    renderer.sprite = sprites[Names[i]];
                    renderer.sharedMaterial = material;
                    renderer.color = new Color(.88f, .90f, .94f, 1f);
                    float scale = Widths[i] / renderer.sprite.bounds.size.x;
                    root.transform.localScale = new Vector3(scale, scale, 1);
                    if (root.GetComponent<SacredDrum.Gameplay.PrototypeDepthSort>() == null) root.AddComponent<SacredDrum.Gameplay.PrototypeDepthSort>();
                    if (i != 6)
                    {
                        var collider = root.GetComponent<CapsuleCollider2D>();
                        if (collider == null) collider = root.AddComponent<CapsuleCollider2D>();
                        collider.direction = CapsuleDirection2D.Horizontal;
                        collider.size = new Vector2(Widths[i] * .68f / scale, .34f / scale);
                        collider.offset = new Vector2(0, .12f / scale);
                    }
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally
                {
                    if (exists) PrefabUtility.UnloadPrefabContents(root);
                    else Object.DestroyImmediate(root);
                }
            }
            foreach (string actor in new[] { "Minh_Farmer", "Scarecrow_VisualOnly" })
            {
                string path = "Assets/Art/Characters/PrototypeCast/Prefabs/" + actor + ".prefab";
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    if (root.GetComponent<SacredDrum.Gameplay.PrototypeDepthSort>() == null) root.AddComponent<SacredDrum.Gameplay.PrototypeDepthSort>();
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets();
        }
    }
}
