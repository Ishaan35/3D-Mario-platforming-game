// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Glow Effect/Glow Replace" {
    SubShader
	{
        Tags { "RenderType" = "Glow" }
		Fog { Mode Off }
        Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile GLOWEFFECT_USE_MAINTEX GLOWEFFECT_USE_MAINTEX_OFF
			#pragma multi_compile GLOWEFFECT_USE_GLOWTEX GLOWEFFECT_USE_GLOWTEX_OFF
			#pragma multi_compile GLOWEFFECT_USE_GLOWCOLOR GLOWEFFECT_USE_GLOWCOLOR_OFF
			#pragma multi_compile GLOWEFFECT_USE_VERTEXCOLOR GLOWEFFECT_USE_VERTEXCOLOR_OFF
			#pragma multi_compile GLOWEFFECT_MULTIPLY_COLOR GLOWEFFECT_MULTIPLY_COLOR_OFF
			#include "UnityCG.cginc"
			
			#if GLOWEFFECT_USE_MAINTEX
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			#endif
			#if GLOWEFFECT_USE_GLOWTEX
			uniform sampler2D _GlowTex;
			uniform float4 _GlowTex_ST;
			#endif
			#if GLOWEFFECT_USE_GLOWCOLOR
			uniform float4 _GlowColor;
			#endif
			#if GLOWEFFECT_MULTIPLY_COLOR
			uniform float4 _GlowColorMult;
			#endif

			struct appdata_glow
			{
				float4 vertex : POSITION;
				half4 texcoord : TEXCOORD0;
				#if GLOWEFFECT_USE_VERTEXCOLOR
				half4 color : COLOR;
				#endif
			};

			struct v2f {
				half4 pos : SV_POSITION;
				#if GLOWEFFECT_USE_MAINTEX
				half2 uv : TEXCOORD0;
				#endif
				#if GLOWEFFECT_USE_GLOWTEX
				half2 uv1 : TEXCOORD1;
				#endif
				#if GLOWEFFECT_USE_VERTEXCOLOR
				half4 color : COLOR;
				#endif
			};	
			
			v2f vert (appdata_glow v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
				#if GLOWEFFECT_USE_MAINTEX
		       	o.uv = TRANSFORM_TEX(v.texcoord, _MainTex).xy;
				#endif
				#if GLOWEFFECT_USE_GLOWTEX
		       	o.uv1 = TRANSFORM_TEX(v.texcoord, _GlowTex).xy;
				#endif
				#if GLOWEFFECT_USE_VERTEXCOLOR
		       	o.color = v.color;
				#endif
				
				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				half4 glow = half4(0,0,0,0);

				#if GLOWEFFECT_USE_MAINTEX
				glow += tex2D(_MainTex,i.uv);
				#endif

				#if GLOWEFFECT_USE_GLOWTEX
				glow += tex2D(_GlowTex,i.uv1);
				#endif

				#if GLOWEFFECT_USE_GLOWCOLOR
				glow += _GlowColor;
				#endif

				#if GLOWEFFECT_USE_VERTEXCOLOR
				glow += i.color;
				#endif

				#if GLOWEFFECT_MULTIPLY_COLOR
				glow *= _GlowColorMult;
				#endif

				return glow;
			}
			ENDCG
        } 
    }
    
    SubShader
	{
        Tags { "RenderType" = "GlowTransparent" }
        ZWrite Off
		Fog { Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile GLOWEFFECT_USE_MAINTEX GLOWEFFECT_USE_MAINTEX_OFF
			#pragma multi_compile GLOWEFFECT_USE_GLOWTEX GLOWEFFECT_USE_GLOWTEX_OFF
			#pragma multi_compile GLOWEFFECT_USE_GLOWCOLOR GLOWEFFECT_USE_GLOWCOLOR_OFF
			#pragma multi_compile GLOWEFFECT_USE_VERTEXCOLOR GLOWEFFECT_USE_VERTEXCOLOR_OFF
			#pragma multi_compile GLOWEFFECT_MULTIPLY_COLOR GLOWEFFECT_MULTIPLY_COLOR_OFF
			#include "UnityCG.cginc"
			
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			#if GLOWEFFECT_USE_GLOWTEX
			uniform sampler2D _GlowTex;
			uniform float4 _GlowTex_ST;
			#endif
			#if GLOWEFFECT_USE_GLOWCOLOR
			uniform float4 _GlowColor;
			#endif
			#if GLOWEFFECT_MULTIPLY_COLOR
			uniform float4 _GlowColorMult;
			#endif

			struct appdata_glow
			{
				float4 vertex : POSITION;
				half4 texcoord : TEXCOORD0;
				#if GLOWEFFECT_USE_VERTEXCOLOR
				half4 color : COLOR;
				#endif
			};

			struct v2f {
				half4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				#if GLOWEFFECT_USE_GLOWTEX
				half2 uv1 : TEXCOORD1;
				#endif
				#if GLOWEFFECT_USE_VERTEXCOLOR
				half4 color : COLOR;
				#endif
			};	
			
			v2f vert (appdata_glow v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
		       	o.uv = TRANSFORM_TEX(v.texcoord, _MainTex).xy;
				#if GLOWEFFECT_USE_GLOWTEX
		       	o.uv1 = TRANSFORM_TEX(v.texcoord, _GlowTex).xy;
				#endif
				#if GLOWEFFECT_USE_VERTEXCOLOR
		       	o.color = v.color;
				#endif
				
				return o;
			}
				
			half4 frag(v2f i) : COLOR
			{
				half4 glow = half4(0,0,0,0);
				half4 mainTex = tex2D(_MainTex,i.uv);

				#if GLOWEFFECT_USE_MAINTEX
				glow += mainTex;
				#endif

				#if GLOWEFFECT_USE_GLOWTEX
				glow += mainTex.a * tex2D(_GlowTex,i.uv1);
				#endif

				#if GLOWEFFECT_USE_GLOWCOLOR
				glow += mainTex.a * _GlowColor;
				#endif

				#if GLOWEFFECT_USE_VERTEXCOLOR
				glow += mainTex.a * i.color;
				#endif

				#if GLOWEFFECT_MULTIPLY_COLOR
				glow *= _GlowColorMult;
				#endif

				return glow;
			}
				
			ENDCG
        } 
    }
	
    SubShader {
        Tags { "RenderType" = "Opaque" }
		Fog { Mode Off }
        Pass {    
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
            half4 frag(v2f_img i) : COLOR
            {
                return half4(0,0,0,0);
            }
            ENDCG
        }
    }
    
    SubShader {
        Tags { "RenderType" = "Transparent" }
        ZWrite Off
		Fog { Mode Off }
        Pass {    
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
            half4 frag(v2f_img i) : COLOR
            {
                return half4(0,0,0,0);
            }
            ENDCG
        }
    }
    
    SubShader {
        Tags { "RenderType" = "TransparentCutout" }
		Fog { Mode Off }
        Pass {    
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
            half4 frag(v2f_img i) : COLOR
            {
                return half4(0,0,0,0);
            }
            ENDCG
        }
    }
    
    SubShader {
        Tags { "RenderType" = "Background" }
		Fog { Mode Off }
        Pass {    
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
            half4 frag(v2f_img i) : COLOR
            {
                return half4(0,0,0,0);
            }
            ENDCG
        }
    }
    
    SubShader {
        Tags { "RenderType" = "Overlay" }
		Fog { Mode Off }
        Pass {    
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
            half4 frag(v2f_img i) : COLOR
            {
                return half4(0,0,0,0);
            }
            ENDCG
        }
    }
    
    SubShader {
        Tags { "RenderType" = "TreeOpaque" }
        Pass {    
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
            half4 frag(v2f_img i) : COLOR
            {
                return half4(0,0,0,0);
            }
            ENDCG
        }
    }
    
    SubShader {
        Tags { "RenderType" = "TreeTransparentCutout" }
		Fog { Mode Off }
        Pass {    
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
            half4 frag(v2f_img i) : COLOR
            {
                return half4(0,0,0,0);
            }
            ENDCG
        }
    }
    
    SubShader {
        Tags { "RenderType" = "TreeBillboard" }
		Fog { Mode Off }
        Pass {    
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
            half4 frag(v2f_img i) : COLOR
            {
                return half4(0,0,0,0);
            }
            ENDCG
        }
    }
    
    SubShader {
        Tags { "RenderType" = "Grass" }
		Fog { Mode Off }
        Pass {    
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
            half4 frag(v2f_img i) : COLOR
            {
                return half4(0,0,0,0);
            }
            ENDCG
        }
    }
    
    SubShader {
        Tags { "RenderType" = "GrassBillboard" }
		Fog { Mode Off }
        Pass {    
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
            half4 frag(v2f_img i) : COLOR
            {
                return half4(0,0,0,0);
            }
            ENDCG
        }
    }	
	Fallback Off
}
