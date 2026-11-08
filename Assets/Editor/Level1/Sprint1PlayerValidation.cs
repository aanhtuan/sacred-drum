using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SacredDrum.Gameplay;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace SacredDrum.EditorTools
{
    /// <summary>Explicit Play Mode QA using the real prefab, Move action and scene colliders.</summary>
    public static class Sprint1PlayerValidation
    {
        [MenuItem("SacredDrum/Sprint 1/Validate player in Play Mode")]
        public static void RunFromMenu() => Debug.Log(Run());

        public static string Run()
        {
            if (!EditorApplication.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != Level1MapBuilder.ScenePath)
                throw new InvalidOperationException("Run in Level1_CanhDong Play Mode.");
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            var body = player.GetComponent<Rigidbody2D>();
            var follow = Camera.main.GetComponent<PlayerFollowCamera>();
            var originalPosition = body.position;
            var originalSimulation = Physics2D.simulationMode;
            var originalKeyboard = Keyboard.current;
            var keyboard = InputSystem.AddDevice<Keyboard>("Sprint1_QA_Keyboard");
            var results = new List<object>();
            bool allPassed = true;
            Action<string, bool, object> check = (name, passed, evidence) =>
            {
                allPassed &= passed;
                results.Add(new { name, status = passed ? "PASS" : "FAIL", evidence });
            };
            Action<Key[]> input = keys =>
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
                InputSystem.Update();
                player.SendMessage("Update");
            };
            Func<Vector2, Key[], int, Vector2> travel = (feet, keys, steps) =>
            {
                body.position = feet + new Vector2(0f, .45f);
                body.linearVelocity = Vector2.zero;
                Physics2D.SyncTransforms();
                input(keys);
                for (int i = 0; i < steps; i++)
                {
                    player.SendMessage("FixedUpdate");
                    Physics2D.Simulate(.02f);
                }
                return body.position - new Vector2(0f, .45f);
            };
            try
            {
                Physics2D.simulationMode = SimulationMode2D.Script;
                var start = new Vector2(34.5f, 30.5f);
                foreach (var key in new[] { Key.W, Key.A, Key.S, Key.D })
                {
                    var end = travel(start, new[] { key }, 25);
                    var expected = key == Key.W ? Vector2.up : key == Key.A ? Vector2.left : key == Key.S ? Vector2.down : Vector2.right;
                    check("WASD_" + key, Vector2.Distance(end, start + expected * 2f) < .03f, new { endX = end.x, endY = end.y });
                }
                float straight = Vector2.Distance(start, travel(start, new[] { Key.D }, 25));
                float diagonal = Vector2.Distance(start, travel(start, new[] { Key.D, Key.W }, 25));
                check("DiagonalSameSpeed", Mathf.Abs(straight - diagonal) < .03f, new { straight, diagonal });
                var facing = player.LastFacingDirection;
                var stopped = body.position;
                input(Array.Empty<Key>());
                for (int i = 0; i < 25; i++) { player.SendMessage("FixedUpdate"); Physics2D.Simulate(.02f); }
                check("ReleaseStopsAndKeepsFacing", Vector2.Distance(stopped, body.position) < .01f && facing == player.LastFacingDirection,
                    new { facingX = facing.x, facingY = facing.y, drift = Vector2.Distance(stopped, body.position) });
                var opposing = travel(start, new[] { Key.W, Key.S, Key.A, Key.D }, 25);
                check("OppositeKeysCancel", Vector2.Distance(start, opposing) < .01f, new { drift = Vector2.Distance(start, opposing) });
                input(new[] { Key.D }); player.SendMessage("FixedUpdate"); player.enabled = false;
                check("DisableStopsVelocity", body.linearVelocity == Vector2.zero, body.linearVelocity.ToString());
                player.enabled = true;

                var tree = travel(new Vector2(5f, 4.6f), new[] { Key.W }, 60);
                check("TreeBlocks", tree.y > 5.4f && tree.y < 5.9f, new { endX = tree.x, endY = tree.y, target = "CanopyTree_13" });
                var rock = travel(new Vector2(37.5f, 38.9f), new[] { Key.W }, 60);
                check("RockBlocks", rock.y > 40f && rock.y < 40.5f, new { endX = rock.x, endY = rock.y, target = "RockCluster_3" });
                var fence = travel(new Vector2(70.5f, 34.5f), new[] { Key.W }, 60);
                check("FenceBlocks", fence.y > 36.5f && fence.y < 37f, new { endX = fence.x, endY = fence.y });
                var river = travel(new Vector2(36f, 20.5f), new[] { Key.D }, 160);
                check("RiverBlocksOutsideBridge", river.x > 39f && river.x < 44f, new { endX = river.x, endY = river.y });
                var bridge = travel(new Vector2(39.5f, 27.5f), new[] { Key.D }, 170);
                check("WoodenBridgeCrosses", bridge.x > 52.5f && Mathf.Abs(bridge.y - 27.5f) < .05f, new { endX = bridge.x, endY = bridge.y });
                var gate = travel(new Vector2(75f, 34f), new[] { Key.W }, 100);
                check("LockedGateBlocks", gate.y > 36.5f && gate.y < 37f, new { endX = gate.x, endY = gate.y });
                var west = travel(new Vector2(4f, 20f), new[] { Key.A }, 100);
                check("MapBoundaryBlocks", west.x > 1f && west.x < 3f, new { endX = west.x, endY = west.y });

                input(Array.Empty<Key>());
                var camera = Camera.main;
                foreach (var corner in new[] { new Vector2(0, 0), new Vector2(88, 0), new Vector2(0, 56), new Vector2(88, 56) })
                {
                    // Explicit QA teleport: interpolated Transform otherwise lags rb.position
                    // until the next rendered frame, which this synchronous probe does not run.
                    player.transform.position = corner; body.position = corner;
                    Physics2D.SyncTransforms(); follow.SnapToTarget();
                    float h = camera.orthographicSize, w = h * camera.aspect;
                    var c = camera.transform.position;
                    var expected = new Vector2(Mathf.Clamp(corner.x, w, 88f - w), Mathf.Clamp(corner.y, h, 56f - h));
                    check("CameraClamp_" + corner, Vector2.Distance(c, expected) < .01f && c.x - w >= -.01f && c.x + w <= 88.01f && c.y - h >= -.01f && c.y + h <= 56.01f,
                        new { x = c.x, y = c.y, halfWidth = w, halfHeight = h });
                }
                player.transform.position = new Vector2(34, 30); body.position = new Vector2(34, 30);
                Physics2D.SyncTransforms(); follow.SnapToTarget();
                var cameraStart = camera.transform.position;
                player.transform.position = new Vector2(38, 30); body.position = new Vector2(38, 30);
                Physics2D.SyncTransforms();
                follow.SendMessage("LateUpdate");
                var cameraNext = camera.transform.position;
                check("CameraSmoothFollowStep", cameraNext.x > cameraStart.x && cameraNext.x < 38f && camera.orthographic,
                    new { beforeX = cameraStart.x, afterX = cameraNext.x, targetX = 38f, zoom = camera.orthographicSize });
            }
            finally
            {
                input(Array.Empty<Key>());
                player.transform.position = originalPosition;
                body.position = originalPosition; body.linearVelocity = Vector2.zero;
                Physics2D.SyncTransforms(); follow.SnapToTarget();
                InputSystem.RemoveDevice(keyboard);
                originalKeyboard?.MakeCurrent();
                Physics2D.simulationMode = originalSimulation;
            }
            var json = JsonConvert.SerializeObject(new
            {
                status = allPassed ? "PASS" : "FAIL", mode = "PlayMode",
                method = "Synthetic Input System keyboard -> real PlayerController Update/FixedUpdate -> Physics2D.Simulate(0.02). Camera LateUpdate invoked explicitly.",
                limitation = "Physical keyboard, visual smoothness at varied frame rates, and Windows build not tested.", results
            }, Formatting.Indented);
            Directory.CreateDirectory("Docs/Verification/Sprint1");
            File.WriteAllText("Docs/Verification/Sprint1/PlayerValidation.json", json);
            return json;
        }
    }
}
