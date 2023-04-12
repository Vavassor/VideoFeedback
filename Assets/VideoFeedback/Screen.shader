Shader "Unlit/Screen"
{
    Properties
    {
        _Brightness("Brightness", float) = 1
        _HueShift("Hue Shift", float) = 0
        _MainTex ("Texture", 2D) = "white" {}
        _PriorTexture("Prior Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        Tags { "RenderType" = "Opaque" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "ColorConversion.cginc"
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            half _Brightness;
            half _HueShift;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _PriorTexture;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 priorColor = tex2D(_PriorTexture, i.uv);
                float3 hsv = rgbToHsv(priorColor.rgb);
                hsv.x = (hsv.x + _HueShift + 360.0) % 360.0;
                priorColor.rgb = hsvToRgb(hsv) * _Brightness;

                fixed4 currentColor = tex2D(_MainTex, i.uv);
                fixed4 col = fixed4(lerp(priorColor.rgb, currentColor.rgb, 0.5), currentColor.a);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return priorColor;
            }
            ENDCG
        }
    }
}
