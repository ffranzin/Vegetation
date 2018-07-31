// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/MyCustomTerrain" {
	Properties {
		_MainTex("GroundTex", 2D) = "white" {}
		_Stone("stone", 2D) = "white" {}
		_Grass("grass", 2D) = "white" {}
		_ground("ground", 2D) = "white" {}
		_splat("splat", 2D) = "white" {}
		_splatMontain("splatM", 2D) = "white" {}
		_WaterMap("Water", 2D) = "white" {}
	}



	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert //tessellate:tessDistance

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 5.0
		#include "Tessellation.cginc"

		sampler2D _MainTex;
		sampler2D _WaterMap;

		sampler2D _Grass;
		sampler2D _ground;
		sampler2D _Stone;
		sampler2D _splat;
		sampler2D _splatMontain;

		struct Input {
			float2 uv_MainTex  : TEXCOORD0;
			float3 worldPos;
			//float3 normal : NORMAL;
		};


		uniform sampler2D _heightMapAux;
		
		uniform sampler2D _debugMap;



		uniform float TERRAIN_HEIGHT_MULTIPLIER;


		void vert(inout appdata_full v)
		{
			float2 uv = v.texcoord.xy;
			//uv.y = 1 - uv.y;
			//uv.x = 1 - uv.x;

			float height = tex2Dlod(_heightMapAux, float4(uv, 0, 0));
			float w = tex2Dlod(_WaterMap, float4(uv, 0, 0)).r;

			if(v.vertex.z >= 0)
				v.vertex.z += height * TERRAIN_HEIGHT_MULTIPLIER;//- smoothstep(0.1, 1, w) * 4;
			else
				v.vertex.z = -5;
		}



		void surf (Input IN, inout SurfaceOutputStandard o) {

			float4 color = float4(0,0,1,1); 

			float2 uv = IN.uv_MainTex;

			float4 splat = tex2D(_splat, uv * 5);
			float4 splatMontain = tex2D(_splatMontain, uv );


			float4 grass =  tex2D(_Grass, uv * 5 );
			float4 ground = tex2D(_ground, uv  * 20);
			float4 stone = tex2D(_Stone, uv *1);

			color = lerp(ground, grass, splat.r);
			
			if(splatMontain.r > 0.1)
				color = lerp(color, stone, 2 * splatMontain.r);

			float4 water = tex2D(_WaterMap, uv);
			if(water.r > 0.1)	color = float4(0,0,1,1);


			o.Albedo = color.rgb;
		}
		ENDCG
	}
	FallBack "Diffuse"
}




/*
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


*/