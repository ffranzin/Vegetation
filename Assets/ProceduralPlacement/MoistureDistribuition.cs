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

[ExecuteInEditMode]
[RequireComponent(typeof(TextureManager))]
public class MoistureDistribuition : MonoBehaviour
{
    [System.Serializable]
    public struct Spread
    {
        [Range(1, 512)] public int maxDistance;
        public AnimationCurve horizontal;
        public int maxHeight;
        public AnimationCurve vertical;
    }

    #region PUBLIC VARIABLES
    [Range(1, 1024)] public int horizontalTiles = 16;
    [Range(0, 512)] public int meanHeightDistance = 1; // in pixels
    [Range(1, 512)] public int slopeDistance = 1; // in pixels
    [Range(0, 1)] public float waterThreshold = 0.99f;

    [Header("Humidity Parameters")]
    public AnimationCurve verticalMoisture;
    public AnimationCurve relativeHeightInfluence;
    [Space]
    public Spread spread;
    [Header("Positive And Negative Influences")]
    [Range(0, 20)] public float relativeHeightWeight = 1f;
    [Header("Positive Influences")]
    [Range(0, 20)] public float waterBodiesWeight = 1f;
    [Header("Negative Influences")]
    [Range(0, 20)] public float slopeWeight = 0.5f;

    //public AnimationCurve verticalHumidity;

    [Space]
    public ComputeShader meanHeightCompute;
    public ComputeShader slopeCompute;
    public ComputeShader waterSpreadCompute;
    #endregion

    #region PRIVATE VARIABLES
    private const float MAX_SLOPE = 0.5f;
    private const int KERNEL_BUFFER_SIZE = 512;

    private ComputeBuffer heightBuffer;
    private ComputeBuffer waterBuffer;
    private ComputeBuffer vPassBuffer;
    private ComputeBuffer hPassBuffer;
    private ComputeBuffer kernelBuffer;

    private ComputeBuffer slopeBuffer;
    private ComputeBuffer meanHeightBuffer;
    private ComputeBuffer waterSpreadBuffer;

    private int slopeKernelHandle;

    private int meanHFirstKernel;
    private int meanHSecondKernel;

    private int wSpreadFirstKernel;
    private int wSpreadSecondKernel;

    /// <summary>
    /// Size of a tile in pixels
    /// </summary>
    private int tileSize { get { return TexManager.Width / horizontalTiles; } }
    private int hTiles { get { return horizontalTiles; } }
    private int vTiles { get { return TexManager.Height / tileSize; } }
    private int totalTiles { get { return hTiles * vTiles; } }

    private float[] heightData = null;
    private float[] waterData = null;
    private float[] waterSpreadData = null;
    private float[] meanHeightData = null;
    private float[] relativeHeightData = null;
    private float[] slopeData = null;
    private float[] moistureData = null;
    private float[] kernelValues = null;

    private TextureManager m_TexManager;
    private TextureManager TexManager
    {
        get {
            if (m_TexManager == null)
                m_TexManager = GetComponent<TextureManager>();
            return m_TexManager;
        }
    }
    #endregion

    #region PUBLIC METHODS
    /// <summary>
    /// 
    /// </summary>
    public void LoadDataFromFiles()
    {
        heightData = TexManager.LoadHeightData();
        waterData = TexManager.LoadWaterData();

        TexManager.UpdateHeightmapTexture();
        TexManager.UpdateWatermapTexture();

        InitBuffers();
    }

    private void InitBuffers()
    {
        TexManager.InitTextures(TexManager.Dimensions, new Vector2(1024, 1024));

        // Kernels
        meanHFirstKernel = meanHeightCompute.FindKernel("FirstPass");
        meanHSecondKernel = meanHeightCompute.FindKernel("SecondPass");

        meanHeightCompute.SetTextureFromGlobal(meanHFirstKernel, "TexHeight", "TexHeight");
        meanHeightCompute.SetTextureFromGlobal(meanHFirstKernel, "TexVPass", "TexVPass");
        meanHeightCompute.SetTextureFromGlobal(meanHFirstKernel, "TexHPass", "TexHPass");

        meanHeightCompute.SetTextureFromGlobal(meanHSecondKernel, "TexVPass", "TexVPass");
        meanHeightCompute.SetTextureFromGlobal(meanHSecondKernel, "TexHPass", "TexHPass");
        meanHeightCompute.SetTextureFromGlobal(meanHSecondKernel, "TexMeanH", "TexMeanH");

        slopeKernelHandle = slopeCompute.FindKernel("CSMain");

        wSpreadFirstKernel = waterSpreadCompute.FindKernel("FirstPass");
        wSpreadSecondKernel = waterSpreadCompute.FindKernel("SecondPass");

        waterSpreadCompute.SetTextureFromGlobal(wSpreadFirstKernel, "TexWater", "TexWater");
        waterSpreadCompute.SetTextureFromGlobal(wSpreadFirstKernel, "TexVPass", "TexVPass");
        waterSpreadCompute.SetTextureFromGlobal(wSpreadFirstKernel, "TexHPass", "TexHPass");

        waterSpreadCompute.SetTextureFromGlobal(wSpreadSecondKernel, "TexWater", "TexWater");
        waterSpreadCompute.SetTextureFromGlobal(wSpreadSecondKernel, "TexVPass", "TexVPass");
        waterSpreadCompute.SetTextureFromGlobal(wSpreadSecondKernel, "TexHPass", "TexHPass");
        waterSpreadCompute.SetTextureFromGlobal(wSpreadSecondKernel, "TexWSpread", "TexWSpread");
    }

