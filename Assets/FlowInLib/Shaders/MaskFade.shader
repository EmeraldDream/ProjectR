// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "FlowInLib/Unlit/MaskFade"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MaskTex("Mask Texture", 2D) = "white" {}
		_Gray("Minimun Gray", Range(0, 1)) = 0
		_AnimX("Anim X", Range(0, 1)) = 0
		_AnimY("Anim Y", Range(0, 1)) =0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 100
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 uv_mask : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _MaskTex;
			fixed _Gray;
			fixed _AnimX;
			fixed _AnimY;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv_mask = v.uv + fixed2(_AnimX, _AnimY);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed gray = tex2D(_MaskTex, i.uv_mask);
				if (gray < _Gray)
					discard;
				
				return tex2D(_MainTex, i.uv);
			}
			ENDCG
		}
	}
}
