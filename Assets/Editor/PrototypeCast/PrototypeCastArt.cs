using UnityEditor;
using UnityEngine;

namespace SacredDrum.EditorTools
{
    public static class PrototypeCastArt
    {
        [MenuItem("SacredDrum/Prototype Cast/Prepare Sprites and Prefabs")]
        public static void Prepare()
        {
            // Refresh approved art without recreating the original static preview variants.
            MinhAnimationAssets.Build();
            ScarecrowAnimationAssets.Build();
            Debug.Log("Approved Minh and scarecrow animation prefabs refreshed.");
        }
    }
}
