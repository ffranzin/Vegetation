
using UnityEngine;


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

    public int vegLevel;
}


public class _QuadTree
{
    public Bounds bound;
    public _QuadTree TL;
    public _QuadTree TR;
    public _QuadTree BL;
    public _QuadTree BR;
    public _QuadTree parent;
    public int level;
    public Atlas.AtlasPageDescriptor atlasPage = null;
    
    public bool positionsHasBeenGenerated = false;
    public bool hasChild = false;
    public bool containsVeg = false;

    public int myIdInNodePool;


    /// <summary>
    /// Create an struct used in computeshader.
    /// Has the same struct in StructuresTrees.cginc
    /// </summary>
    public QuadTreeInfo QuadTreeInfo(int vegLevel, float radius)
    {
        QuadTreeInfo qtInfo = new QuadTreeInfo();

        qtInfo.currentNodeAtlasOrigin = new Vector2(atlasPage.tl.x, atlasPage.tl.y);
        qtInfo.currentNodeAtlasSize = atlasPage.size;
        qtInfo.currentNodeWorldOrigin = bound.min.xz();
        qtInfo.currentNodeWorldSize = bound.size.x;

        if (vegLevel != 1)
        {
            qtInfo.upperNodeAtlasOrigin = new Vector2(parent.atlasPage.tl.x, parent.atlasPage.tl.y);
            qtInfo.upperNodeAtlasSize = parent.atlasPage.size;
            qtInfo.upperNodeWorldOrigin = parent.bound.min.xz();
            qtInfo.upperNodeWorldSize = parent.bound.size.x;
        }

        qtInfo.vegLevel = vegLevel;
        qtInfo.treeRadius = radius;

        return qtInfo;
    }
}




public class QuadTreeManager : MonoBehaviour
{
    public _QuadTree m_quadTree;

    Plane[] frustumPlanes;

    static int QUADTREE_MAX_LEVEL;
    static int QUADTREE_VEG_LEVEL_1;
    static int QUADTREE_VEG_LEVEL_2;
    static int QUADTREE_VEG_LEVEL_3;
    

    static int QUADTREE_MAX_GEN_NODES_PER_FRAME = 1600;
    static int QUADTREE_MAX_OPENED_NODES_PER_FRAME = 1600;

    static int QUADTREE_GEN_NODES = 0;
    static int QUADTREE_OPENED_NODES = 0;


    void Start()
    {
        CreateQuadTree();

        int terrainSize = (int)Mathf.Abs(TerrainManager.TERRAIN_ORIGIN.x - TerrainManager.TERRAIN_END.x);

        QUADTREE_MAX_LEVEL = terrainSize / GlobalManager.lowerestQuadTreeBlockSize;
        QUADTREE_MAX_LEVEL = (int)Mathf.Log(QUADTREE_MAX_LEVEL, 2f);

        QUADTREE_VEG_LEVEL_1 = QUADTREE_MAX_LEVEL - 2;
        QUADTREE_VEG_LEVEL_2 = QUADTREE_MAX_LEVEL - 1;
        QUADTREE_VEG_LEVEL_3 = QUADTREE_MAX_LEVEL;

        QUADTREE_VEG_LEVEL_1 = 2;
        QUADTREE_VEG_LEVEL_2 = 111;
        QUADTREE_VEG_LEVEL_3 = 111;
    }


    public void CreateQuadTree()
    {
        m_quadTree = NodePool.NodeRequest();
        m_quadTree.bound.min = TerrainManager.TERRAIN_ORIGIN.x0z();
        m_quadTree.bound.max = TerrainManager.TERRAIN_END.x0z();
        m_quadTree.parent = null;
        m_quadTree.level = 0;

        Utils.ShowBoundLines(m_quadTree.bound, Color.green, Mathf.Infinity);
    }


    private static void ShowBounds(_QuadTree qt)
    {
        if (qt == null || !qt.hasChild) return;

        Utils.ShowBoundLines(qt.bound, Color.black);

        ShowBounds(qt.BL);
        ShowBounds(qt.BR);
        ShowBounds(qt.TL);
        ShowBounds(qt.TR);
    }


