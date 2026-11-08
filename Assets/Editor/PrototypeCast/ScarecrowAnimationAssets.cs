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
        const string Sheet = Root + "Sources/Scarecrow_CorruptedIdle.png";

        [MenuItem("SacredDrum/Prototype Cast/Build Scarecrow Animation")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play Mode first.");
            ImportFrames();
            var frames = AssetDatabase.LoadAllAssetsAtPath(Sheet).OfType<Sprite>().OrderBy(s => s.name).ToArray();
            string clipPath = Root + "Animations/Scarecrow_CorruptedIdle.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, clipPath); }
            clip.frameRate = 12;
            int[] ticks = { 0, 2, 5, 7, 9, 12, 14, 16, 18 };
            var keys = new ObjectReferenceKeyframe[9];
            for (int i = 0; i < 9; i++) keys[i] = new ObjectReferenceKeyframe { time = ticks[i] / 12f, value = frames[i % 8] };
            AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            string controllerPath = Root + "Animations/Scarecrow.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var machine = controller.layers[0].stateMachine;
            var state = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == "HauntedIdle") ?? machine.AddState("HauntedIdle");
            state.motion = clip; machine.defaultState = state;
            string path = Root + "Prefabs/Scarecrow_VisualOnly.prefab";
            var creature = PrefabUtility.LoadPrefabContents(path);
            try
            {
                creature.GetComponent<SpriteRenderer>().sprite = frames[0];
                var animator = creature.GetComponent<Animator>();
                if (animator == null) animator = creature.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                if (creature.GetComponent<SacredDrum.Gameplay.ScarecrowIdleVariation>() == null)
                    creature.AddComponent<SacredDrum.Gameplay.ScarecrowIdleVariation>();
                PrefabUtility.SaveAsPrefabAsset(creature, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(creature); }
            EditorUtility.SetDirty(clip); EditorUtility.SetDirty(controller); AssetDatabase.SaveAssets();
        }

        static void ImportFrames()
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(Sheet);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.isReadable = true; importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 2048; importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Sheet); var pixels = texture.GetPixels32();
            var factories = new SpriteDataProviderFactories(); factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var old = provider.GetSpriteRects().ToDictionary(s => s.name);
            var rects = new System.Collections.Generic.List<SpriteRect>();
            for (int row = 0; row < 2; row++) for (int col = 0; col < 4; col++)
            {
                int left = Mathf.RoundToInt(col * texture.width / 4f), right = Mathf.RoundToInt((col + 1) * texture.width / 4f);
                int bottom = Mathf.RoundToInt((1 - row) * texture.height / 2f), top = Mathf.RoundToInt((2 - row) * texture.height / 2f);
                int x0 = right, y0 = top, x1 = left, y1 = bottom;
                for (int y = bottom; y < top; y++) for (int x = left; x < right; x++)
                    if (pixels[y * texture.width + x].a > 64)
                    { x0 = Mathf.Min(x0, x); x1 = Mathf.Max(x1, x + 1); y0 = Mathf.Min(y0, y); y1 = Mathf.Max(y1, y + 1); }
                if (x1 <= x0 || y1 <= y0) throw new System.InvalidOperationException("Empty scarecrow frame");
                int feetLeft = x1, feetRight = x0;
                for (int y = y0; y < y0 + (y1 - y0) * .15f; y++) for (int x = x0; x < x1; x++)
                    if (pixels[y * texture.width + x].a > 64) { feetLeft = Mathf.Min(feetLeft, x); feetRight = Mathf.Max(feetRight, x + 1); }
                string name = "Scarecrow_Idle_" + (row * 4 + col);
                rects.Add(new SpriteRect { name = name, spriteID = old.ContainsKey(name) ? old[name].spriteID : GUID.Generate(),
                    rect = new Rect(x0, y0, x1 - x0, y1 - y0), alignment = SpriteAlignment.Custom,
                    pivot = new Vector2(((feetLeft + feetRight) * .5f - x0) / (x1 - x0), 0) });
            }
            // One shared PPU preserves scale differences in the actual drawn poses.
            importer.spritePixelsPerUnit = rects.Average(r => r.rect.height) / 2.25f;
            provider.SetSpriteRects(rects.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(s => new SpriteNameFileIdPair(s.name, s.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
        }
    }
}
