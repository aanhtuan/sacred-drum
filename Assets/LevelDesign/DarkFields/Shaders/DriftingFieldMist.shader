Shader "SacredDrum/DarkFields/DriftingMist"
{
    Properties
    {
        _Color("Mist color",Color)=(0.57,0.61,0.67,1)
        _Opacity("Opacity",Range(0,0.5))=0.24
        _Drift("Drift speed (0 = reduced motion)",Range(0,0.1))=0.025
    }
    SubShader
    {
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float _Opacity;
            float _Drift;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };
            Varyings Vert(Attributes v)
            {
                Varyings o; o.positionCS=TransformObjectToHClip(v.positionOS.xyz); o.uv=v.uv; return o;
            }
            half4 Frag(Varyings i) : SV_Target
            {
                float t=_Time.y*_Drift;
                float2 p=i.uv*float2(48,36);
                float lane=sin(p.y*0.65+sin(p.x*0.14+t)*2.2-t*1.3);
                float wisps=sin(p.x*0.22-p.y*0.37+t)*0.5+0.5;
                float veil=smoothstep(0.25,0.95,lane)*lerp(0.25,1.0,wisps);
                float edges=smoothstep(0.15,0.7,length((i.uv-.5)*float2(1,.8)));
                return half4(_Color.rgb,saturate((veil*.7+edges*.28)*_Opacity));
            }
            ENDHLSL
        }
    }
}
