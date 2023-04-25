#ifndef COLOR_CONVERSION_H_
#define COLOR_CONVERSION_H_

float3 hueToRgb(in float h)
{
    float r = abs(h * 6.0 - 3.0) - 1.0;
    float g = 2.0 - abs(h * 6.0 - 2.0);
    float b = 2.0 - abs(h * 6.0 - 4.0);
    return saturate(float3(r, g, b));
}

float3 hslToRgb(float3 hsl)
{
    hsl.x = clamp(hsl.x, 0.0, 360.0) / 360.0;
    float3 rgb = hueToRgb(hsl.x);
    float c = (1.0 - abs(2.0 * hsl.z - 1.0)) * hsl.y;
    return (rgb - 0.5) * c + hsl.z;
}

float3 hsvToRgb(float3 hsv)
{
    float h = clamp(hsv.x, 0.0, 360.0) / 360.0;
    float s = saturate(hsv.y);
    float v = saturate(hsv.z);
    float3 rgb = hueToRgb(h);
    return ((rgb - 1) * s + 1) * v;
}

float3 rgbToHcv(in float3 rgb)
{
    // Based on work by Sam Hocevar and Emil Persson
    float epsilon = 1e-10;
    float4 p = (rgb.g < rgb.b) ? float4(rgb.bg, -1.0, 2.0 / 3.0) : float4(rgb.gb, 0.0, -1.0 / 3.0);
    float4 q = (rgb.r < p.x) ? float4(p.xyw, rgb.r) : float4(rgb.r, p.yzx);
    float c = q.x - min(q.w, q.y);
    float h = abs((q.w - q.y) / (6.0 * c + epsilon) + q.z);
    return float3(h, c, q.x);
}

float3 rgbToHsl(in float3 rgb)
{
    float epsilon = 1e-10;
    float3 hcv = rgbToHcv(rgb);
    float l = hcv.z - hcv.y * 0.5;
    float s = hcv.y / (1.0 - abs(l * 2.0 - 1.0) + epsilon);
    return float3(360.0 * hcv.x, s, l);
}

float3 rgbToHsv(float3 rgb)
{
    float r = rgb.x;
    float g = rgb.y;
    float b = rgb.z;

    float xMax = max(max(r, g), b);
    float xMin = min(min(r, g), b);

    float delta = xMax - xMin;

    float h;
    if (delta == 0.0)
    {
        h = 0.0;
    }
    else
    {
        if (xMax == r)
        {
            h = (g - b) / delta;
        }
        else if (xMax == g)
        {
            h = (b - r) / delta + 2.0;
        }
        else if (xMax == b)
        {
            h = (r - g) / delta + 4.0;
        }

        h *= 60;

        if (h < 0.0)
        {
            h += 360.0;
        }
    }

    float s = xMax == 0.0 ? 0.0 : delta / xMax;

    return float3(h, s, xMax);
}

float rgbToValue(float3 rgb)
{
    return max(max(rgb.r, rgb.g), rgb.b);
}

#endif // COLOR_CONVERSION_H_
