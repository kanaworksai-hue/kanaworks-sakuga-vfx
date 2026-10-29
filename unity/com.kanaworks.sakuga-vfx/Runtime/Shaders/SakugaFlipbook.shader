// KanaWorks SAKUGA VFX - unlit flipbook for URP (MIT).
// One pass, premultiplied output (Blend One OneMinusSrcAlpha): alpha 1 = normal alpha blending of the straight-alpha
// sheet, alpha 0 = additive, anything between = a mix. So one material switches without a keyword or a second pass.
// _Frame >= 0 picks a cell of the grid (set per renderer by SakugaFlipbook through a MaterialPropertyBlock);
// _Frame < 0 uses the mesh UV as it comes, for a Particle System's Texture Sheet Animation.
// Soft particles: fades where the quad meets scene geometry (needs the URP depth texture; SakugaEffect turns it on
// only when the pipeline provides one, through the global _SakugaDepthReady). HDR core: only the brightest part of the
// drawing is pushed above 1, so Bloom catches the core and not the whole card. Camera push: billboards slide toward
// the camera so a hit drawn on a target is not cut by the target's body.
Shader "KanaWorks/SakugaFlipbook"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Flipbook sheet (straight alpha)", 2D) = "white" {}
        [HDR] _Tint ("Tint (HDR values glow with Bloom)", Color) = (1, 1, 1, 1)
        _Grid ("Grid (cols, rows, frames, -)", Vector) = (8, 8, 64, 0)
        _Frame ("Frame (-1 = mesh UV, for Particle System texture sheets)", Float) = 0
        _Additive ("Blend (0 = alpha, 1 = additive)", Range(0, 1)) = 0
        [Enum(None, 0, Face Camera, 1, Upright, 2, Travel, 3)] _Billboard ("Billboard", Float) = 1
        _AlphaBoost ("Alpha boost", Range(0.5, 3)) = 1
        _SoftFade ("Soft particles: fade distance in metres (0 = off)", Range(0, 2)) = 0.35
        _CamPush ("Push toward the camera (m)", Range(0, 2)) = 0
        _CoreBoost ("HDR core boost (0 = off)", Range(0, 8)) = 0
        _CoreThreshold ("HDR core: brightness where it starts", Range(0, 1)) = 0.78
        _Recolor ("Recolour toward Recolor Tint", Range(0, 1)) = 0
        _RecolorTint ("Recolor Tint", Color) = (1, 1, 1, 1)
        _InstanceColor ("Instance colour (per renderer)", Color) = (1, 1, 1, 1)
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                half4 _Tint;
                float4 _Grid;
                float _Frame, _Additive, _Billboard, _AlphaBoost, _SoftFade, _CamPush, _CoreBoost, _CoreThreshold, _Recolor;
                half4 _RecolorTint;
                half4 _InstanceColor;
            CBUFFER_END
            float _SakugaDepthReady;                        // global, set by the runtime when a depth texture exists

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                float4 screenPos : TEXCOORD1;
                float eyeDepth : TEXCOORD2;
            };

            // The quad's object-space xy is an offset from the pivot (the object origin); turn it toward the camera.
            float3 BillboardWS(float3 posOS)
            {
                float3 center = TransformObjectToWorld(float3(0, 0, 0));
                float3 axisX = UNITY_MATRIX_M._m00_m10_m20;
                float3 axisY = UNITY_MATRIX_M._m01_m11_m21;
                float sx = length(axisX), sy = length(axisY);
                float3 right = UNITY_MATRIX_V[0].xyz;
                float3 up = UNITY_MATRIX_V[1].xyz;
                if (_Billboard > 2.5)                       // travel: face the camera, roll so +X follows the object's +X
                {
                    float3 viewDir = normalize(center - _WorldSpaceCameraPos);
                    float3 fwd = axisX / max(sx, 1e-5);
                    float3 r = fwd - viewDir * dot(fwd, viewDir);
                    if (dot(r, r) > 1e-4)
                    {
                        right = normalize(r);
                        up = normalize(cross(viewDir, right));
                    }
                }
                else if (_Billboard > 1.5)                  // upright: keep the object's up axis (columns stay vertical)
                {
                    up = axisY / max(sy, 1e-5);
                    float3 r = right - up * dot(right, up);
                    right = dot(r, r) > 1e-6 ? normalize(r) : axisX / max(sx, 1e-5);
                }
                if (_CamPush > 0)
                {
                    float3 toCam = _WorldSpaceCameraPos - center;
                    float d = length(toCam);
                    center += toCam / max(d, 1e-4) * min(_CamPush, max(d - 0.5, 0));
                }
                return center + right * (posOS.x * sx) + up * (posOS.y * sy);
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 ws = _Billboard > 0.5 ? BillboardWS(v.positionOS.xyz) : TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(ws);
                o.screenPos = ComputeScreenPos(o.positionCS);
                o.eyeDepth = -TransformWorldToView(ws).z;
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

            half3 Recolor(half3 rgb, half3 tint)
            {
                half lum = dot(rgb, half3(0.3, 0.59, 0.11));
                half3 dark = tint * 0.32, light = lerp(tint, 1, 0.82);
                return lum < 0.5 ? lerp(dark, tint, lum * 2) : lerp(tint, light, (lum - 0.5) * 2);
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                tex.rgb = lerp(tex.rgb, Recolor(tex.rgb, _RecolorTint.rgb), _Recolor);
                half lum = max(tex.r, max(tex.g, tex.b));
                half4 c = tex * _Tint * i.color * _InstanceColor;
                c.rgb *= 1 + _CoreBoost * smoothstep(_CoreThreshold, 1, lum);
                half a = saturate(c.a * _AlphaBoost);
                if (_SakugaDepthReady > 0.5 && _SoftFade > 0)
                {
                    float2 suv = i.screenPos.xy / max(i.screenPos.w, 1e-5);
                    float scene = LinearEyeDepth(SampleSceneDepth(suv), _ZBufferParams);
                    a *= saturate((scene - i.eyeDepth) / _SoftFade);
                }
                return half4(c.rgb * a, a * (1 - _Additive));
            }
            ENDHLSL
        }
    }
    Fallback Off
}
