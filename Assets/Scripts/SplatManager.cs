using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class SplatManager : MonoBehaviour {

    public ComputeShader computeSplat;
    static ComputeShader m_computeSplat;

    public ComputeShader computePositions;
    static ComputeShader m_computePositions;
    
    public static ComputeBuffer positionsBuffer;
    public static ComputeBuffer positionsPerTreeBuffer;
    public static ComputeBuffer positionsPerTreeIndexBuffer;
    
    public static int[] positionsPerTreeIndexData;







    public static void ComputePositions(_QuadTree qt,float radius, bool hasUpper = false)
    {
        positionsBuffer = new ComputeBuffer(100, 8);

        int computePosKernel = m_computePositions.FindKernel("ComputePosition");
        int splatMapKernel = m_computePositions.FindKernel("ComputeSplat");
        
        QuadTreeInfo qtInfo = new QuadTreeInfo();

        qtInfo.currentNodeAtlasOrigin = new Vector2(qt.atlasPage.tl.x, qt.atlasPage.tl.y);
        qtInfo.currentNodeAtlasSize = qt.atlasPage.size;
        qtInfo.currentNodeWorldOrigin = qt.bound.min.xz();
        qtInfo.currentNodeWorldSize = qt.bound.size.x;
        
        if (hasUpper)
        {
            qtInfo.upperNodeAtlasOrigin = new Vector2(qt.root.atlasPage.tl.x, qt.root.atlasPage.tl.y);
            qtInfo.upperNodeAtlasSize = qt.root.atlasPage.size;
            qtInfo.upperNodeWorldOrigin = qt.root.bound.min.xz();
            qtInfo.upperNodeWorldSize = qt.root.bound.size.x;
        }
        qtInfo.hasUpper = hasUpper ? 1 : 0;
        qtInfo.treeRadius = radius;


        ComputeBuffer qtInfoBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(QuadTreeInfo)));
        qtInfoBuffer.SetData(new QuadTreeInfo[1] { qtInfo });
        
        m_computePositions.SetBuffer(computePosKernel, "_positionsBuffer", positionsBuffer);
        m_computePositions.SetBuffer(splatMapKernel, "_positionsBuffer", positionsBuffer);
        
        m_computePositions.SetBuffer(computePosKernel, "_qti", qtInfoBuffer);
        m_computePositions.SetBuffer(splatMapKernel, "_qti", qtInfoBuffer);
        
        m_computePositions.SetTexture(computePosKernel, "_texture", qt.atlasPage.atlas.texture);
        m_computePositions.SetTexture(splatMapKernel, "_texture", qt.atlasPage.atlas.texture);
        //m_computePositions.SetTexture(splatMapKernel, "_texture1", qt.root.atlasPage.atlas.texture);
        
        m_computePositions.SetBuffer(computePosKernel, "_positionsPerTreeBuffer", positionsPerTreeBuffer);
        m_computePositions.SetBuffer(computePosKernel, "_positionsPerTreeIndexBuffer", positionsPerTreeIndexBuffer);
        
        m_computePositions.SetVector("_tmpWriteInChannel", Vector3.up);
        
        m_computePositions.Dispatch(computePosKernel, 1, 1, 1);
        
        int s = Mathf.CeilToInt( qt.atlasPage.atlas.PageSize / 8 );

        m_computePositions.Dispatch(splatMapKernel, s, s, 1);
    }


    

    void Start () {

        m_computeSplat      = computeSplat;
        m_computePositions  = computePositions;

        positionsPerTreeIndexBuffer = new ComputeBuffer(TreePool.treePool.Count, 4);
        positionsPerTreeBuffer = new ComputeBuffer(1000 * TreePool.treePool.Count, 8);

        zeros = new int[TreePool.treePool.Count];
        positionsPerTreeIndexData = new int[TreePool.treePool.Count];
        for (int i = 0; i<zeros.Length; i++)
            zeros[i] = 0;
    }



    int[] zeros;
    void Update () {
        
        if (Input.GetKeyDown(KeyCode.Z) || true)
        {
            positionsPerTreeIndexBuffer.GetData(positionsPerTreeIndexData);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            Vector2[] aaaa = new Vector2[12000];


            positionsPerTreeBuffer.GetData(aaaa);

            for (int i = 0; i < aaaa.Length; i++)
                if(aaaa[i] != Vector2.zero)
                    Debug.Log(i + "  " + aaaa[i]);

        }

        //positionsPerTreeCountBuffer.SetData(zeros);
    }
    
}



