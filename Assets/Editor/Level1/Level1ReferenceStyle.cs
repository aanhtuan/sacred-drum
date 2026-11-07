using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace SacredDrum.EditorTools
{
    // Explicit, editor-only revision of the Level 1 blockout to the owner's visual reference.
    public static class Level1ReferenceStyle
    {
        const string Root="Assets/Art/Environment/CanhDong";
        const string Art=Root+"/ReferenceArt/CanhDong_ReferenceProps.png";
        static readonly string[] Names={"Tree","ThatchedHut","Bamboo","Haystack","Rocks","OilLamp","FestivalBanner","BronzeAltar"};
        static Tilemap Map(string name)=>GameObject.Find("Grid/"+name).GetComponent<Tilemap>();
        static Tile Tile(string name)=>AssetDatabase.LoadAssetAtPath<Tile>(Root+"/ReferenceTiles/"+name+".asset");
        static Sprite Sprite(string name)=>AssetDatabase.LoadAllAssetsAtPath(Art).OfType<Sprite>().Single(s=>s.name==name);
        static Color C(string hex){ColorUtility.TryParseHtmlString("#"+hex,out var c);return c;}
        static GameObject Group(string name,Transform parent){var g=new GameObject(name);g.transform.SetParent(parent,false);return g;}

        [MenuItem("SacredDrum/Level 1/4 - Import reference-inspired art")]
        public static void ImportArt()
        {
            AssetDatabase.Refresh();
            var importer=(TextureImporter)AssetImporter.GetAtPath(Art);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit=64;importer.filterMode=FilterMode.Point;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;
            importer.alphaIsTransparency=true;importer.maxTextureSize=2048;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            var source=new Texture2D(2,2,TextureFormat.RGBA32,false);source.LoadImage(File.ReadAllBytes(Art));
            var factory=new SpriteDataProviderFactories();factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            var old=provider.GetSpriteRects().ToDictionary(r=>r.name);var rects=new List<SpriteRect>();
            // Sprite rects trim empty padding inside each specified atlas cell, without editing pixels.
            for(int i=0;i<8;i++) {
                int x0=i%4*384,y0=(1-i/4)*512,minX=384,minY=512,maxX=0,maxY=0;
                for(int y=0;y<512;y++)for(int x=0;x<384;x++)if(source.GetPixel(x0+x,y0+y).a>.39f){minX=Math.Min(minX,x);minY=Math.Min(minY,y);maxX=Math.Max(maxX,x);maxY=Math.Max(maxY,y);}
                minX=Math.Max(0,minX-8);minY=Math.Max(0,minY-8);maxX=Math.Min(383,maxX+8);maxY=Math.Min(511,maxY+8);
                rects.Add(new SpriteRect{name=Names[i],rect=new Rect(x0+minX,y0+minY,maxX-minX+1,maxY-minY+1),alignment=SpriteAlignment.BottomCenter,pivot=new Vector2(.5f,0),spriteID=old.TryGetValue(Names[i],out var r)?r.spriteID:GUID.Generate()});
            }
            UnityEngine.Object.DestroyImmediate(source);
            provider.SetSpriteRects(rects.ToArray());provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
            provider.Apply();importer.SaveAndReimport();
            foreach(string n in new[]{"Grass","GrassDetail","Soil","Rice","Water","BankWest","BankEast","Boundary","Fence","Terrace","Flowers","Tuft","Stone"})MakeTerrain(n,-1);
            for(int mask=0;mask<16;mask++)MakeTerrain("Path"+mask,mask);
            CreatePalette();
            AssetDatabase.SaveAssets();
        }

        public static void CreatePalette()
        {
            string path=Root+"/Palettes/Level1_Reference_Palette.prefab";
            if(File.Exists(path))return;
            GridPaletteUtility.CreateNewPalette(Root+"/Palettes","Level1_Reference_Palette",GridLayout.CellLayout.Rectangle,GridPalette.CellSizing.Manual,Vector3.one,GridLayout.CellSwizzle.XYZ);
            var palette=PrefabUtility.LoadPrefabContents(path);
            var original=palette.GetComponentInChildren<Tilemap>();if(original!=null)UnityEngine.Object.DestroyImmediate(original.gameObject);
            var groups=new Dictionary<string,string[]>{
                {"Ground",new[]{"Grass","GrassDetail","Soil"}},
                {"Paths",Enumerable.Range(0,16).Select(i=>"Path"+i).ToArray()},
                {"Water",new[]{"Water","BankWest","BankEast"}},
                {"Vegetation",new[]{"Rice","Tuft","Flowers"}},
                {"Structures",new[]{"Fence","Terrace","Stone","Boundary"}},
                {"Decorations",new[]{"Flowers","Tuft"}}
            };
            int row=0;
            foreach(var group in groups) {
                var go=Group(group.Key,palette.transform);var map=go.AddComponent<Tilemap>();go.AddComponent<TilemapRenderer>();
                for(int i=0;i<group.Value.Length;i++)map.SetTile(new Vector3Int(i%8,-row-i/8,0),Tile(group.Value[i]));
                row+=group.Value.Length>8?4:3;map.CompressBounds();
            }
            PrefabUtility.SaveAsPrefabAsset(palette,path);PrefabUtility.UnloadPrefabContents(palette);
        }

        static void MakeTerrain(string name,int mask)
        {
            string path=Root+"/ReferenceTiles/"+name+".png";
            if(!File.Exists(path)) {
                var texture=new Texture2D(16,16,TextureFormat.RGBA32,false);var pixels=new Color[256];
                Action<int,int,string> pixel=(x,y,c)=>{if(x>=0&&x<16&&y>=0&&y<16)pixels[y*16+x]=C(c);};
                Action<int,int,int,int,string> box=(x,y,w,h,c)=>{for(int yy=y;yy<y+h;yy++)for(int xx=x;xx<x+w;xx++)pixel(xx,yy,c);};
                string baseColor=name=="Water"?"278e98":name=="Soil"||name=="Rice"?"8c8c38":name=="Boundary"?"487628":name=="Stone"?"8e9584":"77a82d";
                bool transparent=name=="Fence"||name=="Flowers"||name=="Tuft";
                if(!transparent)box(0,0,16,16,baseColor);
                if(name=="Grass"||name=="GrassDetail") {
                    for(int y=0;y<16;y++)for(int x=0;x<16;x++)if((x*17+y*37)%61==0)pixel(x,y,"7bab31");
                    if(name=="GrassDetail"){box(3,5,2,1,"669529");pixel(3,6,"8bbd35");box(11,11,2,1,"69972a");}
                }
                if(mask>=0) {
                    for(int y=0;y<16;y++)for(int x=0;x<16;x++) {
                        int d=16;
                        if((mask&1)==0)d=Math.Min(d,x);if((mask&2)==0)d=Math.Min(d,15-x);
                        if((mask&4)==0)d=Math.Min(d,y);if((mask&8)==0)d=Math.Min(d,15-y);
                        pixel(x,y,d==0?"77a82d":d==1?"9db145":((x*19+y*31)%47==0?"e7c76d":"cfaf5a"));
                    }
                }
                if(name=="Rice") {
                    box(0,0,16,16,"9c9a3c");
                    for(int x=1;x<16;x+=3)for(int y=0;y<16;y+=5) {
                        box(x,y,1,4,"a9b92f");pixel(x+1,y+2,"d7bd3d");pixel(x,y+4,"f5d74a");pixel(x-1,y+3,"e9c83d");pixel(x+1,y+3,"ffdf57");
                    }
                }
                if(name=="Water"){box(1,4,4,1,"46b2b2");box(10,12,5,1,"47afb2");box(4,3,2,1,"24848f");box(0,0,16,1,"298994");}
                if(name=="BankWest"||name=="BankEast") {
                    int x=name=="BankWest"?10:0;box(x,0,6,16,"8c6940");box(x+1,1,4,6,"a77b44");box(x+1,9,4,5,"a77b44");
                    box(name=="BankWest"?15:0,0,1,16,"216e76");
                }
                if(name=="Terrace") {
                    box(0,0,16,16,"634c34");box(1,1,6,7,"987044");box(9,1,6,7,"a07848");box(1,9,10,4,"a57a43");box(12,9,3,4,"947044");box(0,13,16,3,"79a72d");box(0,15,16,1,"a5c33c");
                }
                if(name=="Fence") {
                    box(0,5,16,2,"af8a43");box(0,11,16,2,"cca859");box(2,2,2,13,"916b35");box(3,3,1,12,"e4bb65");box(11,2,2,13,"916b35");box(12,3,1,12,"e4bb65");
                }
                if(name=="Boundary"){box(1,1,5,7,"527e29");box(9,6,5,8,"507b28");}
                if(name=="Flowers"){box(3,2,1,5,"3d7428");box(10,2,1,7,"3d7428");box(2,6,3,2,"f6eec3");box(9,8,3,2,"eec48e");pixel(3,7,"f6d650");pixel(10,9,"fff4bc");}
                if(name=="Tuft"){for(int x=3;x<13;x+=3){box(x,3,1,4,"548a26");pixel(x+1,7,"9bc237");}}
                if(name=="Stone"){box(1,1,6,6,"a3a58b");box(9,1,6,6,"a0a78e");box(1,9,14,6,"adb098");box(2,13,12,1,"c1bf9c");}
                texture.SetPixels(pixels);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
            }
            AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=16;importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.SaveAndReimport();
            string tilePath=Root+"/ReferenceTiles/"+name+".asset";var tile=AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
            if(tile==null){tile=ScriptableObject.CreateInstance<Tile>();AssetDatabase.CreateAsset(tile,tilePath);}
            tile.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);tile.colliderType=UnityEngine.Tilemaps.Tile.ColliderType.Grid;EditorUtility.SetDirty(tile);
        }

        [MenuItem("SacredDrum/Level 1/5 - Apply owner reference to current Level 1")]
        public static void Apply()
        {
            var scene=SceneManager.GetActiveScene();
            if(scene.path!=Level1MapBuilder.ScenePath||EditorApplication.isPlaying)throw new InvalidOperationException("Open Level1_CanhDong in Edit Mode.");
            if(GameObject.Find("Environment/ReferenceDressing")!=null)throw new InvalidOperationException("Already dressed; refusing duplicate replacement.");
            var ground=Map("Ground");var paths=Map("Paths");var decor=Map("GroundDecoration");var water=Map("Water");var obstacles=Map("Obstacles");
            ground.ClearAllTiles();paths.ClearAllTiles();decor.ClearAllTiles();
            for(int y=0;y<56;y++)for(int x=0;x<88;x++)ground.SetTile(new Vector3Int(x,y,0),Tile((x*37+y*17)%19==0?"GrassDetail":"Grass"));
            // Curves are authored in world XY; the bridge and all three objective approaches remain.
            var road=new HashSet<Vector3Int>();
            Stroke(road,new[]{new Vector2(7,27.5f),new Vector2(13,29),new Vector2(18,33),new Vector2(23,34),new Vector2(30,32),new Vector2(36,29),new Vector2(40,27.5f)},2.1f);
            Stroke(road,new[]{new Vector2(50,27.5f),new Vector2(56,25),new Vector2(62,27),new Vector2(67,32),new Vector2(74,32),new Vector2(75,37.5f)},2.1f);
            Stroke(road,new[]{new Vector2(23,34),new Vector2(23.5f,39.5f)},1.6f);
            Stroke(road,new[]{new Vector2(30,32),new Vector2(32,26),new Vector2(26,20),new Vector2(28.5f,10.5f)},1.7f);
            Stroke(road,new[]{new Vector2(62,27),new Vector2(62,21),new Vector2(61.5f,15.5f)},1.7f);
            Stroke(road,new[]{new Vector2(18,33),new Vector2(15,39),new Vector2(14,43)},1.3f);
            for(int y=30;y<=35;y++)for(int x=32;x<=39;x++)road.Add(new Vector3Int(x,y,0));
            for(int y=30;y<=35;y++)for(int x=54;x<=62;x++)road.Add(new Vector3Int(x,y,0));
            foreach(var p in road){int mask=0;if(road.Contains(p+Vector3Int.left))mask|=1;if(road.Contains(p+Vector3Int.right))mask|=2;if(road.Contains(p+Vector3Int.down))mask|=4;if(road.Contains(p+Vector3Int.up))mask|=8;paths.SetTile(p,Tile("Path"+mask));}
            // Original water/collision alignment is retained. Only the displayed surface changes.
            foreach(var p in water.cellBounds.allPositionsWithin)if(water.GetTile(p) is Tile t){string n=t.name;water.SetTile(p,Tile(n=="BankWest"?"BankWest":n=="BankEast"?"BankEast":"Water"));}
            var oldFence=Level1AssetSetup.Tile("Fence");var oldBoundary=Level1AssetSetup.Tile("Boundary");
            foreach(var p in obstacles.cellBounds.allPositionsWithin) {
                var old=obstacles.GetTile(p);
                if(old==oldBoundary)obstacles.SetTile(p,Tile("Boundary"));
                if(old==oldFence)obstacles.SetTile(p,p.y>=37?Tile("Fence"):null);
            }
            Field(ground,decor,obstacles,road,8,12,22,23);
            Field(ground,decor,obstacles,road,8,36,18,42);
            Field(ground,decor,obstacles,road,27,38,38,44);
            Field(ground,decor,obstacles,road,52,8,57,21);
            Field(ground,decor,obstacles,road,67,9,81,24);
            Field(ground,decor,obstacles,road,51,41,62,48);
            // Less paving leaves the shrine as the focal point rather than an empty stone rectangle.
            for(int y=38;y<=50;y++)for(int x=68;x<=82;x++)if((x>=73&&x<=76&&y<=45)||(x>=71&&x<=79&&y>=42&&y<=48))paths.SetTile(new Vector3Int(x,y,0),Tile("Stone"));
            for(int y=3;y<53;y++)for(int x=3;x<85;x++) {
                var p=new Vector3Int(x,y,0);if(paths.HasTile(p)||water.HasTile(p)||decor.HasTile(p)||ground.GetTile(p)==Tile("Soil")||obstacles.HasTile(p))continue;
                if((x*29+y*53)%113==0)decor.SetTile(p,Tile("Tuft"));
                if((x*37+y*17)%227==0)decor.SetTile(p,Tile("Flowers"));
            }
            // Existing blockout props are preserved but disabled together with their colliders.
            foreach(string name in new[]{"Trees","Rocks","Buildings"})GameObject.Find("Environment/"+name).SetActive(false);
            var dressing=Group("ReferenceDressing",GameObject.Find("Environment").transform);
            var trees=Group("Trees",dressing.transform);var bamboo=Group("Bamboo",dressing.transform);var houses=Group("HutsAndHay",dressing.transform);var props=Group("Landmarks",dressing.transform);
            var treePoints=new[]{new Vector2(5,46),new Vector2(11,49),new Vector2(24,47),new Vector2(33,49),new Vector2(38,45),new Vector2(49,48),new Vector2(62,49),new Vector2(84,47),new Vector2(85,37),new Vector2(4,6),new Vector2(13,4),new Vector2(22,4),new Vector2(35,5),new Vector2(39,15),new Vector2(49,4),new Vector2(61,4),new Vector2(76,4),new Vector2(83,6),new Vector2(38,36),new Vector2(52,32),new Vector2(4,24),new Vector2(82,28)};
            for(int i=0;i<treePoints.Length;i++) {var tree=Prop(trees.transform,"CanopyTree_"+(i+1),"Tree",treePoints[i],1);var collider=tree.AddComponent<CircleCollider2D>();collider.radius=.6f;collider.offset=new Vector2(0,.6f);}
            for(int i=0;i<6;i++)Prop(bamboo.transform,"Bamboo_"+(i+1),"Bamboo",new Vector2(3+i*2,45+(i%2)*2),.8f);
            for(int i=0;i<4;i++)Prop(bamboo.transform,"EastBamboo_"+(i+1),"Bamboo",new Vector2(80+i*1.7f,49),.75f);
            var hut=Prop(houses.transform,"ThatchedHut","ThatchedHut",new Vector2(14,43),1.3f);var hc=hut.AddComponent<BoxCollider2D>();hc.size=new Vector2(5.2f,1.4f);hc.offset=new Vector2(0,.8f);
            Prop(houses.transform,"RiceHaystack","Haystack",new Vector2(21,44),.75f);
            Prop(houses.transform,"RiceHaystackSmall","Haystack",new Vector2(25,45),.5f);
            var rockPoints=new[]{new Vector2(31,22),new Vector2(35,37),new Vector2(57,38),new Vector2(80,32),new Vector2(65,8),new Vector2(7,32),new Vector2(42,45)};
            for(int i=0;i<rockPoints.Length;i++){var rock=Prop(props.transform,"RockCluster_"+(i+1),"Rocks",rockPoints[i],.5f);var c=rock.AddComponent<BoxCollider2D>();c.size=new Vector2(3.6f,1.4f);c.offset=new Vector2(0,.8f);}
            foreach(var p in new[]{new Vector2(39,32),new Vector2(52,31),new Vector2(71,38),new Vector2(79,38)})Prop(props.transform,"FestivalBanner","FestivalBanner",p,.55f);
            for(int i=1;i<=3;i++) {
                var lamp=GameObject.Find("Gameplay/Lamp_0"+i);lamp.GetComponent<SpriteRenderer>().enabled=false;
                var visual=Prop(lamp.transform,"OilLampVisual","OilLamp",Vector2.zero,.4f);visual.transform.localPosition=new Vector3(0,-.25f,0);visual.GetComponent<SpriteRenderer>().sortingOrder=55;
                var p=lamp.transform.position;for(int y=(int)p.y-1;y<=(int)p.y+1;y++)for(int x=(int)p.x-1;x<=(int)p.x+1;x++)paths.SetTile(new Vector3Int(x,y,0),Tile("Stone"));
            }
            var fragment=GameObject.Find("Gameplay/DrumFragment_01");fragment.GetComponent<SpriteRenderer>().enabled=false;
            var altar=Prop(fragment.transform,"BronzeAltarVisual","BronzeAltar",Vector2.zero,1);altar.transform.localPosition=new Vector3(0,-1.5f,0);altar.GetComponent<SpriteRenderer>().sortingOrder=55;
            var altarCollider=altar.AddComponent<BoxCollider2D>();altarCollider.size=new Vector2(4.5f,.6f);altarCollider.offset=new Vector2(0,.5f);
            var bridgeRails=GameObject.Find("Environment/Bridges/WoodenBridge/BridgeRails").GetComponent<Tilemap>();foreach(var p in bridgeRails.cellBounds.allPositionsWithin)if(bridgeRails.HasTile(p))bridgeRails.SetTile(p,Tile("Fence"));
            foreach(var map in UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None)) {
                map.CompressBounds();var c=map.GetComponent<TilemapCollider2D>();if(c!=null)c.ProcessTilemapChanges();var composite=map.GetComponent<CompositeCollider2D>();if(composite!=null)composite.GenerateGeometry();
            }
            Camera.main.backgroundColor=C("193d29");
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);Physics2D.SyncTransforms();
            Level1MapBuilder.FrameSceneView();AssetDatabase.SaveAssets();
        }

        static void Stroke(HashSet<Vector3Int> road,Vector2[] points,float radius)
        {
            for(int y=2;y<54;y++)for(int x=2;x<86;x++) {
                var p=new Vector2(x+.5f,y+.5f);
                for(int i=1;i<points.Length;i++) {
                    var a=points[i-1];var d=points[i]-a;float t=Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude);
                    if(Vector2.Distance(p,a+d*t)<=radius){road.Add(new Vector3Int(x,y,0));break;}
                }
            }
        }
        static void Field(Tilemap ground,Tilemap decor,Tilemap obstacles,HashSet<Vector3Int> road,int x0,int y0,int x1,int y1)
        {
            for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++) {
                var p=new Vector3Int(x,y,0);bool clipped=(x==x0||x==x1)&&(y==y0||y==y1);
                if(clipped||road.Contains(p))continue;
                ground.SetTile(p,Tile("Soil"));decor.SetTile(p,Tile("Rice"));
            }
            for(int x=x0+1;x<x1;x++){var p=new Vector3Int(x,y0-1,0);if(!road.Contains(p))obstacles.SetTile(p,Tile("Terrace"));}
        }
        static GameObject Prop(Transform parent,string name,string sprite,Vector2 position,float scale)
        {
            var g=Group(name,parent);g.transform.position=position;g.transform.localScale=Vector3.one*scale;var sr=g.AddComponent<SpriteRenderer>();sr.sprite=Sprite(sprite);sr.sortingOrder=50;return g;
        }
    }
}
