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
    // An authored, repeatable layout. No runtime generation, new puzzle rules or new artwork.
    public static class Level1MazeAuthoring
    {
        const string Art = "Assets/Art/Environment/CanhDong/AssetPass";
        const string Proof = "Docs/Verification/Level1Maze";
        static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
        static readonly bool[,] Walk = new bool[88, 56];
        static readonly List<Vector2[]> Routes = new List<Vector2[]>();
        static Transform root, vegetation, landmarks;
        static void PrepareQuietGround()
        {
            string path=Art+"/Sources/NinjaAdventure_Floor.png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            var factory=new SpriteDataProviderFactories(); factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var rects=provider.GetSpriteRects().ToList();
            foreach(var item in new[]{new {name="MazeGrass",x=176,y=192},new{name="MazeGrassDetail",x=240,y=192},new{name="MazeSoil",x=192,y=128},new{name="MazeYardNW",x=176,y=112},new{name="MazeYardN",x=192,y=112},new{name="MazeYardNE",x=208,y=112},new{name="MazeYardW",x=176,y=128},new{name="MazeYardE",x=208,y=128},new{name="MazeYardSW",x=176,y=144},new{name="MazeYardS",x=192,y=144},new{name="MazeYardSE",x=208,y=144}})
                {
                    var rect=rects.FirstOrDefault(r=>r.name==item.name);
                    if(rect==null){rect=new SpriteRect{name=item.name,alignment=SpriteAlignment.Center,pivot=new Vector2(.5f,.5f),spriteID=GUID.Generate()};rects.Add(rect);}
                    rect.rect=new Rect(item.x,417-item.y-16,16,16);
                }
            provider.SetSpriteRects(rects.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
            foreach(var name in new[]{"MazeGrass","MazeGrassDetail","MazeSoil","MazeYardNW","MazeYardN","MazeYardNE","MazeYardW","MazeYardE","MazeYardSW","MazeYardS","MazeYardSE"})
            {
                string tilePath=Art+"/Tiles/"+name+".asset";
                var tile=AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
                if(tile==null){tile=ScriptableObject.CreateInstance<Tile>();AssetDatabase.CreateAsset(tile,tilePath);}
                tile.sprite=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Single(r=>r.name==name);
                tile.colliderType=UnityEngine.Tilemaps.Tile.ColliderType.Grid;EditorUtility.SetDirty(tile);
            }
            AssetDatabase.SaveAssets();
        }
        public static readonly Vector2[] CriticalPath = Points(14.5f,41, 24,41, 24,34, 14,34, 14,25, 26,25, 26,16, 36,16, 36,27, 49,27, 49,38, 60,38, 60,29, 70,29, 70,19, 80,19, 80,34, 75,34);
        static Vector2[] Points(params float[] xy)
        { var a = new Vector2[xy.Length / 2]; for (int i=0;i<a.Length;i++) a[i]=new Vector2(xy[2*i],xy[2*i+1]); return a; }
        static float SegmentDistance(Vector2 p, Vector2 a, Vector2 b)
        { var v=b-a; return Vector2.Distance(p,a+v*Mathf.Clamp01(Vector2.Dot(p-a,v)/v.sqrMagnitude)); }
        static void Route(params float[] xy) => Routes.Add(Points(xy));
        static GameObject Group(string name, Transform parent)
        { var go=new GameObject(name); go.transform.SetParent(parent,false); return go; }
        static SpriteRenderer Prop(string key, Vector2 p, float scale, Transform parent, int order=40)
        {
            var go=Group(key,parent); go.transform.position=p; go.transform.localScale=Vector3.one*scale;
            var sr=go.AddComponent<SpriteRenderer>(); sr.sprite=Sprites[key]; sr.sortingOrder=order; return sr;
        }
        static Tilemap Layer(string name, int order, bool collision=false)
        {
            var go=Group(name,root); var map=go.AddComponent<Tilemap>(); go.AddComponent<TilemapRenderer>().sortingOrder=order;
            if(collision)
            {
                go.AddComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Static;
                var tileCollider=go.AddComponent<TilemapCollider2D>();
                go.AddComponent<CompositeCollider2D>(); tileCollider.compositeOperation=Collider2D.CompositeOperation.Merge;
            }
            return map;
        }
        static TileBase Tile(string key) => AssetDatabase.LoadAssetAtPath<TileBase>(Art+"/Tiles/"+key+".asset");
        static void Paint(Tilemap map, int x, int y, string tile, Color? tint=null)
        {
            var c=new Vector3Int(x,y,0); map.SetTile(c,Tile(tile));
            if(tint.HasValue){map.SetTileFlags(c,TileFlags.None); map.SetColor(c,tint.Value);}
        }
        static bool Open(int x,int y) => x>=0 && x<88 && y>=0 && y<56 && Walk[x,y];
        static bool InWater(int x) => x>=43 && x<=46;
        static bool OnBridge(int x,int y) => x>=41 && x<=48 && y>=25 && y<=28;

        [MenuItem("SacredDrum/Level 1/Maze/Build authored maze")]
        public static void Build()
        {
            var scene=SceneManager.GetActiveScene();
            if(EditorApplication.isPlaying || scene.path!=Level1MapBuilder.ScenePath) throw new InvalidOperationException("Open Level 1 in Edit Mode.");
            if(!File.Exists("Assets/Scenes/Archive/Level1_CanhDong_PreMaze.unity")) throw new InvalidOperationException("Save a copy of the current scene before rebuilding.");
            PrepareQuietGround();
            foreach(var guid in AssetDatabase.FindAssets("t:Texture2D",new[]{Art+"/Sources"}))
                foreach(var sprite in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(guid)).OfType<Sprite>()) Sprites[sprite.name]=sprite;
            var previous=GameObject.Find("Level1_Maze"); if(previous!=null) UnityEngine.Object.DestroyImmediate(previous);
            GameObject.Find("Grid")?.SetActive(false); GameObject.Find("Environment")?.SetActive(false);
            root=Group("Level1_Maze",null).transform; root.gameObject.AddComponent<Grid>();
            vegetation=Group("ForestAndBambooWalls",root).transform; landmarks=Group("RuralLandmarks",root).transform;
            Routes.Clear(); Routes.Add(CriticalPath);
            Route(24,41,24,49,35,49,35,42); // false approach towards the far bank
            Route(24,34,34,34,34,41,29,41); // short, local return loop
            Route(14,25,6,25,6,16,15,16,15,7); // long dogleg ending in the harvest yard
            Route(26,16,26,7,37,7); // well dead end
            Route(26,16,26,13,29.5f,13.5f); // second lamp pocket
            Route(36,27,39,27,39,38,40,38); // overlook: apparent eastward route is blocked
            Route(49,27,49,17,54,17,54,8,66,8,66,11); // river landing / false southern approach
            Route(60,38,60,48,68,48,68,44); // northern approach cannot reach shrine
            Route(60,29,60,19,70,19); // eastern return loop, third lamp
            Route(70,29,70,35,67,35); // misleading turn near shrine fence
            Route(75,34,75,44,75.5f,45.5f); // existing locked gate blocks this corridor
            var rooms=new[]{new Rect(8,39,11,11),new Rect(20,31,8,7),new Rect(10,22,8,7),new Rect(23,12,9,7),new Rect(32,13,8,7),new Rect(54,34,10,8),new Rect(58,16,8,7),new Rect(76,15,8,8),new Rect(71,41,9,9)};
            for(int y=0;y<56;y++) for(int x=0;x<88;x++)
            {
                var p=new Vector2(x+.5f,y+.5f);
                Walk[x,y]=x>2&&x<85&&y>2&&y<53 && (rooms.Any(r=>r.Contains(p)) || Routes.Any(r=>Enumerable.Range(1,r.Length-1).Any(i=>SegmentDistance(p,r[i-1],r[i])<1.8f)));
                if(InWater(x) && !OnBridge(x,y)) Walk[x,y]=false;
            }
            var ground=Layer("Ground",0); var floor=Layer("CourtyardSoil",2); var edge=Layer("EarthBanks",4);
            var solid=Layer("ForestCollision",5,true); var water=Layer("River",7,true); var bridge=Layer("WoodenBridge",9);
            var fences=Layer("ShrineFence",15,true); var rice=Layer("RicePlots",8);
            for(int y=0;y<56;y++) for(int x=0;x<88;x++)
            {
                Paint(ground,x,y,(x*17+y*31)%41==0?"MazeGrassDetail":"MazeGrass");
                if(InWater(x))
                {
                    if(OnBridge(x,y)) Paint(bridge,x,y,"LicensedBridge"); else Paint(water,x,y,"Water");
                    continue;
                }
                if(!Walk[x,y])
                {
                    Paint(solid,x,y,"MazeGrass",new Color(.52f,.66f,.61f));
                    bool boundary=Open(x-1,y)||Open(x+1,y)||Open(x,y-1)||Open(x,y+1);
                    if(boundary) Paint(edge,x,y,"MazeSoil",new Color(.66f,.72f,.64f));
                }
                else if(rooms.Where((r,i)=>i==0||i==8).Any(r=>r.Contains(new Vector2(x+.5f,y+.5f))))
                    {
                    var yard=rooms[0].Contains(new Vector2(x+.5f,y+.5f))?rooms[0]:rooms[8];
                    string vertical=y==yard.yMin?"S":y==yard.yMax-1?"N":"";
                    string horizontal=x==yard.xMin?"W":x==yard.xMax-1?"E":"";
                    string tile=vertical+horizontal==""?"MazeSoil":"MazeYard"+vertical+horizontal;
                    Paint(floor,x,y,tile);
                }
            }
            // Fields are intentional islands with continuous collision underneath, never scattered obstacles in a path.
            foreach(var plot in new[]{new RectInt(19,20,4,3),new RectInt(29,20,4,3),new RectInt(63,24,4,3)})
                foreach(var c in plot.allPositionsWithin) if(!Open(c.x,c.y))
                { Paint(solid,c.x,c.y,"Soil"); Paint(rice,c.x,c.y,"Rice",new Color(.85f,.95f,.65f)); }
            // One coherent canopy mass. Small palette changes distinguish districts, not every tree.
            for(float y=1;y<54;y+=2.15f) for(float x=1+((int)(y/2.15f)%2)*.65f;x<87;x+=2.3f)
            {
                int ix=(int)x,iy=(int)y;
                if(InWater(ix)||Open(ix,iy)||Open(ix,iy-1)) continue;
                if(ix>=19&&ix<=22&&iy>=20&&iy<=22 || ix>=29&&ix<=32&&iy>=20&&iy<=22 || ix>=63&&ix<=66&&iy>=24&&iy<=26) continue;
                bool bamboo=(ix<11&&iy<20)||(ix>51&&ix<57)||(ix>79&&iy>37);
                var sr=Prop(bamboo?"Bamboo_0":"Tree_"+(ix<43?((ix/9+iy/8)%2):2),new Vector2(x+.15f*(iy%3-1),y-.2f-(Open(ix,iy+1)||Open(ix,iy+2)?1.25f:0)),bamboo?.72f:1.45f,vegetation);
                sr.color=ix<43?new Color(.74f,.86f,.70f):new Color(.68f,.83f,.73f);
            }
            // Compact landmark clusters: the remainder of each clearing stays empty.
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Art+"/Prefabs/NhaTranh_ThreeBay.prefab");
            var home=(GameObject)PrefabUtility.InstantiatePrefab(prefab); home.transform.SetParent(landmarks); home.transform.position=new Vector3(12.5f,44,0); home.transform.localScale=Vector3.one*.8f;
            Prop("WaterJar",new Vector2(17,45),.8f,landmarks); Prop("WovenBasket",new Vector2(17,46),.75f,landmarks);
            Prop("Chicken",new Vector2(10,41),.8f,landmarks); Prop("Chicken",new Vector2(11,40.5f),.8f,landmarks);
            Prop("Haystack",new Vector2(14.5f,7),1,landmarks); Prop("HarvestSheaf",new Vector2(16,7),.7f,landmarks);
            var well=Prop("Well",new Vector2(37.5f,7),1,landmarks).gameObject; var wc=well.AddComponent<BoxCollider2D>(); wc.size=new Vector2(.8f,.6f); wc.offset=new Vector2(0,.3f);
            Prop("Boat",new Vector2(49,16),1,landmarks); Prop("WoodenBucket",new Vector2(50.5f,17),.65f,landmarks);
            var rock=Prop("Rock_1",new Vector2(40,38),1,landmarks).gameObject; var rc=rock.AddComponent<BoxCollider2D>(); rc.size=new Vector2(1.5f,.7f); rc.offset=new Vector2(0,.35f);
            var pink=Prop("PinkTree_0",new Vector2(61.5f,40),1.2f,landmarks).gameObject; var pc=pink.AddComponent<CircleCollider2D>();pc.radius=.45f;pc.offset=new Vector2(0,.6f); Prop("Bench",new Vector2(57,39.5f),1,landmarks);
            Prop("Flag",new Vector2(72,48),1,landmarks); Prop("Flag",new Vector2(79,48),1,landmarks);
            // Fence at the shrine is a landmark; the original gate and relic stay in use.
            for(int x=70;x<=81;x++) { Paint(fences,x,40,"Fence"); Paint(fences,x,50,"Fence"); }
            for(int x=73;x<=76;x++) fences.SetTile(new Vector3Int(x,40,0),null);
            for(int y=40;y<=50;y++){Paint(fences,70,y,"Fence");Paint(fences,81,y,"Fence");}
            GameObject.Find("Gameplay/Lamp_01").transform.position=new Vector3(24,34,0);
            GameObject.Find("Gameplay/Lamp_02").transform.position=new Vector3(29.5f,13.5f,0);
            GameObject.Find("Gameplay/Lamp_03").transform.position=new Vector3(62.5f,19,0);
            foreach(var c in root.GetComponentsInChildren<TilemapCollider2D>()) c.ProcessTilemapChanges();
            foreach(var c in root.GetComponentsInChildren<CompositeCollider2D>()) c.GenerateGeometry();
            Physics2D.SyncTransforms();
            Camera.main.orthographicSize=6;
            Camera.main.GetComponent<SacredDrum.Gameplay.PlayerFollowCamera>().SnapToTarget();
            Directory.CreateDirectory(Proof);
            File.WriteAllText(Proof+"/AuthoredLayout.json",JsonConvert.SerializeObject(new{width=88,height=56,corridorWidth=3.6f,criticalPath=CriticalPath.Select(p=>new[]{p.x,p.y}),routes=Routes.Select(r=>r.Select(p=>new[]{p.x,p.y})),rooms=rooms.Select(r=>new[]{r.x,r.y,r.width,r.height}),vegetationCount=vegetation.childCount,landmarkCount=landmarks.childCount,reference="https://www.youtube.com/watch?v=F9zuB_IY0DQ",referenceInterval="00:00-10:00",art="Existing licensed source sprites only; no image generation"},Formatting.Indented));
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        }
    }
}
