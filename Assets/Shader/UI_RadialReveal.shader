Shader "UI/RadialReveal"
{
    Properties
    {
        _Radius ("Radius (0~0.5)", Range(-0.1, 1.5)) = 0
        _Softness ("Edge Softness", Range(0, 0.5)) = 0.08
        _Center ("Center (UV 0~1)", Vector) = (0.5, 0.5, 0, 0)
        _Color ("Tint", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags{ "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };

            float _Radius, _Softness; float4 _Center; float4 _Color;

            v2f vert(appdata v){ v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color; return o; }

            fixed4 frag(v2f i):SV_Target
            {
                float2 uv=i.uv;
                float2 d = uv - _Center.xy;
                float aspect = _ScreenParams.x / max(1.0,_ScreenParams.y);
                d.x *= aspect;
                float dist = length(d);

                float e0 = _Radius - _Softness*0.5;
                float e1 = _Radius + _Softness*0.5;
                float a = smoothstep(e0, e1, dist); // 원 안 투명(0), 밖 검정(1)

                fixed4 col = _Color;
                col.a = saturate(a);
                return col;
            }
            ENDHLSL
        }
    }
}
