Shader "Custom/DrawMeshInderect" {
	Properties {
		
	}
	SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model
        #pragma surface surf Standard addshadow fullforwardshadows
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup

        float4 _Color;
		float _terrainSize;
		int _myPositionArrayIndex;
		float TERRAIN_HEIGHT_MULTIPLIER;

		sampler2D _HeightMap;

        struct Input {
            float2 uv_MainTex : TEXCOORD0;
        };

		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			StructuredBuffer<float2> _positionsPerTreeIndexBuffer;
		#endif

		#define NaN 0.0/0.0


        void setup()
        {
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				
				int index = _myPositionArrayIndex + unity_InstanceID;

				float2 data = _positionsPerTreeIndexBuffer[index];
				//data = (NaN, NaN);
				
				float2 uv = data.xy / 1024;

				float h = tex2Dlod(_HeightMap, float4(uv.x, uv.y, 0, 0)).x * TERRAIN_HEIGHT_MULTIPLIER;
				float scale = 1;

				unity_ObjectToWorld._11_21_31_41 = float4(scale, 0, 0, 0);
				unity_ObjectToWorld._12_22_32_42 = float4(0, scale, 0, 0);
				unity_ObjectToWorld._13_23_33_43 = float4(0, 0, scale, 0);
				unity_ObjectToWorld._14_24_34_44 = float4(data.x, h, data.y, 1);
				unity_WorldToObject = unity_ObjectToWorld;
				unity_WorldToObject._14_24_34 *= -1;
				unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
			#endif
        }
		

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = _Color;
			
			o.Albedo = c.rgb;

            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}