using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreePool : MonoBehaviour
{
    public GameObject[] _treePool;
    public static List<Tree> treePool;

    /// <summary>
    /// Store all positions of all tree. Each row in this texture are similar an buffer. 
    /// To access the positions of specific tree use [x, y], where y is the id of tree in treePool.  
    /// </summary>
    public static RenderTexture positionTexture;
    

    public static RenderTexture positionTextureTmp;
    
    /// <summary>
    /// Store the amount of positions generated in GPU.
    /// The position I in this buffers contains the N positions of tree.myindexInTreePool == I.
    /// </summary>
    public static ComputeBuffer treeCountPositionsBuffer;

    /// <summary>
    /// Used to restore get data of 'treeCountPositionsBuffer'
    /// </summary>
    public static int[] positionsPerTreeAmountData;

    public static ComputeBuffer globalTreeHumidityInfo;
    public static ComputeBuffer globalTreeSlopeInfo;
    public static ComputeBuffer globalTreeHeightInfo;
    public static ComputeBuffer globalTreeSensitiveInfo;
    public static ComputeBuffer globalTreeNecessityInfo;


    public static int size
    {
        get
        {
            return treePool == null ? 0 : treePool.Count;
        }
    }
    
    private void Awake()
    {
        treePool = new List<Tree>();

        for (int i = 0; i < _treePool.Length; i++)
        {
            GameObject go = Instantiate(_treePool[i]);

            Tree t = go.GetComponent<Tree>();

            if (t == null) Debug.LogError("Missing Component.");

            t.myIndexInTreePool = i;

            treePool.Add(t);
        }

        treeCountPositionsBuffer = new ComputeBuffer(size, 4);
        
        Shader.SetGlobalBuffer("_globalTreeCountPositionsBuffer", treeCountPositionsBuffer);

        positionsPerTreeAmountData = new int[size];

        Shader.SetGlobalInt("_globalTreePoolSize", size);
        
        FillDiscretizedTreeInfoBuffer();

        CreatePositionTexture();
    }

    /// <summary>
    /// Get the amount of data generated in GPU and update all trees. 
    /// This amount are used to draw the meshes. 
    /// </summary>
    public static void UpdateTreeAmountData()
    {
        treeCountPositionsBuffer.GetData(positionsPerTreeAmountData);

        foreach (Tree t in treePool)
            t.UpdateBuffers();
    }

    
    /// <summary>
    /// Discretized parameters of each tree and store in sequencialbuffer.
    /// Each buffers store all Discretized parameters T of all trees.
    /// Example: All discretized slope information of Tree1 ... treeN are in the same buffer.
    /// </summary>
    private static void FillDiscretizedTreeInfoBuffer()
    {
        List<float> slopeInfo = new List<float>();
        List<float> heightInfo = new List<float>();
        List<float> humidityInfo = new List<float>();
        List<float> sensitiveInfo = new List<float>();
        List<float> necessityInfo = new List<float>();

        for (int i = 0; i < size; i++)
        {
            Tree t = treePool[i];

            slopeInfo.AddRange(t.TreeSlopeDiscretized);
            heightInfo.AddRange(t.TreeHeightDiscretized);
            humidityInfo.AddRange(t.TreeHumidityDiscretized);
            sensitiveInfo.AddRange(t.TreeSensitiveDiscretized);
            necessityInfo.AddRange(t.TreeNecessityDiscretized);
        }

        globalTreeHumidityInfo = new ComputeBuffer(size, sizeof(float) * Tree.N_INFO_DISCRETIZED);
        globalTreeSlopeInfo = new ComputeBuffer(size, sizeof(float) * Tree.N_INFO_DISCRETIZED);
        globalTreeHeightInfo = new ComputeBuffer(size, sizeof(float) * Tree.N_INFO_DISCRETIZED);
        globalTreeSensitiveInfo = new ComputeBuffer(size, sizeof(float) * Tree.N_INFO_DISCRETIZED);
        globalTreeNecessityInfo = new ComputeBuffer(size, sizeof(float) * Tree.N_INFO_DISCRETIZED);

        globalTreeSlopeInfo.SetData(slopeInfo);
        globalTreeHeightInfo.SetData(heightInfo);
        globalTreeHumidityInfo.SetData(humidityInfo);
        globalTreeSensitiveInfo.SetData(sensitiveInfo);
        globalTreeNecessityInfo.SetData(necessityInfo);

        Shader.SetGlobalBuffer("_globalTreeSlopeInfo", globalTreeSlopeInfo);
        Shader.SetGlobalBuffer("_globalTreeHeightInfo", globalTreeHeightInfo);
        Shader.SetGlobalBuffer("_globalTreeHumidityInfo", globalTreeHumidityInfo);
        Shader.SetGlobalBuffer("_globalTreeSensitiveInfo", globalTreeSensitiveInfo);
        Shader.SetGlobalBuffer("_globalTreeNecessityInfo", globalTreeNecessityInfo);

        slopeInfo.Clear();
        heightInfo.Clear();
        humidityInfo.Clear();
        sensitiveInfo.Clear();
        necessityInfo.Clear();
    }


    void CreatePositionTexture()
    {
        int n = 10000;
        positionTexture = new RenderTexture(n, size, 0, RenderTextureFormat.ARGB32);
        positionTexture.enableRandomWrite = true;
        positionTexture.useMipMap = false;
        positionTexture.Create();

        positionTextureTmp = new RenderTexture(n, size, 0, RenderTextureFormat.ARGB32);
        positionTextureTmp.enableRandomWrite = true;
        positionTextureTmp.useMipMap = false;
        positionTextureTmp.Create();

        GameObject g1 = GameObject.Find("Plane1");
        g1.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", positionTexture);

    }

}