    /// <summary>
    /// 
    /// </summary>
    public void CleanData()
    {
        TexManager.ReleaseTextures();

        heightData = null;
        waterData = null;
        waterSpreadData = null;
        meanHeightData = null;
        relativeHeightData = null;
        slopeData = null;
        moistureData = null;
    }

    /// <summary>
    /// 
    /// </summary>
    public void CalculateMeanHeight()
    {
        if (TexManager.IsHeightDataLoaded)
        {
            //Profiler.BeginSample("Mean Height Calculation CPU");

            //int[,] indexes;

            //GetTilesCornersIndexes(out indexes);
            //CalculateCornersMeans(heightData, meanHeightData, indexes);
            //InterpolateValues(meanHeightData, indexes);

            //Profiler.EndSample();

            Profiler.BeginSample("Mean Height Calculation GPU");

            meanHeightCompute.SetInts("Pos", new int[] { 0, 0, TexManager.Width, TexManager.Height });
            meanHeightCompute.SetInt("Distance", meanHeightDistance);

            // first pass
            meanHeightCompute.Dispatch(meanHFirstKernel, TexManager.Height / 8, TexManager.Width / 8, 1);
            // second pass
            meanHeightCompute.Dispatch(meanHSecondKernel, TexManager.Height / 8, TexManager.Width / 8, 1);

            Profiler.EndSample();

            //meanHeightBuffer.GetData(meanHeightData);
            //TexManager.UpdateMeanHeightTexture(meanHeightData);
        }
        else
            Debug.LogError("Mean Height not calculated.");
    }

    /// <summary>
    /// 
    /// </summary>
    public void CalculateRelativeHeight()
    {
        if (TexManager.IsHeightDataLoaded && meanHeightData != null)
        {
            Profiler.BeginSample("Relative Height Calculation CPU");

            for (int i = 0; i < heightData.Length; i++)
                relativeHeightData[i] = ((heightData[i] - meanHeightData[i]) + 1f) * 0.5f;

            Profiler.EndSample();

            TexManager.UpdateRelativeHeightTexture(relativeHeightData);
        }
        else
            Debug.LogError("Relative Height not calculated.");
    }

    /// <summary>
    /// 
    /// </summary>
    public void CalculateSlope()
    {
        if (TexManager.IsHeightDataLoaded)
        {
            Profiler.BeginSample("Slope Calculation CPU");

            slopeData = new float[heightData.Length];

            for (int i = 0; i < TexManager.Height; i++)
            {
                for (int j = 0; j < TexManager.Width; j++)
                {
                    slopeData[To1DIndex(i, j, TexManager.Width)] = CalculatePixelSlope(heightData, i, j);
                }
            }

            Profiler.EndSample();

            Profiler.BeginSample("Slope Calculation GPU");

            slopeCompute.SetInt("Distance", slopeDistance);
            slopeCompute.Dispatch(slopeKernelHandle, TexManager.Height / 8, TexManager.Width / 8, 1);

            Profiler.EndSample();

            slopeBuffer.GetData(slopeData);

            TexManager.UpdateSlopeTexture(slopeData);
        }
        else
            Debug.LogError("Slope not calculated.");
    }

