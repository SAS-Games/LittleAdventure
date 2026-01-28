Shader "Custom/GPUInstancedMatrix"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct InstanceData
            {
                float4x4 objectToWorld;
                float4 color;
            };

            StructuredBuffer<InstanceData> _InstanceDataBuffer;

            struct Attributes
            {
                float3 positionOS : POSITION;
                uint instanceID : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                InstanceData data = _InstanceDataBuffer[v.instanceID];
                float4x4 m = data.objectToWorld;

                float4 worldPos = mul(m, float4(v.positionOS, 1));
                o.positionCS = TransformWorldToHClip(worldPos.xyz);
                o.color = data.color;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
               return i.color;
            }

            ENDHLSL
        }
    }
}