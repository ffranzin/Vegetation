using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using UnityEngine.Rendering;

//[ExecuteInEditMode]
[RequireComponent(typeof(TextureManager))]
public class MoistureDistribuition : MonoBehaviour
{
    #region CUSTOM_STRUCTS
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct ComputeParameters
    {
        // Distances
        [Header("Distances in Pixels")]
        [Range(1, 512)] public float d_MeanHeight;
        [Range(1, 512)] public float d_Slope;
        [Range(1, 512)] public float d_WaterSpread;

        // Weights
        [Header("Influence Curves Weights")]
        [Range(0, 10)] public float w_Height;
        [Range(0, 10)] public float w_RelativeHeight;
        [Range(0, 10)] public float w_Slope;
        [Range(0, 10)] public float w_WaterSpread;

        [Header("Sizes of the textures")]
        public Vector2Int s_Atlas;
        public Vector2Int s_Splat; // factor of 8
    }

    [System.Serializable]
    public struct InfluenceCurves
    {
        [Header("Moisture Influence Curves")]
        public AnimationCurve heightInfluence;
        public AnimationCurve relativeHeightInfluence;
        public AnimationCurve slopeInfluence;
        [Tooltip("Horizontal Water Spread")]
        public AnimationCurve waterHInfluence;
        [Tooltip("Vertical Water Spread")]
        public AnimationCurve waterVInfluence;

        [HideInInspector] public ComputeBuffer heightBuffer;
        [HideInInspector] public ComputeBuffer relativeHBuffer;
        [HideInInspector] public ComputeBuffer slopeBuffer;
        [HideInInspector] public ComputeBuffer waterHBuffer;
        [HideInInspector] public ComputeBuffer waterVBuffer;

        public void UpdateBuffers()
        {
            Profiler.BeginSample("Update Influence Curves");

            if (heightBuffer == null) heightBuffer = new ComputeBuffer(CURVE_BUFFER_SIZE, sizeof(float), ComputeBufferType.Default);
            if (relativeHBuffer == null) relativeHBuffer = new ComputeBuffer(CURVE_BUFFER_SIZE, sizeof(float), ComputeBufferType.Default);
            if (slopeBuffer == null) slopeBuffer = new ComputeBuffer(CURVE_BUFFER_SIZE, sizeof(float), ComputeBufferType.Default);
            if (waterHBuffer == null) waterHBuffer = new ComputeBuffer(CURVE_BUFFER_SIZE, sizeof(float), ComputeBufferType.Default);
            if (waterVBuffer == null) waterVBuffer = new ComputeBuffer(CURVE_BUFFER_SIZE, sizeof(float), ComputeBufferType.Default);

            float[] heightAux = new float[CURVE_BUFFER_SIZE];
            float[] relativeHAux = new float[CURVE_BUFFER_SIZE];
            float[] slopeAux = new float[CURVE_BUFFER_SIZE];
            float[] waterHAux = new float[CURVE_BUFFER_SIZE];
            float[] waterVAux = new float[CURVE_BUFFER_SIZE];

            for (int i = 0; i < CURVE_BUFFER_SIZE; i++)
            {
                float time = (float)(i + 1.0f) / (float)CURVE_BUFFER_SIZE;

                heightAux[i] = heightInfluence.Evaluate(time);
                relativeHAux[i] = relativeHeightInfluence.Evaluate(time);
                slopeAux[i] = slopeInfluence.Evaluate(time);
                waterHAux[i] = waterHInfluence.Evaluate(time);
                waterVAux[i] = waterVInfluence.Evaluate(time);
            }

            heightBuffer.SetData(heightAux);
            relativeHBuffer.SetData(relativeHAux);
            slopeBuffer.SetData(slopeAux);
            waterHBuffer.SetData(waterHAux);
            waterVBuffer.SetData(waterVAux);

            Profiler.EndSample();
        }

        public void ReleaseBuffers()
        {
            if (heightBuffer != null) heightBuffer.Release();
            if (relativeHBuffer != null) relativeHBuffer.Release();
            if (slopeBuffer != null) slopeBuffer.Release();
            if (waterHBuffer != null) waterHBuffer.Release();
            if (waterVBuffer != null) waterVBuffer.Release();

            heightBuffer = null;
            relativeHBuffer = null;
            slopeBuffer = null;
            waterHBuffer = null;
            waterVBuffer = null;
        }
    }
    #endregion

    #region PUBLIC VARIABLES
    public ComputeParameters parameters;
    [Space]
    public InfluenceCurves curves;

