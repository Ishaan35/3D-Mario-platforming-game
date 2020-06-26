// An unlit alpha belnded shader. The alpha channel is grabbed from the red channel of the glow texture - 
// this will be used to determine how much glow to apply
Shader "Glow Effect/Glow using Alpha - Alpha Blend"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "clear" {}
		_GlowMask ("Glow Mask Texture", 2D) = "clear" {}
		_GlowColorMult ("Glow Color Multiplier", Color) = (1, 1, 1, 1)
	}
    
    SubShader
	{
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha     // Alpha blending
        
        Pass {
        
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			uniform sampler2D _MainTex;
			uniform sampler2D _GlowMask;
			uniform float4 _GlowColorMult;
			
			half4 frag(v2f_img i) : COLOR
			{
				return half4(tex2D(_MainTex, i.uv).rgb, tex2D(_GlowMask, i.uv).r) * _GlowColorMult;
			}
			
			ENDCG
        } 
    } 
	
	Fallback "Diffuse"
}