    private void OnDrawGizmos()
    {
        ShowBounds(m_quadTree);
    }


    /// <summary>
    /// Rescale child's bound based in position.
    /// </summary>
    private static void GenBoundChild(Bounds bParent, ref Bounds bNode, int boundPosition)
    {
        //|1 = TL|   |2 = TR|   |3 = BL|  |4 = BR| 

        if (boundPosition == 1)
        {
            bNode.min = new Vector3(bParent.min.x, 0, ((bParent.min.z + bParent.max.z) / 2));
            bNode.max = new Vector3(((bParent.min.x + bParent.max.x) / 2), 0, bParent.max.z);
        }
        else if (boundPosition == 2)
        {
            bNode.min = new Vector3(((bParent.min.x + bParent.max.x) / 2), 0, ((bParent.min.z + bParent.max.z) / 2));
            bNode.max = new Vector3(bParent.max.x, 0, bParent.max.z);
        }
        else if (boundPosition == 3)
        {
            bNode.min = new Vector3(bParent.min.x, 0, bParent.min.z);
            bNode.max = new Vector3(((bParent.min.x + bParent.max.x) / 2), 0, ((bParent.min.z + bParent.max.z) / 2));
        }
        else if (boundPosition == 4)
        {
            bNode.min = new Vector3(((bParent.min.x + bParent.max.x) / 2), 0, bParent.min.z);
            bNode.max = new Vector3(bParent.max.x, 0, ((bParent.min.z + bParent.max.z) / 2));
        }
    }


    
    /// <summary>
    /// Subdivide an node and set all parameters of new childs.
    /// Any node are created, only requested on NodePool.
    /// </summary>
    /// <param name="qt"></param>
    private static void SubdivideQuadTree(_QuadTree qt, bool subdivideAll = false)
    {
        if (qt.level > QUADTREE_MAX_LEVEL || NodePool.freeNodes < 4) return;
        
        qt.TL = NodePool.NodeRequest();
        qt.TR = NodePool.NodeRequest();
        qt.BL = NodePool.NodeRequest();
        qt.BR = NodePool.NodeRequest();
        
        qt.TL.parent = qt.TR.parent = qt.BL.parent = qt.BR.parent = qt;
        qt.TL.level = qt.TR.level = qt.BL.level = qt.BR.level = qt.level + 1;
        qt.hasChild = true;
        qt.positionsHasBeenGenerated = false;

        GenBoundChild(qt.bound, ref qt.TL.bound, 1);
        GenBoundChild(qt.bound, ref qt.TR.bound, 2);
        GenBoundChild(qt.bound, ref qt.BL.bound, 3);
        GenBoundChild(qt.bound, ref qt.BR.bound, 4);

        QUADTREE_GEN_NODES++;

        if (!subdivideAll) return;

        SubdivideQuadTree(qt.TL, subdivideAll);
        SubdivideQuadTree(qt.TR, subdivideAll);
        SubdivideQuadTree(qt.BL, subdivideAll);
        SubdivideQuadTree(qt.BR, subdivideAll);
    }



    /// <summary>
    /// Request to GPU distribute positions to 'qt' node.
    /// Some nodes cannot receive vegetations. 
    /// </summary>
    /// <param name="qt"></param>
    public static void GenerateTreePositions(_QuadTree qt)
    {
        if (qt.positionsHasBeenGenerated) return;

        bool hasDispath = false;

        if (qt.level == QUADTREE_VEG_LEVEL_1)
        {
            qt.atlasPage = GlobalManager.m_atlas.GetPage();
            DispatcherComputePositions.ComputePositions(qt, GlobalManager.VEG_MIN_DIST_L1 / 2, 1);
            hasDispath = true;
        }
        else if (qt.level == QUADTREE_VEG_LEVEL_2 && false)
        {
            qt.atlasPage = GlobalManager.m_atlas.GetPage();
            DispatcherComputePositions.ComputePositions(qt, GlobalManager.VEG_MIN_DIST_L2 / 2, 2);
            hasDispath = true;
        }
        else if (qt.level == QUADTREE_VEG_LEVEL_3 && false)
        {
            qt.atlasPage = GlobalManager.m_atlas.GetPage();
            DispatcherComputePositions.ComputePositions(qt, GlobalManager.VEG_MIN_DIST_L3 / 2, 3);
            hasDispath = true;
        }

        if (hasDispath)
        {
            qt.containsVeg = true;
            QUADTREE_GEN_NODES++;
        }
        
        qt.positionsHasBeenGenerated = true;
    }



