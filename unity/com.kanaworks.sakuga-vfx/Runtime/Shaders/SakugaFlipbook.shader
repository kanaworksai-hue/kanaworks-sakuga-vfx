// KanaWorks SAKUGA VFX - unlit flipbook for URP (MIT).
// One pass, premultiplied output (Blend One OneMinusSrcAlpha): alpha 1 = normal alpha blending of the straight-alpha
// sheet, alpha 0 = additive. So one material switches between the two without a keyword or a second pass.
// _Frame >= 0 picks a cell of the grid (set per renderer by SakugaFlipbook through a MaterialPropertyBlock);
// _Frame < 0 uses the mesh UV as it comes, for a Particle System's Texture Sheet Animation.
Shader "KanaWorks/SakugaFlipbook"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Flipbook sheet (straight alpha)", 2D) = "white" {}
        [HDR] _Tint ("Tint (HDR values glow with Bloom)", Color) = (1, 1, 1, 1)
        _Grid ("Grid (cols, rows, frames, -)", Vector) = (8, 8, 64, 0)
        _Frame ("Frame (-1 = mesh UV, for Particle System texture sheets)", Float) = 0
        [Enum(Alpha Blend, 0, Additive, 1)] _Additive ("Blend", Float) = 0
        [Enum(None, 0, Face Camera, 1, Upright, 2)] _Billboard ("Billboard", Float) = 1
        _AlphaBoost ("Alpha boost", Range(0.5, 3)) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" "PreviewType" = "Plane" }
        Pass
        {
            Name "SakugaFlipbook"
            Tags { "LightMode" = "UniversalForward" }
            Blend One OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                half4 _Tint;
                float4 _Grid;
                float _Frame, _Additive, _Billboard, _AlphaBoost;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };

            // The quad's object-space xy is an offset from the pivot (the object origin); turn it toward the camera.
            float3 BillboardWS(float3 posOS)
            {
                float3 center = TransformObjectToWorld(float3(0, 0, 0));
                float3 axisX = UNITY_MATRIX_M._m00_m10_m20;
                float3 axisY = UNITY_MATRIX_M._m01_m11_m21;
                float sx = length(axisX), sy = length(axisY);
                float3 right = UNITY_MATRIX_V[0].xyz;
                float3 up = UNITY_MATRIX_V[1].xyz;
                if (_Billboard > 1.5)                       // upright: keep the object's up axis (columns stay vertical)
                {
                    up = axisY / max(sy, 1e-5);
                    float3 r = right - up * dot(right, up);
                    right = dot(r, r) > 1e-6 ? normalize(r) : axisX / max(sx, 1e-5);
                }
                return center + right * (posOS.x * sx) + up * (posOS.y * sy);
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 ws = _Billboard > 0.5 ? BillboardWS(v.positionOS.xyz) : TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(ws);
                float2 uv = v.uv;
                if (_Frame >= 0)
                {
                    float2 grid = max(_Grid.xy, 1);
                    float f = clamp(floor(_Frame), 0, max(_Grid.z, 1) - 1);
                    float col = fmod(f, grid.x);
                    float row = floor(f / grid.x);               // first frame is the top-left cell
                    uv = (saturate(uv) + float2(col, grid.y - 1 - row)) / grid;
                }
                o.uv = uv;
                o.color = v.color;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _Tint * i.color;
                half a = saturate(c.a * _AlphaBoost);
                return half4(c.rgb * a, a * (1 - _Additive));
            }
            ENDHLSL
        }
    }
    Fallback Off
}
