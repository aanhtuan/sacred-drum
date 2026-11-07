using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

namespace SacredDrum.EditorTools
{
    public static class Level1MapBuilder
    {
        public const string ScenePath = "Assets/Scenes/Level1_CanhDong.unity";
        public const int Width=88, Height=56;
        static readonly Vector2[] LampPositions={new Vector2(23.5f,39.5f),new Vector2(28.5f,10.5f),new Vector2(61.5f,15.5f)};
        static Tilemap Map(string name) => GameObject.Find("Grid/"+name).GetComponent<Tilemap>();
        static Tile T(string name) => Level1AssetSetup.Tile(name);
        static void Put(Tilemap map,int x,int y,string tile) => map.SetTile(new Vector3Int(x,y,0),T(tile));
        static GameObject Group(string name,Transform parent=null) {var g=new GameObject(name);g.transform.SetParent(parent,false);return g;}
        static void Rect(Tilemap map,int x0,int y0,int x1,int y1,string tile) {for(int y=y0;y<=y1;y++) for(int x=x0;x<=x1;x++)Put(map,x,y,tile);}
        static int RiverLeft(int y) => y<14?43:y<23?44:y<36?43:y<45?42:43;
        static bool InBridge(int x,int y)=>x>=40&&x<=50&&y>=26&&y<=29;
        static bool InCourtyard(int x,int y)=>x>=66&&x<=84&&y>=36&&y<=52;
        static bool OnPath(int x,int y)=>
            (x>=7&&x<=76&&y>=26&&y<=29) || (x>=22&&x<=25&&y>=29&&y<=40) ||
            (x>=27&&x<=30&&y>=9&&y<=26) || (x>=60&&x<=63&&y>=14&&y<=26) ||
            (x>=73&&x<=76&&y>=29&&y<=45);

