using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoadManager : MonoBehaviour {

    public static float ROAD_WIDTH = 8f;

    public List<GameObject> roadPositionsObj;
    
    static List<Vector3> m_roadPositions = new List<Vector3>();

    public static List<Vector4> roadSegments = new List<Vector4>();


    public static bool IsInsideRoad(Vector3 position)
    {
        for(int i = 0; i< m_roadPositions.Count - 1; i++)
        {
            if (Utils.DistancePointToLine(m_roadPositions[i], m_roadPositions[i + 1], position) < ROAD_WIDTH)
                return true;
        }

        return false;
    }


    public void Awake()
    {
        foreach (GameObject g in roadPositionsObj)
            m_roadPositions.Add(g.transform.position);

        GenRoadSegments();
    }


    static void GenRoadSegments()
    {
        for(int i = 0; i<m_roadPositions.Count - 1; i++)
        {
            roadSegments.Add(new Vector4(m_roadPositions[i].x, m_roadPositions[i].z,
                                         m_roadPositions[i + 1].x, m_roadPositions[i + 1].z));
        }
    }


    private void OnDrawGizmos()
    {
        float gap = 15;

        for (int i = 0; i < m_roadPositions.Count - 1; i++)
        {
            Vector3 dir = (m_roadPositions[i + 1] - m_roadPositions[i]).normalized;
            Vector3 pos = m_roadPositions[i];

            while ((pos - m_roadPositions[i + 1]).magnitude > gap)
            {
                pos += dir * gap;
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(TerrainManager.AddHeight(pos), 2);
            }
        }
    }



}
