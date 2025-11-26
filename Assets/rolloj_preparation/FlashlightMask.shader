Shader "Unlit/FlashlightMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0,0,0,0.95) // Dark overlay
        _Position ("Position", Vector) = (0.5,0.5,0,0)
        _Radius ("Radius", Float) = 0.2
        _Smoothness ("Smoothness", Float) = 0.05
        _AspectRatio ("Aspect Ratio", Float) = 1.77 // Pass this from C#
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _Color;
            float4 _Position;
            float _Radius;
            float _Smoothness;
            float _AspectRatio; // New Variable

            v2f vert(appdata_full v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // 1. Correct Aspect Ratio (Fixes the Oval shape)
                float2 center = _Position.xy;
                float2 currentUV = i.uv;

                // Scale the X axis by aspect ratio to make distance calculation uniform
                center.x *= _AspectRatio;
                currentUV.x *= _AspectRatio;

                // 2. Calculate Distance
                float dist = distance(currentUV, center);

                // 3. Calculate the Flashlight Hole
                // We want 0 (transparent) inside, 1 (dark) outside.
                // smoothstep(min, max, val): 
                // If dist < Radius, we want transparency.
                float darkness = smoothstep(_Radius - _Smoothness, _Radius, dist);

                // 4. Apply Alpha
                float alpha = _Color.a * darkness;

                return half4(_Color.rgb, alpha);
            }
            ENDCG
        }
    }
}