// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.27 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.27;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:6384,x:33101,y:32679,varname:node_6384,prsc:2|emission-5302-OUT;n:type:ShaderForge.SFN_Tex2dAsset,id:4713,x:32078,y:32689,ptovrint:False,ptlb:MainTexture,ptin:_MainTexture,varname:node_4713,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:c6dd5cb7a6d17da428c25bcfc4269a5c,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:5999,x:32251,y:32690,varname:node_5999,prsc:2,tex:c6dd5cb7a6d17da428c25bcfc4269a5c,ntxv:0,isnm:False|TEX-4713-TEX;n:type:ShaderForge.SFN_Multiply,id:5302,x:32550,y:32811,varname:node_5302,prsc:2|A-5999-RGB,B-6510-RGB,C-6510-A,D-7793-OUT;n:type:ShaderForge.SFN_VertexColor,id:6510,x:31899,y:33008,varname:node_6510,prsc:2;n:type:ShaderForge.SFN_ValueProperty,id:7793,x:32315,y:33009,ptovrint:False,ptlb:Glow_Intensity,ptin:_Glow_Intensity,varname:node_7793,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;proporder:4713-7793;pass:END;sub:END;*/

Shader "Custom/Additive_Base" {
    Properties {
        _MainTexture ("MainTexture", 2D) = "white" {}
        _Glow_Intensity ("Glow_Intensity", Float ) = 1
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 200
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One One
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _MainTexture; uniform float4 _MainTexture_ST;
            uniform float _Glow_Intensity;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float4 node_5999 = tex2D(_MainTexture,TRANSFORM_TEX(i.uv0, _MainTexture));
                float3 emissive = (node_5999.rgb*i.vertexColor.rgb*i.vertexColor.a*_Glow_Intensity);
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
