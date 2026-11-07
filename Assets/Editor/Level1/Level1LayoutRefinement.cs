using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace SacredDrum.EditorTools
{
    // An explicit authoring pass on the existing scene. Source textures/importers are immutable.
    public static class Level1LayoutRefinement
    {
        const string Root="Assets/Art/Environment/CanhDong";
        const string Output=Root+"/RefinementMeshes";
        static readonly Vector2[] Lamps={new Vector2(23.5f,37.5f),new Vector2(29.5f,13.5f),new Vector2(62.5f,18.5f)};
        static readonly Vector2[] Encounters={new Vector2(34.5f,30.5f),new Vector2(58.5f,29.5f)};
        static readonly Dictionary<string,Tile> Cache=new Dictionary<string,Tile>();
        static readonly Dictionary<string,RefinementMask> Definitions=new Dictionary<string,RefinementMask>();
        static readonly List<Stroke> Roads=new List<Stroke>();
        sealed class Stroke {public Vector2[] samples;public float radius;}
        static Tile Native(string name)=>AssetDatabase.LoadAssetAtPath<Tile>(Root+"/ReferenceTiles/"+name+".asset");
        static Tilemap Map(string name)=>GameObject.Find("Grid/"+name).GetComponent<Tilemap>();
        static float Circle(Vector2 p,Vector2 center,float radius)=>Vector2.Distance(p,center)-radius;
        public static float RiverCenter(float y)=>44.8f+2.3f*Mathf.Sin((y-28)/9f)+.65f*Mathf.Sin((y-28)/3.6f);
        static float HalfRiver(float y)=>1.65f+.12f*Mathf.Sin(y/5f);
        static float RiverDistance(Vector2 p)=>Mathf.Abs(p.x-RiverCenter(p.y))-HalfRiver(p.y);

        [MenuItem("SacredDrum/Level 1/6 - Refine current level layout")]
        public static void Apply()
        {
            var scene=SceneManager.GetActiveScene();
            if(scene.path!=Level1MapBuilder.ScenePath||EditorApplication.isPlaying||scene.isDirty)
                throw new InvalidOperationException("Open a saved Level1_CanhDong in Edit Mode first.");
            if(GameObject.Find("Grid/PathShoulders")!=null)throw new InvalidOperationException("Refinement already applied; edit the scene instead of overwriting it.");
            Roads.Clear();Cache.Clear();Definitions.Clear();
            AddRoad(1.3f,new Vector2(14.5f,41),new Vector2(16,37),new Vector2(20,33),new Vector2(27,32),Encounters[0],new Vector2(40.5f,27.5f));
            AddRoad(1.3f,new Vector2(50.5f,27.5f),new Vector2(54,27.3f),Encounters[1],new Vector2(65,30.5f),new Vector2(71,32),new Vector2(75,37.5f));
            AddRoad(.95f,new Vector2(20,33),new Vector2(22,35),Lamps[0]);
            AddRoad(.95f,new Vector2(27,32),new Vector2(29,26),new Vector2(26.5f,20.5f),Lamps[1]);
            AddRoad(.95f,new Vector2(64,30.5f),new Vector2(64,25),Lamps[2]);
            var ground=Map("Ground");ground.ClearAllTiles();
            for(int y=0;y<56;y++)for(int x=0;x<88;x++)ground.SetTile(new Vector3Int(x,y,0),Native((x*37+y*17)%19==0?"GrassDetail":"Grass"));
            var paths=Map("Paths");paths.ClearAllTiles();paths.color=Color.white;
            var decor=Map("GroundDecoration");decor.ClearAllTiles();
            var water=Map("Water");water.ClearAllTiles();
            var obstacles=Map("Obstacles");
            foreach(var p in obstacles.cellBounds.allPositionsWithin)if(obstacles.GetTile(p)==Native("Terrace"))obstacles.SetTile(p,null);
            // All edges are snapped to 1/16 world-unit pixels. No texture resampling or blur.
            Paint(NewMap("PathShoulders",9,new Color(.78f,.94f,.65f)),Native("Path15"),p=>RoadDistance(p)-.2f,new Rect(3,6,78,39));
            Paint(paths,Native("Path15"),RoadDistance,new Rect(3,6,78,39));
            var combat=NewMap("CombatClearings",7,new Color(.8f,.88f,.73f));
            var combatEdge=NewMap("CombatMargins",6,new Color(.73f,.91f,.66f));
            foreach(var c in Encounters){Paint(combatEdge,Native("Path15"),p=>Circle(p,c,3.9f),Around(c,4.3f));Paint(combat,Native("Path15"),p=>Circle(p,c,3.6f),Around(c,4));}
            // The stone shrine remains enclosed by the same fence and gate; only its paving changes.
            var pads=NewMap("ObjectivePads",11,Color.white);
            Paint(pads,Native("Stone"),p=>Circle(p,new Vector2(75.5f,45.5f),3.7f),new Rect(71,41,9,9));
            Paint(pads,Native("Stone"),p=>SegmentDistance(p,new Vector2(75,37.5f),new Vector2(75.5f,44))-1.35f,new Rect(72,36,7,10));
            foreach(var p in Lamps)Paint(pads,Native("Stone"),q=>Circle(q,p,1.7f),Around(p,2));
            var bank=NewMap("RiverBanks",18,new Color(.91f,.93f,.69f));
            var shore=NewMap("RiverShoulders",17,new Color(.89f,.99f,.81f));
            Paint(shore,Native("GrassDetail"),p=>RiverDistance(p)-.8f,new Rect(37,0,16,56));
            Paint(bank,Native("Soil"),p=>RiverDistance(p)-.3f,new Rect(37,0,16,56));
            Paint(water,Native("Water"),RiverDistance,new Rect(37,0,16,56));
            RebuildRiverCollision();
            // Six separately authored, rounded parcels. Banks remain walkable and connect to paths.
            var fields=new[]{
                SmoothPolygon(new[]{V(8,13),V(11,11.5f),V(17,12),V(21,15),V(21,20),V(18,23),V(12,23),V(8,20)}),
                SmoothPolygon(new[]{V(7.5f,34),V(10,32),V(13,33),V(15,36),V(13,39),V(8,38)}),
                SmoothPolygon(new[]{V(26,39.5f),V(29,38),V(34,39),V(36,42),V(33,44),V(28,44)}),
                SmoothPolygon(new[]{V(51,10),V(54,9),V(58,12),V(57,17),V(55,21),V(51,19),V(50,15)}),
                SmoothPolygon(new[]{V(68,13),V(73,12),V(78,14),V(81,18),V(80,23),V(76,25),V(70,24),V(67,20)}),
                SmoothPolygon(new[]{V(53,40),V(56,39.5f),V(61,42),V(62,46),V(58,49),V(53,47),V(51,44)})
            };
            var soil=NewMap("Farmland",4,Color.white);var fieldBanks=NewMap("FieldBanks",3,new Color(.9f,1,.72f));
            for(int i=0;i<fields.Length;i++) {
                var polygon=fields[i];var bounds=Bounds(polygon,1);
                Func<Vector2,float> shape=p=>FieldDistance(p,polygon);
                Paint(fieldBanks,Native("Soil"),p=>shape(p)-.45f,bounds);
                Paint(soil,Native("Soil"),shape,bounds);
                Paint(decor,Native("Rice"),p=>Mathf.Max(shape(p)+.6f,.08f-Mathf.Abs(Mathf.Repeat(p.y+p.x*.07f,2.6f)-1.3f)),bounds);
            }
            ReclusterProps();
            Move("Gameplay/PlayerSpawn",V(14.5f,41));
            for(int i=0;i<3;i++) {
                var lamp=GameObject.Find("Gameplay/Lamp_0"+(i+1));lamp.transform.position=Lamps[i];
                var visual=lamp.transform.Find("OilLampVisual");visual.localScale=Vector3.one*.48f;
                visual.GetComponent<SpriteRenderer>().sortingOrder=55;
            }
            for(int i=0;i<2;i++)Move("Gameplay/EnemySpawnPoints/EnemySpawn_0"+(i+1),Encounters[i]);
            DecorateClusters(decor,fields);
            BakeMeshLibrary();
            foreach(var map in UnityEngine.Object.FindObjectsByType<Tilemap>()) {
                map.CompressBounds();var tc=map.GetComponent<TilemapCollider2D>();if(tc!=null&&tc.enabled)tc.ProcessTilemapChanges();
                var cc=map.GetComponent<CompositeCollider2D>();if(cc!=null&&cc.enabled)cc.GenerateGeometry();
            }
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Physics2D.SyncTransforms();
        }

        static Vector2 V(float x,float y)=>new Vector2(x,y);
        static Rect Around(Vector2 p,float r)=>new Rect(p.x-r,p.y-r,r*2,r*2);
        static void Move(string path,Vector2 position)=>GameObject.Find(path).transform.position=position;
        static Tilemap NewMap(string name,int order,Color color)
        {var g=new GameObject(name,typeof(Tilemap),typeof(TilemapRenderer));g.transform.SetParent(GameObject.Find("Grid").transform,false);g.GetComponent<TilemapRenderer>().sortingOrder=order;var map=g.GetComponent<Tilemap>();map.color=color;return map;}

        static void AddRoad(float radius,params Vector2[] control)
        {
            var points=new List<Vector2>();
            for(int i=0;i<control.Length-1;i++)for(int k=0;k<8;k++) {
                float t=k/8f;points.Add(Catmull(control[Math.Max(0,i-1)],control[i],control[i+1],control[Math.Min(control.Length-1,i+2)],t));
            }
            points.Add(control.Last());Roads.Add(new Stroke{samples=points.ToArray(),radius=radius});
        }
        static Vector2 Catmull(Vector2 a,Vector2 b,Vector2 c,Vector2 d,float t)=>.5f*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t);
        static Vector2[] SmoothPolygon(Vector2[] vertices)
        {var result=new List<Vector2>();for(int i=0;i<vertices.Length;i++)for(int k=0;k<6;k++)result.Add(Catmull(vertices[(i+vertices.Length-1)%vertices.Length],vertices[i],vertices[(i+1)%vertices.Length],vertices[(i+2)%vertices.Length],k/6f));return result.ToArray();}
        static float SegmentDistance(Vector2 p,Vector2 a,Vector2 b)
        {var d=b-a;return Vector2.Distance(p,a+d*Mathf.Clamp01(Vector2.Dot(p-a,d)/Mathf.Max(.00001f,d.sqrMagnitude)));}
        static float RoadDistance(Vector2 p)
        {float d=1000;foreach(var s in Roads)for(int i=1;i<s.samples.Length;i++)d=Mathf.Min(d,SegmentDistance(p,s.samples[i-1],s.samples[i])-s.radius);return d;}
        static float PolygonDistance(Vector2 p,Vector2[] polygon)
        {
            bool inside=false;float d=1000;
            for(int i=0,j=polygon.Length-1;i<polygon.Length;j=i++) {
                var a=polygon[i];var b=polygon[j];d=Mathf.Min(d,SegmentDistance(p,a,b));
                if((a.y>p.y)!=(b.y>p.y)&&p.x<(b.x-a.x)*(p.y-a.y)/(b.y-a.y)+a.x)inside=!inside;
            }
            return inside?-d:d;
        }
        static float FieldDistance(Vector2 p,Vector2[] polygon)
        {
            float d=Mathf.Max(PolygonDistance(p,polygon),.65f-RoadDistance(p));d=Mathf.Max(d,.85f-RiverDistance(p));
            foreach(var lamp in Lamps)d=Mathf.Max(d,3.05f-Vector2.Distance(p,lamp));
            foreach(var encounter in Encounters)d=Mathf.Max(d,4.45f-Vector2.Distance(p,encounter));return d;
        }
        static Rect Bounds(Vector2[] points,float padding)
        {float x0=points.Min(p=>p.x)-padding,y0=points.Min(p=>p.y)-padding;return new Rect(x0,y0,points.Max(p=>p.x)+padding-x0,points.Max(p=>p.y)+padding-y0);}

        static void Paint(Tilemap map,Tile source,Func<Vector2,float> sdf,Rect bounds)
        {
            for(int y=Math.Max(0,Mathf.FloorToInt(bounds.yMin));y<Math.Min(56,Mathf.CeilToInt(bounds.yMax));y++)
            for(int x=Math.Max(0,Mathf.FloorToInt(bounds.xMin));x<Math.Min(88,Mathf.CeilToInt(bounds.xMax));x++) {
                float center=sdf(new Vector2(x+.5f,y+.5f));if(center>1.1f)continue;
                if(center < -1.1f){map.SetTile(new Vector3Int(x,y,0),source);continue;}
                var rows=new ushort[16];int total=0;
                for(int py=0;py<16;py++)for(int px=0;px<16;px++)if(sdf(new Vector2(x+(px+.5f)/16,y+(py+.5f)/16))<=0){rows[py]|=(ushort)(1<<px);total++;}
                if(total>0)map.SetTile(new Vector3Int(x,y,0),total==256?source:MeshTile(source,rows));
            }
        }
        static Tile MeshTile(Tile source,ushort[] rows)
        {
            string mask=string.Join("",rows.Select(r=>r.ToString("x4")));string digest;
            using(var sha=SHA256.Create())digest=BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(source.name+mask))).Replace("-","").Substring(0,16);
            string key=source.name+"_"+digest;if(Cache.TryGetValue(key,out var cached))return cached;
            Definitions[key]=new RefinementMask{key=key,source=AssetDatabase.GetAssetPath(source),rows=rows};
            var tile=ScriptableObject.CreateInstance<Tile>();tile.name=key;tile.sprite=source.sprite;tile.colliderType=UnityEngine.Tilemaps.Tile.ColliderType.None;
            Cache[key]=tile;return tile;
        }

        static void BakeMeshLibrary()
        {
            string path=Output+"/RefinedTiles.sdmesh";File.WriteAllText(path,JsonConvert.SerializeObject(Definitions.Values.OrderBy(e=>e.key).ToArray()));
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var baked=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Tile>().ToDictionary(t=>t.name);
            if(baked.Count!=Definitions.Count)throw new InvalidOperationException("Baked tile library count mismatch.");
            foreach(var map in UnityEngine.Object.FindObjectsByType<Tilemap>())foreach(var p in map.cellBounds.allPositionsWithin) {
                var tile=map.GetTile(p);if(tile!=null&&baked.TryGetValue(tile.name,out var replacement))map.SetTile(p,replacement);
            }
            foreach(var tile in Cache.Values)UnityEngine.Object.DestroyImmediate(tile);Cache.Clear();
        }

        static void RebuildRiverCollision()
        {
            var go=GameObject.Find("Grid/Water/WaterCollision");go.GetComponent<Tilemap>().ClearAllTiles();
            go.GetComponent<TilemapCollider2D>().enabled=false;go.GetComponent<CompositeCollider2D>().enabled=false;
            var collider=go.GetComponent<PolygonCollider2D>();if(collider==null)collider=go.AddComponent<PolygonCollider2D>();collider.pathCount=2;
            for(int part=0;part<2;part++) {
                float y0=part==0?0:30,y1=part==0?26:56;var left=new List<Vector2>();var right=new List<Vector2>();
                for(float y=y0;y<=y1+.001f;y+=.25f){left.Add(V(RiverCenter(y)-HalfRiver(y)-.27f,y));right.Add(V(RiverCenter(y)+HalfRiver(y)+.27f,y));}
                right.Reverse();left.AddRange(right);collider.SetPath(part,left.ToArray());
            }
        }
        static void ReclusterProps()
        {
            var trees=new[]{V(4.5f,46),V(8,48),V(10.5f,49),V(24.5f,47),V(28,49),V(32,48),V(50,44),V(53,46),V(56,48),V(83,46),V(85,42),V(84.5f,36),V(5,6),V(9,4.5f),V(13,5),V(33,5),V(36,7),V(38,11),V(73,4.5f),V(77,6),V(81,5),V(84,8)};
            var group=GameObject.Find("Environment/ReferenceDressing/Trees").transform;
            for(int i=0;i<group.childCount;i++)group.GetChild(i).position=trees[i%trees.Length];
            var rocks=new[]{V(9.5f,45),V(11,46),V(37.5f,40.5f),V(39,41.5f),V(78,8),V(80,8.5f),V(81.5f,9.5f)};
            var landmarks=GameObject.Find("Environment/ReferenceDressing/Landmarks").transform;
            var rockObjects=landmarks.Cast<Transform>().Where(t=>t.name.StartsWith("RockCluster_")).ToArray();
            for(int i=0;i<rockObjects.Length;i++)rockObjects[i].position=rocks[i%rocks.Length];
            var flags=landmarks.Cast<Transform>().Where(t=>t.name=="FestivalBanner").ToArray();var positions=new[]{V(39,31.5f),V(51,31.5f),V(71.5f,38),V(79,38)};
            for(int i=0;i<flags.Length;i++)flags[i].position=positions[i%positions.Length];
        }
        static void DecorateClusters(Tilemap decor,Vector2[][] fields)
        {
            var anchors=new[]{V(9,46),V(26,47),V(52,44),V(82,45),V(8,7),V(36,7),V(79,8),V(15,44)};
            var offsets=new[]{V(-1.5f,-.5f),V(-.5f,-1),V(1,-.5f),V(1.5f,1),V(-1,1.5f)};
            foreach(var a in anchors)foreach(var o in offsets) {
                var p=a+o;if(RoadDistance(p)<1||RiverDistance(p)<1||fields.Any(f=>PolygonDistance(p,f)<.7f)||Lamps.Any(l=>Vector2.Distance(l,p)<3)||Encounters.Any(c=>Vector2.Distance(c,p)<4.5f))continue;
                decor.SetTile(new Vector3Int(Mathf.FloorToInt(p.x),Mathf.FloorToInt(p.y),0),Native(o.x>0?"Flowers":"Tuft"));
            }
        }

        public static string InteractionReport()
        {
            Physics2D.SyncTransforms();
            Func<Vector2,float,bool> clear=(p,r)=>!Physics2D.OverlapCircleAll(p,r).Any(c=>c.enabled&&!c.isTrigger);
            var lamps=Lamps.Select((p,i)=>new{name="Lamp_0"+(i+1),x=p.x,y=p.y,clearApproaches=Enumerable.Range(0,16).Count(k=>clear(p+V(Mathf.Cos(k*Mathf.PI/8),Mathf.Sin(k*Mathf.PI/8))*1.25f,.35f))}).ToArray();
            var arenas=Encounters.Select(p=>new{x=p.x,y=p.y,radius=3f,clear=clear(p,3)}).ToArray();
            var spawn=GameObject.Find("Gameplay/PlayerSpawn").transform.position;var hut=GameObject.Find("Environment/ReferenceDressing/HutsAndHay/ThatchedHut").transform.position;
            var report=new{status=lamps.All(l=>l.clearApproaches==16)&&arenas.All(a=>a.clear)&&Vector2.Distance(spawn,hut)<3?"PASS":"FAIL",lamps,arenas,spawnToHut=Vector2.Distance(spawn,hut),riverCollision=GameObject.Find("Grid/Water/WaterCollision").GetComponent<PolygonCollider2D>().pathCount,mainRoadWidth=2.6f,sideRoadWidth=1.9f,edgePixelSize=1f/16};
            string json=JsonConvert.SerializeObject(report,Formatting.Indented);Directory.CreateDirectory("Docs/Verification/Refinement");File.WriteAllText("Docs/Verification/Refinement/InteractionAndClearance.json",json);return json;
        }

        public static string PhysicalReport()
        {
            if(!EditorApplication.isPlaying)throw new InvalidOperationException("Physical probes require Play Mode.");
            var mode=Physics2D.simulationMode;var gate=GameObject.Find("Gameplay/LockedGate").GetComponent<BoxCollider2D>();bool originalGate=gate.enabled;
            var probe=new GameObject("__TemporaryRefinementProbe");var rb=probe.AddComponent<Rigidbody2D>();rb.gravityScale=0;rb.freezeRotation=true;rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;probe.AddComponent<CircleCollider2D>().radius=.35f;
            object report;
            try {
                Physics2D.simulationMode=SimulationMode2D.Script;gate.enabled=true;
                var water=Travel(rb,V(36,20.5f),V(5,0),100);var bridge=Travel(rb,V(39.5f,27.5f),V(10,0),65);
                var closed=Travel(rb,V(75,34),V(0,5),80);gate.enabled=false;var opened=Travel(rb,V(75,34),V(0,5),80);
                var boundary=Travel(rb,V(4,5),V(-5,0),80);
                bool pass=water.x<44&&bridge.x>51&&closed.y<37&&opened.y>39&&boundary.x>2;
                report=new{status=pass?"PASS":"FAIL",mode="PlayMode",probeRadius=.35f,waterStops=water.x<44,bridgeCrosses=bridge.x>51,closedGateStops=closed.y<37,openGateCrosses=opened.y>39,boundaryStops=boundary.x>2,waterX=water.x,bridgeX=bridge.x,closedGateY=closed.y,openGateY=opened.y,boundaryX=boundary.x};
            } finally {UnityEngine.Object.DestroyImmediate(probe);gate.enabled=originalGate;Physics2D.simulationMode=mode;Physics2D.SyncTransforms();}
            string json=JsonConvert.SerializeObject(report,Formatting.Indented);File.WriteAllText("Docs/Verification/Refinement/PhysicsProbe.json",json);return json;
        }
        static Vector2 Travel(Rigidbody2D rb,Vector2 start,Vector2 velocity,int steps)
        {
            rb.position=start;rb.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();
            for(int i=0;i<steps;i++){rb.linearVelocity=velocity;Physics2D.Simulate(.02f);}var end=rb.position;rb.linearVelocity=Vector2.zero;return end;
        }
    }
}
