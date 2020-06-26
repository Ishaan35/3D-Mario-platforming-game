// A simple unlit opaque shader which sets the alpha to 0 and the "Opaque" render type to prevent any glow
Shader "Glow Effect/Unlit No Glow"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
	}
    
    SubShader
	{
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        
        Pass {
        
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			uniform sampler2D _MainTex;
			
			half4 frag(v2f_img i) : COLOR
			{
				return half4(tex2D(_MainTex,i.uv).rgb, 0);
			}
			
			ENDCG
        } 
    } 
	
	Fallback "Diffuse"
}