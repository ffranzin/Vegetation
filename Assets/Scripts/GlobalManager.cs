
using System.Collections.Generic;
using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public static Atlas m_atlas;
    
    public static float VEG_MIN_DIST_L1 = 10;
    public static float VEG_MIN_DIST_L2 = 5;
    public static float VEG_MIN_DIST_L3 = 2;

    public static int lowerestQuadTreeBlockSize = 128;
    
    public static float VIEW_RADIUS_VEG_L1 = 2000;
    public static float VIEW_RADIUS_VEG_L2 = 1000;
    public static float VIEW_RADIUS_VEG_L3 = 500;
     

    public static ComputeBuffer positionsBuffer;

    public static ComputeBuffer globalPrecomputedTileBufferL1;
    public static ComputeBuffer globalPrecomputedTileBufferL2;
    public static ComputeBuffer globalPrecomputedTileBufferL3;

    /// <summary>
    /// Precompute tiles of positions. 
    /// These tiles garantee MinDistance between all positions, inside it.
    /// Each layer contains and specific tile.
    /// </summary>
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

    
    public static void CreateAtlas()
    {
        m_atlas = new Atlas(RenderTextureFormat.ARGBFloat, FilterMode.Bilinear, 4096, 128, true);

        if (m_atlas == null) Debug.LogError("Atlas cannot be generated.");

        GameObject tmpDebudAtlas = GameObject.Find("Plane");
        //tmpDebudAtlas.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", m_atlas.texture);
    }
    

    void Start()
    {
        CreateAtlas();

        positionsBuffer = new ComputeBuffer(1000, 8);
    
        FillAllPrecomputedPositinsBuffer();
    }
    

  
    public void Update()
    {
        float camHeight = Mathf.Clamp(Camera.main.transform.position.y, 0, 300) / 300;

        VIEW_RADIUS_VEG_L1 = Mathf.Lerp(500, UI.UI_viewRangeVegL1, camHeight);
        VIEW_RADIUS_VEG_L2 = Mathf.Lerp(300, UI.UI_viewRangeVegL2, camHeight);
        VIEW_RADIUS_VEG_L3 = Mathf.Lerp(100, UI.UI_viewRangeVegL3, camHeight);

        VIEW_RADIUS_VEG_L1 = VIEW_RADIUS_VEG_L2 = VIEW_RADIUS_VEG_L3 = 3000;
    }
}
