using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

namespace SacredDrum.EditorTools
{
    /// <summary>One-time, local edits to the saved maze. Never recreates its root or existing maps.</summary>
    public static class Level1SelectiveRefinement
    {
        public const string Proof = "Docs/Verification/SelectiveRefinement";
        const string Art = "Assets/Art/Environment/CanhDong/AssetPass";
        static readonly RectInt[] Arenas = {
            new RectInt(23,11,16,10), new RectInt(57,7,16,13), new RectInt(65,28,17,9)
        };
        public static readonly Vector2[] Seals = {new Vector2(31.5f,16.5f),new Vector2(65,13.5f),new Vector2(74,33.5f)};
        static readonly Dictionary<string,Sprite> Sprites = new Dictionary<string,Sprite>();
        static Tilemap solid, banks, river, rice, ground, paths, cliffs, tops, bunds, shore;
        static Transform additions;
        static int clearedCells, replacedTrees;
        static Tilemap Map(string name) => GameObject.Find("Level1_Maze/"+name).GetComponent<Tilemap>();
        static TileBase Tile(string name) => AssetDatabase.LoadAssetAtPath<TileBase>(Art+"/Tiles/"+name+".asset");
        static bool Has(Tilemap map,int x,int y) => map.HasTile(new Vector3Int(x,y,0));
        static void Paint(Tilemap map,int x,int y,string tile,Color? color=null)
        {
            var source=Tile(tile);
            var cell=new Vector3Int(x,y,0); map.SetTile(cell,source);
            var native=source as UnityEngine.Tilemaps.Tile;
            map.SetTileFlags(cell,TileFlags.None);map.SetColor(cell,color??(native!=null?native.color:Color.white));
        }
        static GameObject Group(string name,Transform parent)
        {var go=new GameObject(name);go.transform.SetParent(parent,false);return go;}
        static Tilemap Layer(string name,int order)
        {
            var go=Group(name,additions);var map=go.AddComponent<Tilemap>();
            go.AddComponent<TilemapRenderer>().sortingOrder=order;return map;
        }
        static SpriteRenderer Prop(string name,string sprite,Vector2 point,Transform parent,int order=40,float scale=1)
        {
            var go=Group(name,parent);go.transform.position=point;go.transform.localScale=Vector3.one*scale;
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=Sprites[sprite];sr.sortingOrder=order;return sr;
        }
        static void ClearCell(int x,int y)
        {
            var cell=new Vector3Int(x,y,0);
            if(solid.HasTile(cell))clearedCells++;
            solid.SetTile(cell,null);banks.SetTile(cell,null);rice.SetTile(cell,null);
        }
        static void OpenArena(RectInt area)
        {
            // Chamfer the two lower corners, retaining natural shoulders around the clearing.
            foreach(var p in area.allPositionsWithin)
                if(!(p.y==area.yMin && (p.x==area.xMin || p.x==area.xMax-1))) ClearCell(p.x,p.y);
        }
        static bool NearArena(Vector2 p,float margin)
        {
            return Arenas.Any(r=>p.x>r.xMin-margin && p.x<r.xMax+margin && p.y>r.yMin-margin && p.y<r.yMax+margin);
        }
        static void RefineRiver()
        {
            // Only two short reaches change, at most one cell per bank. Bridge rows stay untouched.
            for(int y=5;y<=48;y++)
            {
                int left=43,right=46;
                if(y<=10){left=42;right=45;}
                else if(y<=15){left=42;right=46;}
                else if(y>=39 && y<=43){left=44;right=47;}
                else if(y>=44){right=47;}
                else continue;
                for(int x=42;x<=47;x++)
                {
                    var cell=new Vector3Int(x,y,0);
                    if(x>=left && x<=right)
                    {
                        Paint(river,x,y,"Water"); solid.SetTile(cell,null);banks.SetTile(cell,null);rice.SetTile(cell,null);
                    }
                    else if(river.HasTile(cell))
                    {
                        river.SetTile(cell,null);
                        Paint(solid,x,y,"MazeGrass",new Color(.66f,.80f,.65f));
                    }
                }
                Paint(shore,left-1,y,"MazeYardE");Paint(shore,right+1,y,"MazeYardW");
            }
        }
        static void RiceField(RectInt area)
        {
            foreach(var p in area.allPositionsWithin)
            {
                // Preserve the footprint of walkable lanes instead of planting across them.
                if(!Has(solid,p.x,p.y))continue;
                Paint(bunds,p.x,p.y,"MazeSoil",new Color(.94f,.96f,.77f));
                bool rim=p.x==area.xMin||p.x==area.xMax-1||p.y==area.yMin||p.y==area.yMax-1;
                rice.SetTile(new Vector3Int(p.x,p.y,0),null);
                if(!rim && (p.y-area.yMin)%2==1) Paint(rice,p.x,p.y,"Rice",new Color(.82f,.94f,.70f));
            }
        }
        static void RaisedBank(RectInt area)
        {
            for(int x=area.xMin;x<area.xMax;x++)
            {
                if(!Has(solid,x,area.yMin)||!Has(solid,x,area.yMin+1))continue;
                string cap=x==area.xMin?"Left":x==area.xMax-1?"Right":"Middle";
                Paint(cliffs,x,area.yMin,"RefineCliff"+cap,new Color(.73f,.83f,.72f));
                Paint(tops,x,area.yMin+1,"RefineCliffTop",new Color(.73f,.86f,.72f));
            }
        }
        static void Trail(params Vector2[] points)
        {
            for(int i=1;i<points.Length;i++)
            {
                var from=Vector2Int.RoundToInt(points[i-1]);var to=Vector2Int.RoundToInt(points[i]);
                int steps=Mathf.Max(Mathf.Abs(to.x-from.x),Mathf.Abs(to.y-from.y));
                for(int j=0;j<=steps;j++)
                {
                    var p=Vector2Int.RoundToInt(Vector2.Lerp(from,to,steps==0?0:(float)j/steps));
                    if(Has(solid,p.x,p.y)||Has(river,p.x,p.y))continue;
                    Paint(paths,p.x,p.y,"MazeSoil",new Color(.82f,.89f,.75f));
                }
            }
        }
        static void RefineVegetation()
        {
            var original=GameObject.Find("Level1_Maze/ForestAndBambooWalls").transform;
            var mixed=Group("MixedVegetation",additions).transform;
            foreach(Transform tree in original)
            {
                var sr=tree.GetComponent<SpriteRenderer>();if(sr==null)continue;
                Vector2 p=tree.position;var cell=solid.WorldToCell(p);
                var visualBounds=sr.bounds;
                bool clear=Arenas.Any(r=>new Rect(r.xMin-1,r.yMin-1,r.width+2,r.height+2).Overlaps(new Rect(visualBounds.min.x,visualBounds.min.y,visualBounds.size.x,visualBounds.size.y)));
                clear |= !solid.HasTile(cell) || cliffs.HasTile(cell) || cliffs.HasTile(cell+Vector3Int.down) || tops.HasTile(cell) || bunds.HasTile(cell);
                if(clear){sr.enabled=false;continue;}
                // Keep the perimeter and major tree clumps; mix only selected interior boundary stretches.
                bool interior=p.x>4&&p.x<84&&p.y>4&&p.y<52;
                int hash=Mathf.RoundToInt(p.x*13+p.y*7);
                if(interior && hash%5<2)
                {
                    sr.enabled=false;replacedTrees++;
                    bool rock=hash%4==0;
                    Prop("Boundary_"+(rock?"Rock":"Shrubs"),rock?"Rock_2":"RefineShrub",p,mixed,40,rock?2:1);
                    if(!rock)Prop("ShrubCompanion","RefineGrassClump",p+new Vector2(.75f,.25f),mixed);
                }
                else
                {
                    tree.localScale=Vector3.one;
                    // An offset of whole source pixels keeps silhouettes irregular without fractional scaling.
                    tree.position+=new Vector3((hash%3-1)*.25f,0,0);
                }
            }
            // Props stay out of combat floors; use quiet, clustered grass accents only on their shoulders.
            foreach(var area in Arenas)
                for(int x=area.xMin;x<area.xMax;x+=3)
                    if(Has(solid,x,area.yMin-1)) Prop("EdgeGrass","RefineGrassClump",new Vector2(x+.5f,area.yMin-1),mixed);
        }
        static void CreateSealAreas(Tilemap pads)
        {
            var group=Group("SealAreas",GameObject.Find("Gameplay").transform).transform;
            for(int i=0;i<Seals.Length;i++)
            {
                var area=Group("Area_"+(char)('A'+i),group).transform;
                Vector2 p=Seals[i];var seal=Group("Seal_0"+(i+1),area);seal.transform.position=p;
                seal.AddComponent<CircleCollider2D>().isTrigger=true;
                seal.GetComponent<CircleCollider2D>().radius=.8f;
                Prop("Pedestal","StoneBase",p+new Vector2(0,-.3f),seal.transform,50);
                Prop("SealLampLeft","Lantern",p+new Vector2(-1.25f,.25f),seal.transform,50);
                Prop("SealLampRight","Lantern",p+new Vector2(1.25f,.25f),seal.transform,50);
                int cx=Mathf.FloorToInt(p.x),cy=Mathf.FloorToInt(p.y);
                for(int y=cy-1;y<=cy+1;y++)for(int x=cx-1;x<=cx+1;x++)
                    Paint(pads,x,y,"Stone",new Color(.85f,.91f,.83f));
                var space=Group("CombatSpace_0"+(i+1),area);
                space.transform.position=new Vector2(Arenas[i].center.x,Arenas[i].center.y);
            }
            CreateSpawn(group,"ScarecrowSpawn_01",new Vector2(35.5f,17.5f));
            CreateSpawn(group,"RatSpawn_01",new Vector2(26.5f,14.5f));
            CreateSpawn(group,"ScarecrowSpawn_02",new Vector2(68.5f,15.5f));
            CreateSpawn(group,"ScarecrowSpawn_03",new Vector2(68.5f,33));
        }
        static void CreateSpawn(Transform group,string name,Vector2 p)
        {var marker=Group(name,group);marker.transform.position=p;}

