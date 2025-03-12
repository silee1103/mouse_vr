// CylindricalUnwrap.hlsl
#pragma target 3.0

#ifndef UNITY_SHADER_GRAPH_CUSTOM_FUNCTION
#define UNITY_SHADER_GRAPH_CUSTOM_FUNCTION
#endif

// Shader Graph에 넘겨줄 함수
float4 CylindricalUnwrap(
    float2 uv,                   // 입력: 원본 UV
    TEXTURE2D(_MainTex),         // 입력: 텍스처 (Shader Graph에서 연결)
    SAMPLER(sampler_MainTex),    // 입력: 샘플러
    float4 _MainTex_ST           // 입력: (필요시) _MainTex의 ST 정보
)
{
    float PI = 3.14159265;
    
    // 실린더 언랩 로직
    float angle = uv.x * 360.0 - 180.0;
    float azim;
    float dist;
    float get_s, get_t, pos_s;

    // 4 구간 분기
    if (angle >= -180.0 && angle < -90.0)
    {
        azim  = angle + 180.0 - 45.0;
        pos_s = 0.0;
    }
    else if (angle >= -90.0 && angle < 0.0)
    {
        azim  = angle + 45.0;
        pos_s = 0.25;
    }
    else if (angle >= 0.0 && angle < 90.0)
    {
        azim  = angle - 45.0;
        pos_s = 0.5;
    }
    else // (angle >= 90.0 && angle < 180.0)
    {
        azim  = angle - 180.0 + 45.0;
        pos_s = 0.75;
    }

    dist  = sqrt( pow( tan(PI*azim/180.0), 2.0 ) + 1.0 );
    get_s = (tan(PI * azim / 180.0) + 1.0) / 8.0 + pos_s;
    get_t = (uv.y - 0.5) * dist + 0.5;

    // 범위 확인 후 샘플링
    float4 col;
    float lower = 0.5 - 0.5 / sqrt(2.0);
    float upper = 0.5 + 0.5 / sqrt(2.0);

    if (uv.y < lower || uv.y > upper)
    {
        // 검정 or 투명
        col = float4(0,0,0,0);
    }
    else
    {
        // Unity 2020+ / URP/HDRP에서는 tex2D 대신 SAMPLE_TEXTURE2D 사용
        col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(get_s, get_t));
    }

    return col;
}
