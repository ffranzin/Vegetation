struct QuadTreeInfo
{
    float2 currentNodeAtlasOrigin;
    float currentNodeAtlasSize;
    float2 currentNodeWorldOrigin;
    float currentNodeWorldSize;

    float2 upperNodeAtlasOrigin;
    float upperNodeAtlasSize;
    float2 upperNodeWorldOrigin;
    float upperNodeWorldSize;

    float treeRadius;
	int vegLevel;
};


RWStructuredBuffer<float2> _GlobalPrecomputedPositionL1;
RWStructuredBuffer<float2> _GlobalPrecomputedPositionL2;
RWStructuredBuffer<float2> _GlobalPrecomputedPositionL3;


RWStructuredBuffer<float> _GlobalTreeSlopeInfo;
RWStructuredBuffer<float> _GlobalTreeHeightInfo;
RWStructuredBuffer<float> _GlobalTreeHumidityInfo;
RWStructuredBuffer<float> _GlobalTreeSensitiveInfo;
RWStructuredBuffer<float> _GlobalTreeNecessityInfo;


int _GlobalBufferPerTreeSize;

