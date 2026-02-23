Shader "Custom/URPFlipbookAtlas_Optimized"
{
    Properties
    {
        _MainTex ("Atlas", 2D) = "white" {}
        _Speed ("Speed", Float) = 8
    }

    SubShader
    {
        Tags { "RenderType"="Transparent"
               "Queue"="Transparent" }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float _Speed;
            int _FrameCount;

            float4 _FrameUVs[64];
            float4 _FrameInfo[64];

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 atlasUV : TEXCOORD0;
            };

            float2 ApplyAspectAndPivot(float2 uv, float4 info)
            {
                float2 scale = info.xy;
                float2 pivotOffset = info.zw;

                uv -= 0.5;
                uv /= scale;
                uv -= pivotOffset;
                uv += 0.5;

                return uv;
            }

            Varyings vert (Attributes v)
            {
                Varyings o;

                o.positionHCS =
                    TransformObjectToHClip(v.positionOS.xyz);

                // ---- Frame selection (NOW PER VERTEX) ----
                float time = _Time.y * _Speed;
                int frame = (int)floor(fmod(time, _FrameCount));

                float4 uvRect = _FrameUVs[frame];
                float4 info   = _FrameInfo[frame];

                float2 uv = ApplyAspectAndPivot(v.uv, info);

                // atlas mapping
                o.atlasUV = lerp(uvRect.xy, uvRect.zw, uv);

                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                return SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    i.atlasUV);
            }

            ENDHLSL
        }
    }
}