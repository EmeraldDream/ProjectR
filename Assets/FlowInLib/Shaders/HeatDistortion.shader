// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/HeatDistort"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_IntensityAndScrolling ("Intensity (XY); Scrolling (ZW)", Vector) = (0.1,0.1,1,1)
		[Toggle(MASK)] _Mask ("Use blue channel as mask", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
		Blend One Zero
		Lighting Off
		Fog { Mode Off }
		ZWrite Off
		Cull Back
		LOD 100

		GrabPass { "_GrabTexture" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature MASK
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				half4 texcoord : TEXCOORD0;
				fixed2 screenuv : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _GrabTexture;
			float4 _IntensityAndScrolling;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord.xy = TRANSFORM_TEX(v.texcoord + _Time.gg * _IntensityAndScrolling.zw, _MainTex);
				o.texcoord.zw = v.texcoord;
				float4 screenPos = ComputeGrabScreenPos(o.vertex);
				o.screenuv = screenPos.xy / screenPos.w;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				half2 distort = tex2D(_MainTex, i.texcoord.xy).xy * 2 - 1;
				half2 offset = distort.xy * _IntensityAndScrolling.xy;
			#if MASK
				half mask = tex2D(_MainTex, i.texcoord.zw).b;
				offset *= mask;
			#endif
				fixed4 col = tex2D(_GrabTexture, i.screenuv + offset);
				return col;
			}
			ENDCG
		}
	}
}
