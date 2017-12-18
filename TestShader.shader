Shader "TestShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_DeepPercent ("Deep level", Range (0,1)) = 0
		_MyColor ("Sea Color", COLOR ) = (1,1,1,1)
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float _DeepPercent;
			fixed4 _MyColor;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				col.r *= (1 - _DeepPercent); 
				col.g *= 0.5 + (1 - _DeepPercent)/2;
				col.b *= 0.5 + (1 - _DeepPercent)/2;
				if (col.a = 0) col = _MyColor;
				return col;
			}
			ENDCG
		}
	}
}
