Shader "Custom/MyWaterShader" 
{
	Properties 
	{
		_waterColour ("Colour", Color) = (1,1,1,1)
		_NoiseTex ("Noise text", 2D) = "white" {}
		_LightVector ("Global Light vector", Vector) = (0.0,-1.0,0.0)
	}
	
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off Lighting Off Cull Off Fog { Mode Off } Blend One Zero
		LOD 110

		GrabPass { "_GrabTexture" }
		
		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _GrabTexture;
			sampler2D _NoiseTex;
			
			fixed4 _waterColour;
			float3 _LightVector;

			struct vin_vct
			{
				//float3 normal : NORMAL;
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};

			struct v2f_vct
			{
				//float3 normal : NORMAL;
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;

				float4 position_in_world_space : TEXCOORD2;
				float4 grabUV : TEXCOORD3;
			};

			// Vertex function 
			v2f_vct vert (vin_vct v)
			{
				v2f_vct o;
				//o.normal = v.normal;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = v.texcoord;
				o.texcoord1 = v.texcoord1;

				o.position_in_world_space = mul(unity_ObjectToWorld, v.vertex);
				o.grabUV = ComputeGrabScreenPos(o.vertex);

				
				return o;
			}

			// Fragment function
			fixed4 frag (v2f_vct i) : COLOR
			{
			float time = _Time[1];
			float speed = 0.01;
			fixed4 noise = tex2D(_NoiseTex, i.texcoord);
				

				fixed4 col =  tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(i.grabUV)) * _waterColour;
				float3 codedNormal = float3(noise.r - 0.5, noise.g-0.5, noise.b-0.5);
				codedNormal = normalize(codedNormal );
				float3 lv = normalize(_LightVector);
				float k = codedNormal * lv;
				col = lerp(col, (1,1,1,1), k); 
				return col;
			}
		
			ENDCG
		} 
	}
}