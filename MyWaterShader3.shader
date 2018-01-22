Shader "Custom/MyWaterShader3"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SeaColor("Sea color", Color) = (0,0.5,1,1)
		_SkyColor("Sky color", Color) = (0.75, 0.89, 1, 1)
		_SunraysDirection("Sunrays direction", Vector) = (-1,-1,-1)
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "RenderType"="Transparent" }
		Cull Back
		LOD 100

		Zwrite off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"


			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float4 grabUV : TEXCOORD3;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _SeaColor;
			fixed4 _SkyColor;
			float3 _SunraysDirection;
			sampler2D _GrabTexture;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal = v.normal;
				o.grabUV = ComputeGrabScreenPos(o.vertex);
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(i.grabUV));
				float3 reflectedLight = reflect(_SunraysDirection, i.normal);
				float time = _Time[1];
				float reflectionCoefficient =  dot(reflectedLight, i.normal) / length(reflectedLight);
				col = lerp(_SeaColor, (1,1,1,1), reflectionCoefficient);
				col.a = 0.95;
				return col;
			}
			ENDCG
		}
	}
}
