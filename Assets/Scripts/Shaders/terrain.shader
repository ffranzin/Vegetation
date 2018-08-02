Shader "Custom/terrain"
{
	Properties
	{
		_MainTex ("HeightMap", 2D) = "white" {}
		_WaterMap("WaterMap", 2D) = "white" {}
		_Stone("Stone", 2D) = "white" {}
		_Grass("Grass", 2D) = "white" {}
		_Ground("Ground", 2D) = "white" {}
		_Water("Water", 2D) = "white" {}

		_H("H", range(1.0, 500.0)) = 150

		_L1("L1", range(0.0, 3.0)) = 0.5
		_L2("L2", range(0.0, 3.0)) = 0.5
		_L3("L3", range(0.0, 3.0)) = 0.5
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode"="ForwardBase"}
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
			};

			sampler2D _MainTex;
			sampler2D _WaterMap;
			sampler2D _Grass;
			sampler2D _Ground;
			sampler2D _Stone;
			sampler2D _Water;

			float _L1;
			float _L2;
			float _L3;
			float _H;

			float4 _MainTex_TexelSize;

			float4 _MainTex_ST;
			float4 _Grass_ST;
			float4 _Ground_ST;
			float4 _Stone_ST;
			float4 _Water_ST;
						
			v2f vert (appdata v)
			{
				v2f o;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				float height = tex2Dlod(_MainTex, float4(o.uv, 0, 0)).r;

				float3 n;
				float2 _PixelSizeDir = float2(1,1);
				float bump_level = 8.0 * _L3;
				
				float tl = tex2Dlod(_MainTex, float4(o.uv + (_MainTex_TexelSize.xy * float2(-1, -1)), 0, 0)).r;
				float t  = tex2Dlod(_MainTex, float4(o.uv + (_MainTex_TexelSize.xy * float2(0, -1)), 0, 0)).r;
				float tr = tex2Dlod(_MainTex, float4(o.uv + (_MainTex_TexelSize.xy * float2(1, -1)), 0, 0)).r;

				float l = tex2Dlod(_MainTex, float4(o.uv + (_MainTex_TexelSize.xy * float2(-1, 0)), 0, 0)).r;
				float c = tex2Dlod(_MainTex, float4(o.uv + (_MainTex_TexelSize.xy * float2(0, 0)), 0, 0)).r;
				float r = tex2Dlod(_MainTex, float4(o.uv + (_MainTex_TexelSize.xy * float2(1, 0)), 0, 0)).r;

				float bl = tex2Dlod(_MainTex, float4(o.uv + (_MainTex_TexelSize.xy * float2(-1, 1)), 0, 0)).r;
				float b = tex2Dlod(_MainTex, float4(o.uv + (_MainTex_TexelSize.xy * float2(0, 1)), 0, 0)).r;
				float br = tex2Dlod(_MainTex, float4(o.uv + (_MainTex_TexelSize.xy * float2(1, 1)), 0, 0)).r;

				float dX = tr + 2 * r + br - tl - 2 * l - bl;
				float dY = bl + 2 * b + br - tl - 2 * t - tr;

				// Build the normalized normal
				n = normalize(float3(-dX, 1.0f / bump_level, -dY) * float3(_PixelSizeDir.x, 1, _PixelSizeDir.y));

				o.normal = n;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.vertex.y += height * -_H;

				if (v.vertex.x < 0)
					o.vertex.y = 0;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float w = tex2D(_WaterMap, i.uv);

				float h = tex2D(_MainTex, i.uv * _MainTex_ST.xy);
								
				fixed4 col = lerp(
						tex2D(_Grass, i.uv * _Grass_ST.xy),
						lerp(tex2D(_Ground, i.uv * _Ground_ST.xy),
							 tex2D(_Stone, i.uv * _Stone_ST.xy), 
							 saturate(h * _L1 + (i.normal.r + i.normal.b) * 10)),
					saturate(h * _L2));

				if (w > 0) {
					col = lerp(col, tex2D(_Water, i.uv * _Water_ST.xy) * w, w);
					col += fixed4(-0.2, -0.2, 0.6, 0) * w;
				}


				col *= max(0.0, dot(_WorldSpaceLightPos0.xyz, i.normal)) * _LightColor0;

				if (i.uv.x = 0) col = fixed4(0, 0, 0, 1);

				//col = fixed4(i.normal.xyz, 1);

				return col;
			}
			ENDCG
		}
	}
}
