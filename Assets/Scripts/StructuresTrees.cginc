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
	int hasUpper;
};


RWStructuredBuffer<float2> _GlobalPrecomputedPositionL1;
RWStructuredBuffer<float2> _GlobalPrecomputedPositionL2;
RWStructuredBuffer<float2> _GlobalPrecomputedPositionL3;

