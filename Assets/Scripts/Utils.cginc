
inline float2 NormalizedPosToScaledWorldPos(float2 pos, float2 worldPosOrigin, float worldPosBoundSize)
{
	return worldPosOrigin + pos * worldPosBoundSize;
}

inline float2 WorldPosToNormalizedPos(float2 currentPos, float2 worldPosOrigin, float worldPosBoundSize)
{
	return (currentPos - worldPosOrigin) / worldPosBoundSize;
}


inline float Remap(float org_val, float org_min, float org_max, float new_min, float new_max)
{
	return new_min + saturate(((org_val - org_min) / (org_max - org_min)) * (new_max - new_min));
}



float2 hash( float2 p ) // replace this by something better
{
	p = float2(dot(p,float2(127.1,311.7)),
			   dot(p,float2(269.5,183.3)) );

	return frac(sin(p)*43758.5453123);
}