    [Header("Compute Shaders")]
    public ComputeShader meanHeightCompute;
    public ComputeShader relativeHeightCompute;
    public ComputeShader slopeCompute;
    public ComputeShader waterSpreadCompute;
    public ComputeShader moistureCompute;

    public int GroupSizeX { get { return (int)parameters.s_Splat.x / THREAD_GROUP_SIZE; } }
    public int GroupSizeY { get { return (int)parameters.s_Splat.y / THREAD_GROUP_SIZE; } }
    #endregion

    #region PRIVATE VARIABLES
    private const int CURVE_BUFFER_SIZE = 128; // same as in the .cginc
    private const int THREAD_GROUP_SIZE = 8; // same as in the .cginc

    private int meanHFirstKernel;
    private int meanHSecondKernel;
    private int relativeHKernel;
    private int slopeKernel;
    private int wSpreadFirstKernel;
    private int wSpreadSecondKernel;
    private int moistureKernel;

    private ComputeBuffer paramsBuffer;

    private TextureManager m_TexManager;
    public TextureManager TexManager
    {
        get {
            if (m_TexManager == null)
                m_TexManager = GetComponent<TextureManager>();
            return m_TexManager;
        }
    }
    #endregion

    #region PUBLIC METHODS
    private void Awake()
    {
        LoadDataFromFiles();
        parameters.s_Atlas = TexManager.AtlasDimensions;
        TexManager.InitTextures(parameters.s_Atlas, parameters.s_Splat);
        InitComputes(parameters.s_Atlas, parameters.s_Splat);

        CalculateAll(Vector2Int.zero);
    }

    private void Update()
    {
        return;
        if (Input.GetKeyUp(KeyCode.Space))
        {
            Debug.Log("Update");
            UpdateParameters();
            UpdateCurves();

            CalculateAll(Vector2Int.zero);
        }
    }

    

