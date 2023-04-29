// This approximates the actual transformation.
float srgbtoLinear(float3 color)
{
    return pow(color, 2.2);
}

// This approximates the actual transformation.
float3 linearToSrgb(float3 color)
{
    return pow(color, 0.4545454545);
}

float getLuminance(float3 linearColor)
{
    return dot(linearColor, float3(0.2126, 0.7152, 0.0722));
}

float getPerceptualLightness(float3 linearColor)
{
    float y = getLuminance(linearColor);
    if (y <= (216.0 / 24389.0))
    {
        return y * (24389.0 / 27.0);
    }
    else
    {
        return pow(y, (1.0 / 3.0)) * 116.0 - 16.0;
    }
}

float3 adjustBrightness(float3 color, float amount)
{
    const float scale = 1.5;
    float exponent = 1.0 / (1.0 + scale * amount);
    return pow(color, exponent.xxx);
}

float3 adjustContrast(float3 color, float amount)
{
    return 0.5 + amount * (color - 0.5);
}

float3 adjustExposure(float3 color, float amount)
{
    return (1.0 + color) * amount;
}

float3 adjustSaturation(float3 color, float amount)
{
    const float3 luminosityConv = float3(0.2126, 0.7152, 0.0722);
    float luminosity = dot(color, luminosityConv);
    return lerp(luminosity.xxx, color, 1.0 + amount);
}
