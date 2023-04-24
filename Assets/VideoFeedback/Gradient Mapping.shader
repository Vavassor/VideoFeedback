Shader "Custom/Gradient Mapping"
{
    Properties
    {
        [Header(Main Maps)] _Video0Texture("Video 0 Texture", 2D) = "white" {}
        _Video1Texture("Video 1 Texture", 2D) = "white" {}
        _GradientMapped1Texture("Gradient Mapped 1 Texture", 2D) = "white" {}
        [Header(Gradient Mapping)] _UseGradientMapping("Use Gradient Mapping", float) = 0.0
        _GradientStop0Color("Gradient Stop 0 Color", Color) = (0,0,0,1)
        _GradientStop1Color("Gradient Stop 1 Color", Color) = (1,1,1,1)
        [Header(Camera Settings)] _ShouldClearColor("Should Clear Color", float) = 0.0
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

            #include "ColorConversion.cginc"

            sampler2D _Video0Texture;
            sampler2D _Video1Texture;
            sampler2D _GradientMapped1Texture;

            half _ShouldClearColor;
            half _UseGradientMapping;
            fixed4 _GradientStop0Color;
            fixed4 _GradientStop1Color;

            inline float4 blendOver(float4 s, float4 t)
            {
                float a0 = s.a + (1.0 - s.a) * t.a;
                float3 color = (s.a * s.rgb + (1.0 - s.a) * t.a * t.rgb) / a0;
                return float4(color, a0);
            }

            fixed4 frag(v2f_customrendertexture IN) : COLOR
            {
                fixed4 currentColor = tex2D(_Video0Texture, IN.localTexcoord);
                fixed4 priorColor = tex2D(_Video1Texture, IN.localTexcoord);

                if (_UseGradientMapping != 0.0)
                {
                    float3 value = rgbToValue(currentColor.rgb);
                    currentColor.rgb = lerp(_GradientStop0Color.rgb, _GradientStop1Color.rgb, value);
                }

                float4 col = blendOver(currentColor, priorColor);

                if (_ShouldClearColor == 0.0)
                {
                    col = blendOver(col, tex2D(_GradientMapped1Texture, IN.localTexcoord));
                }

                return col;
            }
            ENDCG
        }
    }
}
