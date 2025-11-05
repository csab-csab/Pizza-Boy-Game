Shader "Custom/DayNightBlend"
{
    Properties
    {
        _Blend ("Blend", Range (0, 1)) = 0.5
        _DaySkybox ("Day Skybox", CUBE) = "" {}
        _NightSkybox ("Night Skybox", CUBE) = "" {}
    }

    SubShader
    {
        Tags { "Queue" = "Background" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma exclude_renderers gles xbox360 ps3
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float4 grabUV : TEXCOORD0;
            };

            uniform float _Blend;
            uniform samplerCUBE _DaySkybox;
            uniform samplerCUBE _NightSkybox;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.grabUV = ComputeGrabScreenPos(o.pos);
                return o;
            }

            fixed4 frag (v2f i) : COLOR
            {
                fixed4 dayColor = texCUBE(_DaySkybox, i.grabUV);
                fixed4 nightColor = texCUBE(_NightSkybox, i.grabUV);
                fixed4 result = lerp(dayColor, nightColor, _Blend);
              // fixed4 result = lerp(fixed4(1, 0, 0, 1), fixed4(0, 0, 1, 1), _Blend);
 
               
               return result;
            }
            ENDCG
        }
    }
}
