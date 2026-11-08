using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace SacredDrum.EditorTools
{
    public static class PrototypeCastArt
    {
        const string Root = "Assets/Art/Characters/PrototypeCast/";

        [MenuItem("SacredDrum/Prototype Cast/Prepare Sprites and Prefabs")]
        public static void Prepare()
        {
            var minh = ImportSprite("Minh_Farmer", 2.2f);
            var scarecrow = ImportSprite("Scarecrow", 2.25f);
            var original = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Characters/Player.prefab");
            var player = (GameObject)PrefabUtility.InstantiatePrefab(original);
            player.name = "Minh_Farmer";
            var renderer = player.GetComponent<SpriteRenderer>();
            renderer.sprite = minh;
            renderer.sortingOrder = 30;
            renderer.color = Color.white;
            var characterMaterial = renderer.sharedMaterial;
            var feet = player.GetComponent<CircleCollider2D>();
            feet.offset = new Vector2(0, .22f);
            feet.radius = .3f;
            PrefabUtility.SaveAsPrefabAsset(player, Root + "Prefabs/Minh_Farmer.prefab");
            Object.DestroyImmediate(player);

            var enemy = new GameObject("Scarecrow_VisualOnly");
            var spriteRenderer = enemy.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = scarecrow;
            spriteRenderer.sortingOrder = 29;
            spriteRenderer.sharedMaterial = characterMaterial;
            PrefabUtility.SaveAsPrefabAsset(enemy, Root + "Prefabs/Scarecrow_VisualOnly.prefab");
            Object.DestroyImmediate(enemy);
            AssetDatabase.SaveAssets();
            Debug.Log("Prototype cast ready: Minh inherits the original Player prefab; scarecrow is visual only.");
        }

        static Sprite ImportSprite(string name, float worldHeight)
        {
            string path = Root + "Sources/" + name + ".png";
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.isReadable = true;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var pixels = texture.GetPixels32();
            int x0 = texture.width, y0 = texture.height, x1 = 0, y1 = 0;
            for (int y = 0; y < texture.height; y++) for (int x = 0; x < texture.width; x++)
                if (pixels[y * texture.width + x].a > 32)
                {
                    x0 = Mathf.Min(x0, x); y0 = Mathf.Min(y0, y);
                    x1 = Mathf.Max(x1, x + 1); y1 = Mathf.Max(y1, y + 1);
                }
            if (x1 <= x0 || y1 <= y0) throw new System.InvalidOperationException("Sprite has no visible pixels: " + name);
            importer.spritePixelsPerUnit = (y1 - y0) / worldHeight;
            var factories = new SpriteDataProviderFactories(); factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var old = provider.GetSpriteRects().FirstOrDefault(rect => rect.name == name);
            var id = old == null ? GUID.Generate() : old.spriteID;
            provider.SetSpriteRects(new[] { new SpriteRect { name = name, spriteID = id,
                rect = new Rect(x0, y0, x1 - x0, y1 - y0), alignment = SpriteAlignment.Custom, pivot = new Vector2(.5f, 0) } });
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(new[] { new SpriteNameFileIdPair(name, id) });
            provider.Apply(); importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().First();
        }
    }
}
