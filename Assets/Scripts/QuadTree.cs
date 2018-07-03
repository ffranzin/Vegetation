
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

public class _QuadTree
{
    public Bounds bound;
    public _QuadTree TL;
    public _QuadTree TR;
    public _QuadTree BL;
    public _QuadTree BR;
    public _QuadTree root;
    public int level;

    public Atlas.AtlasPageDescriptor atlasPage = null;
    
    public Vector3[] m_treesPositions;

    public int[] m_treesModelIndex;

    public bool positionsHasBeenGenerated;
    public bool hasChildGenerated;
    
}

public struct QuadTreeInfo
{
    public Vector2 currentNodeAtlasOrigin;
    public float currentNodeAtlasSize;
    public Vector2 currentNodeWorldOrigin;
    public float currentNodeWorldSize;

    public Vector2 upperNodeAtlasOrigin;
    public float upperNodeAtlasSize;
    public Vector2 upperNodeWorldOrigin;
    public float upperNodeWorldSize;

    public float treeRadius;

    public int hasUpper;
}



public class QuadTree : MonoBehaviour
{
    private static readonly int QUADTREE_MAX_LEVEL = 6;

    public _QuadTree m_quadTree;
    
    public ComputeShader compute;
    static float treeTopSizeL1 = 3;
    static float treeTopSizeL2 = 4;
    static float treeTopSizeL3 = .5f;


    static float vegetationMinHeightL1 = 7;
    static float vegetationMaxHeightL3 = 1.5f;

    static float vegetationMinDistanceL1 = 4;
    static float vegetationMinDistanceL2 = 5;
    static float vegetationMinDistanceL3 = 3;
    

    const int quadTreeL1 = 3;
    const int quadTreeL2 = 4;
    const int quadTreeL3 = 9;
    [Range(0, 1000)]
    public float viewRadiusL1 = 1000;
    [Range(0, 500)]
    public float viewRadiusL2 = 500;
    [Range(0, 100)]
    public float viewRadiusL3 = 100;

    
    private static void SubdivideQuadTree(_QuadTree qt, bool subdivideAll = false)
    {
        if (qt.level > QUADTREE_MAX_LEVEL || qt.hasChildGenerated) return;

        qt.TL = new _QuadTree();
        qt.TR = new _QuadTree();
        qt.BL = new _QuadTree();
        qt.BR = new _QuadTree();

        qt.TL.root = qt.TR.root = qt.BL.root = qt.BR.root = qt;
        qt.TL.level = qt.TR.level = qt.BL.level = qt.BR.level = qt.level + 1;
        
        qt.hasChildGenerated = true;
        
        qt.TL.bound = GenBoundChild(qt.bound, 1);
        qt.TR.bound = GenBoundChild(qt.bound, 2);
        qt.BL.bound = GenBoundChild(qt.bound, 3);
        qt.BR.bound = GenBoundChild(qt.bound, 4);
        
        if (subdivideAll)
        {
            SubdivideQuadTree(qt.TL, subdivideAll);
            SubdivideQuadTree(qt.TR, subdivideAll);
            SubdivideQuadTree(qt.BL, subdivideAll);
            SubdivideQuadTree(qt.BR, subdivideAll);
        }

        PosGenerator(qt);
    }

    

    public static void GenerateTreePositions(_QuadTree qt, float distBetweenTree, int nPosPerLeaf, float treeTopSize)
    {
        if (qt == null)  return;
        
        if(qt.level == quadTreeL1)
        {
            qt.atlasPage = GlobalManager.m_atlas.GetPage();
            SplatManager.ComputePositions(qt, 6);
            
            //SplatManager.ComputeSplat(qt,new Vector3(1, 0, 0));
        }
        if (qt.level == quadTreeL2)
        {
            qt.atlasPage = GlobalManager.m_atlas.GetPage();
            SplatManager.ComputePositions(qt, 6, true);

            //SplatManager.ComputeSplat(qt,new Vector3(1, 0, 0));
        }
        /*
         *  compute my splat in current page considering my upper node  
         *  
         *  
         *  compute all required map - height, humidity, relative height ...
         *  
         *  
         *  in another compute sort one kind of tree for specific posisition generated above
         *  
         *  
         */


        //while (m_root.level != 0)
        //{
        //    for (int i = 0; i < m_trees.Count && m_root.m_treesPositions != null; i++)
        //    {
        //        for (int j = 0; j < m_root.m_treesPositions.Length; j++)
        //        {
        //            if ((m_trees[i] - m_root.m_treesPositions[j]).magnitude < distBetweenTree * 2)
        //                m_trees[i] = Vector3.zero;
        //        }
        //    }
        //    m_trees.RemoveAll(t => t == Vector3.zero);
        //    m_root = m_root.root;
        //}


        //qt.m_treesPositions = new Vector3[m_trees.Count];
        //qt.m_treesPositions = AddHeightInPositions(m_trees);

        //SortModels(qt);

        //Testes.fillpos(m_trees.ToArray());

        //Vector2 origin = qt.bound.min.xz();

        //Testes.FillSplatMap(origin, (int)qt.bound.size.x, distBetweenTree / 2);
    }

    
    public void RenderQuadTree(_QuadTree qt, Vector3 pos)
    {
        if (qt == null) return;

        float dist = Mathf.Sqrt(qt.bound.SqrDistance(pos));

        if (qt.level >= quadTreeL1 && viewRadiusL1 < dist) return;
        else if (qt.level >= quadTreeL2 && viewRadiusL2 < dist) return;
        else if (qt.level >= quadTreeL3 && viewRadiusL3 < dist) return;
        
        Utils.ShowBoundLines(qt.bound, Color.yellow);

        if (!qt.hasChildGenerated && qt.level < QUADTREE_MAX_LEVEL)
            SubdivideQuadTree(qt);
        
        RenderQuadTree(qt.BL, pos);
        RenderQuadTree(qt.BR, pos);
        RenderQuadTree(qt.TL, pos);
        RenderQuadTree(qt.TR, pos);

        return;
    }
    
