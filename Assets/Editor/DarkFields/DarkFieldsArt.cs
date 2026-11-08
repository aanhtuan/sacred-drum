using System.IO;
using UnityEditor;
using UnityEngine;

namespace SacredDrum.EditorTools
{
    // Original code-authored prototype tiles. Coordinates below use a top-left origin like LDtk.
    public static class DarkFieldsArt
    {
        const string AtlasPath = "Assets/LevelDesign/DarkFields/Tilesets/DarkFields.png";
        static readonly Color[] Pixels = new Color[64 * 64];
        static Color C(string hex) { Color c; ColorUtility.TryParseHtmlString("#" + hex, out c); return c; }
        static void P(int tile, int x, int y, Color color)
        {
            if (x < 0 || y < 0 || x >= 16 || y >= 16) return;
            Pixels[(63 - (tile / 4 * 16 + y)) * 64 + tile % 4 * 16 + x] = color;
        }
        static void Line(int tile, int x0, int y0, int x1, int y1, Color color)
        {
            int steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0));
            for (int i = 0; i <= steps; i++)
            {
                float t = steps == 0 ? 0 : (float)i / steps;
                P(tile, Mathf.RoundToInt(Mathf.Lerp(x0, x1, t)), Mathf.RoundToInt(Mathf.Lerp(y0, y1, t)), color);
            }
        }
        static int Hash(int x, int y, int seed) { return (x * 73 + y * 157 + seed * 59 + x * y * 31) & 255; }
        static void Terrain(int tile, string main, string dark, string light)
        {
            for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++)
            {
                int h = Hash(x, y, tile);
                P(tile, x, y, C(h < 12 ? dark : h > 246 ? light : main));
            }
        }
        [MenuItem("SacredDrum/Dark Fields/Generate Native Prototype Atlas")]
        public static void GenerateAtlas()
        {
            System.Array.Clear(Pixels, 0, Pixels.Length);
            Terrain(0, "444648", "393D40", "515052");
            Terrain(1, "82705B", "6C5C4B", "94806A");
            Terrain(2, "51443E", "413938", "635247");
            Terrain(5, "202D39", "1D2532", "354252");
            Terrain(10, "3D3248", "2D293C", "574059");
            Terrain(11, "60544A", "4B4440", "756353");
            for (int y = 3; y < 16; y += 5) Line(2, 0, y, 15, y, C("362F31"));
            Line(1, 2, 0, 6, 5, C("544C45")); Line(1, 6, 5, 4, 10, C("544C45"));
            Line(1, 4, 10, 7, 15, C("544C45")); Line(1, 6, 5, 12, 3, C("64554B"));
            foreach (int x in new[] { 3, 8, 13 })
            {
                Line(3, x, 13, x - 1, 5, C("9A8353"));
                Line(3, x - 1, 8, x - 3, 6, C("786743"));
                Line(3, x - 1, 9, x + 1, 6, C("BA9A60"));
                P(3, x - 1, 4, C("BCA16B"));
            }
            for (int y = 2; y < 16; y++) for (int x = 0; x < 16; x++)
                if (Hash(x, y, 4) > 44) P(4, x, y, C(Hash(x,y,5) < 90 ? "262631" : "33323B"));
            Line(4, 1, 12, 12, 3, C("68525D")); Line(4, 4, 0, 10, 15, C("4F414E"));
            Line(4, 7, 7, 15, 8, C("68525D")); Line(4, 10, 5, 14, 2, C("85716F"));
            Line(5, 2, 4, 8, 4, C("4D5965")); Line(5, 10, 11, 14, 11, C("374954"));
            foreach (int x in new[] { 7, 8 })
            {
                Line(6, x, 15, x, 4, C("827064")); Line(7, x, 0, x + 1, 14, C("64554F"));
            }
            Line(6, 7, 10, 2, 5, C("827064")); Line(6, 2, 5, 2, 1, C("948075"));
            Line(6, 8, 8, 13, 3, C("827064")); Line(6, 13, 3, 12, 0, C("948075"));
            Line(6, 8, 5, 5, 1, C("948075")); Line(7, 8, 10, 3, 15, C("64554F"));
            Line(7, 9, 10, 13, 15, C("64554F"));
            for (int y = 5; y < 14; y++) for (int x = 2; x < 14; x++)
                if ((x-8)*(x-8)+(y-10)*(y-10)<37) P(8,x,y,C(y<8?"77747A":y<11?"5D5C66":"41434F"));
            Line(8, 8, 6, 7, 11, C("343641"));
            foreach (int x in new[] { 3, 12 }) { Line(9,x,2,x,15,C("776452")); Line(9,x+1,4,x+1,15,C("4B4140")); }
            Line(9, 0, 6, 15, 8, C("8F7961")); Line(9, 0, 10, 15, 12, C("655348"));
            Line(10, 0, 13, 7, 7, C("795E8D")); Line(10, 7, 7, 15, 3, C("795E8D"));
            Line(10, 7, 7, 10, 0, C("5B4D75"));
            for (int y = 1; y < 15; y++) for (int x = 1; x < 15; x++)
                P(12,x,y,C(x<3||y<3?"74717A":x>12||y>12?"343443":"56515E"));
            Line(12,1,7,14,7,C("35313F")); Line(12,7,1,7,6,C("35313F"));
            for (int y=5;y<14;y++) for(int x=5;x<11;x++) P(13,x,y,C(x==5||x==10?"6D5140":y<10?"E9B56B":"B9814C"));
            P(13,7,7,C("FFE1A0")); P(13,8,7,C("FFE1A0")); Line(13,4,4,11,4,C("4E4241"));
            foreach(int x in new[]{4,7,11}) { Line(14,x,15,x-2,7,C("6D6759"));Line(14,x-2,7,x,4,C("95876A")); }
            for(int y=2;y<14;y++) for(int x=2;x<14;x++)
            {
                int r=Mathf.Abs(x-8)+Mathf.Abs(y-8);
                if(r<7)P(15,x,y,C(r>4?"675274":r>2?"A078A3":"D2A8BC"));
            }
            var texture=new Texture2D(64,64,TextureFormat.RGBA32,false);
            texture.SetPixels(Pixels); texture.Apply(); File.WriteAllBytes(AtlasPath,texture.EncodeToPNG());
            Object.DestroyImmediate(texture); AssetDatabase.ImportAsset(AtlasPath);
            var importer=(TextureImporter)AssetImporter.GetAtPath(AtlasPath);
            importer.textureType=TextureImporterType.Sprite; importer.spritePixelsPerUnit=16;
            importer.filterMode=FilterMode.Point; importer.mipmapEnabled=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed; importer.isReadable=true;
            importer.npotScale=TextureImporterNPOTScale.None;
            var platform=importer.GetDefaultPlatformTextureSettings(); platform.format=TextureImporterFormat.RGBA32;
            platform.textureCompression=TextureImporterCompression.Uncompressed;
            importer.SetPlatformTextureSettings(platform); importer.SaveAndReimport();
            Debug.Log("Dark Fields native atlas generated. No existing environment art modified.");
        }
    }
}
