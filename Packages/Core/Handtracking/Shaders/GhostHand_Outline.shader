Shader "Lynx/Hand/Outline"
{
    Properties
    {
        _Color ("Base Color", Color) = (0.25,0.25,0.25,1)
        _OutlineSize ("OutlineSize", Range(0,0.05)) = 0.0027
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100
        Cull Front
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha // Alpha blending

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 vCol : COLOR;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 vCol : COLOR;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };
            float4 _Color;
            float _OutlineSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex + v.normal*_OutlineSize);
                o.uv = v.uv;
                o.normal = v.normal;
                o.vCol = v.vCol;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return float4(_Color.xyz, _Color.w * i.vCol.x);
            }
            ENDCG
        }
    }
}
