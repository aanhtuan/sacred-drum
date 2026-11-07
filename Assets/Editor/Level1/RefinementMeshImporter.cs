using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using UnityEngine.Rendering;
using Unity.Collections;

namespace SacredDrum.EditorTools
{
    [Serializable] public sealed class RefinementMask { public string key;public string source;public ushort[] rows; }

    // Bake native vertex/index data into subassets, without modifying source texture pixels.
    [ScriptedImporter(1,"sdmesh")]
    public sealed class RefinementMeshImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var entries=JsonConvert.DeserializeObject<RefinementMask[]>(System.IO.File.ReadAllText(ctx.assetPath));
            var library=new TextAsset("Baked Tile and Sprite subassets; source textures preserved.");ctx.AddObjectToAsset("library",library);ctx.SetMainObject(library);
            foreach(var entry in entries) {
                var source=AssetDatabase.LoadAssetAtPath<Tile>(entry.source).sprite;
                ctx.DependsOnSourceAsset(entry.source);ctx.DependsOnSourceAsset(AssetDatabase.GetAssetPath(source.texture));
                var sprite=Sprite.Create(source.texture,source.rect,new Vector2(.5f,.5f),16,0,SpriteMeshType.Tight);sprite.name=entry.key+"_pixelmesh";
                var vertices=new List<Vector2>();var indices=new List<ushort>();
                for(int y=0;y<16;y++)for(int x=0;x<16;) {
                    if((entry.rows[y]&(1<<x))==0){x++;continue;}int start=x;while(x<16&&(entry.rows[y]&(1<<x))!=0)x++;
                    ushort i=(ushort)vertices.Count;
                    vertices.Add(new Vector2(start/16f-.5f,y/16f-.5f));vertices.Add(new Vector2(x/16f-.5f,y/16f-.5f));
                    vertices.Add(new Vector2(x/16f-.5f,(y+1)/16f-.5f));vertices.Add(new Vector2(start/16f-.5f,(y+1)/16f-.5f));
                    indices.Add(i);indices.Add((ushort)(i+1));indices.Add((ushort)(i+2));indices.Add(i);indices.Add((ushort)(i+2));indices.Add((ushort)(i+3));
                }
                SpriteDataAccessExtensions.SetVertexCount(sprite,vertices.Count);
                using(var positions=new NativeArray<Vector3>(vertices.ConvertAll(v=>new Vector3(v.x,v.y,0)).ToArray(),Allocator.Temp))
                    SpriteDataAccessExtensions.SetVertexAttribute(sprite,VertexAttribute.Position,positions);
                using(var uv=new NativeArray<Vector2>(vertices.ConvertAll(v=>new Vector2((source.rect.x+(v.x+.5f)*16)/source.texture.width,(source.rect.y+(v.y+.5f)*16)/source.texture.height)).ToArray(),Allocator.Temp))
                    SpriteDataAccessExtensions.SetVertexAttribute(sprite,VertexAttribute.TexCoord0,uv);
                using(var triangles=new NativeArray<ushort>(indices.ToArray(),Allocator.Temp))SpriteDataAccessExtensions.SetIndices(sprite,triangles);
                if(sprite.vertices.Length!=vertices.Count)throw new InvalidOperationException("Pixel mesh geometry was not applied: "+entry.key);
                var tile=ScriptableObject.CreateInstance<Tile>();tile.name=entry.key;tile.sprite=sprite;tile.colliderType=Tile.ColliderType.None;
                ctx.AddObjectToAsset(entry.key+"_sprite",sprite);ctx.AddObjectToAsset(entry.key,tile);
            }
        }
    }
}
