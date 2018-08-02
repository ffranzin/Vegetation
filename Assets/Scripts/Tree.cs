
using System.Collections.Generic;
using UnityEngine;


public class Tree : MonoBehaviour
{
    const float WORLD_MIN_TEMPERATURE = -20;
    const float WORLD_MAX_TEMPERATURE = 50;


    private float TreeHeightOccuranceProbability(float worldHeight)
    {
        return Mathf.Clamp01(globalHeightCurve.Evaluate(worldHeight));
    }

    private float TreeHumidityOccuranceProbability(float worldMoisture)
    {
        return Mathf.Clamp01(humidityCurve.Evaluate(worldMoisture));
    }

    private float TreeSlopeOccuranceProbability(float worldInclination)
    {
        return Mathf.Clamp01(slopeCurve.Evaluate(worldInclination));
    }

    private float TreeSensitiveOccuranceProbability(float sensitive)
    {
        return sensitiveToUpperLevelCurve.Evaluate(sensitive);
    }


    [HideInInspector]
    public int myIndexInTreePool = 0;

    [Range(1,3)]
    public int my_layer;

    public AnimationCurve globalHeightCurve;
    public AnimationCurve temperatureCurve;
    public AnimationCurve humidityCurve;
    public AnimationCurve slopeCurve;
    public AnimationCurve sensitiveToUpperLevelCurve;
    
    public static int N_INFO_DISCRETIZED = 25;

    public float[] TreeHeightDiscretized
    {
        get
        {
            float[] info = new float[N_INFO_DISCRETIZED];

            for (int i = 0; i < N_INFO_DISCRETIZED; i++)
            {
                info[i] = TreeHeightOccuranceProbability((float)i / N_INFO_DISCRETIZED);
            }

            return info;
        }
    }

    public float[] TreeSlopeDiscretized
    {
        get
        {
            float[] info = new float[N_INFO_DISCRETIZED];

            for (int i = 0; i < N_INFO_DISCRETIZED; i++)
                info[i] = TreeSlopeOccuranceProbability((float)i / N_INFO_DISCRETIZED);

            return info;
        }
    }

    public float[] TreeSensitiveDiscretized
    {
        get
        {
            float[] info = new float[N_INFO_DISCRETIZED];

            for (int i = 0; i < N_INFO_DISCRETIZED; i++)
                info[i] = TreeSensitiveOccuranceProbability((float)i / N_INFO_DISCRETIZED);

            return info;
        }
    }


    public float[] TreeHumidityDiscretized
    {
        get
        {
            float[] info = new float[N_INFO_DISCRETIZED];

            for (int i = 0; i < N_INFO_DISCRETIZED; i++)
                info[i] = TreeHumidityOccuranceProbability((float)i / N_INFO_DISCRETIZED);

            return info;
        }
    }



    ////////////////////
    public Mesh[] models;

    private ComputeBuffer argsBuffer;

    private Material m_material;
    public Material material;
    public Texture2D maintex;
    public Color temp_color;
    bool enableRender = true;
    uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    public int positionsGenerated = 0;
    
    Mesh drawMesh;

    Bounds bound;
   
    private void Start()
    {
        drawMesh = models[1];

        m_material = new Material(material);

        if (m_material == null) Debug.Log("Some Material arent generated.");

        m_material.SetInt("_myIndexInTreePool", myIndexInTreePool);

        m_material.SetColor("_Color", temp_color);
        
        m_material.SetTexture("_HeightMap", TerrainManager.m_heightMap);

        m_material.SetInt("vegLevel", my_layer);

        m_material.SetTexture("_MainTex", maintex);

        argsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
        args[0] = (uint)drawMesh.GetIndexCount(drawMesh.subMeshCount - 1);
        args[1] = (uint)positionsGenerated;
        args[2] = (uint)drawMesh.GetIndexStart(0);
        args[3] = (uint)drawMesh.GetBaseVertex(0);

        bound = new Bounds(Vector3.zero, Vector3.one * 100000);
    }


    public void Update()
    {
        //if (Input.GetKeyDown(KeyCode.R)) enableRender = !enableRender;
        
        if (positionsGenerated == 0) return;
        bound.center = Camera.main.transform.position;
       // Graphics.DrawMeshInstancedIndirect(drawMesh, 0, m_material, bound, argsBuffer);
    }


    public void UpdateBuffers(out int genPos, out int level)
    {
        positionsGenerated = TreePool.positionsPerTreeAmountData[myIndexInTreePool];
        
        genPos = positionsGenerated;

        level = my_layer;
        
        m_material.SetColor("_Color", temp_color);
        
        m_material.SetTexture("_positionsTexture", TreePool.positionTexture);

        args[1] = (uint)positionsGenerated;
        
        argsBuffer.SetData(args);
    }

    private void OnDestroy()
    {
        argsBuffer.Release();
    }



}

 