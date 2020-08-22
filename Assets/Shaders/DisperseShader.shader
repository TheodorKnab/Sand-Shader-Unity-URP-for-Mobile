Shader "Universal Render Pipeline/temp_name/DisperseShader"
{
    Properties
    {
        _DepthTex("Depth Tex", 2D) = "white" {}
        _SubTex("Sub Tex", 2D) = "white" {}

    }

    SubShader
    {
        // With SRP we introduce a new "RenderPipeline" tag in Subshader. This allows to create shaders
        // that can match multiple render pipelines. If a RenderPipeline tag is not set it will match
        // any render pipeline. In case you want your subshader to only run in LWRP set the tag to
        // "UniversalRenderPipeline"
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" "IgnoreProjector" = "True"}
        LOD 300

        // ------------------------------------------------------------------
        // Forward pass. Shades GI, emission, fog and all lights in a single pass.
        // Compared to Builtin pipeline forward renderer, LWRP forward renderer will
        // render a scene with multiple lights with less drawcalls and less overdraw.
        Pass
        {
            // "Lightmode" tag must be "UniversalForward" or not be defined in order for
            // to render objects.
            Name "Depth"
            Tags{"LightMode" = "UniversalForward"}


            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag


            // Required by all Universal Render Pipeline shaders.
            // It will include Unity built-in shader variables (except the lighting variables)
            // (https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html
            // It will also include many utilitary functions. 
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            sampler2D _DepthTex;
            sampler2D _SubTex;
            float4 _DepthTex_ST;           
            
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv                       : TEXCOORD0;
                float3 positionWS               : TEXCOORD1; // xyz: positionWS, w: vertex fog factor
                half3  normalWS                 : TEXCOORD2;
                float4 positionCS               : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                // VertexPositionInputs contains position in multiple spaces (world, view, homogeneous clip space)
                // Our compiler will strip all unused references (say you don't use view space).
                // Therefore there is more flexibility at no additional cost with this struct.
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

                // Similar to VertexPositionInputs, VertexNormalInputs will contain normal, tangent and bitangent
                // in world space. If not used it will be stripped.
                VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                // TRANSFORM_TEX is the same as the old shader library.
                output.uv = TRANSFORM_TEX(input.uv, _DepthTex);
                //output.uv = input.uv;
                output.positionWS = float3(vertexInput.positionWS);
                output.normalWS = vertexNormalInput.normalWS;

                // We just use the homogeneous clip position from the vertex input
                output.positionCS = vertexInput.positionCS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float newValue = tex2D(_SubTex, input.uv).r;                       
                
                //needs to be scaled, as only 0 - 0.5 is used for up depth in the texture  
                newValue = 0.5 - newValue * 0.5; 
                
                float existingValue = tex2D(_DepthTex, input.uv).r;            
                     
                //is new value smaller than existingValue (branchless)                
                float newValueSmaller = saturate((existingValue - newValue) * 99999999);
                //and smaller than 0.5
                newValueSmaller *= saturate((0.5f - newValue) * 99999999);
                
                //set smaller value
                float color = existingValue * (1 - newValueSmaller) + newValue * newValueSmaller * 0.5f; // 0.5f is used to intensify pushing up the sand
                return half4(color.rrr, 1);
            }
            ENDHLSL
        }        
    }
}