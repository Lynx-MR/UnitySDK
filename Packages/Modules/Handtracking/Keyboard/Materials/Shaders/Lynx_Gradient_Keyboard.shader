Shader "Lynx/UI/Keyboard"
{
	Properties
	{
		_MainTex("Sprite Texture", 2D) = "white" {}
		_TopLeftColor("Gradient Top Left", Color) = (1,1,1,1)
		_TopRightColor("Gradient Top Right", Color) = (1,1,1,1)
		_BottomLeftColor("Gradient Bottom Left", Color) = (1,1,1,1)
		_BottomRightColor("Gradient Bottom Right", Color) = (1,1,1,1)

		_EmissionColor("EmissiveColor", Color) = (0,0,0)
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"RenderType" = "Transparent"
			}

			Cull Off
			Lighting Off
			ZWrite On
			Blend SrcAlpha OneMinusSrcAlpha

			Pass
			{
				Name "Default"
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0

				#include "UnityCG.cginc"
				#include "UnityUI.cginc"

				#pragma multi_compile __ UNITY_UI_CLIP_RECT
				#pragma multi_compile __ UNITY_UI_ALPHACLIP

				struct appdata_t
				{
					float4 vertex   : POSITION;
					float2 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					float2 texcoord  : TEXCOORD0;
					float4 screenPos   : TEXCOORD1;
					UNITY_VERTEX_OUTPUT_STEREO
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				fixed4 _TopLeftColor;
				fixed4 _TopRightColor;
				fixed4 _BottomLeftColor;
				fixed4 _BottomRightColor;

				fixed4 _EmissionColor;

				v2f vert(appdata_t v)
				{
					v2f OUT;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
					OUT.vertex = UnityObjectToClipPos(v.vertex);
					OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					OUT.screenPos = ComputeScreenPos(OUT.vertex);
					return OUT;
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					float2 screenPosition = (IN.screenPos.xy / IN.screenPos.w);

					half4 leftY = lerp(_BottomLeftColor, _TopLeftColor, screenPosition.y);
					half4 rightY = lerp(_BottomRightColor, _TopRightColor, screenPosition.y);
					half4 color = tex2D(_MainTex, IN.texcoord) * lerp(leftY, rightY, screenPosition.x);


					return color + _EmissionColor;
				}

			
			ENDCG
			}
		}
}