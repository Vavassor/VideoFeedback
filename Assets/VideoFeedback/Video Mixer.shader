Shader "Custom/Video Mixer"
{
    Properties
    {
        [Header(Main Maps)] _MainTex("Texture", 2D) = "white" {}
        [Header(Color Filters)] _Brightness("Brightness", float) = 1
        _HueShift("Hue Shift", float) = 0
        _InvertColor("Invert Color", float) = 0
        _Contrast("Contrast", Range(0.9, 1.1)) = 0.0
        _Saturation("Saturation", Range(-0.1, 0.1)) = 0.0
        [Header(Chromatic Aberration)] _ChromaticDistortion("Chromatic Distortion", Range(0, 0.4)) = 0.01
        _ChromaticAberrationFalloff("Chromatic Aberattion Falloff", Range(0, 1)) = 0.65
        _ChromaticAberrationSize("Chromatic Aberattion Size", Range(0, 1)) = 0.9
        [Header(Distortion)] _MirrorTileCount("Mirror Tiles", float) = 1.0
        _FlowDistortion("Flow Distortion", Range(0.0, 32.0)) = 16.0
        [Header(Post Processing)] _EdgeBrightness("Edge Brightness", Range(0, 1)) = 0.0
        _EdgeWidth("Edge Width", Range(0, 16)) = 1.0
        _SharpenAmount("Sharpen", Range(0.0, 0.25)) = 0.0
        [Header(Experimental)] [Toggle(EXPERIMENTAL_ON)] _UseExperimental("Use Experimental", Float) = 0
        _NoiseTexture("Noise Texture", 2D) = "white" {}
        _EdgeNoiseBlend("Edge Noise Blend", Range(0, 1)) = 0.0
        _WaveDistortion("WaveDistortion", Range(-0.24, 0.24)) = 0.0
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
            #pragma shader_feature_local EXPERIMENTAL_ON

            #include "ColorAdjustment.cginc"
            #include "ColorConversion.cginc"
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            half _ChromaticAberrationFalloff;
            half _ChromaticAberrationSize;
            half _ChromaticDistortion;
            
            half _Brightness;
            half _Contrast;
            half _HueShift;
            half _InvertColor;
            half _Saturation;

            half _FlowDistortion;
            half _MirrorTileCount;
            half _SharpenAmount;

            half _EdgeBrightness;
            half _EdgeWidth;

#ifdef EXPERIMENTAL_ON
            sampler2D _NoiseTexture;
            float4 _NoiseTexture_TexelSize;

            half _EdgeNoiseBlend;
            half _WaveDistortion;
#endif

            inline float4 getColorWithChromaticAberration(sampler2D samp, float2 uv, fixed4 center)
            {
                float2 fromCenter = uv - float2(0.5, 0.5);
                float falloff = 1.0 - smoothstep(_ChromaticAberrationSize, _ChromaticAberrationSize - _ChromaticAberrationFalloff, length(fromCenter));
                float offset = fromCenter * falloff * _ChromaticDistortion;
                float r = tex2D(samp, uv + offset).x;
                float b = tex2D(samp, uv - offset).z;
                return float4(r, center.g, b, center.a);
            }

            inline float4 detectEdgesAndSharpen(sampler2D samp, float2 uv, float4 center, float sharpenFactor)
            {
                float4 offset = float4(_EdgeWidth * _MainTex_TexelSize.xy, 0.0, -_EdgeWidth * _MainTex_TexelSize.x);

                // Get samples for the convolution kernel.
                float3 n = tex2D(samp, uv + offset.yz).rgb;
                float3 s = tex2D(samp, uv - offset.yz).rgb;
                float3 e = tex2D(samp, uv + offset.xz).rgb;
                float3 w = tex2D(samp, uv - offset.xz).rgb;
                float3 ne = tex2D(samp, uv + offset.xy).rgb;
                float3 nw = tex2D(samp, uv + offset.wy).rgb;
                float3 se = tex2D(samp, uv - offset.wy).rgb;
                float3 sw = tex2D(samp, uv - offset.xy).rgb;

                // Sharpen
                float3 sharpened = (1.0 + 4.0 * sharpenFactor) * center - sharpenFactor * (n + w + e + s);

                // Detect Edges
                float3 luminanceConv = float3(0.2125, 0.7154, 0.0721);
                float n0 = dot(n, luminanceConv);
                float s0 = dot(s, luminanceConv);
                float e0 = dot(e, luminanceConv);
                float w0 = dot(w, luminanceConv);
                float ne0 = dot(ne, luminanceConv);
                float nw0 = dot(nw, luminanceConv);
                float se0 = dot(se, luminanceConv);
                float sw0 = dot(sw, luminanceConv);

                float x = sw0 + 2.0 * w0 + nw0 - se0 - 2.0 * w0 - ne0;
                float y = sw0 + 2.0 * s0 + se0 - nw0 - 2.0 * e0 - ne0;
                float edge = length(float2(x, y));

#ifdef EXPERIMENTAL_ON
                float2 noiseTexcoord = uv * _MainTex_TexelSize.zw * _NoiseTexture_TexelSize.xy + _Time.y * 21.7;
                float3 noise = tex2D(_NoiseTexture, noiseTexcoord);
                float3 noiseFactor = (1.0 - _EdgeNoiseBlend) + _EdgeNoiseBlend * (noise - 0.5);
                float3 color = sharpened.rgb + _EdgeBrightness * edge * noiseFactor * sharpened.rgb;
#else
                float3 color = sharpened.rgb + _EdgeBrightness * edge * sharpened.rgb;
#endif
 
                return float4(color, center.a);
            }

            // This is based on the HSL flow setting from Visions of Chaos as described in this blog.
            // https://softologyblog.wordpress.com/2016/10/24/video-feedback-simulation-version-3/
            inline float2 getHslFlow(float2 uv, float maxFlow)
            {
                const float degreesToRadians = 0.01745329251;
                float4 flowSample = tex2D(_MainTex, uv);
                float3 hsl = rgbToHsl(flowSample.xyz);
                float angle = hsl.x * degreesToRadians;
                float2 direction = float2(cos(angle), sin(angle));
                float2 flow = direction * maxFlow * hsl.z * _MainTex_TexelSize.xy;
                return flow;
            }

            fixed4 frag(v2f_customrendertexture IN) : COLOR
            {
                float2 distortedTexcoord = abs(2.0 * frac(_MirrorTileCount * 0.5 * IN.localTexcoord + 0.5) - 1.0);

                if (_FlowDistortion > 0.0)
                {
                    distortedTexcoord += getHslFlow(distortedTexcoord, _FlowDistortion);
                }

#ifdef EXPERIMENTAL_ON
                // Wave Distortion
                distortedTexcoord.y += _WaveDistortion * sin(15.708 * IN.localTexcoord.x);
#endif

                // Noise distortion
                // float frameCount = _Time.y / unity_DeltaTime.x;
                // distortedTexcoord += tex2D(_NoiseTexture, distortedTexcoord + frameCount * _NoiseTexture_TexelSize.xy).xy * _MainTex_TexelSize.xy * 4.0;

                float4 center = tex2D(_MainTex, distortedTexcoord);
                float4 colorWithCa = getColorWithChromaticAberration(_MainTex, distortedTexcoord, center);
                float4 currentColor;
                currentColor.rgb = detectEdgesAndSharpen(_MainTex, distortedTexcoord, colorWithCa, _SharpenAmount).rgb;
                currentColor.a = colorWithCa.a;
                float3 hsv = rgbToHsv(currentColor.rgb);

                // To wrap negative numbers, normally you'd use (n % m + m) % m.
                // Because n is smaller than a couple cycles, we can get away with (n + m) % m.
                hsv.x = (hsv.x + _HueShift + 360.0) % 360.0;

                hsv.y = hsv.y + _Saturation;
                currentColor.rgb = hsvToRgb(hsv) * _Brightness;

                fixed4 col = currentColor;
                col.rgb = adjustContrast(col.rgb, _Contrast);

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
