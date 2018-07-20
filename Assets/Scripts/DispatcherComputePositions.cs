
using System.Runtime.InteropServices;
using UnityEngine;

public class DispatcherComputePositions : MonoBehaviour
{

    public ComputeShader computePositions;
    static ComputeShader m_computePositions;

    public ComputeShader computeSplat;
    static ComputeShader m_computeSplat;

    public static void ComputePositions(_QuadTree qt, float radius, int vegLevel)
    {
        if (qt == null || qt.atlasPage == null) return;

        int computePosKernel = m_computePositions.FindKernel("ComputePosition");
        int currentDFKernel = m_computeSplat.FindKernel("ComputeSplat");
        int transferDFKernel = m_computeSplat.FindKernel("TransferDF");
        int clearKernel = m_computeSplat.FindKernel("Clear");
        
        QuadTreeInfo qtInfo = qt.QuadTreeInfo(vegLevel, radius);

        ComputeBuffer qtInfoBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(QuadTreeInfo)));
        qtInfoBuffer.SetData(new QuadTreeInfo[1] { qtInfo });

        m_computePositions.SetBuffer(computePosKernel, "_positionsBuffer", GlobalManager.positionsBuffer);
        m_computeSplat.SetBuffer(currentDFKernel, "_positionsBuffer", GlobalManager.positionsBuffer);
        
        m_computePositions.SetBuffer(computePosKernel, "_qti", qtInfoBuffer);
        m_computeSplat.SetBuffer(currentDFKernel, "_qti", qtInfoBuffer);
        m_computeSplat.SetBuffer(transferDFKernel, "_qti", qtInfoBuffer);
        m_computeSplat.SetBuffer(clearKernel, "_qti", qtInfoBuffer);

        m_computePositions.SetTexture(computePosKernel, "_texture", qt.atlasPage.atlas.texture);
        m_computeSplat.SetTexture(currentDFKernel, "_texture", qt.atlasPage.atlas.texture);
        m_computeSplat.SetTexture(transferDFKernel, "_texture", qt.atlasPage.atlas.texture);
        m_computeSplat.SetTexture(clearKernel, "_texture", qt.atlasPage.atlas.texture);

        m_computePositions.SetTexture(computePosKernel, "_height", TerrainManager.m_heightMapRT);
        
        m_computePositions.SetInt("_myIdInNodePool", qt.myIdInNodePool);

        int s = GlobalManager.m_atlas.PageSize / 16;

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
    }

}



