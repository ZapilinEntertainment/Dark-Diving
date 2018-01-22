Shader "TestShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_DeepPercent ("Deep level", Range (0,1)) = 0
		_MyColor ("Sea Color", COLOR ) = (1,1,1,1)
		_DeltaPos("Sea Render y-axis deltapos", int) = 0
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
			int _DeltaPos;
			fixed4 _MyColor;


			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				col.r *= (1 - _DeepPercent); 
				col.g *= 0.5 + (1 - _DeepPercent) * 0.5;
				col.b *= 0.5 + (1 - _DeepPercent) * 0.5;
				if (_DeepPercent > 0) {
					col.r = 1 - (1 - col.r)*(1-_MyColor.r);
				    col.g = 1 - (1 - col.g)*(1-_MyColor.g);
				    col.b = 1 - (1 - col.b)*(1-_MyColor.b);


				    //float time = _Time[1];
				    //float2 waterDisplacement = (0, sin(time)/100.0);				
					//float2 newUV = i.uv.xy + waterDisplacement;
					//fixed4 col2 = tex2D(_MainTex, newUV);
					//col *= col2;
				}
				return col;
			}
			ENDCG
		}
	}
}
