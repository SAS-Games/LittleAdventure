Shader "Custom/URPFlipbookAtlas"
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
            // xy = size scale
            // zw = pivot offset

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            float2 ApplyAspectAndPivot(float2 uv, float4 info)
            {
                float2 scale = info.xy;
                float2 pivotOffset = info.zw;

                // center UV
                uv -= 0.5;

                // aspect correction (NO stretching)
                uv /= scale;

                // pivot stabilization
                uv -= pivotOffset;

                // restore
                uv += 0.5;

                return uv;
            }

            half4 frag (Varyings i) : SV_Target
            {
                if (_FrameCount <= 0)
                    return 0;

                float time = _Time.y * _Speed;
                int frame = (int)floor(fmod(time, _FrameCount));

                float4 uvRect = _FrameUVs[frame];
                float4 info   = _FrameInfo[frame];

                float2 uv = ApplyAspectAndPivot(i.uv, info);

                // discard outside sprite region
                if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
                    discard;

                float2 atlasUV = lerp(uvRect.xy, uvRect.zw, uv);

                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, atlasUV);
            }

            ENDHLSL
        }
    }
}