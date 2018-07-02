#ifndef HUMIDITY_UTILS
#define HUMIDITY_UTILS

#define GROUP_SIZE 8

uint2 Size;
int Distance;
StructuredBuffer<float> HeightData;
StructuredBuffer<float> WaterData;
RWStructuredBuffer<float> VPass;
RWStructuredBuffer<float> HPass;
//RWStructuredBuffer<float> OutputData;

inline uint To1DIndex(uint i, uint j, uint width)
{
	return i * width + j;
}


#endif