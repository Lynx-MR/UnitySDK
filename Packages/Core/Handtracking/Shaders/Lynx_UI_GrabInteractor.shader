Shader "Lynx/UI/GrabInteractor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    [HDR]
        _Color ("Color", Color) = (1,1,1,1)
        _HoverSize("Hover Size",Float) = 0.1
        _HoverEffectSize("Hover Effect Size",Float) = 0.045
    }
    SubShader
    {
        Tags {
            "Queue"      = "Transparent"
            "RenderType" = "Transparent"
        }
        LOD 100
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag alpha:blend

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 wPos : TEXCOORD1;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 wPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Cutoff;
            float3 LI;
            float3 RI;
            float3 LT;
            float3 RT;

            float _HoverEffectTime;
            float _HoverEffectIntensity = 1.0;


            float _HoverSize;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.wPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)).xyz;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                float mDist = min(min(min(distance(i.wPos, LI), distance(i.wPos, RI)), distance(i.wPos, LT)), distance(i.wPos, RT));
                mDist = clamp((_HoverSize - mDist) + 0.035, 0., 1.) /_HoverSize;


                //Used for Hover effect wich is not active in this version
                /*float he = distance(i.uv, float2(0.5, 0.5)) * 2.0;
                he -= _HoverEffectTime;
                he = clamp(1.0 - he ,0.0,1.0) * _HoverEffectIntensity;*/

                col *= float4(_Color.xyz, col.r * mDist);

                return col;
            }
            ENDCG
        }
    }
}
