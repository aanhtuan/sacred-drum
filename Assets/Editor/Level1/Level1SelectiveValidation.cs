using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SacredDrum.Gameplay;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace SacredDrum.EditorTools
{
    public static class Level1SelectiveValidation
    {
        const int Width=177, Height=113;
        const string Proof="Docs/Verification/SelectiveRefinement";
        static Vector2 Point(int id) => new Vector2((id%Width)*.5f,(id/Width)*.5f);
        static int Id(Vector2 p) => Mathf.RoundToInt(p.y*2)*Width+Mathf.RoundToInt(p.x*2);
        static bool CheckRiverTopology(GameObject player)
        {
            var closure=new GameObject("QA_Temporary_Bridge_Closure");
            try
            {
                closure.transform.position=new Vector3(45,27,0);closure.AddComponent<BoxCollider2D>().size=new Vector2(4,4);Physics2D.SyncTransforms();
                var free=new bool[Width*Height];var visited=new bool[free.Length];
                for(int i=0;i<free.Length;i++)free[i]=!Physics2D.OverlapCircleAll(Point(i),.42f).Any(c=>!c.isTrigger&&c.gameObject!=player);
                var queue=new Queue<int>();int start=Id(new Vector2(14.5f,41));queue.Enqueue(start);visited[start]=true;
                while(queue.Count>0)
                {
                    int at=queue.Dequeue(),x=at%Width,y=at/Width;
                    foreach(int next in new[]{x>0?at-1:-1,x<Width-1?at+1:-1,y>0?at-Width:-1,y<Height-1?at+Width:-1})
                        if(next>=0&&free[next]&&!visited[next]){visited[next]=true;queue.Enqueue(next);}
                }
                bool west=visited[Id(new Vector2(31.5f,16.5f))],east=visited[Id(new Vector2(49,27))],seal2=visited[Id(new Vector2(65,13.5f))],seal3=visited[Id(new Vector2(74,33.5f))];
                bool passed=west&&!east&&!seal2&&!seal3;
                File.WriteAllText(Proof+"/RiverTopology.json",JsonConvert.SerializeObject(new{status=passed?"PASS":"FAIL",temporaryBridgeClosure=true,westSealReachable=west,eastBankReachable=east,seal2Reachable=seal2,seal3Reachable=seal3},Formatting.Indented));
                return passed;
            }
            finally {UnityEngine.Object.DestroyImmediate(closure);Physics2D.SyncTransforms();}
        }
        public static string Run()
        {
            if(!EditorApplication.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=Level1MapBuilder.ScenePath)
                throw new InvalidOperationException("Run in Level 1 Play Mode.");
            var player=UnityEngine.Object.FindFirstObjectByType<PlayerController>(); var body=player.GetComponent<Rigidbody2D>();
            var original=body.position; var oldSimulation=Physics2D.simulationMode; var oldKeyboard=Keyboard.current;
            var keyboard=InputSystem.AddDevice<Keyboard>("Level1Selective_QA"); var results=new List<object>(); bool pass=true;
            Action<string,bool,object> check=(name,ok,evidence)=>{pass &= ok;results.Add(new{name,status=ok?"PASS":"FAIL",evidence});};
            Action<Key[]> input=keys=>{InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));InputSystem.Update();player.SendMessage("Update");};
            Action step=()=>{player.SendMessage("FixedUpdate");Physics2D.Simulate(.02f);};
            Action<Vector2> place=feet=>{body.position=feet+Vector2.up*.45f;body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();};
            Func<Vector2,Key[],int,Vector2> travel=(start,keys,ticks)=>{place(start);input(keys);for(int i=0;i<ticks;i++)step();return body.position-Vector2.up*.45f;};
            try
            {
                Physics2D.simulationMode=SimulationMode2D.Script;
                var start=new Vector2(14.5f,41);
                foreach(var key in new[]{Key.W,Key.A,Key.S,Key.D})
                {
                    var delta=key==Key.W?Vector2.up:key==Key.S?Vector2.down:key==Key.A?Vector2.left:Vector2.right;
                    var end=travel(start,new[]{key},20);check("WASD_"+key,Vector2.Distance(end,start+1.6f*delta)<.04f,new{endX=end.x,endY=end.y});
                }
                float straight=Vector2.Distance(start,travel(start,new[]{Key.D},20));float diagonal=Vector2.Distance(start,travel(start,new[]{Key.D,Key.W},20));
                check("DiagonalSpeed",Mathf.Abs(straight-diagonal)<.03f,new{straight,diagonal});
                var last=player.LastFacingDirection; var stopped=body.position;input(Array.Empty<Key>());for(int i=0;i<25;i++)step();
                check("ReleaseAndFacing",Vector2.Distance(stopped,body.position)<.01f&&last==player.LastFacingDirection,new{drift=Vector2.Distance(stopped,body.position)});
                var forest=travel(new Vector2(14,34),new[]{Key.W},100);check("ForestWallBlocks",forest.y>35&&forest.y<37,new{x=forest.x,y=forest.y});
                var river=travel(new Vector2(49,17),new[]{Key.A},100);check("RiverOutsideBridgeBlocks",river.x>46.5f&&river.x<49,new{x=river.x,y=river.y});
                var bridge=travel(new Vector2(40,27),new[]{Key.D},115);check("WoodenBridgeCrosses",bridge.x>49&&Mathf.Abs(bridge.y-27)<.04f,new{x=bridge.x,y=bridge.y});
                var gate=travel(new Vector2(75,34),new[]{Key.W},100);check("LockedGateBlocks",gate.y>36&&gate.y<37,new{x=gate.x,y=gate.y});
                var fence=travel(new Vector2(74,48),new[]{Key.W},70);check("ShrineFenceBlocks",fence.y>49&&fence.y<50,new{x=fence.x,y=fence.y});
                var rock=travel(new Vector2(40,36.5f),new[]{Key.W},50);check("OverlookRockBlocks",rock.y<38&&rock.y>37,new{x=rock.x,y=rock.y});
                input(Array.Empty<Key>());place(start);
                var free=new bool[Width*Height]; var previous=Enumerable.Repeat(-1,free.Length).ToArray();
                for(int i=0;i<free.Length;i++) free[i]=!Physics2D.OverlapCircleAll(Point(i),.42f).Any(c=>!c.isTrigger && c.enabled && c.gameObject!=player.gameObject);
                int origin=Id(start); previous[origin]=origin; var queue=new Queue<int>();queue.Enqueue(origin);
                while(queue.Count>0)
                {
                    int at=queue.Dequeue();int x=at%Width,y=at/Width;
                    foreach(int next in new[]{x>0?at-1:-1,x<Width-1?at+1:-1,y>0?at-Width:-1,y<Height-1?at+Width:-1})
                        if(next>=0&&free[next]&&previous[next]<0){previous[next]=at;queue.Enqueue(next);}
                }
                var goals=new Dictionary<string,Vector2> {
                    {"Seal_01",new Vector2(31.5f,16.5f)},{"Seal_02",new Vector2(65,13.5f)},
                    {"Seal_03",new Vector2(74,33.5f)},{"GateFront",new Vector2(75,35.5f)}
                };
                var traversed=new List<object>();
                foreach(var destination in goals)
                {
                    int target=Id(destination.Value);bool reachable=previous[target]>=0;
                    check("Reachable_"+destination.Key,reachable,new{x=destination.Value.x,y=destination.Value.y});
                    if(!reachable){check("ControllerWalk_"+destination.Key,false,"No physical path");continue;}
                    var path=new List<int>();
                    for(int at=target;at!=origin;at=previous[at])path.Add(at);path.Reverse();
                    place(start);input(Array.Empty<Key>());int ticks=0;bool complete=true;
                    foreach(int at in path)
                    {
                        var goal=Point(at);int attempts=0;
                        while(Vector2.Distance(body.position-Vector2.up*.45f,goal)>.07f&&attempts++<20)
                        {
                            var delta=goal-(body.position-Vector2.up*.45f);
                            input(new[]{Mathf.Abs(delta.x)>Mathf.Abs(delta.y)?(delta.x>0?Key.D:Key.A):(delta.y>0?Key.W:Key.S)});
                            step();ticks++;
                        }
                        if(attempts>20){complete=false;break;}
                    }
                    check("ControllerWalk_"+destination.Key,complete&&Vector2.Distance(body.position-Vector2.up*.45f,destination.Value)<.1f,
                        new{ticks,length=path.Count*.5f,endX=body.position.x,endFeetY=body.position.y-.45f});
                    if(destination.Key=="GateFront")check("MainRouteNotLonger",path.Count*.5f<=153,new{length=path.Count*.5f,previousLength=153});
                    traversed.Add(new{goal=destination.Key,points=path.Select(i=>new[]{Point(i).x,Point(i).y})});
                }
                File.WriteAllText(Proof+"/TraversedSealRoutes.json",JsonConvert.SerializeObject(traversed,Formatting.Indented));
                check("ClosedGateProtectsFragment",previous[Id(new Vector2(75.5f,45.5f))]<0,"No path around the closed gate");
                place(start);input(Array.Empty<Key>());
                foreach(var goal in goals.Where(g=>g.Key.StartsWith("Seal_")))
                {
                    int clear=0,total=0;
                    for(int x=-4;x<=4;x++)for(int y=-3;y<=3;y++)
                    {
                        total++;var point=goal.Value+new Vector2(x,y);
                        if(!Physics2D.OverlapCircleAll(point,.45f).Any(c=>!c.isTrigger&&c.gameObject!=player.gameObject))clear++;
                    }
                    bool boxClear=!Physics2D.OverlapBoxAll(goal.Value,new Vector2(8,6),0).Any(c=>!c.isTrigger&&c.gameObject!=player.gameObject);
                    check("CombatSpace8x6_"+goal.Key,clear==total&&boxClear,new{clearSamples=clear,totalSamples=total,fullBoxClear=boxClear,footRadius=.45f});
                }
                foreach(string name in new[]{"ScarecrowSpawn_01","RatSpawn_01","ScarecrowSpawn_02","ScarecrowSpawn_03"})
                {
                    var marker=GameObject.Find("Gameplay/SealAreas/"+name);
                    bool freeMarker=marker!=null&&!Physics2D.OverlapCircleAll(marker.transform.position,.5f).Any(c=>!c.isTrigger&&c.gameObject!=player.gameObject);
                    check("SpawnClear_"+name,freeMarker,marker==null?"missing":marker.transform.position.ToString());
                    check("SpawnMarkerOnly_"+name,marker!=null&&marker.GetComponents<Component>().All(c=>c is Transform),"Transform only; no enemy or AI");
                }
                var legacy=UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).Where(t=>t.name=="EnemySpawn_01"||t.name=="EnemySpawn_02");
                foreach(var marker in legacy)check("LegacySpawnSafe_"+marker.name,!marker.gameObject.activeInHierarchy&&!Physics2D.OverlapCircleAll(marker.position,.5f).Any(c=>!c.isTrigger&&c.gameObject!=player.gameObject),"Retained inactive at safe locations");
                check("OnlyBridgeCrossesRiver",CheckRiverTopology(player.gameObject),"Temporarily close bridge; west Seal must remain reachable and east bank must become unreachable");
                var cliff=travel(new Vector2(32,7.5f),new[]{Key.W},60);
                check("RaisedEarthBlocks",cliff.y<9&&cliff.y>8,new{x=cliff.x,y=cliff.y});
                var camera=Camera.main;var follow=camera.GetComponent<PlayerFollowCamera>();
                foreach(var corner in new[]{Vector2.zero,new Vector2(88,0),new Vector2(0,56),new Vector2(88,56)})
                {
                    body.position=corner;body.transform.position=corner;follow.SnapToTarget();var p=camera.transform.position;
                    float hw=camera.orthographicSize*camera.aspect,hh=camera.orthographicSize;
                    check("CameraBounds_"+corner,p.x-hw>=-.01f&&p.x+hw<=88.01f&&p.y-hh>=-.01f&&p.y+hh<=56.01f,new{x=p.x,y=p.y,zoom=camera.orthographicSize});
                }
                var report=JsonConvert.SerializeObject(new{status=pass?"PASS":"FAIL",mode="Play Mode, synthetic Input System keyboard, real PlayerController/Rigidbody2D and scene colliders",checks=results},Formatting.Indented);
                Directory.CreateDirectory(Proof);File.WriteAllText(Proof+"/PlayModeValidation.json",report);return report;
            }
            finally
            {
                input(Array.Empty<Key>());body.position=original;body.transform.position=original;body.linearVelocity=Vector2.zero;
                InputSystem.RemoveDevice(keyboard);if(oldKeyboard!=null)oldKeyboard.MakeCurrent();Physics2D.simulationMode=oldSimulation;Physics2D.SyncTransforms();Camera.main.GetComponent<PlayerFollowCamera>().SnapToTarget();
            }
        }
    }
}
