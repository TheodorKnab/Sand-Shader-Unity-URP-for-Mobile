

Shader "Universal Render Pipeline/temp_name/BlurX" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
	}
		
    Subshader {   
        Tags { "RenderPipeline" = "UniversalPipeline"}
        Pass {
            
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct v2f {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            float4 _MainTex_TexelSize;
            float4 tintColor;
            float _Size;
            
            sampler2D _MainTex;
            
            v2f vert (float4 position : POSITION, float2 uv0 : TEXCOORD0) {
            
                VertexPositionInputs positionInputs = GetVertexPositionInputs(position.xyz);
                v2f o;
                o.pos = positionInputs.positionCS;
            
                o.uv.xy = uv0.xy;
            
            
                return o;
            }
            
            half4 frag (v2f i) : COLOR {
                    half4 sum = half4(0,0,0,0);
    
                    #define GRABPIXEL(weight,kernelx) tex2D( _MainTex, float2(i.uv.x + _MainTex_TexelSize.x * kernelx * _Size, i.uv.y)) * weight          
                    
                    sum += GRABPIXEL(0.05, -4.0);
                    sum += GRABPIXEL(0.09, -3.0);
                    sum += GRABPIXEL(0.12, -2.0);
                    sum += GRABPIXEL(0.15, -1.0);
                    sum += GRABPIXEL(0.18,  0.0);
                    sum += GRABPIXEL(0.15, +1.0);
                    sum += GRABPIXEL(0.12, +2.0);
                    sum += GRABPIXEL(0.09, +3.0);
                    sum += GRABPIXEL(0.05, +4.0);
                    //scale so range is from 0.5 to 1, as the base is at 0.5
                    //is obly done in the first blur
                    return sum;
            }
            
            ENDHLSL
        }
    }
   
    Fallback off
} // shader