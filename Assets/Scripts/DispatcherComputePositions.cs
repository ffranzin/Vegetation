
using System.Runtime.InteropServices;
using UnityEngine;

public class DispatcherComputePositions : MonoBehaviour
{

    public ComputeShader computePositions;
    static ComputeShader m_computePositions;

    public ComputeShader computeSplat;
    static ComputeShader m_computeSplat;

    static MoistureDistribuition moisture;

    public Texture2D aaa;
    public static RenderTexture bbbb;


    public static void ComputePositions(_QuadTree qt, float radius, int vegLevel)
    {
        if (qt == null || qt.atlasPage == null) return;

        int computePosKernel = m_computePositions.FindKernel("ComputePosition");
        int currentDFKernel = m_computeSplat.FindKernel("ComputeSplat");
        int transferDFKernel = m_computeSplat.FindKernel("TransferDF");
        int clearKernel = m_computeSplat.FindKernel("Clear");

       // moisture.CalculateAll(new Vector2Int((int)(qt.bound.min.x / TerrainManager.PIXEL_WIDTH),
         //                                    (int)(qt.bound.min.z / TerrainManager.PIXEL_HEIGHT)));
        moisture.CalculateAll(new Vector2Int(0,0));

        //Debug.Log(new Vector2Int((int)(qt.bound.min.x / TerrainManager.PIXEL_WIDTH),
        //                                    (int)(qt.bound.min.x / TerrainManager.PIXEL_HEIGHT)));

        QuadTreeInfo qtInfo = qt.QuadTreeInfo(vegLevel, radius);

        ComputeBuffer qtInfoBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(QuadTreeInfo)));
        qtInfoBuffer.SetData(new QuadTreeInfo[1] { qtInfo });

        m_computePositions.SetBuffer(computePosKernel, "_positionsBuffer", GlobalManager.positionsBuffer);
        m_computeSplat.SetBuffer(currentDFKernel, "_positionsBuffer", GlobalManager.positionsBuffer);
        

        m_computePositions.SetBuffer(computePosKernel,  "_qti", qtInfoBuffer);
        m_computeSplat.SetBuffer(currentDFKernel,       "_qti", qtInfoBuffer);
        m_computeSplat.SetBuffer(transferDFKernel,      "_qti", qtInfoBuffer);
        m_computeSplat.SetBuffer(clearKernel,           "_qti", qtInfoBuffer);

        m_computePositions.SetTexture(computePosKernel, "_texture", qt.atlasPage.atlas.texture);
        m_computeSplat.SetTexture(currentDFKernel, "_texture", qt.atlasPage.atlas.texture);
        m_computeSplat.SetTexture(transferDFKernel, "_texture", qt.atlasPage.atlas.texture);
        m_computeSplat.SetTexture(clearKernel, "_texture", qt.atlasPage.atlas.texture);
        
        m_computePositions.SetInt("_myIdInNodePool", qt.myIdInNodePool);
        
        
        m_computePositions.SetTexture(computePosKernel, "TexWater", moisture.TexManager.m_waterMapTex);
        m_computePositions.SetTexture(computePosKernel, "TexSlope", moisture.TexManager.m_slopeTex);
        m_computePositions.SetTexture(computePosKernel, "TexSlope", moisture.TexManager.m_slopeTex);
        m_computePositions.SetTexture(computePosKernel, "TexMoisture", moisture.TexManager.m_moistureTex);
        m_computePositions.SetTexture(computePosKernel, "TexHeight", moisture.TexManager.m_heightMapTex);

        int s = GlobalManager.m_atlas.PageSize / 16;

        if (vegLevel == 1 || vegLevel == 3) return;


        m_computeSplat.Dispatch(clearKernel, s, s, 1);

        m_computePositions.SetTexture(computePosKernel, "_positionsTexture", TreePool.positionTexture);
        
        m_computePositions.Dispatch(computePosKernel, 1, 1, 1);

        m_computeSplat.Dispatch(currentDFKernel, 16, 1, 1);

        if (vegLevel > 1)
            m_computeSplat.Dispatch(transferDFKernel, s, s, 1);
    }



    public static void AdjustIniSizePositionsBuffer(_QuadTree qt)
    {
        int adjustiniPosKernel = m_computePositions.FindKernel("AdjustIniPos");

        m_computePositions.SetTexture(adjustiniPosKernel, "_positionsTexture", TreePool.positionTexture);
        
        m_computePositions.SetInt("_myIdInNodePool", qt.myIdInNodePool);

        int s = NodePool.NODE_POOL_SIZE / 500;

        m_computePositions.Dispatch(adjustiniPosKernel, s, 1, 1);
    }


    public static void UpdatePosCounter(_QuadTree qt)
    {
        int updatePosCounter = m_computePositions.FindKernel("UpdatePosCounter");

        m_computePositions.SetInt("_myIdInNodePool", qt.myIdInNodePool);

        m_computePositions.Dispatch(updatePosCounter, 1, 1, 1);
    }

    

    void Start()
    {
        m_computePositions  = computePositions;
        m_computeSplat      = computeSplat;

        
        moisture = GameObject.Find("Calculator").GetComponent<MoistureDistribuition>();
    }

}



