Shader "Custom/BottomShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MyColor("Fog Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("Bump map", 2D) = "grey" {}
		_Bumpness ("Bump strength", Range(0,5)) = 1
		_LightLimit("LightlessDepth", float) = -2000
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
	
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;

		struct Input {
          float2 uv_MainTex;
          float2 uv_BumpMap;
          float3 worldPos;
      };


		fixed4 _Color, _MyColor;
		float _LightLimit;
		float _Bumpness;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color ;
			float depth = IN.worldPos.y / _LightLimit;
			if (depth <= 0 ) depth = 0;
			else {if (depth >= 1) depth = 1;}
			c.r *= 1 - depth;
			c.g *= 0.1 + (1 - depth)  * 0.9 ;
			c.b *= 0.1 + (1 - depth)  * 0.9 ;
			o.Albedo = c;
			//o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			// Metallic and smoothness come from slider variables
			o.Alpha =0.5;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
