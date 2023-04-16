Shader "Custom/Video Mixer"
{
    Properties
    {
        _Brightness("Brightness", float) = 1
        _ChromaticAberrationFalloff("Chromatic Aberattion Falloff", Range(0, 1)) = 0.65
        _ChromaticAberrationSize("Chromatic Aberattion Size", Range(0, 1)) = 0.9
        _ChromaticDistortion("Chromatic Distortion", float) = 0.01
        _HueShift("Hue Shift", float) = 0
        _InvertColor("Invert Color", float) = 0
        _MainTex("Texture", 2D) = "white" {}
        _WaveDistortion("WaveDistortion", Range(-0.04, 0.04)) = 0.0
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

            half _Brightness;
            half _ChromaticAberrationFalloff;
            half _ChromaticAberrationSize;
            half _ChromaticDistortion;
            half _InvertColor;
            half _HueShift;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            half _WaveDistortion;

            inline fixed4 getColorWithChromaticAberration(float2 uv)
            {
                float2 fromCenter = uv - float2(0.5, 0.5);
                float falloff = 1.0 - smoothstep(_ChromaticAberrationSize, _ChromaticAberrationSize - _ChromaticAberrationFalloff, length(fromCenter));
                float offset = fromCenter * falloff * _ChromaticDistortion;
                fixed4 center = tex2D(_MainTex, uv);
                fixed r = tex2D(_MainTex, uv + offset).x;
                fixed b = tex2D(_MainTex, uv - offset).z;
                return fixed4(r, center.g, b, center.a);
            }

            fixed4 frag(v2f_customrendertexture IN) : COLOR
            {
                float2 distortedTexcoord = IN.localTexcoord;
                distortedTexcoord.y += _WaveDistortion * sin(15.708 * IN.localTexcoord.x);

                fixed4 currentColor = getColorWithChromaticAberration(distortedTexcoord);
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
