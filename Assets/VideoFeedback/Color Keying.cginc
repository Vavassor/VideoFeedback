#ifndef COLOR_KEYING_CGINC_
#define COLOR_KEYING_CGINC_

float getLuminance(float3 linearColor)
{
    return dot(linearColor, float3(0.2126, 0.7152, 0.0722));
}

float2 rgbToUv(float3 rgb)
{
    return float2(
        rgb.r * -0.169 + rgb.g * -0.331 + rgb.b * 0.5 + 0.5,
        rgb.r * 0.5 + rgb.g * -0.419 + rgb.b * -0.081 + 0.5
    );
}

inline float4 blendOver(float4 s, float4 t)
{
    float a0 = s.a + (1.0 - s.a) * t.a;
    float3 color = (s.a * s.rgb + (1.0 - s.a) * t.a * t.rgb) / a0;
    return float4(color, a0);
}

float4 ApplyChromaKey(float4 color0, float4 keyColor, float similarity, float smoothness, float spill)
{
    float chromaDistance = distance(rgbToUv(keyColor.rgb), rgbToUv(color0.rgb));
    float baseMask = chromaDistance - similarity;
    float fullMask = pow(saturate(baseMask / smoothness), 1.5);
    float spillVal = pow(saturate(baseMask / spill), 1.5);

    float desat = saturate(getLuminance(color0.rgb));
    float4 keyRemovedColor = float4(lerp(desat.xxx, color0.rgb, spillVal), color0.a * fullMask);

    return keyRemovedColor;
}

float4 ApplyLuminanceKey(float4 color0, float luminanceMin, float luminanceMax, float minSmoothness, float maxSmoothness)
{
    float4 premultipliedColor0 = float4(max(color0.rgb / color0.a, float3(0.0, 0.0, 0.0)), color0.a);
    float luminance = getLuminance(premultipliedColor0.rgb);

    float low = smoothstep(luminanceMin, luminanceMin + minSmoothness, luminance);
    float high = 1.0 - smoothstep(luminanceMax - maxSmoothness, luminanceMax, luminance);
    float mask = high * low;

    float4 keyRemovedColor = float4(premultipliedColor0.rgb, mask);

    return keyRemovedColor;
}

#endif // COLOR_KEYING_CGINC_
