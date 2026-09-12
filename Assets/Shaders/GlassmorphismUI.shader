Shader "UI/URPGlassmorphism"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (0.05, 0.1, 0.15, 0.5)
        _BlurSize ("Blur Size", Range(0, 10)) = 2.0
    }
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _BlurSize;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                
                // Calculate screen position for sampling the background
                output.screenPos = ComputeScreenPos(output.positionCS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                
                // Aspect ratio correction for blur
                float2 texelSize = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y) * _BlurSize;

                // Simple 9-tap Gaussian-like blur
                half3 blurColor = half3(0,0,0);
                blurColor += SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV + float2(-texelSize.x, -texelSize.y)).rgb * 0.0625;
                blurColor += SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV + float2(0, -texelSize.y)).rgb * 0.125;
                blurColor += SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV + float2(texelSize.x, -texelSize.y)).rgb * 0.0625;
                
                blurColor += SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV + float2(-texelSize.x, 0)).rgb * 0.125;
                blurColor += SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV).rgb * 0.25;
                blurColor += SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV + float2(texelSize.x, 0)).rgb * 0.125;
                
                blurColor += SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV + float2(-texelSize.x, texelSize.y)).rgb * 0.0625;
                blurColor += SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV + float2(0, texelSize.y)).rgb * 0.125;
                blurColor += SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV + float2(texelSize.x, texelSize.y)).rgb * 0.0625;

                // Mix with tint color
                half4 finalColor = half4(lerp(blurColor, _Color.rgb, _Color.a), 1.0);
                
                // Apply Image component alpha/color over it (for fading in/out)
                finalColor.a = input.color.a; 
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}
