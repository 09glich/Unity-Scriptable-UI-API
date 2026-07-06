Shader "IMGUIMainFrame/URPUnlitCircleSDFOpaque"
{
    Properties
    {

    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        

        Pass
        {
            
            Tags {"RenderType"="Opaque" }

            Cull Off
            ZWrite On
            

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 VertColor : COLOR;
                float2 uv : TEXCOORD0;
            };

            int _ScreenWidth;
            int _ScreenHeight;


            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float2 clip;

                clip.x = (IN.positionOS.x / _ScreenWidth) * 2.0 - 1.0;
                clip.y = (IN.positionOS.y / _ScreenHeight) * 2.0 - 1.0;
                
                OUT.positionHCS = float4(clip, IN.positionOS.z, 1);
                OUT.uv = IN.uv;
                OUT.VertColor = IN.color;
                return OUT;
            }

            

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = IN.VertColor;

                if(
                    length(float2(0.5,0.5) - IN.uv) > 0.5
                ) {
                    
                    discard;
                }

                return color;
            }
            ENDHLSL
        }
    }
}
