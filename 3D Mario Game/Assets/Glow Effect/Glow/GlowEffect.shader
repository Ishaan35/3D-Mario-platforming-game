// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Main shader which computes the glow effect
Shader "Glow Effect/Glow Effect" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

    SubShader {
		Pass { // pass 0 - glow
			name "Glow"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode Off }
			Blend Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile GLOWEFFECT_BLEND_ADDITIVE GLOWEFFECT_BLEND_SCREEN GLOWEFFECT_BLEND_MULTIPLY GLOWEFFECT_BLEND_SUBTRACT
	
			#include "UnityCG.cginc"
	
			uniform sampler2D _MainTex;
			uniform half4 _MainTex_TexelSize;
			uniform sampler2D _Glow;
			uniform half _GlowStrength;
			uniform float4 _GlowColorMultiplier;
			
			struct v2f {
				half4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				half2 uv1 : TEXCOORD1;
			};	
			
			v2f vert (appdata_img v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
		       	o.uv = v.texcoord;
		       	o.uv1 = v.texcoord;
		       	
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0) {
					o.uv1.y = 1 - o.uv1.y;
				}
				#endif
				
				return o;
			}
			
			fixed4 frag (v2f i) : COLOR
			{
				fixed4 mainTex = tex2D(_MainTex, i.uv);
				half4 glow = tex2D(_Glow, i.uv1) * _GlowStrength * _GlowColorMultiplier;	
				#if GLOWEFFECT_BLEND_SCREEN
				return 1 - ((1 - mainTex) * (1 - glow));
				#elif GLOWEFFECT_BLEND_MULTIPLY		
				return mainTex * glow;
				#elif GLOWEFFECT_BLEND_SUBTRACT
				return mainTex - glow;
				#else // additive
				return mainTex + glow;
				#endif
			}
			ENDCG 
		}
		
		pass { // pass 1 - blur the main texture
		    name "SimpleBlur"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode Off }
			Blend Off
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			
			#include "UnityCG.cginc"
				
			uniform sampler2D _MainTex;
			uniform half4 _MainTex_TexelSize;
			uniform half _BlurSpread;
				
			struct v2f {
				half4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				half2 uv2[4] : TEXCOORD1;
			};	
				
			v2f vert (appdata_img v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
		       	o.uv = v.texcoord;
		       	
		       	o.uv2[0] = v.texcoord + _MainTex_TexelSize.xy * half2(_BlurSpread, _BlurSpread);					
				o.uv2[1] = v.texcoord + _MainTex_TexelSize.xy * half2(-_BlurSpread, -_BlurSpread);
				o.uv2[2] = v.texcoord + _MainTex_TexelSize.xy * half2(_BlurSpread, -_BlurSpread);
				o.uv2[3] = v.texcoord + _MainTex_TexelSize.xy * half2(-_BlurSpread, _BlurSpread);
				return o;
			}	
			
			fixed4 frag ( v2f i ) : COLOR
			{
				fixed4 blur = tex2D(_MainTex, i.uv ) * 0.4;
				blur += tex2D(_MainTex, i.uv2[0]) * 0.15;
				blur += tex2D(_MainTex, i.uv2[1]) * 0.15;
				blur += tex2D(_MainTex, i.uv2[2]) * 0.15;	
				blur += tex2D(_MainTex, i.uv2[3]) * 0.15;
				return blur;
			}
				 			
			ENDCG	
		}
		
		pass { // pass 2 - blur the main texture and multiple the rgb channels by the alpha 
			name "BlurandAlphaMultRGB"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode Off }
			Blend Off
					
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			
			#include "UnityCG.cginc"
				
			uniform sampler2D _MainTex;
			uniform half4 _MainTex_TexelSize;
			uniform half _BlurSpread;
				
			struct v2f {
				half4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				half2 uv2[4] : TEXCOORD1;
			};	
				
			v2f vert (appdata_img v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
		       	
		       	o.uv = v.texcoord;
		       	o.uv2[0] = v.texcoord + _MainTex_TexelSize.xy * half2(_BlurSpread, _BlurSpread);					
				o.uv2[1] = v.texcoord + _MainTex_TexelSize.xy * half2(-_BlurSpread, -_BlurSpread);
				o.uv2[2] = v.texcoord + _MainTex_TexelSize.xy * half2(_BlurSpread, -_BlurSpread);
				o.uv2[3] = v.texcoord + _MainTex_TexelSize.xy * half2(-_BlurSpread, _BlurSpread);
					
				return o;
			}	
	
			fixed4 frag ( v2f i ) : COLOR
			{
				fixed4 mainTex = tex2D(_MainTex, i.uv);
				mainTex *= mainTex.a * 0.4f;
				fixed4 blur = mainTex;
				
				mainTex = tex2D(_MainTex, i.uv2[0]);
				mainTex *= mainTex.a * 0.15;
				blur += mainTex;
				
				mainTex = tex2D(_MainTex, i.uv2[1]);
				mainTex *= mainTex.a * 0.15;
				blur += mainTex;
				
				mainTex = tex2D(_MainTex, i.uv2[2]);
				mainTex *= mainTex.a * 0.15;
				blur += mainTex;
				
				mainTex = tex2D(_MainTex, i.uv2[3]);
				mainTex *= mainTex.a * 0.15;
				blur += mainTex;
				
				return blur;
			}		
			ENDCG
		}
		
		pass { // pass 3 - blur the glow texture and apply that glow to the main texture 
			name "SimpleBlurandGlow"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode Off }
			Blend Off
					
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			
			#include "UnityCG.cginc"
				
			uniform sampler2D _MainTex;
			uniform half4 _MainTex_TexelSize;
			uniform sampler2D _Glow;
			uniform half4 _Glow_TexelSize;
			uniform half _GlowStrength;
			uniform float4 _GlowColorMultiplier;
				
			struct v2f {
				half4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				half2 uv1 : TEXCOORD1;
				half2 uv2[4] : TEXCOORD2;
			};	
				
			v2f vert (appdata_img v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
		       	
		       	o.uv = v.texcoord;
		       	o.uv1 = v.texcoord;
		       	o.uv2[0] = v.texcoord + _Glow_TexelSize.xy * half2(2.5,2.5);					
				o.uv2[1] = v.texcoord + _Glow_TexelSize.xy * half2(-2.5,-2.5);
				o.uv2[2] = v.texcoord + _Glow_TexelSize.xy * half2(2.5,-2.5);
				o.uv2[3] = v.texcoord + _Glow_TexelSize.xy * half2(-2.5,2.5);
				
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0) {
					o.uv1.y = 1 - o.uv1.y;
					o.uv2[0].y = 1 - o.uv2[0].y;
					o.uv2[1].y = 1 - o.uv2[1].y;
					o.uv2[2].y = 1 - o.uv2[2].y;
					o.uv2[3].y = 1 - o.uv2[3].y;
				}
				#endif
					
				return o;
			}	
	
			fixed4 frag ( v2f i ) : COLOR
			{	
				fixed4 blur = tex2D(_Glow, i.uv1 ) * 0.3;	
				blur += tex2D(_Glow, i.uv2[0]) * 0.175;	
				blur += tex2D(_Glow, i.uv2[1]) * 0.175;	
				blur += tex2D(_Glow, i.uv2[2]) * 0.175;
				blur += tex2D(_Glow, i.uv2[3]) * 0.175;
				blur *= _GlowStrength * _GlowColorMultiplier;
				
				return tex2D(_MainTex, i.uv) + blur;
			}		
			ENDCG
		}
		
		Pass { // pass 4 - apply a glow to the blurred main texture based on the alpha channel of the glow
			name "SimpleBlurGlowFromAlpha"
			ZTest Always Cull Off ZWrite Off
			Fog { Mode Off }
			Blend Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			
			#include "UnityCG.cginc"
				
			uniform sampler2D _MainTex;
			uniform half4 _MainTex_TexelSize;
			uniform float _GlowStrength;
			uniform float4 _GlowColorMultiplier;
				
			struct v2f {
				half4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				half2 uv2[4] : TEXCOORD1;
			};	
				
			v2f vert (appdata_img v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
		       	
		       	o.uv = v.texcoord;
		       	o.uv2[0] = v.texcoord + _MainTex_TexelSize.xy * half2(2.5,2.5);					
				o.uv2[1] = v.texcoord + _MainTex_TexelSize.xy * half2(-2.5,-2.5);
				o.uv2[2] = v.texcoord + _MainTex_TexelSize.xy * half2(2.5,-2.5);
				o.uv2[3] = v.texcoord + _MainTex_TexelSize.xy * half2(-2.5,2.5);
				 
				return o;
			}	
	
			fixed4 frag ( v2f i ) : COLOR
			{
				fixed4 blur = tex2D(_MainTex, i.uv ) * 0.3;
				blur += tex2D(_MainTex, i.uv2[0]) * 0.175;
				blur += tex2D(_MainTex, i.uv2[1]) * 0.175;
				blur += tex2D(_MainTex, i.uv2[2]) * 0.175;
				blur += tex2D(_MainTex, i.uv2[3]) * 0.175;
				blur *= _GlowStrength * _GlowColorMultiplier;
			
				return tex2D(_MainTex, i.uv) + blur * blur.a;
			}	
			ENDCG
		}
	}

Fallback Off

}