    /// <summary>
    /// Are visible node if is inside frustum camera and inside an radius of view.
    /// The radius of view are variable between node height.
    /// </summary>
    public bool NodeIsVisible(_QuadTree qt)
    {
        if (!GeometryUtility.TestPlanesAABB(frustumPlanes, qt.bound)) return false;
    
        float dist = Mathf.Sqrt(qt.bound.SqrDistance(Camera.main.transform.position.x0z()));

        if (qt.level <= QUADTREE_VEG_LEVEL_1 && GlobalManager.VIEW_RADIUS_VEG_L1 > dist) return true;
        if (qt.level <= QUADTREE_VEG_LEVEL_2 && GlobalManager.VIEW_RADIUS_VEG_L2 > dist) return true;
        if (qt.level <= QUADTREE_VEG_LEVEL_3 && GlobalManager.VIEW_RADIUS_VEG_L3 > dist) return true;

        return false;
    }

    
    /// <summary>
    /// Release all nodes that arent visible.
    /// These nodes are re-added in nodePool
    /// </summary>
    /// <param name="qt"></param>
    public void ClearNodes(_QuadTree qt)
    {
        if (qt == null) return;
        
        if (qt.hasChild)
        {
            ClearNodes(qt.TL);
            ClearNodes(qt.TR);
            ClearNodes(qt.BL);
            ClearNodes(qt.BR);
        }

        if (qt.parent != null && !NodeIsVisible(qt.parent))
            NodePool.NodeRelease(qt);
    }


    /// <summary>
    /// Verify if has new visible nodes.
    /// If necessary subdivide and request distribution of vegetation.
    /// </summary>
    /// <param name="qt"></param>
    public void VerifyNewNodes(_QuadTree qt)
    {
        if (qt == null || 
            QUADTREE_GEN_NODES >= QUADTREE_MAX_GEN_NODES_PER_FRAME ||
            QUADTREE_OPENED_NODES >= QUADTREE_MAX_OPENED_NODES_PER_FRAME ||
            !NodeIsVisible(qt) || qt.level > QUADTREE_VEG_LEVEL_1) return;
        
        if (!qt.hasChild)
            SubdivideQuadTree(qt);

        if (!qt.positionsHasBeenGenerated)
            GenerateTreePositions(qt);

        VerifyNewNodes(qt.BL);
        VerifyNewNodes(qt.BR);
        VerifyNewNodes(qt.TL);
        VerifyNewNodes(qt.TR);
    }

    
    
    private void Update()
    {
        if (m_quadTree == null) return;
           
        QUADTREE_OPENED_NODES = QUADTREE_GEN_NODES = 0;
         
        frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        if(!NodePool.LockToRelease)
            VerifyNewNodes(m_quadTree);

        ClearAll();
        
        TreePool.UpdateTreeAmountData();
    }


    int lastFrame = 0;
    /// <summary>
    /// Release nodes non visible.
    /// </summary>
    public void ClearAll()
    {
        int currentFrameCount = Time.frameCount;

        if (currentFrameCount - lastFrame < 160) return;

        lastFrame = currentFrameCount;
        
        ClearNodes(m_quadTree);

        Debug.Log("FREE NODES " + NodePool.freeNodes);
    }


}

