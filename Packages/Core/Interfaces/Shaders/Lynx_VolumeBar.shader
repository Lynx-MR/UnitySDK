Shader "Lynx/UI/Volume Bar"
{
    Properties
    {
        _MainTex ("AudioIco", 2D) = "white" {}
        _volume("Volume", Range(0.0,1.0)) = 1.0
        _ratio("ImageRatio (x/y)", float) = 1.0
    }
    SubShader
    {
       Tags {
            "Queue" = "AlphaTest"
            "RenderType" = "TransparentCutout"
        }
        LOD 100
        Cull Off
        ZWrite On
        AlphaTest Greater[_Cutoff]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag alphatest:_Cutoff
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _volume;
            float _ratio;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }


            float sdBox(in float2 p, in float2 b)
            {
                float2 d = abs(p) - b;
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
            }


            float VolumeBar(in float2 uv, in float ratio)
            {
                uv.x *= ratio;
                float2 c = uv - float2(ratio / 2.0, 0.5);
                float r = sdBox(c, float2(ratio / 2.0 - .5, 0.0)) - 0.47;

                r = abs(r) - 0.03;
                r = min(r, sdBox(c - float2(lerp(-(ratio / 2.0 - .5), 0.0, _volume), .0), float2((ratio / 2.0 - .5) * _volume, .0)) - .4);

                return smoothstep(0.001, 0.0, r);
            }



            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);
                float2 icoUV = i.uv * float2(_ratio,1.0);

                fixed4 col = VolumeBar(i.uv, _ratio).xxxx;
                

                col = lerp(col, float4(0.0,0.0,0.0,0.0), tex2D(_MainTex, icoUV).a);
                //col = tex2D(_MainTex, i.uv);

                clip(col.a - 0.3);
                return col;
            }
            ENDCG
        }
    }
}
