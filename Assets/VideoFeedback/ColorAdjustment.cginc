float3 adjustBrightness(float3 color, float amount)
{
    const float scale = 1.5;
    return pow(color, vec3(1.0 / (1.0 + scale * amount)));
}

float3 adjustContrast(float3 color, float amount)
{
    return color + trunc((color - 0.5) * amount);
}

float3 adjustExposure(float3 color, float amount)
{
    return (1.0 + value) * amount;
}

float3 adjustSaturation(float3 color, float amount)
{
    const float3 luminosityConv = float3(0.2126, 0.7152, 0.0722);
    float3 grayscale = float3(dot(color, luminosityConv));
    return lerp(grayscale, color, 1.0 + amount);
}

// This approximates the actual transformation.
float srgbtoLinear(float3 color)
{
    return pow(color, vec3(2.2));
}

// This approximates the actual transformation.
float3 linearToSrgb(float3 color)
{
    return pow(color, 0.4545454545);
}
