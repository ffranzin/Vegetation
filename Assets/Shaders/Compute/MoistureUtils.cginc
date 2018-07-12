#ifndef HUMIDITY_UTILS
#define HUMIDITY_UTILS

#define GROUP_SIZE 8
#define CURVE_BUFFER_SIZE 128

struct Parameters
{
	// Distances
	float d_meanHeight;
	float d_slope;
	float d_waterSpread;

	// Weights
	float w_height;
	float w_relativeHeight;
	float w_slope;
	float w_waterSpread;

	// Size of the Atlas Textures (TexHeight and TexWater)
	float2 s_atlas;

	// Size of the Splats and Aux textures (TexMeanH, TexSlope, TexMoisture, TexVPass, etc.)
	float2 s_splat;
};

// ************************************************ //
// Sampling Parameters
// ************************************************ //

// Distances and Weights parameters
StructuredBuffer<Parameters> Params;
// Inlfuence Curves
StructuredBuffer<float> CurveHeight;
StructuredBuffer<float> CurveRelativeHeight;
StructuredBuffer<float> CurveSlope;
StructuredBuffer<float> CurveWaterH;
StructuredBuffer<float> CurveWaterV;

// ************************************************ //
// Textures
// ************************************************ //
float2 Pos; // origin (in the Altas) from where the splats will be generated

// ************************************************ //
// Textures
// ************************************************ //

// Atlas
Texture2D<float> TexHeight;
Texture2D<float> TexWater;
// Splats
RWTexture2D<float> TexMeanH;
RWTexture2D<float> TexRelativeH;
RWTexture2D<float> TexSlope;
RWTexture2D<float> TexWSpread;
RWTexture2D<float> TexMoisture;
// Aux
RWTexture2D<float> TexVPass;
RWTexture2D<float> TexHPass;

//
//
//
inline uint2 GetIndex(float2 origin, float2 size, float2 index, float2 offset)
{
//#if !UNITY_UV_STARTS_AT_TOP
//	offset.y = -offset.y;
//#endif

	const int2 min = int2(0, 0);
	const int2 one = int2(1, 1);

	return uint2(clamp(origin + index + offset, min, (size - one)));
}

//
//
//
inline uint EvalAt(float t)
{
	const float _size = float(CURVE_BUFFER_SIZE);
	return uint(round((_size - 1.0) * clamp(0.0, 1.0, t)));
}

//
//
//
float EvalHeight(float at)
{
	return CurveHeight[EvalAt(at)] * Params[0].w_height;
}

//
//
//
float EvalRelativeHeight(float at)
{
	return CurveRelativeHeight[EvalAt(at)] * Params[0].w_relativeHeight;
}

//
//
//
float EvalSlope(float at)
{
	return CurveSlope[EvalAt(at)] * Params[0].w_slope;
}

//
//
//
float EvalWaterSpreadH(float at)
{
	return CurveWaterH[EvalAt(at)] * Params[0].w_waterSpread;
}

//
//
//
float EvalWaterSpreadV(float at)
{
	return CurveWaterV[EvalAt(at)] * Params[0].w_waterSpread;
}

#endif