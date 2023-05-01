// Show a fullscreen texture that's only visible in the VRChat handheld camera.
Shader "Unlit/Handheld Camera Monitor"
{
    Properties
    {
        _ActiveRadius ("Active Radius", float) = 0.5
        _MainTex ("Texture", 2D) = "white" {}
        [Toggle] _IsLetterboxed("Is Letterboxed", Float) = 1.0
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" "RenderType" = "Overlay" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" "LightMode" = "Always" "DisableBatching" = "true" }
        Cull Off
        Lighting Off
        ZTest Always
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "VRChatBuiltins.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            half _ActiveRadius;
            half _IsLetterboxed;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float2 uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv = uv;

                float3 objectPosition = mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0));
#ifdef UNITY_SINGLE_PASS_STEREO
                float3 cameraPosition = (unity_StereoWorldSpaceCameraPos[0] + unity_StereoWorldSpaceCameraPos[1]) * 0.5;
#else
                float3 cameraPosition = _WorldSpaceCameraPos;
#endif

                if ((_VRChatCameraMode == VRCHAT_CAMERA_MODE_VR_HANDHELD || _VRChatCameraMode == VRCHAT_CAMERA_MODE_DESKTOP_HANDHELD) && distance(cameraPosition, objectPosition) < _ActiveRadius)
                {
                    o.vertex = float4(float2(1.0, -1.0) * (uv * 2.0 - 1.0), 0.0, 1.0);
                }
                else
                {
                    // Rendering the vertex outside the camera hides the effect.
                    v.vertex.xyz = 1.0e25;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                }

                return o;
            }

            fixed4 sampleLetterboxed(sampler2D samp, float4 samplerTexelSize, float2 texcoord)
            {
                float screenAspectRatio = _ScreenParams.x / _ScreenParams.y;
                float textureAspectRatio = samplerTexelSize.z * samplerTexelSize.y;
                float clipTexcoord;

                if (textureAspectRatio > screenAspectRatio)
                {
                    float scaledSize = textureAspectRatio / screenAspectRatio;
                    float letterboxSize = 0.5 * (1.0 - scaledSize);
                    texcoord.y = scaledSize * texcoord.y + letterboxSize;
                    clipTexcoord = texcoord.y;
                }
                else
                {
                    float scaledSize = screenAspectRatio / textureAspectRatio;
                    float letterboxSize = 0.5 * (1.0 - scaledSize);
                    texcoord.x = scaledSize * texcoord.x + letterboxSize;
                    clipTexcoord = texcoord.x;
                }

                fixed4 col;
                col = tex2D(samp, texcoord);

                if (clipTexcoord < 0.0 || clipTexcoord > 1.0)
                {
                    col.rgb = fixed4(0.0, 0.0, 0.0, 1.0);
                }

                return col;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                if (_IsLetterboxed == 1.0)
                {
                    return sampleLetterboxed(_MainTex, _MainTex_TexelSize, i.uv);
                }
                else
                {
                    // Stretch the image to fit the screen.
                    return tex2D(_MainTex, i.uv);
                }
            }
            ENDCG
        }
    }
}
