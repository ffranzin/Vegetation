// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/MyCustomTerrain" {
	Properties {
		_MainTex("HeightMap", 2D) = "white" {}
		_WaterMap("WaterMap", 2D) = "white" {}
		_Stone("Stone", 2D) = "white" {}
		_Grass("Grass", 2D) = "white" {}
		_Ground("Ground", 2D) = "white" {}
		_Water("Water", 2D) = "white" {}

		_H("H", range(1.0, 500.0)) = 150

		_L1("L1", range(0.0, 3.0)) = 0.5
		_L2("L2", range(0.0, 3.0)) = 0.5
		_L3("NormalIntesity", range(0.0, 3.0)) = 0.5
	}



	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert //tessellate:tessDistance

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 5.0

		sampler2D _MainTex;
		sampler2D _WaterMap;

		sampler2D _Grass;
		sampler2D _Ground;
		sampler2D _Stone;
		sampler2D _Water;

		struct Input {
			float2 uv_MainTex  : TEXCOORD0;
			float3 worldPos;
			float3 worldNormal;
		};

		float _L1;
		float _L2;
		float _L3;
		float _H;

		float4 _MainTex_TexelSize;
		
		float4 _WaterMap_ST;
		float4 _Grass_ST;
		float4 _Ground_ST;
		float4 _Stone_ST;
		float4 _Water_ST;

		void vert(inout appdata_base v)
		{
			float2 uv = v.texcoord.xy;
			//uv.y = 1 - uv.y;
			//uv.x = 1 - uv.x;

			float height = tex2Dlod(_MainTex, float4(uv, 0, 0));
			
			float3 n;
			float2 _PixelSizeDir = float2(1,-1);
			float bump_level = 8.0 * _L3;

			float tl = tex2Dlod(_MainTex, float4(uv + (_MainTex_TexelSize.xy * float2(-1, -1)), 0, 0)).r;
			float t = tex2Dlod(_MainTex, float4(uv + (_MainTex_TexelSize.xy * float2(0, -1)), 0, 0)).r;
			float tr = tex2Dlod(_MainTex, float4(uv + (_MainTex_TexelSize.xy * float2(1, -1)), 0, 0)).r;

			float l = tex2Dlod(_MainTex, float4(uv + (_MainTex_TexelSize.xy * float2(-1, 0)), 0, 0)).r;
			float c = tex2Dlod(_MainTex, float4(uv + (_MainTex_TexelSize.xy * float2(0, 0)), 0, 0)).r;
			float r = tex2Dlod(_MainTex, float4(uv + (_MainTex_TexelSize.xy * float2(1, 0)), 0, 0)).r;

			float bl = tex2Dlod(_MainTex, float4(uv + (_MainTex_TexelSize.xy * float2(-1, 1)), 0, 0)).r;
			float b = tex2Dlod(_MainTex, float4(uv + (_MainTex_TexelSize.xy * float2(0, 1)), 0, 0)).r;
			float br = tex2Dlod(_MainTex, float4(uv + (_MainTex_TexelSize.xy * float2(1, 1)), 0, 0)).r;

			float dX = tr + 2 * r + br - tl - 2 * l - bl;
			float dY = bl + 2 * b + br - tl - 2 * t - tr;

			// Build the normalized normal
			n = normalize(float3(-dX, 1.0f / bump_level, -dY) * float3(_PixelSizeDir.x, 1, _PixelSizeDir.y));

			v.normal.xyz = n.xzy;

			if (v.vertex.z >= 0) {
				v.vertex.z += height * _H;
			}
			else {
				v.vertex.z = -5;
				v.normal = fixed3(0,0,0);
			}
		}


		void surf (Input IN, inout SurfaceOutputStandard o) {

			float h = tex2D(_MainTex, IN.uv_MainTex);

			fixed4 col = lerp(
				tex2D(_Grass, IN.uv_MainTex * _Grass_ST.xy),
				lerp(tex2D(_Ground, IN.uv_MainTex * _Ground_ST.xy),
					tex2D(_Stone, IN.uv_MainTex * _Stone_ST.xy),
					saturate(h * _L1)),
				saturate(h * _L2));
			
			float s = length(IN.worldNormal.rb);

			col = lerp(col,
				tex2D(_Stone, IN.uv_MainTex * _Stone_ST.xy),
				saturate(s * (1.0 + _L1)));
			
			float w = tex2D(_WaterMap, IN.uv_MainTex);
			if (w > 0) {
				col = lerp(col, tex2D(_Water, IN.uv_MainTex * _Water_ST.xy), saturate(w));
				col += fixed4(-0.15, -0.15, 0.35, 0);
			}
			
			o.Albedo = col;
			o.Metallic = 0;
			o.Occlusion = 1 - s;
			o.Smoothness = w;
		}
		ENDCG
	}
	FallBack "Diffuse"
}