    private void LateUpdate()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (m_quadTree != null)
            {
                for (int i = 0; i < TreePool.treePool.Count; i++)
                    TreePool.treePool[i].positions.Clear();

                RenderQuadTree(m_quadTree, Camera.main.transform.position.x0z());

                for (int i = 0; i < TreePool.treePool.Count; i++)
                    TreePool.treePool[i].UpdateBuffers();
            }
        }
    }


    private void OnDrawGizmos()
    {
        //ShowBounds(m_quadTree);
    }




    //////////////////////////
    //////////////////////////
    //////////////////////////
    //////////////////////////
    //////////////////////////
    //////////////////////////
    //////////////////////////

    static List<Tree> FindTreeForSpecificPosition(_QuadTree qt, Vector3 pos)
    {
        List<Tree> m_allTreeModels = new List<Tree>();

        switch (qt.level)
        {
            case quadTreeL1:
                m_allTreeModels.AddRange(TreePool.treePool.FindAll(t => t.treeMeshHeight > vegetationMinHeightL1));
                break;
            case quadTreeL2:
                m_allTreeModels.AddRange(TreePool.treePool.FindAll(t => t.treeMeshHeight > vegetationMaxHeightL3 && t.treeMeshHeight < vegetationMinHeightL1));
                break;
            case quadTreeL3:
                m_allTreeModels.AddRange(TreePool.treePool.FindAll(t => t.treeMeshHeight < vegetationMaxHeightL3));
                break;
        }

        return m_allTreeModels;
    }



    public static void SortModels(_QuadTree qt)
    {
        qt.m_treesModelIndex = new int[qt.m_treesPositions.Length];

        //TODO consider probaility
        for (int i = 0; i < qt.m_treesModelIndex.Length; i++)
        {
            List<Tree> ableTrees = FindTreeForSpecificPosition(qt, qt.m_treesPositions[i]);

            if (ableTrees.Count == 0)
                qt.m_treesModelIndex[i] = qt.m_treesModelIndex[0];

            int r = Random.Range(0, ableTrees.Count);
            qt.m_treesModelIndex[i] = ableTrees[r].myIndexInTreePool;
        }
    }
    public static void PosGenerator(_QuadTree qt)
    {
        switch (qt.level)
        {
            case quadTreeL1:
                GenerateTreePositions(qt, vegetationMinDistanceL1, 10, treeTopSizeL1);
                break;
            case quadTreeL2:
                GenerateTreePositions(qt, vegetationMinDistanceL2, 10, treeTopSizeL2);
                break;
            //case quadTreeL3:
            //    GenerateTreePositions(qt, vegetationMinDistanceL3, 10, treeTopSizeL3);
            //    break;
        }
    }


    public void CreateQuadTree()
    {
        m_quadTree = new _QuadTree();
        m_quadTree.bound = new Bounds();
        m_quadTree.bound.min = TerrainManager.TERRAIN_ORIGIN.x0z();
        m_quadTree.bound.max = TerrainManager.TERRAIN_END.x0z();
        m_quadTree.root = null;
        m_quadTree.level = 0;

        Utils.ShowBoundLines(m_quadTree.bound, Color.green, Mathf.Infinity);
    }


    public static Vector3[] AddHeightInPositions(List<Vector3> pos)
    {
        Vector3[] posWithHeight = new Vector3[pos.Count];

        for (int i = 0; i < pos.Count; i++)
        {
            posWithHeight[i] = TerrainManager.AddHeight(pos[i]);
        }

        return posWithHeight;
    }



    void Start()
    {
        CreateQuadTree();
    }

    private static Bounds GenBoundChild(Bounds b, int boundPosition)
    {
        //|1 = TL|   |2 = TR|   |3 = BL|  |4 = BR| 

        Bounds newBound = new Bounds();
        if (boundPosition == 1)
        {
            newBound.min = new Vector3(b.min.x, 0, ((b.min.z + b.max.z) / 2));
            newBound.max = new Vector3(((b.min.x + b.max.x) / 2), 0, b.max.z);
        }
        else if (boundPosition == 2)
        {
            newBound.min = new Vector3(((b.min.x + b.max.x) / 2), 0, ((b.min.z + b.max.z) / 2));
            newBound.max = new Vector3(b.max.x, 0, b.max.z);
        }
        else if (boundPosition == 3)
        {
            newBound.min = new Vector3(b.min.x, 0, b.min.z);
            newBound.max = new Vector3(((b.min.x + b.max.x) / 2), 0, ((b.min.z + b.max.z) / 2));
        }
        else if (boundPosition == 4)
        {
            newBound.min = new Vector3(((b.min.x + b.max.x) / 2), 0, b.min.z);
            newBound.max = new Vector3(b.max.x, 0, ((b.min.z + b.max.z) / 2));
        }

        return newBound;
    }


    private static void ShowBounds(_QuadTree qt)
    {
        if (qt == null) return;

        Utils.ShowBoundLines(qt.bound, Color.black);

        ShowBounds(qt.BL);
        ShowBounds(qt.BR);
        ShowBounds(qt.TL);
        ShowBounds(qt.TR);
    }

}



