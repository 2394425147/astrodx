Shader "AstroDX/Note"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _GradientTex ("Gradient Texture", 2D) = "white" {}
        
        _Color ("Tint", Color) = (1,1,1,1)
        _ShadowColor ("Shadow Tint", Color) = (0,0,0,1)
        _Grayscale("Grayscale", Range(0, 1)) = 0
        [MaterialToggle] _Shine ("Shine", Float) = 0
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                fixed4 vertex : POSITION;
                fixed4 color : COLOR;
                fixed2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                fixed4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                fixed2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            fixed _Shine;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif

                return OUT;
            }

            sampler2D _MainTex;
            sampler2D _GradientTex;
            sampler2D _AlphaTex;
            fixed4 _ShadowColor;
            fixed _AlphaSplitEnabled;
            fixed _Grayscale;

            fixed4 sample_sprite_texture(float2 uv)
            {
                const fixed4 color = tex2D(_MainTex, uv);

                // setting to 0.004 and 0.996 prevents getting wrong texture edge colors 
                const fixed gradient_position = lerp(0.005, 0.995, saturate(color.r * 2 - 1));
                const fixed4 gradient_map = tex2D(_GradientTex, fixed2(gradient_position, 0));

                #if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				if (_AlphaSplitEnabled)
				{
				    color.a = tex2D (_AlphaTex, uv).r;
				}
                #endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

                // choke starts at 0.1 higher than the gradient map threshold
                // to allow a smoother color blending (see touch)
                const fixed red_choke = smoothstep(0, 0.5, color.r);
                
                const fixed3 shadow_color = fixed3(color.b * _ShadowColor.rgb);
                
                fixed3 sum = gradient_map * red_choke + shadow_color * _ShadowColor.a * color.b;
                sum = color.g.xxxx + sum * (1 - color.g);
                
                return fixed4(sum.rgb, color.a);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = sample_sprite_texture(IN.texcoord);

                //Rough human eye adjusted grayscale computation
                const fixed mono_rgb = 0.299 * c.r + 0.587 * c.g + 0.114 * c.b;

                c.rgb *= IN.color.rgb;

                const fixed shine_multiplier = (sin(_Time.y * 16) * 0.5 + 0.5) * _Shine;
                c.rgb += shine_multiplier.rrr * 0.2;
                c.rgb *= 1 + shine_multiplier.rrr * 0.3;

                const fixed3 out_color = lerp(c.rgb, mono_rgb.rrr, _Grayscale);
                return fixed4(out_color, c.a * IN.color.a);
            }
            ENDCG
        }
    }
}