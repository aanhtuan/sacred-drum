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
        const string Sheet = Root + "Sources/Minh_Animations.png";
        static readonly string[] Directions = { "Down", "Side", "Up" };
        static readonly string[] Actions = { "Idle", "Run", "Attack" };

        [MenuItem("SacredDrum/Prototype Cast/Build Minh Animations")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Exit Play Mode first.");
            if (!AssetDatabase.IsValidFolder(Root + "Animations"))
                AssetDatabase.CreateFolder(Root.TrimEnd('/'), "Animations");
            ImportFrames();
            var frames = AssetDatabase.LoadAllAssetsAtPath(Sheet).OfType<Sprite>().ToDictionary(s => s.name);
            string controllerPath = Root + "Animations/Minh.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var machine = controller.layers[0].stateMachine;
            for (int row = 0; row < 9; row++)
            {
                string name = Actions[row % 3] + "_" + Directions[row / 3];
                string clipPath = Root + "Animations/" + name + ".anim";
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if (clip == null) { clip = new AnimationClip(); AssetDatabase.CreateAsset(clip, clipPath); }
                float fps = row % 3 == 0 ? 5 : row % 3 == 1 ? 12 : 10;
                clip.frameRate = fps;
                var keys = new ObjectReferenceKeyframe[7];
                for (int f = 0; f < 6; f++) keys[f] = new ObjectReferenceKeyframe { time = f / fps, value = frames[name + "_" + f] };
                keys[6] = new ObjectReferenceKeyframe { time = 6 / fps, value = frames[name + (row % 3 == 2 ? "_5" : "_0")] };
                AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = row % 3 != 2;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                var state = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == name) ?? machine.AddState(name);
                state.motion = clip;
                if (row == 0) machine.defaultState = state;
                EditorUtility.SetDirty(clip);
            }
            string prefabPath = Root + "Prefabs/Minh_Farmer.prefab";
            var player = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                player.GetComponent<SpriteRenderer>().sprite = frames["Idle_Down_0"];
                var animator = player.GetComponent<Animator>();
                if (animator == null) animator = player.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                if (player.GetComponent<SacredDrum.Gameplay.MinhSpriteAnimation>() == null)
                    player.AddComponent<SacredDrum.Gameplay.MinhSpriteAnimation>();
                PrefabUtility.SaveAsPrefabAsset(player, prefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(player); }
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }

        static void ImportFrames()
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(Sheet);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 70;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
            var factories = new SpriteDataProviderFactories(); factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var old = provider.GetSpriteRects().ToDictionary(s => s.name);
            // Hand-audited transparent row boundaries; attack poses extend above their heads.
            int[] top = { 0, 181, 350, 523, 684, 840, 1003, 1165, 1328, 1536 };
            int[] baseline = { 173, 350, 519, 681, 838, 994, 1158, 1327, 1508 };
            var rects = new System.Collections.Generic.List<SpriteRect>();
            for (int row = 0; row < 9; row++) for (int f = 0; f < 6; f++)
            {
                string name = Actions[row % 3] + "_" + Directions[row / 3] + "_" + f;
                int left = Mathf.RoundToInt(f * 1024f / 6), right = Mathf.RoundToInt((f + 1) * 1024f / 6);
                int height = top[row + 1] - top[row];
                rects.Add(new SpriteRect { name = name, spriteID = old.ContainsKey(name) ? old[name].spriteID : GUID.Generate(),
                    rect = new Rect(left, 1536 - top[row + 1], right - left, height), alignment = SpriteAlignment.Custom,
                    pivot = new Vector2(.5f, (top[row + 1] - baseline[row]) / (float)height) });
            }
            provider.SetSpriteRects(rects.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(s => new SpriteNameFileIdPair(s.name, s.spriteID)));
            provider.Apply(); importer.SaveAndReimport();
        }
    }
}
