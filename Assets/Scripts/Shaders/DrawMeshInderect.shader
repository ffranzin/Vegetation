Shader "Custom/DrawMeshInderect" {
	Properties {
		
	}
	SubShader {
        Tags { "RenderType"="Opaque"}
        LOD 200
		Cull Off
        CGPROGRAM

		

        // Physically based Standard lighting model
        #pragma surface surf Standard addshadow fullforwardshadows
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup

        float4 _Color;
		int _myIndexInTreePool;
		float TERRAIN_HEIGHT_MULTIPLIER;

		sampler2D _HeightMap;
		

        struct Input {
            float2 uv_MainTex : TEXCOORD0;
        };
		
		
		sampler2D _MainTex;

		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			UNITY_DECLARE_TEX2D(_positionsTexture);
		#endif


        void setup()
        {
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				
				int3 posUV = int3(unity_InstanceID, _myIndexInTreePool, 0);

				half2 data = _positionsTexture.Load(posUV).rg;

				float2 uv = data.xy / 512;

				float h = tex2Dlod(_HeightMap, float4(uv.x, uv.y, 0, 0)).x * TERRAIN_HEIGHT_MULTIPLIER;
				
				//h = 1;
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

			//o.Albedo = tex2D(_MainTex, IN.uv_MainTex);

            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}