        [MenuItem("SacredDrum/Level 1/2 - Create new map (never overwrite)")]
        public static void BuildLayout()
        {
            if(EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
            if(File.Exists(ScenePath)) throw new InvalidOperationException("Scene exists; refusing to overwrite. Edit it normally.");
            if(SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save current work first.");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camGo=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener),typeof(UniversalAdditionalCameraData));
            camGo.tag="MainCamera";camGo.transform.position=new Vector3(44,28,-10);
            var camera=camGo.GetComponent<Camera>();camera.orthographic=true;camera.orthographicSize=30;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.12f,.2f,.17f);camera.nearClipPlane=.1f;camera.farClipPlane=100;
            var global=new GameObject("Global Light 2D",typeof(Light2D)).GetComponent<Light2D>();global.lightType=Light2D.LightType.Global;global.intensity=1;
            var grid=Group("Grid");grid.AddComponent<Grid>();
            string[] layers={"Ground","Paths","Water","GroundDecoration","Obstacles","Foreground"};
            int[] orders={0,10,20,30,40,60};
            for(int i=0;i<layers.Length;i++) {var child=Group(layers[i],grid.transform);child.AddComponent<Tilemap>();child.AddComponent<TilemapRenderer>().sortingOrder=orders[i];}
            var environment=Group("Environment");
            foreach(string name in new[]{"Trees","Rocks","Fences","Bridges","Buildings"})Group(name,environment.transform);
            var ground=Map("Ground");var paths=Map("Paths");var water=Map("Water");var decoration=Map("GroundDecoration");var obstacles=Map("Obstacles");var foreground=Map("Foreground");
            for(int y=0;y<Height;y++)for(int x=0;x<Width;x++) {
                Put(ground,x,y,(x*37+y*19)%53==0?"Town_001":"Town_000");
                if(OnPath(x,y)) Put(paths,x,y,"Town_025");
                if(x<2||x>=Width-2||y<2||y>=Height-2)Put(obstacles,x,y,"Boundary");
            }
            // Authored path rectangles use matching 3x3 edge tiles, no randomized terrain seams.
            EdgePath(paths,7,26,39,29);EdgePath(paths,51,26,76,29);
            EdgePath(paths,22,29,25,40);EdgePath(paths,27,9,30,25);
            EdgePath(paths,60,14,63,25);EdgePath(paths,73,30,76,45);
            // Open plazas for encounters; crops do not occupy approach corridors.
            EdgePath(paths,31,30,39,36);EdgePath(paths,53,30,64,36);
            for(int y=2;y<Height-2;y++) {
                int left=RiverLeft(y);
                for(int x=left;x<=left+3;x++)Put(water,x,y,"Water");
                Put(water,left-1,y,"BankWest");Put(water,left+4,y,"BankEast");
            }
            // Bridge overlays water; bank/water collision is removed only within this crossing.
            GameObject.Find("Environment/Bridges").AddComponent<Grid>();
            var bridge=Group("WoodenBridge",GameObject.Find("Environment/Bridges").transform);
            var bridgeMap=bridge.AddComponent<Tilemap>();bridge.AddComponent<TilemapRenderer>().sortingOrder=25;
            Rect(bridgeMap,40,26,50,29,"Bridge");
            var rails=Group("BridgeRails",bridge.transform).AddComponent<Tilemap>();rails.gameObject.AddComponent<TilemapRenderer>().sortingOrder=45;
            for(int x=40;x<=50;x++){Put(rails,x,25,"Fence");Put(rails,x,30,"Fence");}
            // Golden crop rows stand for rice until a dedicated Vietnamese rice tileset is drawn.
            Field(ground,decoration,10,12,23,21);Field(ground,decoration,10,33,18,41);
            Field(ground,decoration,52,7,57,21);Field(ground,decoration,67,8,79,21);
            // Sealed altar courtyard, gate opening exactly four tiles across its south edge.
            Rect(paths,68,38,82,50,"StoneFloor");
            for(int x=67;x<=83;x++) {Put(obstacles,x,51,"Fence");if(x<73||x>76)Put(obstacles,x,37,"Fence");}
            for(int y=38;y<51;y++){Put(obstacles,67,y,"Fence");Put(obstacles,83,y,"Fence");}
            // Sparse tree clusters, deliberately absent from the main paths and fighting plazas.
            var random=new System.Random(7241);
            for(int i=0;i<150;i++) {
                int x=random.Next(4,84),y=random.Next(4,52);
                bool edge=x<7||x>80||y>48;
                bool grove=(x>=32&&x<=38&&y>=42&&y<=47)||(x>=52&&x<=60&&y>=42&&y<=49)||(x>=33&&x<=38&&y>=7&&y<=19);
                if(!(edge||grove)||OnPath(x,y)||InCourtyard(x,y)||Math.Abs(x-RiverLeft(y))<7)continue;
                AddProp("Tree_"+i,new Vector2(x+.5f,y+.5f),i%3==0?"Town_003":"Town_004","Trees",true,new Vector2(.55f,.55f),new Vector2(0,-.2f));
            }
            foreach(var p in new[]{new Vector2(19.5f,45.5f),new Vector2(32.5f,22.5f),new Vector2(56.5f,38.5f),new Vector2(79.5f,30.5f),new Vector2(65.5f,10.5f)})
                AddProp("FieldRock",p,"Farm_077","Rocks",true,new Vector2(.65f,.55f),Vector2.zero);
            // Small low huts: neutral placeholders, no imported European castle/church silhouettes.
            Hut(11,45);Hut(23,46);
            // Small fence strips frame fields, with both ends open for navigation.
            for(int x=10;x<23;x++)Put(obstacles,x,11,"Fence");
            for(int x=68;x<80;x++)Put(obstacles,x,22,"Fence");
            for(int y=4;y<52;y++)for(int x=4;x<84;x++) {
                if(OnPath(x,y)||paths.HasTile(new Vector3Int(x,y,0))||InCourtyard(x,y)||Math.Abs(x-RiverLeft(y))<7||obstacles.HasTile(new Vector3Int(x,y,0)))continue;
                if((x*31+y*47)%151==0)Put(decoration,x,y,"Town_017");
                if((x*17+y*23)%367==0)Put(decoration,x,y,"Town_002");
            }
            foreach(var m in UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))m.CompressBounds();
            // Empty foreground is intentionally ready for future canopies; not a duplicate map.
            PolishLayout();
            EditorSceneManager.SaveScene(scene,ScenePath);
            FrameSceneView();
        }

