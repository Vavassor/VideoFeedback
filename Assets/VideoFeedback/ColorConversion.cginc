#ifndef COLOR_CONVERSION_H_
#define COLOR_CONVERSION_H_

float3 hueToRgb(in float h)
{
    float r = abs(h * 6.0 - 3.0) - 1.0;
    float g = 2.0 - abs(h * 6.0 - 2.0);
    float b = 2.0 - abs(h * 6.0 - 4.0);
    return saturate(float3(r, g, b));
}

float3 hsvToRgb(float3 hsv)
{
    float h = clamp(hsv.x, 0.0, 360.0) / 360.0;
    float s = saturate(hsv.y);
    float v = saturate(hsv.z);
    float3 rgb = hueToRgb(h);
    return ((rgb - 1) * s + 1) * v;
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

#endif // COLOR_CONVERSION_H_
