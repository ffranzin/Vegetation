
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

    static int QUADTREE_MAX_GEN_NODES_PER_FRAME = 15;
    static int QUADTREE_MAX_OPENED_NODES_PER_FRAME = 15;

    static int QUADTREE_GEN_NODES = 0;
    static int QUADTREE_OPENED_NODES = 0;



    private static void SubdivideQuadTree(_QuadTree qt, bool subdivideAll = false)
    {
        if (qt.level > QUADTREE_MAX_LEVEL) return;

        qt.TL = new _QuadTree();
        qt.TR = new _QuadTree();
        qt.BL = new _QuadTree();
        qt.BR = new _QuadTree();

        qt.TL.parent = qt.TR.parent = qt.BL.parent = qt.BR.parent = qt;
        qt.TL.level = qt.TR.level = qt.BL.level = qt.BR.level = qt.level + 1;
        qt.hasChild = true;

        qt.TL.bound = GenBoundChild(qt.bound, 1);
        qt.TR.bound = GenBoundChild(qt.bound, 2);
        qt.BL.bound = GenBoundChild(qt.bound, 3);
        qt.BR.bound = GenBoundChild(qt.bound, 4);

        if (!subdivideAll) return;

        SubdivideQuadTree(qt.TL, subdivideAll);
        SubdivideQuadTree(qt.TR, subdivideAll);
        SubdivideQuadTree(qt.BL, subdivideAll);
        SubdivideQuadTree(qt.BR, subdivideAll);
    }


    public bool NodeIsVisible(_QuadTree qt)
    {
        float dist = Mathf.Sqrt(qt.bound.SqrDistance(Camera.main.transform.position.x0z()));

        if (qt.level >= QUADTREE_VEG_LEVEL_1 && GlobalManager.VIEW_RADIUS_VEG_L1 < dist) return false;
        else if (qt.level >= QUADTREE_VEG_LEVEL_2 && GlobalManager.VIEW_RADIUS_VEG_L2 < dist) return false;
        else if (qt.level >= QUADTREE_VEG_LEVEL_3 && GlobalManager.VIEW_RADIUS_VEG_L3 < dist) return false;

        return GeometryUtility.TestPlanesAABB(frustumPlanes, qt.bound);
    }

    int count;
    public void ClearNodes(_QuadTree qt)
    {
        if (qt == null || !qt.positionsHasBeenGenerated) return;

        ClearNodes(qt.TL);
        ClearNodes(qt.TR);
        ClearNodes(qt.BL);
        ClearNodes(qt.BR);

        //if (qt.parent == null)
        //{
        //    qt.hasChild = false;
        //    qt.positionsHasBeenGenerated = false;
        //    return;
        //}

        if (qt.atlasPage != null)
        {
            GlobalManager.m_atlas.ReleasePage(qt.atlasPage);
            qt.atlasPage = null;
        }
        count++;
        qt.positionsHasBeenGenerated = false;
        //qt = null;
    }

    public static void GenerateTreePositions(_QuadTree qt)
    {
        if (qt == null || qt.positionsHasBeenGenerated) return;

        if (qt.level == QUADTREE_VEG_LEVEL_1)
        {
            qt.atlasPage = GlobalManager.m_atlas.GetPage();
            SplatManager.ComputePositions(qt, GlobalManager.VEG_MIN_DIST_L1 / 2, 1);
            hasDispath = true;
        }
        else if (qt.level == QUADTREE_VEG_LEVEL_2)
        {
            qt.atlasPage = GlobalManager.m_atlas.GetPage();
            SplatManager.ComputePositions(qt, GlobalManager.VEG_MIN_DIST_L2 / 2, 2);
            hasDispath = true;
        }
        else if (qt.level == QUADTREE_VEG_LEVEL_3 && false)
        {
            qt.atlasPage = GlobalManager.m_atlas.GetPage();
            SplatManager.ComputePositions(qt, GlobalManager.VEG_MIN_DIST_L3 / 2, 3);
            hasDispath = true;
        }

        qt.positionsHasBeenGenerated = true;
    }


    public void RenderQuadTree(_QuadTree qt)
    {
        if (qt == null || 
            QUADTREE_GEN_NODES == QUADTREE_MAX_GEN_NODES_PER_FRAME ||
            QUADTREE_OPENED_NODES == QUADTREE_MAX_OPENED_NODES_PER_FRAME ||
            !NodeIsVisible(qt)) return;
        
        if (!qt.hasChild && qt.level < QUADTREE_MAX_LEVEL)
        {
            SubdivideQuadTree(qt);
            QUADTREE_GEN_NODES++;
        }

        if (!qt.positionsHasBeenGenerated)
        {
            GenerateTreePositions(qt);
            QUADTREE_GEN_NODES++;
        }

        RenderQuadTree(qt.BL);
        RenderQuadTree(qt.BR);
        RenderQuadTree(qt.TL);
        RenderQuadTree(qt.TR);
    }


    int lastFrame = 0;
    static bool hasDispath = false;
    private void Update()
    {
        if (m_quadTree != null)
        {
            hasDispath = false;
            QUADTREE_OPENED_NODES = 0;
            QUADTREE_GEN_NODES = 0;
            frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            RenderQuadTree(m_quadTree);
        }
        if(hasDispath)
        {
            GlobalManager.UpdateTreeAmountData();

            foreach (Tree t in TreePool.treePool)
                t.UpdateBuffers();
        }
    }


    private void LateUpdate()
    {
        int currentFrameCount = Time.frameCount;

        if (currentFrameCount - lastFrame < 160) return;

        ClearAll();

        lastFrame = currentFrameCount;
    }



    public void ClearAll()
    {
        ClearNodes(m_quadTree);
        
        //GlobalManager.UpdateTreeAmountData();

        //foreach (Tree t in TreePool.treePool)
        //    t.UpdateBuffers();
        GlobalManager.ResetPositionsAmount();
        
        count = 0;
    }

    //////////////////////////
    //////////////////////////
    //////////////////////////
    //////////////////////////
    //////////////////////////
    //////////////////////////
    //////////////////////////


    public void CreateQuadTree()
    {
        m_quadTree = new _QuadTree();
        m_quadTree.bound = new Bounds();
        m_quadTree.bound.min = TerrainManager.TERRAIN_ORIGIN.x0z();
        m_quadTree.bound.max = TerrainManager.TERRAIN_END.x0z();
        m_quadTree.parent = null;
        m_quadTree.level = 0;

        Utils.ShowBoundLines(m_quadTree.bound, Color.green, Mathf.Infinity);
    }



    void Start()
    {
        CreateQuadTree();
        //InvokeRepeating("ClearAll", 2, 1f);

        int terrainSize = (int)Mathf.Abs(TerrainManager.TERRAIN_ORIGIN.x - TerrainManager.TERRAIN_END.x);

        QUADTREE_MAX_LEVEL = (int)terrainSize / GlobalManager.lowerestQuadTreeBlockSize;
        QUADTREE_MAX_LEVEL = (int)Mathf.Log((float)QUADTREE_MAX_LEVEL, (float)2);

        QUADTREE_VEG_LEVEL_1 = QUADTREE_MAX_LEVEL - 2;
        QUADTREE_VEG_LEVEL_2 = QUADTREE_MAX_LEVEL - 1;
        QUADTREE_VEG_LEVEL_3 = QUADTREE_MAX_LEVEL;
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

