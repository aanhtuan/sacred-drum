using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace SacredDrum.EditorTools
{
    public static class ScarecrowAnimationAssets
    {
        const string Root = "Assets/Art/Characters/PrototypeCast/";
        static readonly string[] Actions = { "Idle", "Walk", "Attack", "Hurt", "Die" };
        static readonly string[] Directions = { "Down", "Right", "Up", "Left" };
        static readonly int[] Columns = { 6, 8, 6, 4, 6 };

        [MenuItem("SacredDrum/Prototype Cast/Build Scarecrow Animation")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play Mode first.");
            string controllerPath = Root + "Animations/Scarecrow.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var machine = controller.layers[0].stateMachine;
            foreach (var child in machine.states) machine.RemoveState(child.state);
            for (int action = 0; action < Actions.Length; action++)
            {
                ImportFrames(Actions[action], Columns[action]);
                var frames = LoadFrames(Actions[action]);
                for (int direction = 0; direction < Directions.Length; direction++)
                    AddClip(machine, Actions[action], Directions[direction], frames, direction * Columns[action], Columns[action]);
            }
            machine.defaultState = machine.states.First(s => s.state.name == "IdleDown").state;
            string prefabPath = Root + "Prefabs/Scarecrow_VisualOnly.prefab";
            var creature = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var renderer = creature.GetComponent<SpriteRenderer>();
                renderer.sprite = LoadFrames("Idle")[0];
                renderer.color = new Color(.70f, .74f, .80f, 1f);
                var animator = creature.GetComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                PrefabUtility.SaveAsPrefabAsset(creature, prefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(creature); }
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }

        static Sprite[] LoadFrames(string action) => AssetDatabase.LoadAllAssetsAtPath(Root + "Sources/ScarecrowStraw/" + action + ".png")
            .OfType<Sprite>().OrderBy(s => int.Parse(s.name.Substring(s.name.LastIndexOf('_') + 1))).ToArray();

        static void AddClip(AnimatorStateMachine machine, string action, string direction, Sprite[] frames, int first, int count)
        {
            string name = action + direction;
            string path = Root + "Animations/Scarecrow_" + name + ".anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, path); }
            clip.name = name;
            clip.frameRate = 100;
            bool loop = action == "Idle" || action == "Walk";
            int[] ticks = action == "Idle" ? new[] { 30, 24, 24, 30, 24, 24 }
                : action == "Walk" ? new[] { 14, 10, 12, 10, 14, 10, 12, 10 }
                : action == "Attack" ? new[] { 26, 20, 10, 8, 14, 24 }
                : action == "Hurt" ? new[] { 8, 14, 12, 20 }
                : new[] { 18, 16, 12, 14, 22, 40 };
            var keys = new ObjectReferenceKeyframe[count + 1];
            int time = 0;
            for (int i = 0; i < count; i++)
            {
                keys[i] = new ObjectReferenceKeyframe { time = time / 100f, value = frames[first + i] };
                time += ticks[i];
            }
            keys[count] = new ObjectReferenceKeyframe { time = time / 100f, value = frames[first + (loop ? 0 : count - 1)] };
            AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            machine.AddState(name).motion = clip;
            EditorUtility.SetDirty(clip);
        }

        static void ImportFrames(string action, int columns)
        {
            string path = Root + "Sources/ScarecrowStraw/" + action + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.isReadable = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var pixels = texture.GetPixels32();
            var factories = new SpriteDataProviderFactories();
            factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            var old = provider.GetSpriteRects().ToDictionary(s => s.name);
            var rects = new System.Collections.Generic.List<SpriteRect>();
            float baselineFromTop = 0;
            for (int row = 0; row < 4; row++) for (int col = 0; col < columns; col++)
            {
                int left = Mathf.RoundToInt(col * texture.width / (float)columns);
                int right = Mathf.RoundToInt((col + 1) * texture.width / (float)columns);
                int bottom = Mathf.RoundToInt((3 - row) * texture.height / 4f);
                int top = Mathf.RoundToInt((4 - row) * texture.height / 4f);
                int x0 = right, x1 = left, y0 = top, y1 = bottom;
                for (int y = bottom; y < top; y++) for (int x = left; x < right; x++)
                    if (pixels[y * texture.width + x].a > 102)
                    { x0 = Mathf.Min(x0, x); x1 = Mathf.Max(x1, x + 1); y0 = Mathf.Min(y0, y); y1 = Mathf.Max(y1, y + 1); }
                if (x1 <= x0 || y1 <= y0) throw new System.InvalidOperationException("Empty scarecrow cell: " + action + " " + row + "," + col);
                if (row == 0 && col == 0)
                {
                    baselineFromTop = texture.height - y0;
                    importer.spritePixelsPerUnit = (y1 - y0) / 1.55f;
                }
                string name = action + "_" + (row * columns + col);
                rects.Add(new SpriteRect { name = name, spriteID = old.ContainsKey(name) ? old[name].spriteID : GUID.Generate(),
                    rect = new Rect(left, bottom, right - left, top - bottom), alignment = SpriteAlignment.Custom,
                    pivot = new Vector2(.5f, (top - baselineFromTop - bottom) / (top - bottom)) });
            }
            provider.SetSpriteRects(rects.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(s => new SpriteNameFileIdPair(s.name, s.spriteID)));
            provider.Apply();
            importer.SaveAndReimport();
        }
    }
}
