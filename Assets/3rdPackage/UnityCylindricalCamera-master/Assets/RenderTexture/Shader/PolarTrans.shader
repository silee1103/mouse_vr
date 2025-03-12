Shader "Custom/PolarTrans"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _FisheyeStrength("Fisheye Strength", Range(0.1,4.0)) = 0.2
        _InnerRadius("Inner Radius", Range(0, 0.5)) = 0.1
        _OuterRadius("Outer Radius", Range(0.1, 2.0)) = 0.5
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float _FisheyeStrength;
            float _InnerRadius;
            float _OuterRadius;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float PI = 3.14159265;

                // 1) (0.5, 0.5) 중심으로 이동해 반지름/각도 계산
                float2 center = i.uv - 0.5;   // -0.5..+0.5
                float r = length(center);
                float theta = atan2(center.y, -center.x); // -PI..PI

                // 2) polar 정규화
                float thetaNorm = (theta + PI) / (2.0 * PI);
                float rNorm     = r / 0.5;  // 0~1 범위 넘어갈 수 있음 (모서리 ~1.414)

                // 3) 반지름 정규화
                float normalizedR = (rNorm - _InnerRadius) / (_OuterRadius - _InnerRadius);
                normalizedR = saturate(normalizedR); // 0~1 범위 보장
                
                rNorm = pow(normalizedR, _FisheyeStrength * 0.5) + pow(normalizedR * normalizedR, _FisheyeStrength);

                // 4) 최종 uv로 샘플링
                float2 polarUV = float2(thetaNorm, rNorm);
                return tex2D(_MainTex, polarUV);
            }
            ENDCG
        }
    }
}
