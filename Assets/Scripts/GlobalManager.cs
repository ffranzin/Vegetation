
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public static Atlas m_atlas;

    public static readonly int treeBufferSize = 50000;

    public static float VEG_MIN_DIST_L1 = 10;
    public static float VEG_MIN_DIST_L2 = 5;
    public static float VEG_MIN_DIST_L3 = 2;

    public static int lowerestQuadTreeBlockSize = 32;
    
    public static float VIEW_RADIUS_VEG_L1 = 500;
    public static float VIEW_RADIUS_VEG_L2 = 400;
    public static float VIEW_RADIUS_VEG_L3 = 500;
     
    /// <summary>
    /// BUFFERS
    /// </summary>
    public static ComputeBuffer positionsBuffer;
    public static ComputeBuffer positionsPerTreeBuffer;
    public static ComputeBuffer positionsPerTreeIndexBuffer;

    public static ComputeBuffer globalPrecomputedTileBufferL1;
    public static ComputeBuffer globalPrecomputedTileBufferL2;
    public static ComputeBuffer globalPrecomputedTileBufferL3;

    public static ComputeBuffer globalTreeHumidityInfo;
    public static ComputeBuffer globalTreeSlopeInfo;
    public static ComputeBuffer globalTreeHeightInfo;
    public static ComputeBuffer globalTreeSensitiveInfo;
    public static ComputeBuffer globalTreeNecessityInfo;


    public static ComputeBuffer globalDiscretizedTreeInfo;
    public static int[] zeros;
    public static int[] positionsPerTreeAmountData;


    private static void FillAllPrecomputedPositinsBuffer()
    {
        List<Vector2> positions = new List<Vector2>();
        positions.Clear();
        PrecomputedPositionsBuffer.GeneratePos(positions, lowerestQuadTreeBlockSize * 4, VEG_MIN_DIST_L1, 3);
        globalPrecomputedTileBufferL1 = new ComputeBuffer(positions.Count, 8);
        globalPrecomputedTileBufferL1.SetData(positions);
        
        positions.Clear();
        PrecomputedPositionsBuffer.GeneratePos(positions, lowerestQuadTreeBlockSize * 2, VEG_MIN_DIST_L2, 2);
        globalPrecomputedTileBufferL2 = new ComputeBuffer(positions.Count, 8);
        globalPrecomputedTileBufferL2.SetData(positions);
         
        positions.Clear();
        PrecomputedPositionsBuffer.GeneratePos(positions, lowerestQuadTreeBlockSize, VEG_MIN_DIST_L3, 1);
        globalPrecomputedTileBufferL3 = new ComputeBuffer(positions.Count, 8);
        globalPrecomputedTileBufferL3.SetData(positions);

        Shader.SetGlobalBuffer("_GlobalPrecomputedPositionL1", globalPrecomputedTileBufferL1);
        Shader.SetGlobalBuffer("_GlobalPrecomputedPositionL2", globalPrecomputedTileBufferL2);
        Shader.SetGlobalBuffer("_GlobalPrecomputedPositionL3", globalPrecomputedTileBufferL3);
        
        positions.Clear();
        positions = null;
    }



    private static void FillDiscretizedTreeInfoBuffer()
    {
        List<float> slopeInfo = new List<float>();
        List<float> heightInfo = new List<float>();
        List<float> humidityInfo = new List<float>();
        List<float> sensitiveInfo = new List<float>();
        List<float> necessityInfo = new List<float>();
        
        for (int i = 0; i < TreePool.size; i++)
        {
            Tree t = TreePool.treePool[i];

            slopeInfo.AddRange(t.TreeSlopeDiscretized);
            heightInfo.AddRange(t.TreeHeightDiscretized);
            humidityInfo.AddRange(t.TreeHumidityDiscretized);
            sensitiveInfo.AddRange(t.TreeSensitiveDiscretized);
            necessityInfo.AddRange(t.TreeNecessityDiscretized);
        }
        
        globalTreeHumidityInfo  = new ComputeBuffer(TreePool.size, sizeof(float) * Tree.N_INFO_DISCRETIZED);
        globalTreeSlopeInfo     = new ComputeBuffer(TreePool.size, sizeof(float) * Tree.N_INFO_DISCRETIZED);
        globalTreeHeightInfo    = new ComputeBuffer(TreePool.size, sizeof(float) * Tree.N_INFO_DISCRETIZED);
        globalTreeSensitiveInfo = new ComputeBuffer(TreePool.size, sizeof(float) * Tree.N_INFO_DISCRETIZED);
        globalTreeNecessityInfo = new ComputeBuffer(TreePool.size, sizeof(float) * Tree.N_INFO_DISCRETIZED);
        
        globalTreeSlopeInfo.SetData(slopeInfo);
        globalTreeHeightInfo.SetData(heightInfo);
        globalTreeHumidityInfo.SetData(humidityInfo);
        globalTreeSensitiveInfo.SetData(sensitiveInfo);
        globalTreeNecessityInfo.SetData(necessityInfo);
        
        Shader.SetGlobalBuffer("_GlobalTreeSlopeInfo", globalTreeSlopeInfo);
        Shader.SetGlobalBuffer("_GlobalTreeHeightInfo", globalTreeHeightInfo);
        Shader.SetGlobalBuffer("_GlobalTreeHumidityInfo", globalTreeHumidityInfo);
        Shader.SetGlobalBuffer("_GlobalTreeSensitiveInfo", globalTreeSensitiveInfo);
        Shader.SetGlobalBuffer("_GlobalTreeNecessityInfo", globalTreeNecessityInfo);

        slopeInfo.Clear();
        heightInfo.Clear();
        humidityInfo.Clear();
        sensitiveInfo.Clear();
        necessityInfo.Clear();
    }
    


    public static void CreateAtlas()
    {
        m_atlas = new Atlas(RenderTextureFormat.ARGBFloat, FilterMode.Bilinear, 8192, 128, true);

        if (m_atlas == null) Debug.LogError("Atlas cannot be generated.");

        tmpDebudAtlas = GameObject.Find("Plane");
        tmpDebudAtlas.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", m_atlas.texture);
    }


    static GameObject tmpDebudAtlas;
   
    void Start()
    {
        CreateAtlas();

        positionsBuffer = new ComputeBuffer(treeBufferSize, 8);
        positionsPerTreeIndexBuffer = new ComputeBuffer(TreePool.size, 4);
        positionsPerTreeBuffer = new ComputeBuffer(treeBufferSize * TreePool.size, 8);
        
        positionsPerTreeAmountData = new int[TreePool.size];
        zeros = new int[TreePool.size];

        FillAllPrecomputedPositinsBuffer();

        FillDiscretizedTreeInfoBuffer();

        Shader.SetGlobalInt("_GlobalBufferPerTreeSize", treeBufferSize);
    }
    
    public static void UpdateTreeAmountData()
    {
        positionsPerTreeIndexBuffer.GetData(positionsPerTreeAmountData);
    }

    public static void ResetPositionsAmount()
    {
        positionsPerTreeIndexBuffer.SetData(zeros);
    }
    
}
