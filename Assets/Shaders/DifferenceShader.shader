

Shader "Universal Render Pipeline/temp_name/DifferenceShader" {
	Properties {
		_OldTex ("Old Texture", 2D) = "" {}
		_NewTex ("New Texture", 2D) = "" {}
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
            
            
            sampler2D _OldTex;
            sampler2D _NewTex;
            
            v2f vert (float4 position : POSITION, float2 uv0 : TEXCOORD0) {
            
                VertexPositionInputs positionInputs = GetVertexPositionInputs(position.xyz);
                v2f o;
                o.pos = positionInputs.positionCS;
            
                o.uv.xy = uv0.xy;
                       
                return o;
            }
            
            half4 frag (v2f i) : COLOR {
            
                half4 sum = tex2D( _NewTex, i.uv) - tex2D( _OldTex, i.uv); //Difference                                                              
                return sum;
            }
            
            ENDHLSL
        }
    }    
    Fallback off
} // shader