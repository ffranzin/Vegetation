
using System.Collections.Generic;
using UnityEngine;

public class _QuadTree
{
    public Bounds bound;
    public _QuadTree TL;
    public _QuadTree TR;
    public _QuadTree BL;
    public _QuadTree BR;
    public _QuadTree root;
    
    public EnvironmentConditions nodeConditions = new EnvironmentConditions();
}



public class QuadTree : MonoBehaviour
{
    [Range(1, 10)] public int treesPerNode;
    [Range(1, 20)] public float distBetweenTree;

    private static readonly int QUADTREE_MAX_LEVEL = 6;

    _QuadTree m_quadTree;
    List<Vector3> treesPositions = new List<Vector3>();
    List<GameObject> trees = new List<GameObject>();
    public List<GameObject> treesPrefabs = new List<GameObject>();

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
            newBound.min = new Vector3(((b.min.x + b.max.x) / 2),0 , ((b.min.z + b.max.z) / 2));
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


    private static bool HasChild(_QuadTree qt)
    {
        return (qt.BR != null || qt.BL != null || qt.TL != null || qt.TR != null) ?  true : false;
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


    private static void SubdivideQuadTree(_QuadTree qt, int curentLevel = 0)
    {
        if (curentLevel > QUADTREE_MAX_LEVEL) return;
        
        qt.TL = new _QuadTree();
        qt.TL.bound = GenBoundChild(qt.bound, 1);
        qt.TL.root = qt;
        SubdivideQuadTree(qt.TL, curentLevel + 1);
        
        ////////////////////////////
        qt.TR = new _QuadTree();
        qt.TR.bound = GenBoundChild(qt.bound, 2);
        qt.TR.root = qt;
        SubdivideQuadTree(qt.TR, curentLevel + 1);

        ////////////////////////////
        qt.BL = new _QuadTree();
        qt.BL.bound = GenBoundChild(qt.bound, 3);
        qt.BL.root = qt;
        SubdivideQuadTree(qt.BL, curentLevel + 1);

        ////////////////////////////
        qt.BR = new _QuadTree();
        qt.BR.bound = GenBoundChild(qt.bound, 4);
        qt.BR.root = qt;
        SubdivideQuadTree(qt.BR, curentLevel + 1);
    }


    void CreateQuadTree(Vector3 origin, Vector3 end)
    {
        m_quadTree = new _QuadTree();
        m_quadTree.bound = new Bounds();
        m_quadTree.bound.min = origin.x0z();
        m_quadTree.bound.max = end.x0z();
        m_quadTree.root = null;

        SubdivideQuadTree(m_quadTree);
    }


    void ClearQuadtree(_QuadTree qt)
    {
        if (qt.TL == null) return;

        ClearQuadtree(qt.BL);
        ClearQuadtree(qt.BR);
        ClearQuadtree(qt.TL);
        ClearQuadtree(qt.TR);

        if (!HasChild(qt.BL) && !TerrainManager.HasTreeInPos(qt.BL.bound.center)) qt.BL = null;
        if (!HasChild(qt.BR) && !TerrainManager.HasTreeInPos(qt.BR.bound.center)) qt.BR = null;
        if (!HasChild(qt.TL) && !TerrainManager.HasTreeInPos(qt.TL.bound.center)) qt.TL = null;
        if (!HasChild(qt.TR) && !TerrainManager.HasTreeInPos(qt.TR.bound.center)) qt.TR = null;
    }
    
    
    void Start()
    {
        CreateQuadTree(TerrainManager.TERRAIN_ORIGIN, TerrainManager.TERRAIN_END);

        ClearQuadtree(m_quadTree);

        GenerateTreePositions(m_quadTree, 1, 5);
        RemoveNearPositions(1);
        SpawnTrees();

        Debug.Log("Trees Generated : " + treesPositions.Count);
    }


    void GenerateTreePositions(_QuadTree qt, float minDist, int nPosPerLeaf)
    {
        if (qt == null) return;

        if (!HasChild(qt))
        {
            List<Vector3> m_trees = Utils.GenerateRandomPosInsideBound(qt.bound, nPosPerLeaf, minDist);

            treesPositions.AddRange( m_trees.FindAll(a => !RoadManager.IsInsideRoad(a) && 
                                                          !TerrainManager.HasLakeInPos(a)));
            return;
        }

        GenerateTreePositions(qt.BL, minDist, nPosPerLeaf);
        GenerateTreePositions(qt.BR, minDist, nPosPerLeaf);
        GenerateTreePositions(qt.TL, minDist, nPosPerLeaf);
        GenerateTreePositions(qt.TR, minDist, nPosPerLeaf);
    }

    

    void RemoveNearPositions(float minDist)
    {
        return;
        for (int i = 0; i < treesPositions.Count - 1; i++)
        {
            for (int j = i + 1; j < treesPositions.Count; j++)
            {
                if ((treesPositions[i] - treesPositions[j]).magnitude < minDist)
                {
                    treesPositions.RemoveAt(j);
                    j--;
                }
            }
        }
    }

    void SpawnTrees()
    {
        foreach (Vector3 tree in treesPositions)
        {
            GameObject prefab = treesPrefabs[Random.Range(0, treesPrefabs.Count - 1)];
            
            GameObject go = Instantiate(prefab, tree, Quaternion.identity);
            go.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            go.transform.position = TerrainManager.AddHeight(tree);
            go.transform.Rotate(Vector3.up, Random.Range(0, 380));
            go.hideFlags = HideFlags.HideInHierarchy;
            trees.Add(go);
        }
    }


    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            m_quadTree = null;
            treesPositions.Clear();

            foreach (GameObject g in trees)
                Destroy(g);

            CreateQuadTree(TerrainManager.TERRAIN_ORIGIN, TerrainManager.TERRAIN_END);
            ClearQuadtree(m_quadTree);

            GenerateTreePositions(m_quadTree, distBetweenTree, treesPerNode);
            RemoveNearPositions(1);
            SpawnTrees();
        }
    }


    private void OnDrawGizmos()
    {
        ShowBounds(m_quadTree);
    }

}