#if UNITY_EDITOR
[CustomEditor(typeof(QuadTree))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        QuadTree myScript = (QuadTree)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Generate Position"))
        {
            //myScript.generate();
        }

        if (GUILayout.Button("Generate QuadTree"))
        {
            //myScript.CreateQuadTree();
        }
    }
}
#endif




//if (splat == null)
//        {public RenderTexture splat;
//float resolution = .5f; //in meters
//float terrainSize = Mathf.Abs((TerrainManager.TERRAIN_END.x - TerrainManager.TERRAIN_ORIGIN.x));
//splat = new RenderTexture((int)(terrainSize / resolution), (int) (terrainSize / resolution), 1, RenderTextureFormat.ARGB32);
//            splat.wrapMode = TextureWrapMode.Clamp;
//            splat.enableRandomWrite = true;
//            splat.autoGenerateMips = false;
//            splat.Create();
//        }



//public bool IsEmptyPos(Vector3 pos)
//{
//    float STEP_WIDTH = TerrainManager.TERRAIN_END.z / splat1.width;
//    float STEP_HEIGHT = TerrainManager.TERRAIN_END.z / splat1.height;

//    int x = Mathf.RoundToInt((float)(pos.x / STEP_WIDTH));
//    int y = Mathf.RoundToInt((float)(pos.z / STEP_HEIGHT));

//    Color c = splat1.GetPixel(x, y);

//    return c.r > 0.9f && c.g > 0.9f ? true : false;
//}


//void ClearQuadtree(_QuadTree qt)
//{
//    if (qt.TL == null) return;

//    ClearQuadtree(qt.BL);
//    ClearQuadtree(qt.BR);
//    ClearQuadtree(qt.TL);
//    ClearQuadtree(qt.TR);

//    if (!HasChild(qt.BL) && !TerrainManager.HasTreeInPos(qt.BL.bound.center)) qt.BL = null;
//    if (!HasChild(qt.BR) && !TerrainManager.HasTreeInPos(qt.BR.bound.center)) qt.BR = null;
//    if (!HasChild(qt.TL) && !TerrainManager.HasTreeInPos(qt.TL.bound.center)) qt.TL = null;
//    if (!HasChild(qt.TR) && !TerrainManager.HasTreeInPos(qt.TR.bound.center)) qt.TR = null;
//}

// void FillSplatMap(RenderTexture texture, Vector4[] positions, int channel, float treeRadius, float pixelSize)
//{
//    ComputeBuffer randomPositionsBuffer = new ComputeBuffer(1000, 8);
//    randomPositionsBuffer.SetData(positions);

//    int kernel = compute.FindKernel("CSMain");

//    compute.SetBuffer(kernel, "_randomPos", randomPositionsBuffer);
//    compute.SetInt("_nRandomPos", positions.Length);
//    compute.SetInt("_writeInChannel", channel);
//    compute.SetFloat("_pixelSize", pixelSize);
//    compute.SetFloat("_treeRadius", treeRadius);

//    uint[] args = new uint[3];

//    args[0] = (uint)(texture.width / 8);
//    args[1] = (uint)(texture.height / 8);
//    args[2] = 1;

//    ComputeBuffer argsBuffer = new ComputeBuffer(3, sizeof(uint), ComputeBufferType.IndirectArguments);
//    argsBuffer.SetData(args);
//    //compute.DispatchIndirect(kernel, argsBuffer);

//    compute.Dispatch(kernel, (texture.width / 8), (texture.height / 8), 1);
//}





/*
 ///USED TO GET ALL LODs


      if (Input.GetKeyDown(KeyCode.Q))
        {
            for(int i =0; i< treesPrefabs.Count; i++)
            {
                for (int j = 0; j < treesPrefabs[i].transform.childCount; j++)
                {
                    if(treesPrefabs[i].transform.GetChild(j).name.Contains("LOD1"))
                    {
                        treesPrefabs[i] = treesPrefabs[i].transform.GetChild(j).gameObject;
                        if (treesPrefabs == null)
                            Debug.Log("erros");
                        break;
                    }
                }
            }
        }
     
     */
