using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace SacredDrum.EditorTools
{
    public static class MinhAnimationAssets
    {
        const string Root = "Assets/Art/Characters/PrototypeCast/";
        static readonly string[] Sheets = { "Idle", "Run", "Jump", "Crouch", "StaffAttack", "Up", "Side" };

        [MenuItem("SacredDrum/Prototype Cast/Build Minh Animations")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play Mode first.");
            foreach (string sheet in Sheets) ImportFrames(sheet);
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(Root + "Animations/Minh.controller");
            var machine = controller.layers[0].stateMachine;
            foreach (var state in machine.states) machine.RemoveState(state.state);
            AddState(machine, "Idle", "Idle", 0, new[] { 28, 22, 24, 22, 28, 22, 22, 28 }, true);
            AddState(machine, "Run", "Run", 0, new[] { 9, 8, 9, 8, 9, 8, 9, 8 }, true);
            AddState(machine, "IdleUp", "Up", 0, new[] { 28, 22, 24, 22, 28, 22, 22, 28 }, true);
            AddState(machine, "RunUp", "Up", 8, new[] { 9, 8, 9, 8, 9, 8, 9, 8 }, true);
            AddState(machine, "IdleSide", "Side", 0, new[] { 28, 22, 24, 22, 28, 22, 22, 28 }, true);
            AddState(machine, "RunSide", "Run", 0, new[] { 9, 8, 9, 8, 9, 8, 9, 8 }, true);
            AddState(machine, "Jump", "Jump", 0, new[] { 24, 18, 10, 13, 20, 12, 18, 26 }, false);
            AddState(machine, "StaffAttack", "StaffAttack", 0, new[] { 26, 18, 20, 8, 10, 12, 20, 26 }, false);
            AddState(machine, "CrouchDown", "Crouch", 0, new[] { 26, 16, 18, 24 }, false);
            AddState(machine, "CrouchHold", "Crouch", 4, new[] { 25 }, true);
            AddState(machine, "CrouchUp", "Crouch", 5, new[] { 18, 16, 26 }, false);
            machine.defaultState = machine.states.First(s => s.state.name == "Idle").state;
            string prefabPath = Root + "Prefabs/Minh_Farmer.prefab";
            var player = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                player.GetComponent<SpriteRenderer>().sprite = LoadFrames("Idle")[0];
                var animator = player.GetComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                PrefabUtility.SaveAsPrefabAsset(player, prefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(player); }
            EditorUtility.SetDirty(controller); AssetDatabase.SaveAssets();
        }

        static void AddState(AnimatorStateMachine machine, string name, string sheet, int first, int[] delays, bool loop)
        {
            string path = Root + "Animations/Minh_" + name + ".anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, path); }
            clip.name = name; clip.frameRate = 100;
            var frames = LoadFrames(sheet);
            var keys = new ObjectReferenceKeyframe[delays.Length + 1];
            int tick = 0;
            for (int i = 0; i < delays.Length; i++)
            {
                keys[i] = new ObjectReferenceKeyframe { time = tick / 100f, value = frames[first + i] };
                tick += delays[i];
            }
            keys[delays.Length] = new ObjectReferenceKeyframe { time = tick / 100f, value = frames[first + (loop ? 0 : delays.Length - 1)] };
            AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            machine.AddState(name).motion = clip; EditorUtility.SetDirty(clip);
        }

        static Sprite[] LoadFrames(string sheet) => AssetDatabase.LoadAllAssetsAtPath(Root + "Sources/MinhChibi/" + sheet + ".png")
            .OfType<Sprite>().OrderBy(s => int.Parse(s.name.Substring(s.name.LastIndexOf('_') + 1))).ToArray();

        static void ImportFrames(string sheet)
        {
            string path = Root + "Sources/MinhChibi/" + sheet + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.isReadable = true; importer.filterMode = FilterMode.Bilinear; importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 2048; importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path); var pixels = texture.GetPixels32();
            var factories = new SpriteDataProviderFactories(); factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var old = provider.GetSpriteRects().ToDictionary(s => s.name);
            var rects = new System.Collections.Generic.List<SpriteRect>();
            float baselineFromTop = 0;
            int rows = sheet == "Up" || sheet == "Side" ? 4 : 2;
            for (int row = 0; row < rows; row++) for (int col = 0; col < 4; col++)
            {
                int left = Mathf.RoundToInt(col * texture.width / 4f), right = Mathf.RoundToInt((col + 1) * texture.width / 4f);
                int bottom = Mathf.RoundToInt((rows - row - 1) * texture.height / (float)rows);
                int top = Mathf.RoundToInt((rows - row) * texture.height / (float)rows);
                int x0 = right, x1 = left, y0 = top, y1 = bottom;
                for (int y = bottom; y < top; y++) for (int x = left; x < right; x++)
                    if (pixels[y * texture.width + x].a > 102)
                    { x0 = Mathf.Min(x0, x); x1 = Mathf.Max(x1, x + 1); y0 = Mathf.Min(y0, y); y1 = Mathf.Max(y1, y + 1); }
                if (x1 <= x0 || y1 <= y0) throw new System.InvalidOperationException("Empty Minh sprite: " + sheet);
                if (row == 0 && col == 0)
                {
                    baselineFromTop = texture.height - y0;
                    importer.spritePixelsPerUnit = (y1 - y0) / 1.45f;
                }
                string name = sheet + "_" + (row * 4 + col);
                float baseline = top - baselineFromTop;
                rects.Add(new SpriteRect { name = name, spriteID = old.ContainsKey(name) ? old[name].spriteID : GUID.Generate(),
                    rect = new Rect(x0, bottom, x1 - x0, top - bottom), alignment = SpriteAlignment.Custom,
                    pivot = new Vector2(((left + right) * .5f - x0) / (x1 - x0), (baseline - bottom) / (top - bottom)) });
            }
            provider.SetSpriteRects(rects.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(s => new SpriteNameFileIdPair(s.name, s.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
        }
    }
}
