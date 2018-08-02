
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Profiling;

public class DispatcherComputePositions : MonoBehaviour
{
    public ComputeShader computePositions;
    static ComputeShader m_computePositions;

    public ComputeShader computeSplat;
    static ComputeShader m_computeSplat;

    static MoistureDistribuition moisture;
     
    static int computePosKernel;
    static int currentDFKernel;
    static int transferDFKernel;
    static int clearKernel;
    static int setIniSizeBufferKernel;

    public static void ComputePositions(_QuadTree qt, float radius, int vegLevel)
    {
        if (qt == null || qt.atlasPage == null) return;

        ComputeBuffer a = new ComputeBuffer(1, 4);

        // moisture.CalculateAll(new Vector2Int((int)(qt.bound.min.x / TerrainManager.PIXEL_WIDTH),
        //                                    (int)(qt.bound.min.z / TerrainManager.PIXEL_HEIGHT)));

        moisture.CalculateAll(new Vector2Int(0, 0));
        
        QuadTreeInfo qtInfo = qt.QuadTreeInfo(vegLevel, radius);
        ComputeBuffer qtInfoBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(QuadTreeInfo)));
        qtInfoBuffer.SetData(new QuadTreeInfo[1] { qtInfo });
        ///////////////////////////////////////////////////
        //SET ALL PARAMETERS
        ///////////////////////////////////////////////////
        m_computePositions.SetBuffer(computePosKernel, "_qti", qtInfoBuffer);
        m_computeSplat.SetBuffer(currentDFKernel, "_qti", qtInfoBuffer);
        m_computeSplat.SetBuffer(transferDFKernel, "_qti", qtInfoBuffer);
        m_computeSplat.SetBuffer(clearKernel, "_qti", qtInfoBuffer);

        m_computePositions.SetTexture(computePosKernel, "_texture", qt.atlasPage.atlas.texture);
        m_computeSplat.SetTexture(currentDFKernel, "_texture", qt.atlasPage.atlas.texture);
        m_computeSplat.SetTexture(transferDFKernel, "_texture", qt.atlasPage.atlas.texture);
        m_computeSplat.SetTexture(clearKernel, "_texture", qt.atlasPage.atlas.texture);
        
        m_computePositions.SetInt("_myIdInNodePool", qt.myIdInNodePool);

        m_computePositions.SetTexture(computePosKernel, "_positionsTexture", TreePool.positionTexture);
        
        m_computePositions.SetBuffer(computePosKernel, "_positionsBuffer", GlobalManager.positionsBuffer);
        m_computeSplat.SetBuffer(currentDFKernel, "_positionsBuffer", GlobalManager.positionsBuffer);

        int s = GlobalManager.m_atlas.PageSize / 16;
        
        m_computeSplat.SetBuffer(currentDFKernel, "_locked", a);


        ///////////////////////////////////////////////////
        //DISPATCHS
        ///////////////////////////////////////////////////
        Profiler.BeginSample("Splats");
        m_computeSplat.Dispatch(clearKernel, s, s, 1);

        m_computePositions.Dispatch(setIniSizeBufferKernel, TreePool.size, 1, 1);

        
        m_computePositions.Dispatch(computePosKernel, 15, 1, 1);
        
        m_computeSplat.Dispatch(currentDFKernel, 128, 1, 1);
        
        if (vegLevel > 1)
            m_computeSplat.Dispatch(transferDFKernel, s, s, 1);
        Profiler.EndSample();
        //a.GetData(new int[4]);
        
        //Debug.Log(" TIMER : " + sw.Elapsed.Milliseconds + "|| blockSize : " + qt.bound.size.x);
    }



    public static void AdjustIniSizePositionsBuffer(_QuadTree qt)
    {
        int adjustiniPosKernel = m_computePositions.FindKernel("AdjustIniPos");

        m_computePositions.SetTexture(adjustiniPosKernel, "_positionsTexture", TreePool.positionTexture);

        m_computePositions.SetInt("_myIdInNodePool", qt.myIdInNodePool);

        int s = NodePool.NODE_POOL_SIZE / 1000;

        m_computePositions.Dispatch(adjustiniPosKernel, s, 1, 1);
    }


    public static void UpdatePosCounter(_QuadTree qt)
    {
        ComputeBuffer a = new ComputeBuffer(4, 4);

        int updatePosCounter = m_computePositions.FindKernel("UpdatePosCounter");

        m_computePositions.SetInt("_myIdInNodePool", qt.myIdInNodePool);

        m_computePositions.SetBuffer(updatePosCounter, "_locked", a);

        m_computePositions.Dispatch(updatePosCounter, TreePool.size, 1, 1);
    }


    void Start()
    {
        m_computePositions = computePositions;
        m_computeSplat = computeSplat;
        computePosKernel = m_computePositions.FindKernel("ComputePosition");
        currentDFKernel = m_computeSplat.FindKernel("ComputeSplat");
        transferDFKernel = m_computeSplat.FindKernel("TransferDF");
        clearKernel = m_computeSplat.FindKernel("Clear");
        setIniSizeBufferKernel = m_computePositions.FindKernel("SetIniSizeBuffer");

        moisture = GameObject.Find("Calculator").GetComponent<MoistureDistribuition>();
        //moisture.CalculateAll(new Vector2Int(0, 0));
        
        m_computePositions.SetTexture(computePosKernel, "TexWater", moisture.TexManager.m_waterMapTex);
        m_computePositions.SetTexture(computePosKernel, "TexSlope", moisture.TexManager.m_slopeTex);
        m_computePositions.SetTexture(computePosKernel, "TexSlope", moisture.TexManager.m_slopeTex);
        m_computePositions.SetTexture(computePosKernel, "TexMoisture", moisture.TexManager.m_moistureTex);
        m_computePositions.SetTexture(computePosKernel, "TexHeight", moisture.TexManager.m_heightMapTex);
    }
}



