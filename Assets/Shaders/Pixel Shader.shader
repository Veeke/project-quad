Shader "Custom/Pixel Art Shader"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalRenderPipeline"
            }
        LOD 100
        HLSLINCLUDE 

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        ENDHLSL

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _BaseMap;
            float4 _BaseMap_TexelSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 boxSize = clamp(fwidth(i.uv) * _BaseMap_TexelSize.zw, 1e-5, 1);
                float2 tx = i.uv * _BaseMap_TexelSize.zw - 0.5 * boxSize;
                float2 txOffset = smoothstep(1 - boxSize, 1, frac(tx));
                float2 uv = (floor(tx) + 0.5 + txOffset) * _BaseMap_TexelSize.xy;
                return tex2Dgrad(_BaseMap, uv, ddx(i.uv), ddy(i.uv));
            }
            ENDHLSL
        }
    }
}
