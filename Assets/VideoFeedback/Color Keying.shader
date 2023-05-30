Shader "Custom/Color Keying"
{
    Properties
    {
        _ChromaKey("Chroma Key", Color) = (0.05, 0.63, 0.14, 1.0)
        _Similarity("Similarity", Range(0.0, 1.0)) = 0.4
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.08
        _Spill("Spill", Range(0.0, 1.0)) = 0.1
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

            float4 _ChromaKey;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _ForegroundTexture;
            float4 _ForegroundTexture_ST;
            float _Similarity;
            float _Smoothness;
            float _Spill;

            fixed4 frag(v2f_customrendertexture IN) : COLOR
            {
                float4 col = tex2D(_MainTex, TRANSFORM_TEX(IN.localTexcoord, _MainTex));
                float4 foreground = tex2D(_ForegroundTexture, TRANSFORM_TEX(IN.localTexcoord, _ForegroundTexture));
                col = ApplyChromaKey(col, _ChromaKey, _Similarity, _Smoothness, _Spill);
                col = blendOver(foreground, col);
                return col;
            }
            ENDCG
        }
    }
}
