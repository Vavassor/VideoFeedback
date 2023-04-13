Shader "Custom/Video Mixer"
{
    Properties
    {
        _Brightness("Brightness", float) = 1
        _HueShift("Hue Shift", float) = 0
        _MainTex("Texture", 2D) = "white" {}
        _PriorTexture("Prior Texture", 2D) = "white" {}
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

            half _Brightness;
            half _HueShift;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _PriorTexture;

            fixed4 frag(v2f_customrendertexture IN) : COLOR
            {                
                fixed4 priorColor = tex2D(_PriorTexture, IN.localTexcoord);
                float3 hsv = rgbToHsv(priorColor.rgb);
                hsv.x = (hsv.x + _HueShift + 360.0) % 360.0;
                priorColor.rgb = hsvToRgb(hsv) * _Brightness;

                fixed4 currentColor = tex2D(_MainTex, IN.localTexcoord);
                fixed4 col = fixed4(lerp(priorColor.rgb, currentColor.rgb, 0.5), currentColor.a);

                return col;
            }
            ENDCG
        }
    }
}
