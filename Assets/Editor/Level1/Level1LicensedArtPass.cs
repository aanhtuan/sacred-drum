using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace SacredDrum.EditorTools
{
    // Authoring only: source PNGs are downloaded artwork, never generated or repainted.
    public static class Level1LicensedArtPass
    {
        const string Root = "Assets/Art/Environment/CanhDong/AssetPass";
        const string Ninja = Root + "/Sources/NinjaAdventure_Tileset.png";
        const string Farm = Root + "/Sources/LPC_Farm.png";
        const string Bamboo = Root + "/Sources/LPC_Bamboo.png";
        const string Nature = Root + "/Sources/LPC_Nature.png";
        const string Floor = Root + "/Sources/NinjaAdventure_Floor.png";
        const string Traveller = Root + "/Sources/CC0_Traveller.png";
        const string Relic = Root + "/Sources/CC0_RelicToken.png";
        const string Animals = Root + "/Sources/Kenney_TinyFarm.png";
        static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
        static readonly Dictionary<string, Tile> Tiles = new Dictionary<string, Tile>();
        static readonly Dictionary<string, List<SpriteRect>> Rects = new Dictionary<string, List<SpriteRect>>();

        static void Slice(string name, string file, int x, int y, int width, int height, bool bottom = true)
        {
            if (!Rects.TryGetValue(file, out var list)) Rects[file] = list = new List<SpriteRect>();
            int textureHeight = file == Ninja ? 640 : file == Farm ? 1536 : file == Nature ? 160 : file == Floor ? 417 : file == Traveller ? 112 : file == Relic ? 11 : file == Animals ? 176 : 64;
            list.Add(new SpriteRect { name = name, rect = new Rect(x, textureHeight - y - height, width, height),
                alignment = bottom ? SpriteAlignment.BottomCenter : SpriteAlignment.Center,
                pivot = bottom ? new Vector2(.5f, 0) : new Vector2(.5f, .5f), spriteID = GUID.Generate() });
        }
        static void Cell(string name, int x, int y) => Slice(name, Ninja, x * 16, y * 16, 16, 16, false);
        static Sprite S(string name) => Sprites[name];
        static Tilemap Map(string name) => GameObject.Find("Grid/" + name).GetComponent<Tilemap>();

        [MenuItem("SacredDrum/Level 1/Licensed art/1 - Import selected source art")]
        public static void Import()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before importing art.");
            Rects.Clear(); Sprites.Clear(); Tiles.Clear();
            Slice("Grass", Floor, 48, 192, 16, 16, false);
            Slice("GrassDetail", Floor, 16, 192, 16, 16, false);
            Slice("Soil", Floor, 0, 176, 16, 16, false);
            Slice("Path", Floor, 16, 128, 16, 16, false);
            Cell("Water", 20, 8); Cell("Stone", 15, 32);
            Cell("Fence", 22, 1); Cell("Tuft", 0, 8); Cell("Flowers", 1, 8);
            Cell("Seedlings", 0, 8); Cell("Boundary", 4, 9);
            Slice("Traveller_Placeholder", Traveller, 0, 0, 16, 16, false);
            Slice("RelicToken_Placeholder", Relic, 0, 0, 9, 11, false);
            Slice("Chicken", Animals, 32, 160, 16, 16);
            Slice("Cow", Animals, 16, 160, 16, 16);
            for (int i = 0; i < 6; i++) Slice("Tree_" + i, Ninja, i * 32, 160, 32, 32);
            Slice("PinkTree_0", Ninja, 64, 256, 32, 32);
            Slice("PinkTree_1", Ninja, 96, 256, 32, 32);
            Slice("PinkTree_2", Ninja, 128, 256, 32, 32);
            Slice("Bush_0", Ninja, 192, 128, 16, 16);
            Slice("Bush_1", Ninja, 208, 128, 16, 16);
            Slice("Bush_2", Ninja, 192, 144, 16, 16);
            Slice("Bush_3", Ninja, 208, 144, 16, 16);
            Slice("Rock_0", Nature, 192, 64, 64, 64);
            Slice("Rock_1", Nature, 256, 64, 64, 64);
            Slice("Rock_2", Nature, 200, 8, 24, 24);
            Slice("Rock_3", Nature, 200, 40, 24, 24);
            Slice("RockCluster", Nature, 256, 64, 64, 64);
            Slice("Well", Ninja, 48, 64, 16, 32);
            Slice("WaterJar", Ninja, 48, 96, 16, 16);
            Slice("TerracottaPot", Ninja, 16, 96, 16, 16);
            Slice("LargePot", Ninja, 144, 128, 16, 16);
            Slice("EmptyBasket", Ninja, 48, 112, 16, 16);
            Slice("WoodCrate", Ninja, 32, 96, 16, 16);
            Slice("CrateOfFood", Ninja, 128, 96, 16, 16);
            Slice("Lantern", Ninja, 64, 96, 16, 16);
            Slice("MarketBasket", Ninja, 144, 96, 16, 16);
            Slice("LogPile", Ninja, 96, 128, 32, 16);
            Slice("CutLog", Ninja, 96, 144, 16, 16);
            Slice("Stump", Ninja, 160, 96, 16, 16);
            Slice("HandCart", Ninja, 96, 144, 32, 16);
            Slice("Bench", Ninja, 224, 96, 32, 16);
            Slice("Boat", Ninja, 416, 160, 32, 16);
            Slice("BridgeDeck", Ninja, 384, 128, 16, 16, false);
            Slice("FencePost", Ninja, 320, 16, 16, 16);
            Slice("Flag", Ninja, 48, 48, 16, 32);
            Slice("StoneBase", Ninja, 432, 112, 16, 16);
            Slice("WallLeft", Ninja, 0, 32, 16, 16);
            Slice("WallPanel", Ninja, 32, 32, 16, 16);
            Slice("WallRight", Ninja, 48, 32, 16, 16);
            Slice("Door", Ninja, 144, 48, 16, 16);
            Slice("Bamboo_0", Bamboo, 0, 0, 32, 64);
            Slice("Bamboo_1", Bamboo, 32, 0, 32, 64);
            Slice("ThatchRoof", Farm, 192, 512, 192, 160);
            Slice("Haystack", Farm, 232, 1168, 48, 80);
            Slice("HarvestSheaf", Farm, 192, 1168, 32, 80);
            Slice("WovenBasket", Farm, 224, 1008, 32, 48);
            Slice("GrainBasket", Farm, 256, 1008, 32, 48);
            Slice("WoodenBucket", Farm, 96, 1264, 32, 48);
            Slice("Trough", Farm, 128, 928, 64, 64);
            Slice("WattlePanel", Farm, 512, 1152, 64, 64);
            Slice("StrawMat", Farm, 512, 1216, 64, 64);

            foreach (var pair in Rects)
            {
                var imp = (TextureImporter)AssetImporter.GetAtPath(pair.Key);
                imp.textureType = TextureImporterType.Sprite; imp.spriteImportMode = SpriteImportMode.Multiple;
                imp.spritePixelsPerUnit = pair.Key == Farm || pair.Key == Nature ? 32 : 16;
                imp.filterMode = FilterMode.Point; imp.mipmapEnabled = false;
                imp.textureCompression = TextureImporterCompression.Uncompressed; imp.alphaIsTransparency = true;
                imp.maxTextureSize = 2048;
                var settings = new TextureImporterSettings(); imp.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect; imp.SetTextureSettings(settings); imp.SaveAndReimport();
                var factory = new SpriteDataProviderFactories(); factory.Init();
                var provider = factory.GetSpriteEditorDataProviderFromObject(imp); provider.InitSpriteEditorDataProvider();
                var existing = provider.GetSpriteRects().ToDictionary(r => r.name);
                foreach (var rect in pair.Value) if (existing.TryGetValue(rect.name, out var old)) rect.spriteID = old.spriteID;
                // Retain slices owned by later art passes (for example the maze ground tiles).
                var allRects = pair.Value.Concat(provider.GetSpriteRects().Where(r => !pair.Value.Any(n => n.name == r.name))).ToArray();
                provider.SetSpriteRects(allRects);
                provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(allRects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
                provider.Apply(); imp.SaveAndReimport();
                foreach (var sprite in AssetDatabase.LoadAllAssetsAtPath(pair.Key).OfType<Sprite>()) Sprites[sprite.name] = sprite;
            }
            foreach (string oldName in new[] { "Grass", "GrassDetail", "Soil", "Rice", "Water", "BankWest", "BankEast", "Boundary", "Fence", "Terrace", "Flowers", "Tuft", "Stone" })
            {
                string sprite = oldName == "Rice" ? "Seedlings" : oldName.StartsWith("Bank") || oldName == "Terrace" ? "Soil" : oldName;
                MakeTile(oldName, sprite);
            }
            for (int i = 0; i < 16; i++) MakeTile("Path" + i, "Path");
            var entries = JsonConvert.DeserializeObject<RefinementMask[]>(File.ReadAllText("Assets/Art/Environment/CanhDong/RefinementMeshes/RefinedTiles.sdmesh"));
            foreach (var e in entries) e.source = Root + "/Tiles/" + Path.GetFileName(e.source);
            File.WriteAllText(Root + "/Meshes/LicensedTiles.sdmesh", JsonConvert.SerializeObject(entries));
            AssetDatabase.ImportAsset(Root + "/Meshes/LicensedTiles.sdmesh", ImportAssetOptions.ForceSynchronousImport);
            SaveHousePrefab(); AssetDatabase.SaveAssets();
        }

        static void MakeTile(string name, string sprite)
        {
            string path = Root + "/Tiles/" + name + ".asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null) { tile = ScriptableObject.CreateInstance<Tile>(); AssetDatabase.CreateAsset(tile, path); }
            tile.name = name; tile.sprite = S(sprite); tile.colliderType = Tile.ColliderType.Grid;
            if (name.StartsWith("Path")) tile.color = new Color(1f, 1f, .93f);
            if (name == "Soil") tile.color = new Color(.68f, .7f, .58f);
            if (name == "Water") tile.color = new Color(.65f, .86f, .87f);
            if (name == "Fence") tile.color = new Color(.74f, .72f, .5f);
            if (name == "Grass" || name == "GrassDetail") tile.color = new Color(.82f, .96f, .85f);
            EditorUtility.SetDirty(tile); Tiles[name] = tile;
        }
        static GameObject Group(string name, Transform parent)
        { var go = new GameObject(name); go.transform.SetParent(parent, false); return go; }
        static GameObject Prop(Transform parent, string name, string sprite, Vector2 world, float scale = 1f, int order = 50)
        {
            var go = Group(name, parent); go.transform.position = world; go.transform.localScale = Vector3.one * scale;
            var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = S(sprite); renderer.sortingOrder = order;
            return go;
        }
        static void SaveHousePrefab()
        {
            var house = new GameObject("NhaTranh_ThreeBay");
            try
            {
                for (int i = 0; i < 6; i++)
                    Prop(house.transform, "TimberBay_" + i, i == 0 ? "WallLeft" : i == 5 ? "WallRight" : "WallPanel", new Vector2(i - 2.5f, 0));
                for (int row = 1; row <= 3; row++)
                    for (int i = 0; i < 6 - (row - 1) * 2; i++)
                        Prop(house.transform, "TimberGable_" + row + "_" + i, "WallPanel", new Vector2(i - 2.5f + row - 1, row));
                Prop(house.transform, "Door", "Door", new Vector2(0, 0), 1, 51);
                Prop(house.transform, "ThatchedRoof", "ThatchRoof", new Vector2(0, .75f), 1, 52);
                var col = house.AddComponent<BoxCollider2D>(); col.size = new Vector2(6, 1.4f); col.offset = new Vector2(0, .7f);
                PrefabUtility.SaveAsPrefabAsset(house, Root + "/Prefabs/NhaTranh_ThreeBay.prefab");
            }
            finally { UnityEngine.Object.DestroyImmediate(house); }
        }
        static void LoadArt()
        {
            Sprites.Clear(); Tiles.Clear();
            foreach (string path in new[] { Ninja, Farm, Bamboo, Nature, Floor, Traveller, Relic, Animals })
                foreach (var sprite in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()) Sprites[sprite.name] = sprite;
            foreach (string guid in AssetDatabase.FindAssets("t:Tile", new[] { Root + "/Tiles" }))
            { var tile = AssetDatabase.LoadAssetAtPath<Tile>(AssetDatabase.GUIDToAssetPath(guid)); Tiles[tile.name] = tile; }
        }

        [MenuItem("SacredDrum/Level 1/Licensed art/2 - Apply to current Level 1")]
        public static void Apply()
        {
            var scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != Level1MapBuilder.ScenePath) throw new InvalidOperationException("Open Level 1 in Edit Mode.");
            if (GameObject.Find("Environment/LicensedVillageDressing") != null) throw new InvalidOperationException("Already applied; edit the scene rather than overwrite it.");
            LoadArt();
            var replacement = new Dictionary<string, Tile>(Tiles);
            foreach (var tile in AssetDatabase.LoadAllAssetsAtPath(Root + "/Meshes/LicensedTiles.sdmesh").OfType<Tile>()) replacement[tile.name] = tile;
            foreach (var map in UnityEngine.Object.FindObjectsByType<Tilemap>())
            {
                // Preserve all occupied cells and collision geometry; swap only the art references.
                foreach (var cell in map.cellBounds.allPositionsWithin)
                {
                    var old = map.GetTile(cell);
                    if (old != null && replacement.TryGetValue(old.name, out var next)) map.SetTile(cell, next);
                }
                map.RefreshAllTiles();
            }
            int trees = 0, rocks = 0, bamboos = 0;
            foreach (var renderer in UnityEngine.Object.FindObjectsByType<SpriteRenderer>())
            {
                if (!renderer.enabled || renderer.sprite == null || !AssetDatabase.GetAssetPath(renderer.sprite).Contains("/ReferenceArt/")) continue;
                string old = renderer.sprite.name;
                renderer.enabled = false;
                Vector2 p = renderer.transform.position;
                if (old == "ThatchedHut")
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/NhaTranh_ThreeBay.prefab");
                    var house = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    house.transform.SetParent(renderer.transform, false); house.transform.position = p;
                    house.transform.localScale = Vector3.one / renderer.transform.lossyScale.x;
                    // The existing house collider remains authoritative.
                    UnityEngine.Object.DestroyImmediate(house.GetComponent<Collider2D>());
                    continue;
                }
                string sprite = old == "Tree" ? "Tree_" + trees++ % 6 : old == "Bamboo" ? "Bamboo_" + bamboos++ % 2
                    : old == "Haystack" ? "Haystack" : old == "Rocks" ? "RockCluster" : old == "OilLamp" ? "Lantern"
                    : old == "FestivalBanner" ? "Flag" : "StoneBase";
                var visual = Prop(renderer.transform, "LicensedVisual_" + sprite, sprite, p, 1, renderer.sortingOrder);
                visual.transform.localScale = Vector3.one / renderer.transform.lossyScale.x;
                if (old == "Rocks") rocks++;
                if (old == "BronzeAltar") visual.transform.localScale *= 2.5f;
            }
            // Replace the bridge/gate procedural placeholders with pixels from the licensed sheet.
            var bridge = GameObject.Find("Environment/Bridges/WoodenBridge");
            foreach (var map in bridge.GetComponentsInChildren<Tilemap>())
                foreach (var cell in map.cellBounds.allPositionsWithin)
                    if (map.HasTile(cell))
                    {
                        string key = map.name.Contains("Rail") ? "Fence" : "LicensedBridge";
                        if (!Tiles.ContainsKey(key)) MakeTile(key, "BridgeDeck");
                        map.SetTile(cell, Tiles[key]);
                    }
            foreach (var sr in GameObject.Find("Gameplay/LockedGate").GetComponentsInChildren<SpriteRenderer>())
            { sr.sprite = S("Fence"); sr.color = new Color(.74f, .72f, .5f); }

            var dressing = Group("LicensedVillageDressing", GameObject.Find("Environment").transform);
            DressVillage(dressing.transform);
            foreach (var collider in UnityEngine.Object.FindObjectsByType<TilemapCollider2D>())
                if (collider.enabled) collider.ProcessTilemapChanges();
            foreach (var collider in UnityEngine.Object.FindObjectsByType<CompositeCollider2D>())
                if (collider.enabled) collider.GenerateGeometry();
            Physics2D.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            WriteReport();
        }

        static bool ClearForDecoration(Vector2 point)
        {
            if (point.x < 3 || point.x > 85 || point.y < 3 || point.y > 52) return false;
            if (Physics2D.OverlapCircleAll(point, .65f).Any(c => !c.isTrigger && c.enabled)) return false;
            foreach (var lamp in new[] { new Vector2(23.5f, 37.5f), new Vector2(29.5f, 13.5f), new Vector2(62.5f, 18.5f), new Vector2(34.5f, 30.5f), new Vector2(58.5f, 29.5f), new Vector2(14.5f, 41) })
                if (Vector2.Distance(point, lamp) < 3.5f) return false;
            var road = Map("Paths"); var fields = Map("Farmland"); var water = Map("Water");
            var cell = road.WorldToCell(point);
            for (int y = -1; y <= 1; y++) for (int x = -1; x <= 1; x++)
                if (road.HasTile(cell + new Vector3Int(x, y, 0)) || water.HasTile(cell + new Vector3Int(x, y, 0))) return false;
            return !fields.HasTile(cell);
        }
        static void DressVillage(Transform parent)
        {
            var groups = new[] { "HomeYards", "BambooGroves", "Orchards", "FieldEdges", "RiverLanding", "ShrineGarden" }
                .ToDictionary(n => n, n => Group(n, parent).transform);
            var props = new[] { "WaterJar", "TerracottaPot", "WoodCrate", "WovenBasket", "GrainBasket", "HarvestSheaf", "Haystack", "LogPile", "CutLog", "WoodenBucket", "Bench", "LargePot", "CrateOfFood", "HandCart", "Stump", "Trough" };
            var anchors = new[] { new Vector2(18, 47), new Vector2(25, 47), new Vector2(7, 27), new Vector2(22, 8), new Vector2(61, 7), new Vector2(78, 27), new Vector2(66, 49) };
            int placed = 0;
            foreach (var a in anchors)
                for (int i = 0; i < 8; i++)
                {
                    var p = a + new Vector2(i % 4 * 1.6f - 2.4f, i / 4 * 1.8f);
                    if (!ClearForDecoration(p)) continue;
                    string sprite = props[placed++ % props.Length];
                    Prop(groups["HomeYards"], sprite + "_" + placed, sprite, p);
                }
            for (int i = 0; i < 36; i++)
            {
                var p = new Vector2(i < 18 ? 4 + i % 6 * 1.25f : 77 + i % 6 * 1.2f, i < 18 ? 42 + i / 6 * 3 : 35 + (i - 18) / 6 * 5);
                if (ClearForDecoration(p))
                {
                    var bamboo = Prop(groups["BambooGroves"], "BambooClump_" + i, "Bamboo_" + i % 2, p);
                    var collider = bamboo.AddComponent<CircleCollider2D>(); collider.radius = .3f; collider.offset = new Vector2(0, .3f);
                }
            }
            var orchard = new[] { new Vector2(20, 49), new Vector2(24, 50), new Vector2(28, 49), new Vector2(58, 37), new Vector2(62, 38), new Vector2(66, 37), new Vector2(5, 15), new Vector2(9, 16), new Vector2(13, 15) };
            for (int i = 0; i < orchard.Length; i++) if (ClearForDecoration(orchard[i]))
            {
                var tree = Prop(groups["Orchards"], "FloweringTree_" + i, "PinkTree_" + i % 3, orchard[i]);
                var collider = tree.AddComponent<CircleCollider2D>(); collider.radius = .4f; collider.offset = new Vector2(0, .35f);
            }
            for (int y = 4; y < 52; y += 3) for (int x = 4; x < 85; x += 4)
            {
                var p = new Vector2(x + (y % 2) * .5f, y);
                if (!ClearForDecoration(p)) continue;
                string sprite = (x + y) % 4 == 0 ? "Rock_" + x % 4 : "Bush_" + y % 4;
                Prop(groups["FieldEdges"], sprite + "_" + x + "_" + y, sprite, p);
            }
            // A boat and landing props are visual only, kept below the main bridge approach.
            Prop(groups["RiverLanding"], "FishingBoat", "Boat", new Vector2(43, 24));
            Prop(groups["RiverLanding"], "RiverWaterJar", "WaterJar", new Vector2(40, 24));
            Prop(groups["RiverLanding"], "FishBasket", "MarketBasket", new Vector2(39, 24));
            Prop(groups["ShrineGarden"], "OfferingBasket", "WovenBasket", new Vector2(71, 43));
            Prop(groups["ShrineGarden"], "OfferingJar", "LargePot", new Vector2(80, 43));
        }

        [MenuItem("SacredDrum/Level 1/Licensed art/3 - Refresh imported art in dressed scene")]
        public static void RefreshCurrentArt()
        {
            if (EditorApplication.isPlaying || SceneManager.GetActiveScene().path != Level1MapBuilder.ScenePath)
                throw new InvalidOperationException("Open Level 1 in Edit Mode.");
            Import(); LoadArt();
            var group = GameObject.Find("Environment/LicensedVillageDressing");
            if (group == null) throw new InvalidOperationException("Apply the licensed art pass first.");
            foreach (var sr in UnityEngine.Object.FindObjectsByType<SpriteRenderer>())
                foreach (var key in new[] { "RockCluster", "Rock_0", "Rock_1", "Rock_2", "Rock_3" })
                    if (sr.name == "LicensedVisual_" + key || key != "RockCluster" && sr.name.StartsWith(key + "_")) sr.sprite = S(key);
            foreach (var t in group.GetComponentsInChildren<Transform>())
            {
                if (t.GetComponent<Collider2D>() != null) continue;
                if (!t.name.StartsWith("BambooClump_") && !t.name.StartsWith("FloweringTree_") && !t.name.StartsWith("Rock_")) continue;
                var collider = t.gameObject.AddComponent<CircleCollider2D>();
                collider.radius = t.name.StartsWith("Rock_") ? .45f : .3f; collider.offset = new Vector2(0, .3f);
            }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/NhaTranh_ThreeBay.prefab");
            foreach (var p in new[] { new Vector2(24, 7), new Vector2(64, 47) })
            {
                string name = "FarmOutbuilding_" + p.x;
                if (group.transform.Find(name) != null) continue;
                var house = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                house.name = name; house.transform.SetParent(group.transform, false); house.transform.position = p;
                house.transform.localScale = Vector3.one * .6f;
            }
            if (group.transform.Find("YardWell") == null)
            {
                var well = Prop(group.transform, "YardWell", "Well", new Vector2(19, 44));
                well.AddComponent<BoxCollider2D>().size = new Vector2(.8f, .6f);
                Prop(group.transform, "DryingMat", "StrawMat", new Vector2(20.5f, 44.5f));
            }
            var playerPrefab = PrefabUtility.LoadPrefabContents("Assets/Prefabs/Characters/Player.prefab");
            try
            {
                playerPrefab.GetComponent<SpriteRenderer>().sprite = S("Traveller_Placeholder");
                PrefabUtility.SaveAsPrefabAsset(playerPrefab, "Assets/Prefabs/Characters/Player.prefab");
            }
            finally { PrefabUtility.UnloadPrefabContents(playerPrefab); }
            var fragment = GameObject.Find("Gameplay/DrumFragment_01").transform;
            if (fragment.Find("LicensedRelicToken") == null)
                Prop(fragment, "LicensedRelicToken", "RelicToken_Placeholder", fragment.position, 1, 56);
            if (group.transform.Find("FarmyardAnimals") == null)
            {
                var animals = Group("FarmyardAnimals", group.transform).transform;
                var positions = new[] { new Vector2(18, 42.5f), new Vector2(19, 42), new Vector2(21, 43) };
                for (int i = 0; i < positions.Length; i++) Prop(animals, "Chicken_" + i, "Chicken", positions[i], .65f);
                Prop(animals, "Cow", "Cow", new Vector2(26, 46), 1.1f);
            }
            foreach (var map in UnityEngine.Object.FindObjectsByType<Tilemap>()) map.RefreshAllTiles();
            Physics2D.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets(); WriteReport();
        }

        [MenuItem("SacredDrum/Level 1/Licensed art/4 - Refine field-edge clusters")]
        public static void RefineFieldClusters()
        {
            if (EditorApplication.isPlaying || SceneManager.GetActiveScene().path != Level1MapBuilder.ScenePath)
                throw new InvalidOperationException("Open Level 1 in Edit Mode.");
            LoadArt();
            var parent = GameObject.Find("Environment/LicensedVillageDressing/FieldEdges").transform;
            Undo.RegisterFullObjectHierarchyUndo(parent.gameObject, "Refine licensed field-edge clusters");
            foreach (Transform child in parent.Cast<Transform>().ToArray()) Undo.DestroyObjectImmediate(child.gameObject);
            Physics2D.SyncTransforms();
            var anchors = new[] { new Vector2(6, 9), new Vector2(12, 28), new Vector2(30, 7), new Vector2(38, 10),
                new Vector2(4, 38), new Vector2(29, 46), new Vector2(38, 48), new Vector2(53, 6), new Vector2(65, 10),
                new Vector2(78, 5), new Vector2(80, 32), new Vector2(59, 50), new Vector2(85, 24), new Vector2(50, 35) };
            var offsets = new[] { new Vector2(-1.2f, .3f), new Vector2(.6f, -.5f), new Vector2(1.4f, .7f), new Vector2(-.3f, 1.5f) };
            var props = new[] { "Bush_0", "Bush_1", "Rock_0", "Bush_2", "Rock_1", "Bush_3", "Rock_2", "CutLog", "Rock_3", "Flowers" };
            int index = 0;
            foreach (var anchor in anchors) foreach (var offset in offsets)
            {
                var p = anchor + offset;
                if (!ClearForDecoration(p)) continue;
                string sprite = props[index % props.Length];
                var go = Prop(parent, sprite + "_Cluster_" + index++, sprite, p);
                if (sprite.StartsWith("Rock_"))
                {
                    var collider = go.AddComponent<CircleCollider2D>(); collider.radius = .4f; collider.offset = new Vector2(0, .3f);
                }
            }
            foreach (var marker in GameObject.Find("Gameplay/EnemySpawnPoints").GetComponentsInChildren<SpriteRenderer>()) marker.enabled = false;
            Physics2D.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            WriteReport();
        }

        public static string WriteReport()
        {
            var renderers = UnityEngine.Object.FindObjectsByType<SpriteRenderer>();
            var sources = renderers.Where(r => r.enabled && r.sprite != null).Select(r => AssetDatabase.GetAssetPath(r.sprite)).ToArray();
            var report = new { scene = SceneManager.GetActiveScene().path,
                activeAIAtlasRenderers = sources.Count(p => p.Contains("/ReferenceArt/")),
                licensedSpriteRenderers = sources.Count(p => p.StartsWith(Root)),
                uniqueLicensedSprites = renderers.Where(r => r.enabled && r.sprite != null && AssetDatabase.GetAssetPath(r.sprite).StartsWith(Root)).Select(r => r.sprite.name).Distinct().Count(),
                importedSprites = new[] { Ninja, Farm, Bamboo, Nature, Floor, Traveller, Relic, Animals }.Sum(p => AssetDatabase.LoadAllAssetsAtPath(p).OfType<Sprite>().Count()),
                nativeTiles = AssetDatabase.FindAssets("t:Tile", new[] { Root + "/Tiles" }).Length,
                spawn = GameObject.Find("Gameplay/PlayerSpawn").transform.position.ToString(),
                lamps = Enumerable.Range(1, 3).Select(i => GameObject.Find("Gameplay/Lamp_0" + i).transform.position.ToString()).ToArray(),
                gate = GameObject.Find("Gameplay/LockedGate").transform.position.ToString(),
                fragment = GameObject.Find("Gameplay/DrumFragment_01").transform.position.ToString(),
                noImageGeneration = true, originalArtPreserved = true };
            string json = JsonConvert.SerializeObject(report, Formatting.Indented);
            Directory.CreateDirectory("Docs/Verification/VietnamAssetPass"); File.WriteAllText("Docs/Verification/VietnamAssetPass/Integration.json", json); return json;
        }
    }
}
