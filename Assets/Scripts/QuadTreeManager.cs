
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

    public _QuadTree[] children = new _QuadTree[4];

    public _QuadTree parent;
    public int level;
    public Atlas.AtlasPageDescriptor atlasPage = null;

    public bool positionsHasBeenGenerated = false;

    public bool containsVeg = false;

    public int myIdInNodePool;


    public bool hasChild
    {
        get
        {
            for (int i = 0; i < 4; i++)
                if (children[i] == null)
                    return false;

            return true;
        }
    }





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

    static Plane[] frustumPlanes;

    static int QUADTREE_MAX_LEVEL;
    static int QUADTREE_VEG_LEVEL_1;
    static int QUADTREE_VEG_LEVEL_2;
    static int QUADTREE_VEG_LEVEL_3;
    
    static int QUADTREE_MAX_GEN_POS_NODES_PER_FRAME = 64;

    static int QUADTREE_GEN_POS_NODE = 0;


    void Start()
    {
        CreateQuadTree();

        int terrainSize = (int)Mathf.Abs(TerrainManager.TERRAIN_ORIGIN.x - TerrainManager.TERRAIN_END.x);

        QUADTREE_MAX_LEVEL = terrainSize / GlobalManager.lowerestQuadTreeBlockSize;
        QUADTREE_MAX_LEVEL = (int)Mathf.Log(QUADTREE_MAX_LEVEL, 2f);

        QUADTREE_VEG_LEVEL_1 = QUADTREE_MAX_LEVEL - 2;
        QUADTREE_VEG_LEVEL_2 = QUADTREE_MAX_LEVEL - 1;
        QUADTREE_VEG_LEVEL_3 = QUADTREE_MAX_LEVEL;
        
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

        foreach (_QuadTree c in qt.children)
            ShowBounds(c);
    }


    private void OnDrawGizmos()
    {
        ShowBounds(m_quadTree);
    }


    /// <summary>
    /// Are visible node if is inside frustum camera and inside an radius of view.
    /// The radius of view are variable between node height.
    /// </summary>
    public static bool NodeIsVisible(_QuadTree qt)
    {
        if (!GeometryUtility.TestPlanesAABB(frustumPlanes, qt.bound)) return false;

        float dist = Mathf.Sqrt(qt.bound.SqrDistance(Camera.main.transform.position.x0z()));

        if (qt.level <= QUADTREE_VEG_LEVEL_1 && GlobalManager.VIEW_RADIUS_VEG_L1 > dist) return true;
        if (qt.level <= QUADTREE_VEG_LEVEL_2 && GlobalManager.VIEW_RADIUS_VEG_L2 > dist) return true;
        if (qt.level <= QUADTREE_VEG_LEVEL_3 && GlobalManager.VIEW_RADIUS_VEG_L3 > dist) return true;

        return false;
    }



    /// <summary>
    /// Rescale child's bound based in position.
    /// </summary>
    private static void GenBoundChild(Bounds bParent, ref Bounds bNode, int boundPosition)
    {
        //|0 = TL|   |1 = TR|   |2 = BL|  |3 = BR| 

        if (boundPosition == 0)
        {
            bNode.min = new Vector3(bParent.min.x, 0, ((bParent.min.z + bParent.max.z) / 2));
            bNode.max = new Vector3(((bParent.min.x + bParent.max.x) / 2), 0, bParent.max.z);
        }
        else if (boundPosition == 1)
        {
            bNode.min = new Vector3(((bParent.min.x + bParent.max.x) / 2), 0, ((bParent.min.z + bParent.max.z) / 2));
            bNode.max = new Vector3(bParent.max.x, 0, bParent.max.z);
        }
        else if (boundPosition == 2)
        {
            bNode.min = new Vector3(bParent.min.x, 0, bParent.min.z);
            bNode.max = new Vector3(((bParent.min.x + bParent.max.x) / 2), 0, ((bParent.min.z + bParent.max.z) / 2));
        }
        else if (boundPosition == 3)
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

        for (int i = 0; i < qt.children.Length; i++)
        {
            qt.children[i] = NodePool.NodeRequest();
            qt.children[i].parent = qt;
            qt.children[i].level = qt.level + 1;
            qt.children[i].positionsHasBeenGenerated = false;
            qt.children[i].containsVeg = false;
            GenBoundChild(qt.bound, ref qt.children[i].bound, i);
        }
        
        for (int i = 0; subdivideAll && i < qt.children.Length; i++)
            SubdivideQuadTree(qt.children[i], subdivideAll);
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
        else if (qt.level == QUADTREE_VEG_LEVEL_2)
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
            QUADTREE_GEN_POS_NODE++;
        }

        qt.positionsHasBeenGenerated = true;
    }
    

    /// <summary>
    /// Release all nodes that arent visible.
    /// These nodes are re-added in nodePool
    /// </summary>
    /// <param name="qt"></param>
    public void ClearNodes(_QuadTree qt)
    {
        if (qt == null) return;

        for (int i = 0; i < qt.children.Length; i++)
            ClearNodes(qt.children[i]);

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
            QUADTREE_GEN_POS_NODE >= QUADTREE_MAX_GEN_POS_NODES_PER_FRAME ||
            !NodeIsVisible(qt)) return;

        if (!qt.hasChild)
            SubdivideQuadTree(qt);

        if (!qt.positionsHasBeenGenerated)
            GenerateTreePositions(qt);

        for (int i = 0; i < qt.children.Length; i++)
            VerifyNewNodes(qt.children[i]);
    }
    
    

    private void Update()
    {
        if (m_quadTree == null) return;

        QUADTREE_GEN_POS_NODE = 0;

        frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

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

        if (currentFrameCount - lastFrame < 20) return;

        lastFrame = currentFrameCount;

        ClearNodes(m_quadTree);

        //Debug.Log("FREE NODES " + NodePool.freeNodes);
    }


}

