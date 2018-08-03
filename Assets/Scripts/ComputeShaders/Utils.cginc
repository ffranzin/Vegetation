
#define INFINITY 9999999

#define BLOCK_SIZE 1000

#include "StructuresTrees.cginc"

float _globalPixelSize;


inline float2 NormalizedPos2ScaledWorldPos(float2 pos, float2 worldPosOrigin, float worldPosBoundSize)
{
	return worldPosOrigin + pos * worldPosBoundSize;
}


inline float2 NormalizedPos2AtlasCoord(float2 pos, float2 atlasOrigin, float atlasSize)
{
	return atlasOrigin + pos * atlasSize;
}


inline float2 WorldPos2NormalizedPos(float2 currentPos, float2 worldPosOrigin, float worldPosBoundSize)
{
	return (currentPos - worldPosOrigin) / worldPosBoundSize;
}


inline float Remap(float org_val, float org_min, float org_max, float new_min, float new_max)
{
	return new_min + saturate(((org_val - org_min) / (org_max - org_min)) * (new_max - new_min));
}



float2 hash( float2 p ) 
{
	p = float2(dot(p,float2(127.1,311.7)),
			   dot(p,float2(269.5,183.3)) );

	return frac(sin(p)*43758.5453123);
}


int CustomRand(float2 seed, int min, int max)
{
	float rand = frac(sin(dot(seed, float2(12.9898, 78.233))) * 43758.5453);

	float map = rand * (max - min) + min;

	return (int)map;
}


float CustomRandf(float2 seed)
{
	float rand = frac(sin(dot(seed, float2(12.9888, 78.233))) * 43758.5453);

	return rand;
}

//////////////////////////////////
///////// ATLAS HELPER  /////////
/////////////////////////////////




///Get the information on atlas receiving one position from current atlas 
inline float2 CurrentAtlasPos2UpperLevelAtlasUV(QuadTreeInfo _qti, float2 id)
{
	//Get my real world position (RWP)
	float2 uvCurrentPos		 = id / _qti.currentNodeAtlasSize;
	float2 uvCurrentWorldPos = _qti.currentNodeWorldOrigin + uvCurrentPos * _qti.currentNodeWorldSize;
	
	//Get the correspondent uv of RWP in upper level node 
	float2 uvInUpperLevel = (uvCurrentWorldPos - _qti.upperNodeWorldOrigin) / _qti.upperNodeWorldSize;

	//pixel position of RWP in upper level
	uvInUpperLevel = _qti.upperNodeAtlasOrigin + uvInUpperLevel * _qti.upperNodeAtlasSize;

	return uvInUpperLevel;
}





//Get the information in atlas upper level receiving one world positions 
float2 WorldPosition2AtlasInfoUpperLevelUV(QuadTreeInfo _qti, float2 wPos)
{
	float2 uvInUpperLevel = (wPos - _qti.upperNodeWorldOrigin) / _qti.upperNodeWorldSize;

	uvInUpperLevel = _qti.upperNodeAtlasOrigin + uvInUpperLevel * _qti.upperNodeAtlasSize;

	return uvInUpperLevel;
}




//Get the information in current atlas level receiving world positions 
float2 WorldPosition2AtlasCoord(QuadTreeInfo _qti, float2 wPos)
{
	float2 p01 = (wPos - _qti.currentNodeWorldOrigin) / _qti.currentNodeWorldSize;

	return _qti.currentNodeAtlasOrigin + p01 * _qti.currentNodeAtlasSize;
}



inline float2 WorldCoord2TexCoord(float2 pos)
{
	return pos / _globalPixelSize;
}