        static void EdgePath(Tilemap map,int x0,int y0,int x1,int y1)
        {
            for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++) {
                int col=x==x0?0:x==x1?2:1;
                int row=y==y1?0:y==y0?2:1;
                Put(map,x,y,"Town_"+(12+row*12+col).ToString("D3"));
            }
        }
        static void Field(Tilemap ground,Tilemap decoration,int x0,int y0,int x1,int y1)
        {
            Rect(ground,x0,y0,x1,y1,"Town_025");
            for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++) {
                var p=new Vector3Int(x,y,0);ground.SetTileFlags(p,TileFlags.None);ground.SetColor(p,new Color(.68f,.72f,.65f));
            }
            for(int y=y0+1;y<=y1-1;y+=2)for(int x=x0+1;x<=x1-1;x++)Put(decoration,x,y,(x+y)%4==0?"Farm_067":"Farm_066");
        }
        static void Hut(int x,int y)
        {
            var parent=Group("RuralHut_Placeholder",GameObject.Find("Environment/Buildings").transform);
            for(int row=0;row<3;row++)for(int col=0;col<3;col++) {
                string tile=row==2?"Farm_"+(93+col).ToString("D3"):row==1?"Farm_"+(105+col).ToString("D3"):"Town_"+(col==1?85:84).ToString("D3");
                var piece=AddProp("HutTile",new Vector2(x+col+.5f,y+row+.5f),tile,"Buildings",false,Vector2.zero,Vector2.zero);piece.transform.SetParent(parent.transform,true);
            }
            var collider=parent.AddComponent<BoxCollider2D>();collider.offset=new Vector2(x+1.5f,y+1.1f);collider.size=new Vector2(3,2.2f);
        }
        static GameObject AddProp(string name,Vector2 pos,string tile,string group,bool solid,Vector2 size,Vector2 offset)
        {
            var go=Group(name,GameObject.Find("Environment/"+group).transform);go.transform.position=pos;
            var sr=go.AddComponent<SpriteRenderer>();sr.sprite=T(tile).sprite;sr.sortingOrder=50;
            if(solid){var collider=go.AddComponent<BoxCollider2D>();collider.size=size;collider.offset=offset;}
            return go;
        }

        [MenuItem("SacredDrum/Level 1/3 - Add collisions and gameplay markers")]
        public static void AddCollisionsAndMarkers()
        {
            if(SceneManager.GetActiveScene().path!=ScenePath)throw new InvalidOperationException("Open Level1_CanhDong first.");
            if(GameObject.Find("Gameplay")!=null)throw new InvalidOperationException("Gameplay markers already exist; refusing duplicates.");
            Collider(Map("Obstacles"));
            // Keep visible water intact; hidden child only carries blocking cells outside bridge.
            var blocked=Group("WaterCollision",GameObject.Find("Grid/Water").transform);var map=blocked.AddComponent<Tilemap>();
            foreach(var p in Map("Water").cellBounds.allPositionsWithin)
                if(Map("Water").HasTile(p)&&!InBridge(p.x,p.y))map.SetTile(p,T("Water"));
            map.CompressBounds();Collider(map);
            var railMap=GameObject.Find("Environment/Bridges/WoodenBridge/BridgeRails").GetComponent<Tilemap>();Collider(railMap);
            var gameplay=Group("Gameplay");
            Marker(gameplay.transform,"PlayerSpawn",new Vector2(8.5f,27.5f),"Spawn",false);
            for(int i=0;i<3;i++) {
                Vector2 p=LampPositions[i];
                Rect(Map("Paths"),(int)p.x-1,(int)p.y-1,(int)p.x+1,(int)p.y+1,"StoneFloor");
                Marker(gameplay.transform,"Lamp_0"+(i+1),p,"OilLamp",true);
            }
            var gate=Group("LockedGate",gameplay.transform);gate.transform.position=new Vector3(75,37.5f,0);
            var gateCollider=gate.AddComponent<BoxCollider2D>();gateCollider.size=new Vector2(4,1);
            for(int i=0;i<4;i++) {
                var panel=Group("GatePanel_"+(i+1),gate.transform);panel.transform.localPosition=new Vector3(i-1.5f,0,0);
                var sr=panel.AddComponent<SpriteRenderer>();sr.sprite=T("Gate").sprite;sr.sortingOrder=55;
            }
            Rect(Map("Paths"),73,43,77,47,"StoneFloor");
            Marker(gameplay.transform,"DrumFragment_01",new Vector2(75.5f,45.5f),"BronzeFragment",true);
            var enemies=Group("EnemySpawnPoints",gameplay.transform);
            Marker(enemies.transform,"EnemySpawn_01",new Vector2(35.5f,33.5f),"EnemySpawn",false);
            Marker(enemies.transform,"EnemySpawn_02",new Vector2(58.5f,33.5f),"EnemySpawn",false);
            var bounds=Group("LevelBounds");
            // Thin physical walls back the visible two-tile border; no ground-wide collider.
            Wall(bounds.transform,"West",new Vector2(0,28),new Vector2(1,58));
            Wall(bounds.transform,"East",new Vector2(88,28),new Vector2(1,58));
            Wall(bounds.transform,"South",new Vector2(44,0),new Vector2(90,1));
            Wall(bounds.transform,"North",new Vector2(44,56),new Vector2(90,1));
            foreach(var m in UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))m.CompressBounds();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Physics2D.SyncTransforms();
        }
        static void Collider(Tilemap map)
        {
            var rb=map.gameObject.AddComponent<Rigidbody2D>();rb.bodyType=RigidbodyType2D.Static;
            var tileCollider=map.gameObject.AddComponent<TilemapCollider2D>();
            var composite=map.gameObject.AddComponent<CompositeCollider2D>();composite.geometryType=CompositeCollider2D.GeometryType.Polygons;
            tileCollider.compositeOperation=Collider2D.CompositeOperation.Merge;
            tileCollider.ProcessTilemapChanges();composite.GenerateGeometry();
        }
        static void Marker(Transform parent,string name,Vector2 position,string tile,bool trigger)
        {
            var g=Group(name,parent);g.transform.position=position;
            var sr=g.AddComponent<SpriteRenderer>();sr.sprite=T(tile).sprite;sr.sortingOrder=55;
            if(trigger){var c=g.AddComponent<CircleCollider2D>();c.radius=.85f;c.isTrigger=true;}
        }
        static void Wall(Transform parent,string name,Vector2 p,Vector2 size)
        {var wall=Group(name,parent);wall.transform.position=p;wall.AddComponent<BoxCollider2D>().size=size;}
        public static void FrameSceneView()
        {
            var view=SceneView.lastActiveSceneView;
            if(view!=null){view.in2DMode=true;view.LookAtDirect(new Vector3(44,28,0),Quaternion.identity,32);view.Repaint();}
        }

        // Repair only this authored map; no reference scenes or imported source art are touched.
        public static void PolishLayout()
        {
            if(SceneManager.GetActiveScene().path!=ScenePath && SceneManager.GetActiveScene().path!="")throw new InvalidOperationException("Wrong scene");
            var bridges=GameObject.Find("Environment/Bridges");if(bridges.GetComponent<Grid>()==null)bridges.AddComponent<Grid>();
            var ground=Map("Ground");var decor=Map("GroundDecoration");var paths=Map("Paths");
            foreach(var p in ground.cellBounds.allPositionsWithin)if(ground.GetTile(p)==T("Farm_001")) {
                ground.SetTile(p,T("Town_025"));ground.SetTileFlags(p,TileFlags.None);ground.SetColor(p,new Color(.68f,.72f,.65f));
            }
            foreach(var p in decor.cellBounds.allPositionsWithin)if(decor.GetTile(p)==T("Farm_070"))decor.SetTile(p,T("Farm_067"));
            // Choose terrain edges from the union of corridors, so junctions don't contain grass seams.
            var cells=new HashSet<Vector3Int>(paths.cellBounds.allPositionsWithin.Where(p=>paths.HasTile(p)&&paths.GetTile(p)!=T("StoneFloor")));
            foreach(var p in cells) {
                int col=!cells.Contains(p+Vector3Int.left)?0:!cells.Contains(p+Vector3Int.right)?2:1;
                int row=!cells.Contains(p+Vector3Int.up)?0:!cells.Contains(p+Vector3Int.down)?2:1;
                paths.SetTile(p,T("Town_"+(12+row*12+col).ToString("D3")));
            }
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}
