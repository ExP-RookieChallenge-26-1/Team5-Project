Shader "Custom/ViewMaskFade"
{
    // 화면을 덮는 오버레이. 시야 중심(_Center)에서 바깥으로 갈수록
    // 알파가 0 -> 1 로 부드럽게 올라가며 어두워(희미해)집니다.
    Properties
    {
        _Color ("Overlay Color", Color) = (0, 0, 0, 1)
        _Center ("Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _InnerRadius ("Inner Radius (clear)", Float) = 0.25
        _OuterRadius ("Outer Radius (dark)", Float) = 0.5
        _Aspect ("Aspect (w/h)", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Overlay"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Off

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
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _Center;
                float _InnerRadius;
                float _OuterRadius;
                float _Aspect;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // 중심으로부터의 거리. x축을 화면 비율로 보정해 화면상에서 정원(正圓)이 되게 함.
                float2 d = IN.uv - _Center.xy;
                d.x *= _Aspect;
                float dist = length(d);

                // InnerRadius 안쪽은 완전 투명(0), OuterRadius 바깥은 완전 불투명(1)
                float a = smoothstep(_InnerRadius, _OuterRadius, dist);

                return half4(_Color.rgb, _Color.a * a);
            }
            ENDHLSL
        }
    }
}
