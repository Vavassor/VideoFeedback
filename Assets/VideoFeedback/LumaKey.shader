Shader "Custom/Luma Key"
{
    Properties
    {
        _LuminanceMin("Luminance Min", Range(0.0, 1.0)) = 0.0
        _MinSmoothness("Min Smoothness", Range(0.0, 1.0)) = 0.0
        _LuminanceMax("Luminance Max", Range(0.0, 1.0)) = 1.0
        _MaxSmoothness("Max Smoothness", Range(0.0, 1.0)) = 0.0
        _MainTex("Background Texture", 2D) = "white" {}
        _ForegroundTexture("Foreground Texture", 2D) = "white" {}
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

            #include "Color Keying.cginc"

            float _LuminanceMin;
            float _LuminanceMax;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _ForegroundTexture;
            float4 _ForegroundTexture_ST;
            float _MinSmoothness;
            float _MaxSmoothness;

            fixed4 frag(v2f_customrendertexture IN) : COLOR
            {
                float4 col = tex2D(_MainTex, TRANSFORM_TEX(IN.localTexcoord, _MainTex));
                float4 foreground = tex2D(_ForegroundTexture, TRANSFORM_TEX(IN.localTexcoord, _ForegroundTexture));
                col = ApplyLuminanceKey(col, _LuminanceMin, _LuminanceMax, _MinSmoothness, _MaxSmoothness);
                col = blendOver(foreground, col);
                return col;
            }
            ENDCG
        }
    }
}
