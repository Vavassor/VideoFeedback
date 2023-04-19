﻿Shader "Custom/Video Mixer"
{
    Properties
    {
        [Header(Main Maps)] _MainTex("Texture", 2D) = "white" {}
        [Header(Color Filters)] _Brightness("Brightness", float) = 1
        _HueShift("Hue Shift", float) = 0
        _InvertColor("Invert Color", float) = 0
        [Header(Chromatic Aberration)] _ChromaticDistortion("Chromatic Distortion", Range(0, 0.4)) = 0.01
        _ChromaticAberrationFalloff("Chromatic Aberattion Falloff", Range(0, 1)) = 0.65
        _ChromaticAberrationSize("Chromatic Aberattion Size", Range(0, 1)) = 0.9
        [Header(Distortion)] _MirrorTileCount("Mirror Tiles", float) = 1.0
        // _WaveDistortion("WaveDistortion", Range(-0.24, 0.24)) = 0.0
        [Header(Post Processing)] _EdgeBrightness("Edge Brightness", Range(0, 1)) = 0.0
    }
    SubShader
    {
        Lighting Off
        Blend One Zero

        Pass
        {
            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            #include "ColorConversion.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            half _ChromaticAberrationFalloff;
            half _ChromaticAberrationSize;
            half _ChromaticDistortion;
            
            half _Brightness;
            half _HueShift;
            half _InvertColor;

            half _MirrorTileCount;
            // half _WaveDistortion;

            half _EdgeBrightness;

            inline fixed4 getColorWithChromaticAberration(sampler2D samp, float2 uv, fixed4 center)
            {
                float2 fromCenter = uv - float2(0.5, 0.5);
                float falloff = 1.0 - smoothstep(_ChromaticAberrationSize, _ChromaticAberrationSize - _ChromaticAberrationFalloff, length(fromCenter));
                float offset = fromCenter * falloff * _ChromaticDistortion;
                fixed r = tex2D(samp, uv + offset).x;
                fixed b = tex2D(samp, uv - offset).z;
                return fixed4(r, center.g, b, center.a);
            }

            inline fixed4 detectEdges(sampler2D samp, float2 uv, fixed4 c)
            {
                float3 offset = float3(_MainTex_TexelSize.xy, 0.0);
                fixed4 n = tex2D(samp, uv + offset.yz);
                fixed4 s = tex2D(samp, uv - offset.yz);
                fixed4 e = tex2D(samp, uv + offset.xz);
                fixed4 w = tex2D(samp, uv - offset.xz);
                return abs(n - c) + abs(s - c) + abs(e - c) + abs(w - c);
            }

            fixed4 frag(v2f_customrendertexture IN) : COLOR
            {
                float2 distortedTexcoord = abs(2.0 * frac(_MirrorTileCount * 0.5 * IN.localTexcoord + 0.5) - 1.0);
                // distortedTexcoord.y += _WaveDistortion * sin(15.708 * IN.localTexcoord.x);

                fixed4 center = tex2D(_MainTex, distortedTexcoord);
                fixed4 colorWithCa = getColorWithChromaticAberration(_MainTex, distortedTexcoord, center);
                fixed4 edges = detectEdges(_MainTex, distortedTexcoord, colorWithCa);
                fixed4 currentColor = colorWithCa + _EdgeBrightness * colorWithCa * edges;
                float3 hsv = rgbToHsv(currentColor.rgb);

                // To wrap negative numbers, normally you'd use (n % m + m) % m.
                // Because n is smaller than a couple cycles, we can get away with (n + m) % m.
                hsv.x = (hsv.x + _HueShift + 360.0) % 360.0;
                currentColor.rgb = hsvToRgb(hsv) * _Brightness;

                fixed4 col = currentColor;

                if (_InvertColor != 0.0)
                {
                    // Premultiply the alpha so that the background is dark when the output is
                    // shown in an opaque shader.
                    col.rgb = col.a * (1.0 - col.rgb);
                }

                return col;
            }
            ENDCG
        }
    }
}
