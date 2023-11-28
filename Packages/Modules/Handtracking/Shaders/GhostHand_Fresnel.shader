Shader "Lynx/Hand/Fresnel"
{
    Properties
    {
        _Color ("Base Color", Color) = (0.45,0.45,0.45,0.75)
        _FresnelPower ("Fresnel Power", Range(0,5)) = 1.48
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100
        ZWrite On
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha // Alpha blending

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            float Unity_FresnelEffect_float(float3 nrm, float3 ViewDir, float Power)
            {
				return pow((1.0 - saturate(dot(normalize(nrm), normalize(ViewDir)))), Power);
			}


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
                float3 worldPos : TEXCOORD1;
                float3 normal : NORMAL;
                float3 objPos : TEXCOORD2;
                float4 vCol : COLOR;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float4 _Color;
            float _FresnelPower;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.objPos = v.vertex;
                o.uv = v.uv;
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                o.normal = worldNormal;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.vCol = v.vCol;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                half3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));

                //get Fresnel value
                float fnl = Unity_FresnelEffect_float(i.normal,worldViewDir,_FresnelPower);

                //get vertex color gradient for wrist alpha
                float wrist = i.vCol.x;

                // apply fog
                float3 pos = i.objPos;
                
                UNITY_APPLY_FOG(i.fogCoord, col);

                //return(i.vCol);
                //return fresnel + validation mix with Texture alpha
                return float4(_Color.xyz, _Color.w*(1-fnl)*wrist);
            }
            ENDCG
        }
    }
}