    /// <summary>
    /// 
    /// </summary>
    public void CalculateWaterSpread()
    {
        if (TexManager.IsWaterDataLoaded)
        {
            Profiler.BeginSample("Water Spread Calculation GPU");

            waterSpreadCompute.SetInts("Pos", new int[] { 0, 0, TexManager.Width, TexManager.Height });
            waterSpreadCompute.SetInt("Distance", spread.maxDistance);

            //for (int i = 0; i < spread.maxDistance; i++)
            //{
            //    kernelValues[i] = spread.horizontal.Evaluate((float)i / (float)spread.maxDistance);
            //    Debug.Log("kernelValues[i]: " + kernelValues[i]);
            //}

            //kernelBuffer.SetData(kernelValues);

            // first pass
            waterSpreadCompute.Dispatch(wSpreadFirstKernel, TexManager.Height / 8, TexManager.Width / 8, 1);
            // second pass
            waterSpreadCompute.Dispatch(wSpreadSecondKernel, TexManager.Height / 8, TexManager.Width / 8, 1);

            Profiler.EndSample();

            //waterSpreadBuffer.GetData(waterSpreadData);

            //TexManager.UpdateWaterSpreadTexture(waterSpreadData);

        }
        else
            Debug.LogError("Water Spread not calculated.");
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDestroy()
    {
        heightBuffer.Release();
        waterBuffer.Release();
        vPassBuffer.Release();
        hPassBuffer.Release();
        meanHeightBuffer.Release();
        slopeBuffer.Release();
        waterSpreadBuffer.Release();
        kernelBuffer.Release();
    }

    /// <summary>
    /// 
    /// </summary>
    public void CalculateMoisture()
    {
        if (TexManager.IsHeightDataLoaded && meanHeightData != null && relativeHeightData != null && slopeData != null)
        {
            Profiler.BeginSample("Humidity Calculation CPU");

            moistureData = new float[TexManager.Resolution];

            for (int k = 0; k < moistureData.Length; k++)
            {
                float baseHumidity = verticalMoisture.Evaluate(heightData[k]);

                float relativeHumidity = relativeHeightWeight * relativeHeightInfluence.Evaluate(relativeHeightData[k]); //(1f - (relativeHeightData[k]));

                float water = waterSpreadData[k] * spread.vertical.Evaluate(heightData[k]);

                float finalHumidity = baseHumidity * (1 + relativeHumidity - slopeWeight * (slopeData[k])) + 0.1f * relativeHumidity + water * waterBodiesWeight + waterData[k];

                moistureData[k] = Mathf.Clamp01(finalHumidity);
            }

            Profiler.EndSample();

            TexManager.UpdateHumidityTexture(moistureData);
        }
        else
            Debug.LogError("Humidity not calculated.");
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateTextures()
    {
        TexManager.UpdateAllTextures(meanHeightData, relativeHeightData, slopeData, waterSpreadData, moistureData);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SaveTextures()
    {
        TexManager.SaveAllTextures(meanHeightData, relativeHeightData, slopeData, waterSpreadData, moistureData);
    }

    /// <summary>
    /// 
    /// </summary>
    public void PrintHeightMapValues()
    {
        Debug.Log("Nothing here");

        //if (heightData == null || heightData.Length == 0)
        //    ExtractHeightData();

        //byte[] relative = relativeHeightTex?.GetRawTextureData();

        //Debug.Log("Texture Format HM.: " + heightMapTex.format.ToString() + "\n size: " + heightMapTex.dimension.ToString());
        //Debug.Log("Texture Format RHM: " + heightMapTex.format.ToString() + "\n size: " + heightMapTex.dimension.ToString());
        //Debug.Log(heightData.Length);
        //Debug.Log(relative.Length);

        //StringBuilder sb1 = new StringBuilder();
        //StringBuilder sb2 = new StringBuilder();
        //for (int i = 0; i < 20; i++)
        //{
        //    sb1.Append(heightData[i].ToString() + " | ");
        //    if (relative != null) sb2.Append(relative[i].ToString() + " | ");
        //}

        //Debug.Log("HeightMap Data........: " + sb1.ToString());
        //Debug.Log("RelativeHeightMap Data: " + sb2.ToString());

    }
    #endregion

    #region PRIVATE METHODS
    /// <summary>
    /// 
    /// </summary>
    private void GetTilesCornersIndexes(out int[,] indexes)
    {
        indexes = new int[2, totalTiles * 2];

        for (int k = 0; k < totalTiles; k++)
        {
            int offset = k * 2;

            indexes[0, offset] = (k / hTiles) * tileSize;
            indexes[0, offset + 1] = indexes[0, offset] + tileSize - 1;
            indexes[1, offset] = (k % hTiles) * tileSize;
            indexes[1, offset + 1] = indexes[1, offset] + tileSize - 1;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void CalculateCornersMeans(float[] srcData, float[] dstData, int[,] idx)
    {
        for (int k = 0; k < totalTiles; k++)
        {
            int offset = k * 2;

            dstData[To1DIndex(idx[0, offset], idx[1, offset], TexManager.Width)] = CalculatePixelMean(srcData, idx[0, offset], idx[1, offset]);
            dstData[To1DIndex(idx[0, offset], idx[1, offset + 1], TexManager.Width)] = CalculatePixelMean(srcData, idx[0, offset], idx[1, offset + 1]);
            dstData[To1DIndex(idx[0, offset + 1], idx[1, offset + 1], TexManager.Width)] = CalculatePixelMean(srcData, idx[0, offset + 1], idx[1, offset + 1]);
            dstData[To1DIndex(idx[0, offset + 1], idx[1, offset], TexManager.Width)] = CalculatePixelMean(srcData, idx[0, offset + 1], idx[1, offset]);
        }
    }

    /// <summary>
    /// Calculates the mean value of the pixel in the position [i, j] and its four neighbours (top, bottom, left and right).
    /// </summary>
    /// <param name="srcData">Array containing the data.</param>
    /// <param name="i">Row index.</param>
    /// <param name="j">Column index.</param>
    /// <param name="size">Texture size.</param>
    /// <returns>The calculated mean.</returns>
    private float CalculatePixelMean(float[] srcData, int i, int j)
    {
        float thisPixel = srcData[To1DIndex(i, j, TexManager.Width)];

        float mean = thisPixel;
        mean += (i > 0) ? srcData[To1DIndex(i - 1, j, TexManager.Width)] : thisPixel; // top
        mean += (i < (TexManager.Height - 1)) ? srcData[To1DIndex(i + 1, j, TexManager.Width)] : thisPixel; // bottom
        mean += (j > 0) ? srcData[To1DIndex(i, j - 1, TexManager.Width)] : thisPixel; // left
        mean += (j < (TexManager.Width - 1)) ? srcData[To1DIndex(i, j + 1, TexManager.Width)] : thisPixel; // right

        return mean / 5f;
    }

    /// <summary>
    /// 
    /// </summary>
    private float CalculatePixelSlope(float[] srcData, int i, int j)
    {
        // top, bottom, left and right pixels
        float t = srcData[To1DIndex(i - Mathf.Min(i, slopeDistance), j, TexManager.Width)];
        float b = srcData[To1DIndex(Mathf.Min(TexManager.Height - 1, i + slopeDistance), j, TexManager.Width)];
        float l = srcData[To1DIndex(i, j - Mathf.Min(j, slopeDistance), TexManager.Width)];
        float r = srcData[To1DIndex(i, Mathf.Min(TexManager.Width - 1, j + slopeDistance), TexManager.Width)];

        float slopeX = (l - r) / 2f;
        float slopeY = (t - b) / 2f;

        return Mathf.Sqrt((slopeX * slopeX) + (slopeY * slopeY)) / MAX_SLOPE;
    }

    /// <summary>
    /// 
    /// </summary>
    private void InterpolateValues(float[] data, int[,] idx)
    {
        float div = 1.0f / (float)tileSize;

        for (int k = 0; k < totalTiles; k++)
        {
            int offset = k * 2;
            int x1 = idx[0, offset];
            int x2 = idx[0, offset + 1];
            int y1 = idx[1, offset];
            int y2 = idx[1, offset + 1];

            float tl = data[To1DIndex(x1, y1, TexManager.Width)];
            float tr = data[To1DIndex(x1, y2, TexManager.Width)];
            float br = data[To1DIndex(x2, y2, TexManager.Width)];
            float bl = data[To1DIndex(x2, y1, TexManager.Width)];

            for (int x = x1; x <= x2; x++)
            {
                for (int y = y1; y <= y2; y++)
                {
                    data[To1DIndex(x, y, TexManager.Width)] = LinearInterpolation(
                        LinearInterpolation(tl, tr, (y - y1) * div),
                        LinearInterpolation(bl, br, (y - y1) * div),
                        (x - x1) * div);
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private int[] LocateWater(float[] data)
    {
        List<int> indexes = new List<int>(TexManager.Resolution / 4); // pre-allocates 25% of the max size to try to reduce the impact of adding to the list

        for (int k = 0; k < data.Length; k++)
        {
            if (waterData[k] <= waterThreshold)
            {
                indexes.Add(k);
            }
        }

        return indexes.ToArray();
    }

    #endregion

    #region UTILS
    /// <summary>
    /// 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float LinearInterpolation(float a, float b, float t)
    {
        return (b - a) * t + a;
    }

    /// <summary>
    /// 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int To1DIndex(int i, int j, int rowLength)
    {
        return i * rowLength + j;
    }

    /// <summary>
    /// 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void To2DIndex(int index1D, int rowLength, out int i, out int j)
    {
        i = index1D / rowLength;
        j = index1D % rowLength;
    }
    #endregion

    private void OnValidate()
    {
        //Debug.Log("ON VALIDATE");
        horizontalTiles = NearestPowerOfTwo(horizontalTiles); /* += horizontalTiles % 2;*/

        CalculateMeanHeight();
    }

    private int NearestPowerOfTwo(int n)
    {
        if (n <= 1) return 1;
        int power = System.Convert.ToInt32(Math.Round(Math.Log((double)n) / Math.Log(2.0)));
        return 1 << power;
    }
}
