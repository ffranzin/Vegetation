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


StructuredBuffer<float2> _GlobalPrecomputedPositionL1;
StructuredBuffer<float2> _GlobalPrecomputedPositionL2;
StructuredBuffer<float2> _GlobalPrecomputedPositionL3;


StructuredBuffer<float> _globalTreeSlopeInfo;
StructuredBuffer<float> _globalTreeHeightInfo;
StructuredBuffer<float> _globalTreeHumidityInfo;
StructuredBuffer<float> _globalTreeSensitiveInfo;
StructuredBuffer<float> _globalTreeNecessityInfo;

