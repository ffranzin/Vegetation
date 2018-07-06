#ifndef HUMIDITY_UTILS
#define HUMIDITY_UTILS

#define GROUP_SIZE 8
#define SPLAT_SIZE 1024

int4 Pos;
int2 Size;
int2 Offset;
int Distance;

StructuredBuffer<float> HeightData;
StructuredBuffer<float> WaterData;
RWStructuredBuffer<float> VPass;
RWStructuredBuffer<float> HPass;

Texture2D<float> TexHeight;
Texture2D<float> TexWater;

RWTexture2D<float> TexMeanH;
RWTexture2D<float> TexRelativeH;
RWTexture2D<float> TexSlope;
RWTexture2D<float> TexWSpread;
RWTexture2D<float> TexMoisture;

RWTexture2D<float> TexVPass;
RWTexture2D<float> TexHPass;

inline uint To1DIndex(uint i, uint j, uint width)
{
	return i * width + j;
}

inline uint2 GetIndex(int2 origin, int2 max, int2 index, int2 offset)
{
#if !UNITY_UV_STARTS_AT_TOP
	offset.y = -offset.y;
#endif

	const int2 min = int2(0, 0);

	return uint2(clamp(origin + index + offset, min, max));
}


#endif