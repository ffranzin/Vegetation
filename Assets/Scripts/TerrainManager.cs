﻿using System.Collections.Generic;
using UnityEngine;


public class TerrainManager : MonoBehaviour {
    
    public Texture2D heightMap;
    public Texture2D treeDistribuition;

    public static readonly Vector3 TERRAIN_ORIGIN   = new Vector3(0, 0, 0);
    public static readonly Vector3 TERRAIN_END      = new Vector3(5000, 0, 5000);

    public static readonly float MINHEIGHT = 0;
    public static readonly float MAXHEIGHT = 100;

    public static Texture2D m_heightMap;
    public static Texture2D m_treeSplat;

    public static double STEP_HEIGHT;
    public static double STEP_WIDTH;
    public static float TERRAIN_HEIGHT_MULTIPLIER = 30;
    public static float TERRAIN_HEIGHT_LAKE = -1;

    Material terrain;



    public static float SampleHeight(Vector3 pos)
    {   
        int x = Mathf.RoundToInt((float)(pos.x / STEP_WIDTH));
        int y = Mathf.RoundToInt((float)(pos.z / STEP_HEIGHT));

        return m_heightMap.GetPixel(x, y).r * TERRAIN_HEIGHT_MULTIPLIER;
    }


    public static bool IsInsideTerrain(Vector3 p)
    {
        return (p.x < 0 || p.z < 0 || p.x > TERRAIN_END.z || p.z > TERRAIN_END.z) ? false : true;
    }


    public static bool HasTreeInPos(Vector3 pos, float considerMinGradient = 0.4f)
    {
        if (!IsInsideTerrain(pos)) return false;

        int x = Mathf.RoundToInt((float)(pos.x / STEP_WIDTH));
        int y = Mathf.RoundToInt((float)(pos.z / STEP_HEIGHT));
        
        return m_treeSplat.GetPixel(x, y).r > considerMinGradient ? true : false;
    }


    public static bool HasLakeInPos(Vector3 pos)
    {
        if (!IsInsideTerrain(pos)) return false;
        
        return SampleHeight(pos) < TERRAIN_HEIGHT_LAKE ? true : false;
    }


    public static Vector3 AddHeight(Vector3 pos)
    {
        return new Vector3(pos.x, SampleHeight(pos), pos.z);
    }
    






    private void Awake()
    {
        m_heightMap = heightMap;
        m_treeSplat = treeDistribuition;
        STEP_WIDTH = TERRAIN_END.z / m_heightMap.width;
        STEP_HEIGHT = TERRAIN_END.z / m_heightMap.height;
        terrain = gameObject.GetComponent<MeshRenderer>().materials[0];
    }

    
    private void Update()
    {
        if (terrain == null) return;

        terrain.SetFloat("TERRAIN_HEIGHT_MULTIPLIER", TERRAIN_HEIGHT_MULTIPLIER);
        terrain.SetFloat("TERRAIN_HEIGHT_LAKE", TERRAIN_HEIGHT_LAKE);
        terrain.SetFloat("ROAD_WIDTH", RoadManager.ROAD_WIDTH);

        terrain.SetVectorArray("ROAD_SEGMENTS", RoadManager.roadSegments);
        terrain.SetInt("ROAD_SEGMENTS_COUNT", RoadManager.roadSegments.Count);
    }
    
}

