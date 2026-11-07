using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SacredDrum.EditorTools
{
    // Editor-only authoring utilities. No runtime gameplay or automatic scene mutation.
    public static class Level1AssetSetup
    {
        public const string Root = "Assets/Art/Environment/CanhDong";
        public static readonly int[] TownIds = { 0,1,2,3,4,5,12,13,14,15,16,17,24,25,26,28,29,36,37,38,43,44,45,46,47,56,58,59,68,69,70,71,72,73,80,81,82,84,85,86,87,92,107,130,131 };
        public static readonly int[] FarmIds = { 1,13,16,25,28,37,40,64,65,66,67,68,70,77,93,94,95,105,106,107,117,118,119 };
        public static readonly string[] Originals = { "Water", "BankWest", "BankEast", "Bridge", "Fence", "StoneFloor", "OilLamp", "Gate", "BronzeFragment", "Spawn", "EnemySpawn", "Boundary" };

        [MenuItem("SacredDrum/Level 1/1 - Import licensed tiles and palette")]
        public static void Import()
        {
            AssetDatabase.Refresh();
            Slice("Kenney_TinyTown", "Town");
            Slice("Kenney_TinyFarm", "Farm");
            foreach (string name in Originals) MakeOriginal(name);
            foreach (int id in TownIds) MakeTile("Town_" + id.ToString("D3"), SheetSprite("Kenney_TinyTown", "Town_" + id.ToString("D3")));
            foreach (int id in FarmIds) MakeTile("Farm_" + id.ToString("D3"), SheetSprite("Kenney_TinyFarm", "Farm_" + id.ToString("D3")));
            foreach (string name in Originals) MakeTile(name, AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Props/" + name + ".png"));
            MakePalette();
            AssetDatabase.SaveAssets();
        }

        static void Slice(string sheet, string prefix)
        {
            string path = Root + "/Tilesets/" + sheet + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            Configure(importer, SpriteImportMode.Multiple);
            importer.SaveAndReimport();
            var factories = new SpriteDataProviderFactories();
            factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            // Packed source is exactly 192x176, 12x11 tiles, no margin or spacing.
            var old = provider.GetSpriteRects().ToDictionary(r => r.name);
            var rects = new List<SpriteRect>();
            for (int i = 0; i < 132; i++)
            {
                string name = prefix + "_" + i.ToString("D3");
                rects.Add(new SpriteRect { name = name, rect = new Rect(i % 12 * 16, (10 - i / 12) * 16, 16, 16),
                    alignment = SpriteAlignment.Center, pivot = new Vector2(.5f,.5f),
                    spriteID = old.TryGetValue(name, out var existing) ? existing.spriteID : GUID.Generate() });
            }
            provider.SetSpriteRects(rects.ToArray());
            var ids = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            ids.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
            provider.Apply();
            importer.SaveAndReimport();
            if (AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Count() != 132)
                throw new InvalidOperationException("Unexpected slice count: " + sheet);
        }

        static void Configure(TextureImporter i, SpriteImportMode mode)
        {
            i.textureType = TextureImporterType.Sprite;
            i.spriteImportMode = mode;
            i.spritePixelsPerUnit = 16;
            i.filterMode = FilterMode.Point;
            i.textureCompression = TextureImporterCompression.Uncompressed;
            i.mipmapEnabled = false;
            i.alphaIsTransparency = true;
            i.wrapMode = TextureWrapMode.Clamp;
            var s = new TextureImporterSettings(); i.ReadTextureSettings(s);
            s.spriteMeshType = SpriteMeshType.FullRect; i.SetTextureSettings(s);
        }

        static Sprite SheetSprite(string sheet, string name) => AssetDatabase.LoadAllAssetsAtPath(Root + "/Tilesets/" + sheet + ".png").OfType<Sprite>().Single(s=>s.name==name);
        public static Tile Tile(string name) => AssetDatabase.LoadAssetAtPath<Tile>(Root + "/Tiles/" + name + ".asset");

        static void MakeTile(string name, Sprite sprite)
        {
            string path = Root + "/Tiles/" + name + ".asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null) { tile = ScriptableObject.CreateInstance<Tile>(); AssetDatabase.CreateAsset(tile, path); }
            tile.sprite = sprite;
            // Collider ownership is explicit: only obstacle/water maps get Collider2D.
            tile.colliderType = UnityEngine.Tilemaps.Tile.ColliderType.Grid;
            EditorUtility.SetDirty(tile);
        }

        static Color C(string hex) { ColorUtility.TryParseHtmlString("#"+hex, out var c); return c; }

        static void MakeOriginal(string name)
        {
            string path = Root + "/Props/" + name + ".png";
            if (File.Exists(path)) return; // Never overwrite art edited by an artist.
            var t = new Texture2D(16,16,TextureFormat.RGBA32,false);
            var pixels = Enumerable.Repeat(Color.clear,256).ToArray();
            Action<int,int,int,int,string> box = (x,y,w,h,c) => {
                for(int yy=y;yy<y+h;yy++) for(int xx=x;xx<x+w;xx++) if(xx>=0&&xx<16&&yy>=0&&yy<16) pixels[yy*16+xx]=C(c);
            };
            if(name=="Water") { box(0,0,16,16,"4c8897"); box(2,4,5,1,"70a9ad");box(10,12,5,1,"70a9ad");box(0,0,16,1,"498592"); }
            if(name=="BankWest" || name=="BankEast") {
                box(0,0,16,16,"87bd65"); int x=name=="BankWest"?12:0;
                box(x,0,4,16,"b9b077");box(x+(name=="BankWest"?2:0),0,2,16,"4c8897");
            }
            if(name=="Bridge") { box(0,0,16,16,"805b42"); for(int y=1;y<16;y+=4){box(0,y,16,3,"ba8e58");box(3,y,1,1,"d6af72");box(12,y,1,1,"d6af72");} }
            if(name=="Fence" || name=="Gate") { box(0,5,16,2,"9e754a");box(0,10,16,2,"ba8e58");box(1,2,3,12,"805b42");box(2,4,1,10,"d0a56a");box(12,2,3,12,"805b42");box(13,4,1,10,"d0a56a"); if(name=="Gate"){box(5,2,2,12,"9e754a");box(9,2,2,12,"9e754a");box(6,7,5,3,"60564e");box(8,8,1,1,"eac36e");} }
            if(name=="StoneFloor") {box(0,0,16,16,"777c72");box(1,1,14,6,"9c9d87");box(1,9,6,6,"9c9d87");box(9,9,6,6,"8c917e");box(2,6,11,1,"b0af93");}
            if(name=="OilLamp") {box(3,1,10,2,"5c5145");box(5,3,6,4,"b27c41");box(6,7,4,2,"e0b55d");box(7,9,2,5,"e9a745");box(6,10,4,3,"ffcd65");box(7,11,2,3,"fff0b0");}
            if(name=="BronzeFragment") {box(3,3,10,10,"68583f");box(4,4,8,8,"b28c4c");box(5,5,6,6,"d6b565");box(7,3,2,10,"7e744c");box(3,7,10,2,"7e744c");box(6,6,4,4,"d6b565");box(7,7,2,2,"f2d58b");box(11,10,3,4,"87bd65");}
            if(name=="Spawn") {box(3,3,10,10,"305f53");box(4,4,8,8,"80cdb1");box(7,6,2,5,"eef0b0");box(5,8,6,1,"eef0b0");}
            if(name=="EnemySpawn") {box(4,4,8,8,"82684c");box(6,4,4,10,"bd9b65");box(2,8,12,2,"bd9b65");box(6,11,4,4,"d6b273");box(7,12,1,1,"51473d");box(9,12,1,1,"51473d");}
            if(name=="Boundary") {box(0,0,16,16,"527a48");box(1,2,5,8,"628a4d");box(8,6,7,7,"648e50");}
            t.SetPixels(pixels);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);
            AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);Configure(importer,SpriteImportMode.Single);importer.SaveAndReimport();
        }

        static void MakePalette()
        {
            string path=Root+"/Palettes/Level1_CanhDong_Palette.prefab";
            if(File.Exists(path)) return;
            GridPaletteUtility.CreateNewPalette(Root+"/Palettes","Level1_CanhDong_Palette",GridLayout.CellLayout.Rectangle,GridPalette.CellSizing.Manual,Vector3.one,GridLayout.CellSwizzle.XYZ);
            var go=PrefabUtility.LoadPrefabContents(path);
            var originalMap=go.GetComponentInChildren<Tilemap>();
            if(originalMap!=null) UnityEngine.Object.DestroyImmediate(originalMap.gameObject);
            string[][] groups={
                new[]{"Town_000","Town_001","Town_024","Farm_001","StoneFloor"},
                new[]{"Town_012","Town_013","Town_014","Town_024","Town_025","Town_026","Town_036","Town_037","Town_038","Town_043"},
                new[]{"Water","BankWest","BankEast","Bridge"},
                new[]{"Town_003","Town_004","Town_005","Town_015","Town_016","Town_028","Farm_066","Farm_067","Farm_070"},
                new[]{"Fence","Gate","Town_084","Town_085","Town_086","Town_087","Farm_093","Farm_094","Farm_095","Boundary"},
                new[]{"OilLamp","BronzeFragment","Spawn","EnemySpawn","Farm_077","Town_002","Town_017","Town_029"}
            };
            string[] names={"Ground","Paths","Water","Vegetation","Structures","Decorations"};
            for(int g=0;g<groups.Length;g++) {
                var child=new GameObject(names[g],typeof(Tilemap),typeof(TilemapRenderer));child.transform.SetParent(go.transform,false);
                var map=child.GetComponent<Tilemap>();
                for(int i=0;i<groups[g].Length;i++) map.SetTile(new Vector3Int(i,-g*3,0),Tile(groups[g][i]));
                map.CompressBounds();
            }
            PrefabUtility.SaveAsPrefabAsset(go,path);PrefabUtility.UnloadPrefabContents(go);
        }
    }
}
