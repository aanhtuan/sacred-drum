using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace SacredDrum.EditorTools
{
    // Tests actual scene colliders, independent of the layout generation data.
    public static class Level1MapValidation
    {
        const float Radius=.35f;
        static Vector2Int Cell(Vector3 p)=>new Vector2Int(Mathf.FloorToInt(p.x),Mathf.FloorToInt(p.y));
        static bool Solid(Vector2 p)=>Physics2D.OverlapCircleAll(p,Radius).Any(c=>c.enabled&&!c.isTrigger);
        static HashSet<Vector2Int> Reachable()
        {
            var free=new HashSet<Vector2Int>();
            for(int y=0;y<56;y++)for(int x=0;x<88;x++)if(!Solid(new Vector2(x+.5f,y+.5f)))free.Add(new Vector2Int(x,y));
            var start=Cell(GameObject.Find("Gameplay/PlayerSpawn").transform.position);
            var reached=new HashSet<Vector2Int>();var queue=new Queue<Vector2Int>();
            if(free.Contains(start)){reached.Add(start);queue.Enqueue(start);}
            var directions=new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};
            while(queue.Count>0) {
                var p=queue.Dequeue();foreach(var d in directions) {
                    var next=p+d;if(!free.Contains(next)||reached.Contains(next))continue;
                    // Check clearance along each edge, not just cell centers.
                    bool blocked=Physics2D.CircleCastAll(new Vector2(p.x+.5f,p.y+.5f),Radius,d,1).Any(h=>h.collider&&!h.collider.isTrigger);
                    if(blocked)continue;reached.Add(next);queue.Enqueue(next);
                }
            }
            return reached;
        }

        public static string GeometryReport()
        {
            if(SceneManager.GetActiveScene().path!=Level1MapBuilder.ScenePath)throw new InvalidOperationException("Wrong scene");
            Physics2D.SyncTransforms();
            var gate=GameObject.Find("Gameplay/LockedGate").GetComponent<BoxCollider2D>();
            bool gateWasEnabled=gate.enabled;
            object report;
            try {
                gate.enabled=true;Physics2D.SyncTransforms();var closed=Reachable();
                var names=new[]{"Lamp_01","Lamp_02","Lamp_03"};
                var lamps=names.Select(n=>new { name=n, reachable=closed.Contains(Cell(GameObject.Find("Gameplay/"+n).transform.position)) }).ToArray();
                var fragmentCell=Cell(GameObject.Find("Gameplay/DrumFragment_01").transform.position);
                bool blockedFragment=!closed.Contains(fragmentCell);
                bool bridge=closed.Contains(new Vector2Int(40,27))&&closed.Contains(new Vector2Int(50,27));
                gate.enabled=false;Physics2D.SyncTransforms();var opened=Reachable();
                bool fragmentAfterOpen=opened.Contains(fragmentCell);
                var tiles=UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None).Select(m=>new {name=m.name,tiles=m.GetUsedTilesCount(),occupiedCells=m.GetTilesBlock(m.cellBounds).Count(t=>t!=null),collider=m.GetComponent<TilemapCollider2D>()!=null}).ToArray();
                bool noGroundCollider=GameObject.Find("Grid/Ground").GetComponent<Collider2D>()==null;
                bool pass=lamps.All(l=>l.reachable)&&blockedFragment&&bridge&&fragmentAfterOpen&&noGroundCollider;
                report=new {status=pass?"PASS":"FAIL",mode=EditorApplication.isPlaying?"PlayMode":"EditMode",scene=SceneManager.GetActiveScene().path,playerRadius=Radius,lamps,bridgeReachable=bridge,fragmentBlockedByClosedGate=blockedFragment,fragmentReachableAfterOpeningGate=fragmentAfterOpen,noGroundCollider,closedReachableCells=closed.Count,openedReachableCells=opened.Count,tilemaps=tiles};
            } finally {gate.enabled=gateWasEnabled;Physics2D.SyncTransforms();}
            string json=JsonConvert.SerializeObject(report,Formatting.Indented);
            Directory.CreateDirectory("Docs/Verification");File.WriteAllText("Docs/Verification/"+(EditorApplication.isPlaying?"PlayModeGeometry":"EditModeGeometry")+".json",json);
            return json;
        }

        public static string PhysicalProbeReport()
        {
            if(!EditorApplication.isPlaying)throw new InvalidOperationException("Run the physical probe in Play Mode.");
            var mode=Physics2D.simulationMode;
            var gate=GameObject.Find("Gameplay/LockedGate").GetComponent<BoxCollider2D>();bool gateWasEnabled=gate.enabled;
            var probe=new GameObject("__TemporaryLevel1PhysicsProbe");var rb=probe.AddComponent<Rigidbody2D>();
            rb.gravityScale=0;rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;rb.freezeRotation=true;probe.AddComponent<CircleCollider2D>().radius=Radius;
            object report;
            try {
                Physics2D.simulationMode=SimulationMode2D.Script;
                gate.enabled=true;
                Vector2 water=Travel(rb,new Vector2(40.5f,20.5f),new Vector2(5,0),80);
                Vector2 bridge=Travel(rb,new Vector2(39.5f,27.5f),new Vector2(10,0),65);
                Vector2 closed=Travel(rb,new Vector2(75,34),new Vector2(0,5),80);
                gate.enabled=false;
                Vector2 opened=Travel(rb,new Vector2(75,34),new Vector2(0,5),80);
                Vector2 boundary=Travel(rb,new Vector2(4,5),new Vector2(-5,0),80);
                bool pass=water.x<43&&bridge.x>51&&closed.y<37&&opened.y>39&&boundary.x>2;
                report=new { status=pass?"PASS":"FAIL",mode="PlayMode",probeRadius=Radius,waterStops=water.x<43,bridgeCrosses=bridge.x>51,closedGateStops=closed.y<37,openGateCrosses=opened.y>39,boundaryStops=boundary.x>2,endpoints=new{waterX=water.x,bridgeX=bridge.x,closedGateY=closed.y,openGateY=opened.y,boundaryX=boundary.x},limitations="Temporary circular Rigidbody2D probe only; no player input or lamp/gate gameplay implemented."};
            } finally {
                UnityEngine.Object.DestroyImmediate(probe);gate.enabled=gateWasEnabled;Physics2D.simulationMode=mode;Physics2D.SyncTransforms();
            }
            string json=JsonConvert.SerializeObject(report,Formatting.Indented);Directory.CreateDirectory("Docs/Verification");File.WriteAllText("Docs/Verification/PlayModePhysicsProbe.json",json);return json;
        }
        static Vector2 Travel(Rigidbody2D rb,Vector2 start,Vector2 velocity,int steps)
        {
            rb.position=start;rb.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();
            for(int i=0;i<steps;i++){rb.linearVelocity=velocity;Physics2D.Simulate(.02f);}
            var end=rb.position;rb.linearVelocity=Vector2.zero;return end;
        }
    }
}
