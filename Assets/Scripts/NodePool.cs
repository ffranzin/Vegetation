using System.Collections.Generic;
using UnityEngine;

public class NodePool : MonoBehaviour {

    public static readonly int NODE_POOL_SIZE = 10000;

    public static List<_QuadTree> qt_NodePool = new List<_QuadTree>();
    public static List<_QuadTree> qt_NodeToReleasePos = new List<_QuadTree>();


    public static bool hasFreeNodes
    {
        get
        {
            return qt_NodePool.Count > 0;
        }
    }

    public static bool isFull
    {
        get
        {
            return qt_NodePool.Count == 0;
        }
    }

    public static int freeNodes
    {
        get
        {
            return qt_NodePool.Count;
        }
    }



    /// <summary>
    /// Store initial position and lenght of positions of 
    /// </summary>
    public static ComputeBuffer posIniSizeBuffer;

    /// <summary>
    /// Used to getdata of 'posIniSizeBuffer'/
    /// </summary>
    static Vector2[] iniTam;

    void Start () {

        iniTam = new Vector2[TreePool.size];

        for (int i = 0; i < NODE_POOL_SIZE; i++)
        {
            qt_NodePool.Add(new _QuadTree());
            qt_NodePool[i].bound = new Bounds();
            qt_NodePool[i].myIdInNodePool = i;
        }

        posIniSizeBuffer = new ComputeBuffer(NODE_POOL_SIZE * TreePool.size, 8);

        Shader.SetGlobalBuffer("_globalPosTreeIniSizeBuffer", posIniSizeBuffer);
        Shader.SetGlobalInt("_globalNodePoolSize", NODE_POOL_SIZE);
    }


    /// <summary>
    /// Return pre-allocated node.
    /// </summary>
    /// <returns></returns>
    public static _QuadTree NodeRequest()
    {
        if (!hasFreeNodes) return null;

        _QuadTree aux = qt_NodePool[0];

        qt_NodePool.RemoveAt(0);

        return aux;
    }

    /// <summary>
    /// Reset all parameters of node.
    /// Release atlas node.
    /// If this 'qt' node contains positions inside buffers in GPU put it in queue to release these buffers.
    /// </summary>
    /// <param name="qt"></param>
    public static void NodeRelease(_QuadTree qt)
    {
        if (qt == null) return;
        
        qt.parent.hasChild = false;
        qt.hasChild = false;
        qt.positionsHasBeenGenerated = false;
        qt.level = -1;
        
        if(qt.atlasPage != null)
        {
            GlobalManager.m_atlas.ReleasePage(qt.atlasPage);
            qt.atlasPage = null;
        }
        
        if (qt.containsVeg)
            qt_NodeToReleasePos.Add(qt);
        else
           qt_NodePool.Add(qt);

        qt.containsVeg = false;
    }


    

    public static bool LockToRelease;
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            LockToRelease = !LockToRelease;
        
        if (qt_NodeToReleasePos.Count == 0 || !LockToRelease) return;
        
        _QuadTree qt = qt_NodeToReleasePos[0];

        ReleaseBuffers(qt);

        qt_NodeToReleasePos.RemoveAt(0);
        qt_NodePool.Add(qt);
    }

    

    /// <summary>
    /// Release all necessary buffer to invalidade 'qt' node.
    /// </summary>
    /// <param name="qt"></param>
    static void ReleaseBuffers(_QuadTree qt)
    {
        posIniSizeBuffer.GetData(iniTam, 0, qt.myIdInNodePool * TreePool.size, TreePool.size);

        for (int i = 0; i < TreePool.size; i++)
        {
            int ini = (int)iniTam[i].x;
            int tam = (int)iniTam[i].y;

            if (tam > 0)
            {
                Graphics.CopyTexture(TreePool.positionTexture, TreePool.positionTextureTmp);

                MoveBlock(ini, tam, i);

                DispatcherComputePositions.AdjustIniSizePositionsBuffer(qt);

                Graphics.CopyTexture(TreePool.positionTextureTmp, TreePool.positionTexture);
            }
        }

        DispatcherComputePositions.UpdatePosCounter(qt);
    }


    /// <summary>
    /// Move block memory of positions to avoid sparse buffer. 
    /// </summary>
    /// <param name="ini"></param>
    /// <param name="tam"></param>
    /// <param name="h"></param>
    static void MoveBlock(int ini, int tam, int h)
    {
        int srcX = ini + tam;
        int srcY = h;
        int srcWidth = TreePool.positionTexture.width - srcX;
        
        int dstX = ini;
        
        Graphics.CopyTexture(TreePool.positionTexture,     0, 0, srcX, h, srcWidth, 1,
                             TreePool.positionTextureTmp,  0, 0, dstX, h);
    }


}
