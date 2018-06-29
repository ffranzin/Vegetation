#ifndef HUMIDITY_UTILS
#define HUMIDITY_UTILS

inline half Calc(uint x)
{
	return half((x & 25) / 25.0);
}

inline uint To1DIndex(uint i, uint x, uint width)
{
	return i * width + j;
}


#endif