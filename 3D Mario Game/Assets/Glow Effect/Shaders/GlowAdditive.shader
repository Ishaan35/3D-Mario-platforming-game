// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// A simple unlit additive shader. _Glow* properties are not used in this shader but are used by the replacement shader.
Shader "Glow Effect/Glow Additive"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_GlowTex ("Glow Texture", 2D) = "white" {}
		_GlowColor ("Glow Color", Color) = (1, 1, 1, 1)
		_GlowColorMult ("Glow Color Multiplier", Color) = (1, 1, 1, 1)
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="GlowTransparent" }
		Blend SrcAlpha One
		Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
		
        Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float4 _GlowColor;
			uniform float4 _GlowColorMult;

			struct appdata_color
			{
				float4 vertex : POSITION;
				half4 texcoord : TEXCOORD0;
				half4 color : COLOR;
			};

			struct v2f {
				half4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				half4 color : COLOR;
			};	
			
			v2f vert (appdata_color v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
		       	o.uv = TRANSFORM_TEX(v.texcoord, _MainTex).xy;
		       	o.color = v.color;				
				return o;
			}
				
			half4 frag(v2f i) : COLOR
			{
				return tex2D(_MainTex,i.uv) * i.color;
			}
			
			ENDCG
		}
	}
	
	Fallback "Diffuse"
	CustomEditor "GlowMaterialInspector"
}