
using System.Collections.Generic;
using UnityEngine;


public class Tree : MonoBehaviour
{
    const float TERRAIN_MIN_HEIGHT = 0;
    const float TERRAIN_MAX_HEIGHT = 100;

    const float WORLD_MIN_TEMPERATURE = -20;
    const float WORLD_MAX_TEMPERATURE = 50;


    public float TreeGlobalHeightOccuranceProbability(float worldHeight)
    {
        worldHeight = Utils.Remap(worldHeight, TERRAIN_MIN_HEIGHT, TERRAIN_MAX_HEIGHT, 0f, 1f);
        return Mathf.Clamp01(globalHeightCurve.Evaluate(worldHeight));
    }


    public float TreeMoistureOccuranceProbability(float worldMoisture)
    {
        return Mathf.Clamp01(moistureCurve.Evaluate(worldMoisture));
    }


    public float TreeInclinationOccuranceProbability(float worldInclination)
    {
        return Mathf.Clamp01(inclinationCurve.Evaluate(worldInclination));
    }


    public float TreeTemperatureOccuranceProbability(float worldTemperature)
    {
        worldTemperature = temperatureCurve.Evaluate(worldTemperature);
        return Utils.Remap(worldTemperature, WORLD_MIN_TEMPERATURE, WORLD_MAX_TEMPERATURE, 0, 1);
    }

    [HideInInspector]
    public int myIndexInTreePool = 0;

    public float treeMeshHeight;
    
    public AnimationCurve globalHeightCurve;
    public AnimationCurve localHeightCurve;
    public AnimationCurve temperatureCurve;
    public AnimationCurve moistureCurve;
    public AnimationCurve inclinationCurve;
    public AnimationCurve slopeCurve;
    public Mesh[] models;
    

    ////////////////////

        
    private ComputeBuffer argsBuffer;
    

    public List<Vector3> positions = new List<Vector3>();
    private Material m_material;
    public Material material;
    
    public Color temp_color;

    bool areUptodate = false;
    
    uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    
    int positionsGenerated = 0;


    public void Start()
    {
        if (treeMeshHeight == 0)    Debug.LogError("Some prefabs have the height zero.");

        argsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
        m_material = new Material(material);
        if (m_material == null) Debug.LogError("Some Material arent generated.");
    }
    

    public void LateUpdate()
    {
        positionsGenerated = SplatManager.positionsPerTreeIndexData[myIndexInTreePool];

        if (positionsGenerated == 0) return;

        UpdateBuffers();
        
        Graphics.DrawMeshInstancedIndirect(models[0], 0, m_material, new Bounds(Vector3.zero, Vector3.one * 100000), argsBuffer);
    }


    public void UpdateBuffers(Mesh mesh, int level)
    {
        areUptodate = false;
        
        m_material.SetColor("_Color", temp_color);

        m_material.SetBuffer("_positionsPerTreeIndexBuffer", SplatManager.positionsPerTreeBuffer);

        m_material.SetInt("_myPositionArrayIndex", myIndexInTreePool * 1000);
        
        args[0] = (uint)mesh.GetIndexCount(mesh.subMeshCount - 1);
        args[1] = (uint)positionsGenerated;
        args[2] = (uint)mesh.GetIndexStart(0);
        args[3] = (uint)mesh.GetBaseVertex(0);

        argsBuffer.SetData(args);
        areUptodate = true;
    }
    

    public void UpdateBuffers()
    {
        UpdateBuffers(models[0], 1);
    }

}
