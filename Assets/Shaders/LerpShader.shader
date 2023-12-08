Shader "Unlit/LerpShader"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
        _LerpColour("Lerp Colour", Color) = (1,1,1,1)
        _LerpAmount("Lerp Amount", Range(0.0, 1.0)) = 0.0
    }
        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }

            Cull Off
            Lighting Off
            ZWrite Off
            Blend One OneMinusSrcAlpha

            Pass
            {
                CGPROGRAM
                #pragma vertex SpriteVert
                #pragma fragment SpriteFrag
                #pragma target 2.0
                #pragma multi_compile_instancing
                #pragma multi_compile_local _ PIXELSNAP_ON
                #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

                //#include "UnityCG.cginc"
    /*
                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float4 _LerpColour;
                float _LerpAmount;

                v2f vert (appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    // sample the texture
                    fixed4 col = tex2D(_MainTex, i.uv);
                    return lerp(col, _LerpColour, _LerpAmount);
                }*/

    #include "UnityCG.cginc"

    

    CBUFFER_START(UnityPerDrawSprite)
    #ifndef UNITY_INSTANCING_ENABLED
        fixed4 _RendererColor;
        fixed2 _Flip;
    #endif
        float _EnableExternalAlpha;
    CBUFFER_END

        // Material Color.
        fixed4 _Color;

        struct appdata_t
        {
            float4 vertex   : POSITION;
            float4 color    : COLOR;
            float2 texcoord : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f
        {
            float4 vertex   : SV_POSITION;
            fixed4 color : COLOR;
            float2 texcoord : TEXCOORD0;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        inline float4 UnityFlipSprite(in float3 pos, in fixed2 flip)
        {
            return float4(pos.xy * flip, pos.z, 1.0);
        }

        float4 _LerpColour;
        float _LerpAmount;


        v2f SpriteVert(appdata_t IN)
        {
            v2f OUT;

            UNITY_SETUP_INSTANCE_ID(IN);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

            OUT.vertex = UnityFlipSprite(IN.vertex, _Flip);
            OUT.vertex = UnityObjectToClipPos(OUT.vertex);
            OUT.texcoord = IN.texcoord;
            OUT.color = IN.color * _Color * _RendererColor;

            return OUT;
        }

        sampler2D _MainTex;
        sampler2D _AlphaTex;
        fixed4 SampleSpriteTexture(float2 uv)
        {
            fixed4 color = tex2D(_MainTex, uv);
            return color;
        }

        fixed4 SpriteFrag(v2f IN) : SV_Target
        {
            fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
            fixed4 lerping = fixed4(_LerpColour.r, _LerpColour.g, _LerpColour.b, c.a);
            c = lerp(c, lerping, _LerpAmount);
            c.rgb *= c.a;
            return c;
        }

            ENDCG
        }
        }
}