        static void PrepareCliffTiles()
        {
            string path=Art+"/Sources/NinjaAdventure_Tileset.png";
            var imp=(TextureImporter)AssetImporter.GetAtPath(path);
            var factory=new SpriteDataProviderFactories();factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(imp);provider.InitSpriteEditorDataProvider();
            var rects=provider.GetSpriteRects().ToList();
            var definitions=new[]{new{name="RefineCliffLeft",x=176,y=272},new{name="RefineCliffMiddle",x=192,y=272},new{name="RefineCliffRight",x=208,y=272},new{name="RefineCliffTop",x=192,y=256},new{name="RefineShrub",x=64,y=144},new{name="RefineGrassClump",x=208,y=144}};
            foreach(var d in definitions)
            {
                var r=rects.FirstOrDefault(a=>a.name==d.name);
                if(r==null){r=new SpriteRect{name=d.name,alignment=SpriteAlignment.Center,pivot=new Vector2(.5f,.5f),spriteID=GUID.Generate()};rects.Add(r);}
                r.rect=new Rect(d.x,640-d.y-16,16,16);
            }
            provider.SetSpriteRects(rects.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
            provider.Apply();imp.SaveAndReimport();
            foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{Art+"/Sources"}))
                foreach(var sprite in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(guid)).OfType<Sprite>()) Sprites[sprite.name]=sprite;
            foreach(var d in definitions)
            {
                if(!d.name.StartsWith("RefineCliff"))continue;
                string tilePath=Art+"/Tiles/"+d.name+".asset";
                var tile=AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
                if(tile==null){tile=ScriptableObject.CreateInstance<Tile>();AssetDatabase.CreateAsset(tile,tilePath);}
                tile.sprite=Sprites[d.name];tile.colliderType=UnityEngine.Tilemaps.Tile.ColliderType.Grid;EditorUtility.SetDirty(tile);
            }
            // Use the existing native atlas. Keep hard pixel edges and preserve original PNG bytes.
            var atlas=AssetDatabase.LoadAssetAtPath<SpriteAtlas>(Art+"/MazeGround.spriteatlas");
            var source=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            // Retain the existing ground-only atlas; preserve the mesh importer source-texture contract.
            if(atlas.GetPackables().Contains(source))atlas.Remove(new UnityEngine.Object[]{source});
            EditorUtility.SetDirty(atlas);AssetDatabase.SaveAssets();
            SpriteAtlasUtility.PackAtlases(new[]{atlas},EditorUserBuildSettings.activeBuildTarget);
        }

        public static void RefreshNaturalProps()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Edit Mode required.");
            PrepareCliffTiles();
            solid=Map("ForestCollision");banks=Map("EarthBanks");rice=Map("RicePlots");river=Map("River");
            foreach(var p in new RectInt(54,16,6,3).allPositionsWithin)ClearCell(p.x,p.y);
            var waterColor=((UnityEngine.Tilemaps.Tile)Tile("Water")).color;
            foreach(var cell in river.cellBounds.allPositionsWithin)if(river.HasTile(cell)){river.SetTileFlags(cell,TileFlags.None);river.SetColor(cell,waterColor);}
            paths=GameObject.Find("Level1_Maze/SelectiveRefinement/DirtTrails").GetComponent<Tilemap>();
            paths.ClearAllTiles();
            Trail(new Vector2(26,25),new Vector2(26,17),Seals[0]);
            Trail(new Vector2(49,27),new Vector2(49,17),new Vector2(60,17),new Vector2(60,13),Seals[1]);
            Trail(new Vector2(60,38),new Vector2(60,29),new Vector2(70,29),new Vector2(70,33),Seals[2],new Vector2(75,35));
            solid.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();solid.GetComponent<CompositeCollider2D>().GenerateGeometry();Physics2D.SyncTransforms();
            foreach(var sr in GameObject.Find("Level1_Maze/SelectiveRefinement/MixedVegetation").GetComponentsInChildren<SpriteRenderer>())
            {
                if(sr.name=="Boundary_Shrubs")sr.sprite=Sprites["RefineShrub"];
                else if(sr.name=="ShrubCompanion"||sr.name=="EdgeGrass")sr.sprite=Sprites["RefineGrassClump"];
            }
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        public static void FinalizeTerrainReadability()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Edit Mode required.");
            const string key="RefineTerrainMass";string path=Art+"/Tiles/"+key+".asset";
            var mass=AssetDatabase.LoadAssetAtPath<Tile>(path);
            if(mass==null){mass=ScriptableObject.CreateInstance<Tile>();AssetDatabase.CreateAsset(mass,path);}
            mass.sprite=((UnityEngine.Tilemaps.Tile)Tile("MazeGrass")).sprite;
            mass.color=new Color(.68f,.80f,.68f);mass.colliderType=UnityEngine.Tilemaps.Tile.ColliderType.Grid;EditorUtility.SetDirty(mass);
            solid=Map("ForestCollision");
            foreach(var cell in solid.cellBounds.allPositionsWithin)if(solid.HasTile(cell))solid.SetTile(cell,mass);
            foreach(string name in new[]{"RefineCliffLeft","RefineCliffMiddle","RefineCliffRight","RefineCliffTop"})
            {
                var tile=AssetDatabase.LoadAssetAtPath<UnityEngine.Tilemaps.Tile>(Art+"/Tiles/"+name+".asset");
                tile.color=name.EndsWith("Top")?new Color(.73f,.86f,.72f):new Color(.73f,.83f,.72f);EditorUtility.SetDirty(tile);
            }
            cliffs=GameObject.Find("Level1_Maze/SelectiveRefinement/RaisedEarthFaces").GetComponent<Tilemap>();
            tops=GameObject.Find("Level1_Maze/SelectiveRefinement/RaisedEarthTops").GetComponent<Tilemap>();
            cliffs.ClearAllTiles();tops.ClearAllTiles();
            foreach(var area in new[]{new RectInt(18,27,9,2),new RectInt(28,9,10,2),new RectInt(72,21,4,2),new RectInt(13,36,7,2),new RectInt(28,43,5,2),new RectInt(65,37,8,2),new RectInt(77,37,5,2),new RectInt(48,40,6,2)})RaisedBank(area);
            var faceBounds=new List<Bounds>();
            foreach(var cell in cliffs.cellBounds.allPositionsWithin)if(cliffs.HasTile(cell))faceBounds.Add(new Bounds(new Vector3(cell.x+.5f,cell.y+.5f,0),new Vector3(.95f,.95f,1)));
            var vegetation=GameObject.Find("Level1_Maze/ForestAndBambooWalls").GetComponentsInChildren<SpriteRenderer>();
            var mixed=GameObject.Find("Level1_Maze/SelectiveRefinement/MixedVegetation").GetComponentsInChildren<SpriteRenderer>();
            foreach(var sr in vegetation.Concat(mixed))if(sr.enabled&&faceBounds.Any(b=>b.Intersects(sr.bounds)))sr.enabled=false;
            ground=Map("Ground");
            foreach(var anchor in new[]{new Vector2Int(24,12),new Vector2Int(36,19),new Vector2Int(58,9),new Vector2Int(70,18),new Vector2Int(67,29),new Vector2Int(80,34),new Vector2Int(19,40),new Vector2Int(50,22)})
                foreach(var offset in new[]{Vector2Int.zero,Vector2Int.right,Vector2Int.up,new Vector2Int(1,1),new Vector2Int(2,1),new Vector2Int(1,2)})
                    Paint(ground,anchor.x+offset.x,anchor.y+offset.y,"MazeGrassDetail");
            solid.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();solid.GetComponent<CompositeCollider2D>().GenerateGeometry();Physics2D.SyncTransforms();
            foreach(var map in new[]{solid,Map("River")})
            {
                map.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
                var composite=map.GetComponent<CompositeCollider2D>();
                composite.geometryType=CompositeCollider2D.GeometryType.Polygons;composite.GenerateGeometry();
            }
            Physics2D.SyncTransforms();
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        [MenuItem("SacredDrum/Level 1/Selective refinement/Apply local refinements")]
        public static void Apply()
        {
            var scene=SceneManager.GetActiveScene();
            if(EditorApplication.isPlaying||scene.path!=Level1MapBuilder.ScenePath)throw new InvalidOperationException("Open saved Level 1 in Edit Mode.");
            var maze=GameObject.Find("Level1_Maze");
            if(maze==null||GameObject.Find("Level1_Maze/SelectiveRefinement")!=null)throw new InvalidOperationException("Requires the existing maze, before this one-time refinement.");
            Undo.RegisterFullObjectHierarchyUndo(maze,"Level 1 selective refinement");
            Undo.RegisterFullObjectHierarchyUndo(GameObject.Find("Gameplay"),"Level 1 seal markers");
            PrepareCliffTiles();
            solid=Map("ForestCollision");banks=Map("EarthBanks");river=Map("River");rice=Map("RicePlots");ground=Map("Ground");
            additions=Group("SelectiveRefinement",maze.transform).transform;
            paths=Layer("DirtTrails",10);cliffs=Layer("RaisedEarthFaces",6);tops=Layer("RaisedEarthTops",6);
            bunds=Layer("FieldBunds",8);shore=Layer("RiverBankContours",6);var pads=Layer("SealPads",12);
            rice.GetComponent<TilemapRenderer>().sortingOrder=9;
            foreach(var area in Arenas)OpenArena(area);
            foreach(var p in new RectInt(54,16,6,3).allPositionsWithin)ClearCell(p.x,p.y);
            RefineRiver();
            RaisedBank(new RectInt(18,29,10,2));RaisedBank(new RectInt(29,9,10,2));
            RaisedBank(new RectInt(59,21,11,2));RaisedBank(new RectInt(66,39,14,2));
            RiceField(new RectInt(18,20,5,4));RiceField(new RectInt(29,21,5,3));RiceField(new RectInt(61,23,8,4));
            Trail(new Vector2(26,25),new Vector2(26,17),Seals[0]);
            Trail(new Vector2(49,27),new Vector2(49,17),new Vector2(60,17),new Vector2(60,13),Seals[1]);
            Trail(new Vector2(60,38),new Vector2(60,29),new Vector2(70,29),new Vector2(70,33),Seals[2],new Vector2(75,35));
            RefineVegetation();CreateSealAreas(pads);
            var legacy=GameObject.Find("Gameplay/EnemySpawnPoints");
            legacy.transform.Find("EnemySpawn_01").position=new Vector3(35.5f,17.5f,0);
            legacy.transform.Find("EnemySpawn_02").position=new Vector3(68.5f,15.5f,0);
            legacy.SetActive(false);
            foreach(var map in new[]{solid,river})map.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
            foreach(var map in new[]{solid,river})map.GetComponent<CompositeCollider2D>().GenerateGeometry();
            Physics2D.SyncTransforms();
            Directory.CreateDirectory(Proof);
            File.WriteAllText(Proof+"/RefinementLayout.json",JsonConvert.SerializeObject(new{size=new[]{88,56},baseCommit="a6b5ea7",mode="in-place refinement; existing roots/maps/objects retained",clearedSolidCells=clearedCells,replacedTreeVisuals=replacedTrees,arenas=Arenas.Select(r=>new[]{r.x,r.y,r.width,r.height}),seals=Seals.Select(p=>new[]{p.x,p.y}),bridgeUnchanged=true,gateAndRelicUnchanged=true,newExternalAssets=false,sourcePixelsModified=false},Formatting.Indented));
            FinalizeTerrainReadability();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }

        [DrawGizmo(GizmoType.Selected|GizmoType.NonSelected)]
        static void DrawDesignMarkers(Transform marker,GizmoType type)
        {
            bool seal=marker.name.StartsWith("Seal_"),scarecrow=marker.name.StartsWith("ScarecrowSpawn_"),rat=marker.name.StartsWith("RatSpawn_");
            if(!seal&&!scarecrow&&!rat)return;
            Gizmos.color=seal?new Color(.95f,.78f,.25f):scarecrow?new Color(.88f,.35f,.18f):new Color(.75f,.52f,.34f);
            Gizmos.DrawWireSphere(marker.position,seal?.8f:scarecrow?.5f:.35f);
            Handles.Label(marker.position+Vector3.up*.9f,marker.name);
        }
    }
}
