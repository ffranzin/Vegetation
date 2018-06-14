// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/MyCustomTerrain" {
	Properties {
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}

		_NoiseTex("NoiseTex", 2D) = "white" {}

		_GrassTex("GrassTex", 2D) = "white" {}
		_GrassNormalTex("GrassNormalTex", 2D) = "white" {}

		_GroundTex("GroundTex", 2D) = "white" {}
		_GroundNormalTex("GroundNormalTex", 2D) = "white" {}

		_HeightMap("HeightMap", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_EdgeLength("EdgeLength", Range(1,100)) = 5.0
	}



	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert tessellate:tessDistance

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		#include "Tessellation.cginc"

		sampler2D _MainTex;
		sampler2D _NoiseTex;
		sampler2D _HeightMap;
		
		sampler2D _GroundTex;
		sampler2D _GrassTex;

		sampler2D _GroundNormalTex;
		sampler2D _GrassNormalTex;


		struct Input {
			float2 uv_MainTex  : TEXCOORD0;
			float3 worldPos;
			//float3 normal : NORMAL;

		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _EdgeLength;

		uniform float TERRAIN_HEIGHT_MULTIPLIER;
		uniform float TERRAIN_HEIGHT_LAKE;
		uniform float ROAD_WIDTH;
		uniform float4 ROAD_SEGMENTS[100];
		uniform int ROAD_SEGMENTS_COUNT;

		float4 tessDistance (appdata_full v0, appdata_full v1, appdata_full v2) 
		{
            float minDist = 1.0;
            float maxDist = 25.0;
            return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, minDist, maxDist, 10000);
        }


		float4 tessDistance1(appdata_full v0, appdata_full v1, appdata_full v2)
		{
			return UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, 1);
		}


		float DistanceLinePoint(float2 l0, float2 l1, float2 p)
		{
			float2 dir1 = (l1 - l0);
			float2 dir2 = (l0 - l1);

			float2 l0p = (p - l0);
			float2 l1p = (p - l1);

			float dot1 = dot(l0p, normalize(dir1));
			float dot2 = dot(l1p, normalize(dir2));

			if (dot1 < 0 || dot2 < 0) return 99999;

			float2 projection = l0 + normalize(dir1) * dot1;

			return length(projection - p);
		}


		bool IsInsideRoad(float2 pos)
		{
			for (int i = 0; i < ROAD_SEGMENTS_COUNT; i++)
				if (DistanceLinePoint(ROAD_SEGMENTS[i].xy, ROAD_SEGMENTS[i].zw, pos) < ROAD_WIDTH)
					return true;
			return false;
		}



		void vert(inout appdata_full v)
		{
			float h = TERRAIN_HEIGHT_MULTIPLIER;
			float2 wPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xz;

			if (IsInsideRoad(wPos))
				h -= 3;
			
			v.vertex.z += tex2Dlod(_HeightMap, float4(v.texcoord.xy, 0, 0)).r *TERRAIN_HEIGHT_MULTIPLIER;
		}



		void surf (Input IN, inout SurfaceOutputStandard o) {

			float4 c; 
			float4 n;

			c = tex2D(_GroundTex, IN.uv_MainTex * 15);
			/*
			if (IsInsideRoad(IN.worldPos.xz))
			{
				c = tex2D(_RoadTex, IN.uv_MainTex * 100);
				n = tex2D(_RoadNormalTex, IN.uv_MainTex * 100);
			}
			else
			{
				float4 grass = tex2D(_GrassTex, IN.uv_MainTex * 50);
				float4 grassNormal = tex2D(_GrassNormalTex, IN.uv_MainTex * 50);

				float4 ground = tex2D(_GroundTex, IN.uv_MainTex * 50);
				float4 groundNormal = tex2D(_GroundNormalTex, IN.uv_MainTex * 50);

				float noise = tex2D(_NoiseTex, IN.uv_MainTex * 2).r;
				float h = tex2D(_HeightMap, IN.uv_MainTex).r;

				c = lerp(ground, grass, noise);
				n = lerp(groundNormal, grassNormal, noise);
			}
			*/
			o.Albedo = c.rgb;
			//o.Albedo = float3(tex2D(_HeightMap, IN.uv_MainTex).r, 0, 0);
			//o.Normal += n.rgb;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
