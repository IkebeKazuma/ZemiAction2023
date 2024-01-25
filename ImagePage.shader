Shader "Unlit/ImagePage"
{
	Properties
	{
		[PerRendererData] _MainTex("Texture", 2D) = "white" {}
		_AlphaMask("AlphaMask", Range(0, 1)) = 0.1
		_Flip("Flip",Range(-1, 1)) = 0
		_FlipOffset("Flip Offset", Range(0, 1)) = 0.0
	}
	SubShader
	{
		Tags {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType"="Plane"
        }
        LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 puv : TEXCOORD1;
				float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _AlphaMask;
			float _Flip;
			float _FlipOffset;

            bool _UseClipRect;
            float4 _ClipRect;

            bool _UseAlphaClip;

			float l2(float x)
			{
				return 1 - _Flip + 0.01 * cos(x * 1.5);
			}

			float l1(float y)
			{
				return _Flip + 0.1 * sin(y * 3);
			}

			float l0(float x)
			{
				return (x + _FlipOffset) - _Flip;
			}

			v2f vert(appdata v)
			{
				v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//コンテンツとページテクスチャ色取得
				float4 content_col = tex2D(_MainTex, i.uv);

				//L0より右の描画を無視
				float l0_y = l0(i.uv.x);
				clip(i.uv.y - l0_y);

				//範囲内ならば暗い色に
				if (i.uv.x > l1(i.uv.y) && i.uv.y < l2(i.uv.x))
					content_col = float4(0.5, 0.5, 0.5, 1);
				//else
				//	discard;

				return content_col;
			}
			ENDCG
		}
	}
}