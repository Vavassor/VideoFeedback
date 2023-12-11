Shader "Hidden/Amazing Assets/Texture Channel Packer"
{
    Properties
    {
        _Color("", Color) = (1, 1, 1, 1)
        _MainTex("", 2D) = "" {}

        _RedTexture("", 2D) = "black"{}
        _GreenTexture("", 2D) = "black"{}
        _BlueTexture("", 2D) = "black"{}
        _AlphaTexture("", 2D) = "black"{}

        _Channels("", vector) = (0, 0, 0, 0)
        _Invert("", vector) = (0, 0, 0, 0)
        _Values("", vector) = (0, 0, 0, 0)
    }

    CGINCLUDE
    #include "UnityCG.cginc" 


    sampler2D _RedTexture;
    sampler2D _GreenTexture;
    sampler2D _BlueTexture;
    sampler2D _AlphaTexture;

    int4 _Channels;
    float4 _Invert;
    float4 _Values;
    float _IsInLinear;


    float4 frag_adj(v2f_img i) : SV_Target
    {
        float red = tex2D(_RedTexture, i.uv)[_Channels.x];
        float green = tex2D(_GreenTexture, i.uv)[_Channels.y];
        float blue = tex2D(_BlueTexture, i.uv)[_Channels.z];
        float alpha = tex2D(_AlphaTexture, i.uv)[_Channels.w];

        float4 res = float4(red, green, blue, alpha);
        res = lerp(res, float4(1, 1, 1, 1) - res, _Invert);


        res.r = _Values.r > -0.5 ? _Values.r : lerp(res.r, LinearToGammaSpaceExact(res.r), _IsInLinear);
        res.g = _Values.g > -0.5 ? _Values.g : lerp(res.g, LinearToGammaSpaceExact(res.g), _IsInLinear);
        res.b = _Values.b > -0.5 ? _Values.b : lerp(res.b, LinearToGammaSpaceExact(res.b), _IsInLinear);
        res.a = _Values.a > -0.5 ? _Values.a : lerp(res.a, LinearToGammaSpaceExact(res.a), _IsInLinear);


        return res;
    }


    ENDCG


    Subshader
    {
        ZTest Always Cull Off ZWrite Off
        Fog{ Mode off }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_adj
            #pragma target 3.0

            ENDCG
        }
    }
}
