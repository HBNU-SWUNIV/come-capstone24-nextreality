// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SpriteGradient" {
    Properties {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _direction("Direction", Range(0, 1)) = 0
        _color1 ("Color 1", Color) = (1, 0.5, 0.5, 1)
        _color2 ("Color 2", Color) = (0.5, 1, 1, 1)
    }
    SubShader {
        Tags {"Queue"="Transparent"  "IgnoreProjector"="True"}
        LOD 100
        ZWrite Off
        Pass {        
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            sampler2D _MainTex;
            float4 _color1;
            float4 _color2; 
            float4 _finalColor;
            float _direction;
            
            float _HorizontalSpeed;
            float _VerticalSpeed;

            struct appdata {
                float4 vertex : POSITION;
                float4 tex : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 vertex : TEXCOORD;
                float4 pos : POSITION;
                float4 color : COLOR;
            };

            v2f vert(appdata v, v2f o) {
                o.pos = UnityObjectToClipPos(v.vertex);
                o.vertex = v.tex;
                return o;
            };

            half4 frag(v2f i) : COLOR {
				_finalColor = ((_color1*i.vertex.y*_direction+_color1*i.vertex.x*(1-_direction))+(_color2*(1-i.vertex.y)*_direction+_color2*(1-i.vertex.x)*(1-_direction)));
                return _finalColor;
            };
            ENDCG
        }
    } 
    FallBack "Diffuse"
}