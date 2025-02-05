Shader "Custom/Fisheye"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Strength ("Distortion Strength", Range(0, 2)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Uniforms
            sampler2D _MainTex;
            float _Strength;

            // Vertex Input
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            // Vertex Output
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // Vertex Shader
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // Convert to clip space
                o.uv = v.uv; // Pass through UVs
                return o;
            }

            // Fragment Shader
            fixed4 frag (v2f i) : SV_Target
            {
                // Fisheye distortion
                float2 uv = i.uv - 0.5; // Center UVs around (0, 0)
                float dist = length(uv); // Distance from center
                uv += uv * dist * _Strength; // Apply fisheye distortion
                uv += 0.5; // Recenter UVs

                // Prevent sampling outside texture bounds
                if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
                    return float4(0, 0, 0, 1); // Black for out-of-bounds

                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}
