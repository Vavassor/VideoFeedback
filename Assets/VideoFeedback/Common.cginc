#define PI 3.1415926538
#define TAU 6.2831853072

inline float unlerp(float a, float b, float t)
{
    return (t - a) / (b - a);
}

inline float remapRange(float a0, float b0, float a1, float b1, float t)
{
    return lerp(a1, b1, unlerp(a0, b0, t));
}
