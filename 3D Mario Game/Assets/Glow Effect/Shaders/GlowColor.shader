Shader "Glow Effect/Glow Color"
{
	Properties
	{
		_GlowColor ("Glow Color", Color) = (1, 1, 1, 1)
	}
    
    SubShader
	{
        Tags { "RenderType" = "Glow" "Queue" = "Geometry" }
        
        Pass {
        
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			uniform half4 _GlowColor;
			
			half4 frag(v2f_img i) : COLOR
			{
				return _GlowColor;
			}
			
			ENDCG
        } 
    } 
	
	Fallback "Diffuse"
}