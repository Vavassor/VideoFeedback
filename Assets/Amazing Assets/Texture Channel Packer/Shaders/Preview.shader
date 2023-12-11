Shader "Hidden/Amazing Assets/Texture Channel Packer/Preview"
{
    Properties
    {
        _Color("", Color) = (1, 1, 1, 1)
        _MainTex("", 2D) = "" {}
    }

    CGINCLUDE
    #include "UnityCG.cginc" 

    float4 _Color;
    sampler2D _MainTex;

    int _Channel;
    int _Invert;
    float _Value;
    float _IsInLinear;


    float4 frag_adj(v2f_img i) : SV_Target
    {
        float res = tex2D(_MainTex, i.uv)[_Channel];
        res = _Invert > 0.5 ? (1 - res) : (res);
                
        res = _Value > -0.5 ? _Value : (_IsInLinear > 0.5 ? LinearToGammaSpaceExact(res) : res);

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