    public void CalculateAll(Vector2Int position)
    {
        // Calculate All
        Profiler.BeginSample("Calculate All GPU");

        UpdatePosition(position);
        
        CalculateMeanHeight();
        CalculateRelativeHeight();
        CalculateSlope();
        CalculateWaterSpread();
        CalculateMoisture();

        Profiler.EndSample();
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadDataFromFiles()
    {
        TexManager.LoadHeightData();
        TexManager.LoadWaterData();
    }

    private void InitComputes(Vector2 mainTexturesSize, Vector2 mapsTexturesSize)
    {
        meanHFirstKernel = meanHeightCompute.FindKernel("FirstPass");
        meanHSecondKernel = meanHeightCompute.FindKernel("SecondPass");
        relativeHKernel = relativeHeightCompute.FindKernel("CSMain");
        slopeKernel = slopeCompute.FindKernel("CSMain");
        wSpreadFirstKernel = waterSpreadCompute.FindKernel("FirstPass");
        wSpreadSecondKernel = waterSpreadCompute.FindKernel("SecondPass");
        moistureKernel = moistureCompute.FindKernel("CSMain");

        UpdateParameters();

        UpdateCurves();

        UpdateTextures();
    }

    /// <summary>
    /// 
    /// </summary>
    public void CleanData()
    {
        curves.ReleaseBuffers();
        if (paramsBuffer != null)
        {
            paramsBuffer.Release();
            paramsBuffer = null;
        }
        TexManager.ReleaseTextures();
    }


    private void OnDestroy()
    {
        CleanData();
    }
    /// <summary>
    /// 
    /// </summary>
    public void CalculateMeanHeight()
    {
        Profiler.BeginSample("Mean Height Calculation GPU");

        meanHeightCompute.Dispatch(meanHFirstKernel, GroupSizeX, GroupSizeY, 1);
        meanHeightCompute.Dispatch(meanHSecondKernel, GroupSizeX, GroupSizeY, 1);

        Profiler.EndSample();
    }

    /// <summary>
    /// 
    /// </summary>
    public void CalculateRelativeHeight()
    {
        Profiler.BeginSample("Relative Height Calculation GPU");

        relativeHeightCompute.Dispatch(relativeHKernel, GroupSizeX, GroupSizeY, 1);

        Profiler.EndSample();
    }

    /// <summary>
    /// 
    /// </summary>
    public void CalculateSlope()
    {
       Profiler.BeginSample("Slope Calculation GPU");

        slopeCompute.Dispatch(slopeKernel, GroupSizeX, GroupSizeY, 1);

       Profiler.EndSample();
    }

    /// <summary>
    /// 
    /// </summary>
    public void CalculateWaterSpread()
    {
        Profiler.BeginSample("Water Spread Calculation GPU");

        waterSpreadCompute.Dispatch(wSpreadFirstKernel, GroupSizeX, GroupSizeY, 1);
        waterSpreadCompute.Dispatch(wSpreadSecondKernel, GroupSizeX, GroupSizeY, 1);

        Profiler.EndSample();
    }

    /// <summary>
    /// 
    /// </summary>
    public void CalculateMoisture()
    {
        Profiler.BeginSample("Moisture Calculation GPU");

        moistureCompute.Dispatch(moistureKernel, GroupSizeX, GroupSizeY, 1);

       Profiler.EndSample();
    }

    public void UpdateParameters()
    {
        if (paramsBuffer == null) paramsBuffer = new ComputeBuffer(
            1,
            7 * sizeof(float) + 4 * sizeof(int),
            ComputeBufferType.Default);

        paramsBuffer.SetData(new ComputeParameters[1] { parameters });

        SetShaderParameters(meanHeightCompute, meanHFirstKernel);
        SetShaderParameters(meanHeightCompute, meanHSecondKernel);
        SetShaderParameters(relativeHeightCompute, relativeHKernel);
        SetShaderParameters(slopeCompute, slopeKernel);
        SetShaderParameters(waterSpreadCompute, wSpreadFirstKernel);
        SetShaderParameters(waterSpreadCompute, wSpreadSecondKernel);
        SetShaderParameters(moistureCompute, moistureKernel);
    }

    public void UpdateCurves()
    {
        curves.UpdateBuffers();

        //SetShaderCurves(meanHeightCompute, meanHFirstKernel); // curves not used in this compute shader
        //SetShaderCurves(meanHeightCompute, meanHSecondKernel); // curves not used in this compute shader
        //SetShaderCurves(relativeHeightCompute, relativeHKernel); // curves not used in this compute shader
        //SetShaderCurves(slopeCompute, slopeKernel); // curves not used in this compute shader

        SetShaderCurves(waterSpreadCompute, wSpreadFirstKernel);
        SetShaderCurves(waterSpreadCompute, wSpreadSecondKernel);
        SetShaderCurves(moistureCompute, moistureKernel);
    }
    #endregion

    #region PRIVATE METHODS
    private void UpdateTextures()
    {
        SetShaderTextures(meanHeightCompute, meanHFirstKernel);
        SetShaderTextures(meanHeightCompute, meanHSecondKernel);
        SetShaderTextures(relativeHeightCompute, relativeHKernel);
        SetShaderTextures(slopeCompute, slopeKernel);
        SetShaderTextures(waterSpreadCompute, wSpreadFirstKernel);
        SetShaderTextures(waterSpreadCompute, wSpreadSecondKernel);
        SetShaderTextures(moistureCompute, moistureKernel);
    }

    private void UpdatePosition(Vector2Int position)
    {
        int[] pos = new int[] { position.x, position.y };

        meanHeightCompute.SetInts("Pos", pos);
        relativeHeightCompute.SetInts("Pos", pos);
        slopeCompute.SetInts("Pos", pos);
        waterSpreadCompute.SetInts("Pos", pos);
        moistureCompute.SetInts("Pos", pos);
    }

    private void SetShaderParameters(ComputeShader shader, int kernel)
    {
        shader.SetBuffer(kernel, "Params", paramsBuffer);
    }

    private void SetShaderCurves(ComputeShader shader, int kernel)
    {
        shader.SetBuffer(kernel, "CurveHeight", curves.heightBuffer);
        shader.SetBuffer(kernel, "CurveRelativeHeight", curves.relativeHBuffer);
        shader.SetBuffer(kernel, "CurveSlope", curves.slopeBuffer);
        shader.SetBuffer(kernel, "CurveWaterH", curves.waterHBuffer);
        shader.SetBuffer(kernel, "CurveWaterV", curves.waterVBuffer);
    }

    private void SetShaderTextures(ComputeShader shader, int kernel)
    {
        shader.SetTexture(kernel, "TexHeight", TexManager.m_heightMapTex);
        shader.SetTexture(kernel, "TexMeanH", TexManager.m_meanHeightTex);
        shader.SetTexture(kernel, "TexRelativeH", TexManager.m_relativeHeightTex);
        shader.SetTexture(kernel, "TexSlope", TexManager.m_slopeTex);
        shader.SetTexture(kernel, "TexWater", TexManager.m_waterMapTex);
        shader.SetTexture(kernel, "TexWSpread", TexManager.m_waterSpreadTex);
        shader.SetTexture(kernel, "TexMoisture", TexManager.m_moistureTex);
        shader.SetTexture(kernel, "TexVPass", TexManager.m_vPassTex);
        shader.SetTexture(kernel, "TexHPass", TexManager.m_hPassTex);
    }
    #endregion
}
