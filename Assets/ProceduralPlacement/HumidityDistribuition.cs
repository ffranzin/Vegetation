using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

[ExecuteInEditMode]
public class HumidityDistribuition : MonoBehaviour
{
    [System.Serializable]
    public struct TextureSize
    {
        public int width;
        public int height;

        public TextureSize(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
    }

    [System.Serializable]
    public struct Spread
    {
        public int maxDistance;
        public AnimationCurve horizontal;
        public int maxHeight;
        public AnimationCurve vertical;
    }

    #region PUBLIC VARIABLES
    public int horizontalTiles = 16;
    public int slopeDistance = 1;
    [Range(0, 255)] public byte waterThreshold = 254;

    [Header("Humidity Parameters")]
    public AnimationCurve verticalHumidity;
    [Space]
    public Spread spread;
    [Header("Positive Influences")]
    [Range(0, 20)] public float relativeHeightWeight = 1f;
    [Range(0, 20)] public float waterbodiesWeight = 1f;
    [Header("Negative Influences")]
    [Range(0, 20)] public float slopeWeight = 0.5f;

    [Space]
    public RawImage heightMapRawImg;
    public RawImage waterMapRawImg;
    public RawImage waterSpreadRawImg;
    public RawImage meanHeightRawImg;
    public RawImage relativeHeightRawImg;
    public RawImage slopeRawImg;
    public RawImage humidityRawImg;

    [Space]
    public ComputeShader waterSpreadCompute;
    #endregion

    #region PRIVATE VARIABLES
    private const float MAX_SLOPE = 180f;

    private RenderTexture waterSpreadRT;

    private Texture2D heightMapTex;
    private Texture2D waterMapTex;
    private Texture2D waterSpreadTex;
    private Texture2D meanHeightTex;
    private Texture2D relativeHeightTex;
    private Texture2D slopeTex;
    private Texture2D humidityTex;

    private byte[] heightData;
    private byte[] waterData;
    private byte[] waterSpreadData;
    private byte[] meanHeightData;
    private byte[] relativeHeightData;
    private byte[] slopeData;
    private byte[] humidityData;
    #endregion

    #region PUBLIC METHODS
    /// <summary>
    /// 
    /// </summary>
    public void ExtractHeightData()
    {
        if (heightMapRawImg != null)
        {
            //heightMapTex = new Texture2D(heightMapRawImg.texture.width, heightMapRawImg.texture.height, TextureFormat.RFloat, false, true);//heightMapRawImg.texture. as Texture2D;
            //heightMapTex.LoadRawTextureData((heightMapRawImg.texture as Texture2D).GetRawTextureData());
            heightMapTex = heightMapRawImg.texture as Texture2D;
            heightData = heightMapTex.GetRawTextureData();
            //byte[] tempBuffer = heightMapTex.GetRawTextureData();
            //heightData = new float[tempBuffer.Length / 4];
            //Buffer.BlockCopy(tempBuffer, 0, heightData, 0, tempBuffer.Length);



            Debug.Log("HeightMap :: data length: " +  heightData.Length 
                + " | width: " + heightMapTex.width + " | height: " + heightMapTex.height);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void CalculateMeanHeight()
    {
        if (heightMapRawImg != null && heightMapRawImg.texture != null)
        {
            ExtractHeightData();
            meanHeightData = new byte[heightData.Length];

            TextureSize texSize = new TextureSize(heightMapTex.width, heightMapTex.height);
            int tileSize = texSize.width / horizontalTiles;
            int totalTiles = horizontalTiles * (texSize.height / tileSize);
            int[,] indexes;

            GetTilesCornersIndexes(out indexes, texSize, tileSize);
            CalculateCornersMeans(heightData, meanHeightData, indexes, texSize, totalTiles);
            InterpolateValues(meanHeightData, indexes, texSize, tileSize);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void CalculateRelativeHeight()
    {
        if(heightData != null && meanHeightData != null)
        {
            relativeHeightData = new byte[heightData.Length];

            for (int i = 0; i < heightData.Length; i++)
            {
                relativeHeightData[i] = System.Convert.ToByte(Mathf.Round(
                        ((((float)heightData[i] - (float)meanHeightData[i]) / 255f) + 1f) * 127.5f));
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void CalculateSlope()
    {
        if (heightMapRawImg != null && heightMapRawImg.texture != null)
        {
            ExtractHeightData();
            slopeData = new byte[heightData.Length];

            TextureSize texSize = new TextureSize(heightMapTex.width, heightMapTex.height);
            
            for(int i = 0; i < texSize.height; i++)
            {
                for (int j = 0; j < texSize.width; j++)
                {
                    slopeData[To1DIndex(i, j, texSize.width)] = CalculatePixelSlope(heightData, i, j, texSize);
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void CalculateWaterSpread()
    {
        if (heightData != null && waterMapRawImg != null)
        {
            waterMapTex = waterMapRawImg.texture as Texture2D;
            waterData = waterMapTex.GetRawTextureData();
            waterSpreadData = new byte[waterData.Length];

            //int length = spread.maxDistance * 2 + 1;
            float[] kernel = new float[spread.maxDistance * 2 + 1];
            //kernel[spread.maxDistance] = 1f;

            for (int k = 0; k <= spread.maxDistance; k++)
            {
                float value = spread.horizontal.Evaluate((float)k / (float)spread.maxDistance);
                kernel[spread.maxDistance + k] = value;
                kernel[spread.maxDistance - k] = value;
            }

            TextureSize size = new TextureSize(waterMapTex.width, waterMapTex.height);

            //if (waterSpreadRT != null) waterSpreadRT.Release();
            //waterSpreadRT = new RenderTexture(size.width, size.height, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
            //waterSpreadRT.enableRandomWrite = true;
            //waterSpreadRT.Create();

            int kernelHandle = waterSpreadCompute.FindKernel("CSMain");

            ComputeBuffer waterBuffer = new ComputeBuffer(waterData.Length, sizeof(float), ComputeBufferType.Default);
            waterBuffer.SetData(waterData.Select(b => { return (float)b / 255f; }).ToArray());

            ComputeBuffer waterSpreadBuffer = new ComputeBuffer(waterData.Length, sizeof(float), ComputeBufferType.Default);

            waterSpreadCompute.SetInt("Width", size.width);
            waterSpreadCompute.SetInt("Distance", spread.maxDistance);
            waterSpreadCompute.SetBuffer(kernelHandle, "WaterData", waterBuffer);
            waterSpreadCompute.SetBuffer(kernelHandle, "WaterSpreadData", waterSpreadBuffer);
            waterSpreadCompute.Dispatch(kernelHandle, waterData.Length / 64, 1, 1);
            //waterSpreadCompute.Dispatch(kernelHandle, size.width / 8, size.height / 8, 1);

            float[] result = new float[waterData.Length];
            waterSpreadBuffer.GetData(result);

            for(int i = 0; i < waterSpreadData.Length; i++)
            {
                waterSpreadData[i] = System.Convert.ToByte(Mathf.Clamp01(result[i]) * 255f);
            }

            waterSpreadTex = new Texture2D(size.width, size.height, TextureFormat.R8, false, true);
            waterSpreadTex.alphaIsTransparency = false;
            waterSpreadTex.LoadRawTextureData(waterSpreadData);
            waterSpreadTex.Apply();

            waterSpreadRawImg.texture = waterSpreadTex;

            waterBuffer.Release();
            waterSpreadBuffer.Release();
            //int[] positions = LocateWater(waterData, size);

            //for (int k = 0; k < positions.Length; k++)
            //{
            //    int i, j;
            //    To2DIndex(positions[k], size.width, out i, out j);

            //    // horizontal spread
            //    for(int x = (-spread.maxDistance); x <= spread.maxDistance; x++)
            //    {
            //        if (i + x < 0) continue;
            //        if (i + x >= size.width) break;

            //        waterSpreadData[k]
            //    }

            //    // vertical spread (not height)
            //    for (int y = 0; y < spread.maxDistance; y++)
            //    {
            //        if (i + y < 0) continue;
            //        if (i + y >= size.height) break;
            //    }
            //}
        }
    }

    private void OnDestroy()
    {
        if (waterSpreadRT != null)
        {
            waterSpreadRT.Release();
            waterSpreadRT = null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void CalculateHumidity()
    {
        if (heightData != null && meanHeightData != null && relativeHeightData != null && slopeData != null)
        {
            humidityData = new byte[heightData.Length];

            for(int k = 0; k < humidityData.Length; k++)
            {
                float baseHumidity = verticalHumidity.Evaluate((float)heightData[k] / 255f);

                float relativeHumidity = relativeHeightWeight * (1f - ((float)relativeHeightData[k] / 255f));

                float finalHumidity = baseHumidity * ( 1 + relativeHumidity - slopeWeight * ((float)slopeData[k] / 255f)) + 0.1f * relativeHumidity;
                
                humidityData[k] = System.Convert.ToByte(Mathf.Clamp01(finalHumidity) * 255f);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadHeightTextures()
    {
        meanHeightTex = new Texture2D(heightMapTex.width, heightMapTex.height, TextureFormat.R8, false, true);
        meanHeightTex.alphaIsTransparency = false;
        meanHeightTex.LoadRawTextureData(meanHeightData);
        meanHeightTex.Apply();

        meanHeightRawImg.texture = meanHeightTex;

        relativeHeightTex = new Texture2D(heightMapTex.width, heightMapTex.height, TextureFormat.R8, false, true);
        relativeHeightTex.alphaIsTransparency = false;
        relativeHeightTex.LoadRawTextureData(relativeHeightData);
        relativeHeightTex.Apply();

        relativeHeightRawImg.texture = relativeHeightTex;

        slopeTex = new Texture2D(heightMapTex.width, heightMapTex.height, TextureFormat.R8, false, true);
        slopeTex.alphaIsTransparency = false;
        slopeTex.LoadRawTextureData(slopeData);
        slopeTex.Apply();

        slopeRawImg.texture = slopeTex;

        humidityTex = new Texture2D(heightMapTex.width, heightMapTex.height, TextureFormat.RFloat, false, true);
        humidityTex.alphaIsTransparency = false;
        humidityTex.LoadRawTextureData(humidityData);
        humidityTex.Apply();

        humidityRawImg.texture = humidityTex;
    }

    /// <summary>
    /// 
    /// </summary>
    public void PrintHeightMapValues()
    {
        ExtractHeightData();

        byte[] relative = relativeHeightTex?.GetRawTextureData();

        Debug.Log("Texture Format HM.: " + heightMapTex.format.ToString() + "\n size: " + heightMapTex.dimension.ToString());
        Debug.Log("Texture Format RHM: " + heightMapTex.format.ToString() + "\n size: " + heightMapTex.dimension.ToString());
        Debug.Log(heightData.Length);
        Debug.Log(relative.Length);

        StringBuilder sb1 = new StringBuilder();
        StringBuilder sb2 = new StringBuilder();
        for (int i = 0; i < 20; i++)
        {
            sb1.Append(heightData[i].ToString() + " | ");
            if (relative != null) sb2.Append(relative[i].ToString() + " | ");
        }

        Debug.Log("HeightMap Data........: " + sb1.ToString());
        Debug.Log("RelativeHeightMap Data: " + sb2.ToString());
    }
    #endregion

    #region PRIVATE METHODS
    /// <summary>
    /// 
    /// </summary>
    private void GetTilesCornersIndexes(out int[,] indexes, TextureSize size, int tileSize)
    {
        int hTiles = size.width  / tileSize;
        int vTiles = size.height / tileSize;
        int totalTiles = hTiles * vTiles;

        indexes = new int[2, totalTiles * 2];
        
        for (int k = 0; k < totalTiles; k++)
        {
            int offset = k * 2;

            indexes[0, offset]     = (k / hTiles)       * tileSize;
            indexes[0, offset + 1] = indexes[0, offset] + tileSize - 1;
            indexes[1, offset]     = (k % hTiles)       * tileSize;
            indexes[1, offset + 1] = indexes[1, offset] + tileSize - 1;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void CalculateCornersMeans(byte[] srcData, byte[] dstData, int[,] idx, TextureSize size, int totalTiles)
    {
        for (int k = 0; k < totalTiles; k++)
        {
            int offset = k * 2;

            dstData[To1DIndex(idx[0, offset]  , idx[1, offset]  , size.width)] = CalculatePixelMean(srcData, idx[0, offset]  , idx[1, offset]  , size);
            dstData[To1DIndex(idx[0, offset]  , idx[1, offset+1], size.width)] = CalculatePixelMean(srcData, idx[0, offset]  , idx[1, offset+1], size);
            dstData[To1DIndex(idx[0, offset+1], idx[1, offset+1], size.width)] = CalculatePixelMean(srcData, idx[0, offset+1], idx[1, offset+1], size);
            dstData[To1DIndex(idx[0, offset+1], idx[1, offset]  , size.width)] = CalculatePixelMean(srcData, idx[0, offset+1], idx[1, offset]  , size);
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
    private byte CalculatePixelMean(byte[] srcData, int i, int j, TextureSize size)
    {
        byte thisPixel = srcData[To1DIndex(i, j, size.width)];

        int mean = thisPixel;
        mean += (i > 0)               ? srcData[To1DIndex(i - 1, j, size.width)] : thisPixel;
        mean += (i < (size.height-1)) ? srcData[To1DIndex(i + 1, j, size.width)] : thisPixel;
        mean += (j > 0)               ? srcData[To1DIndex(i, j - 1, size.width)] : thisPixel;
        mean += (j < (size.width-1))  ? srcData[To1DIndex(i, j + 1, size.width)] : thisPixel;
        
        return System.Convert.ToByte(mean / 5);
    }

    /// <summary>
    /// 
    /// </summary>
    private byte CalculatePixelSlope(byte[] srcData, int i, int j, TextureSize size)
    {
        // top, bottom, left and right pixels
        float t = srcData[To1DIndex(i - Mathf.Min(i, slopeDistance), j, size.width)];
        float b = srcData[To1DIndex(Mathf.Min(size.height-1, i + slopeDistance), j, size.width)];
        float l = srcData[To1DIndex(i, j - Mathf.Min(j, slopeDistance), size.width)];
        float r = srcData[To1DIndex(i, Mathf.Min(size.width-1, j + slopeDistance), size.width)];

        float slopeX = (l - r) / 2f;
        float slopeY = (t - b) / 2f;

        return System.Convert.ToByte(Mathf.Round(
            Mathf.Sqrt((slopeX * slopeX) + (slopeY * slopeY)) / MAX_SLOPE * 255f));
    }

    ///// <summary>
    ///// 
    ///// </summary>
    //private byte CalculatePixelWaterSpread(byte[] srcData, int i, int j, TextureSize size)
    //{
    //    // top, bottom, left and right pixels
    //    float t = srcData[To1DIndex(i - Mathf.Min(i, slopeDistance), j, size.width)];
    //    float b = srcData[To1DIndex(Mathf.Min(size.height - 1, i + slopeDistance), j, size.width)];
    //    float l = srcData[To1DIndex(i, j - Mathf.Min(j, slopeDistance), size.width)];
    //    float r = srcData[To1DIndex(i, Mathf.Min(size.width - 1, j + slopeDistance), size.width)];

    //    float slopeX = (l - r) / 2f;
    //    float slopeY = (t - b) / 2f;

    //    return System.Convert.ToByte(Mathf.Round(
    //        Mathf.Sqrt((slopeX * slopeX) + (slopeY * slopeY)) / MAX_SLOPE * 255f));
    //}

    /// <summary>
    /// 
    /// </summary>
    private void InterpolateValues(byte[] data, int[,] idx, TextureSize size, int tileSize)
    {
        int hTiles = size.width  / tileSize;
        int vTiles = size.height / tileSize;
        int totalTiles = hTiles * vTiles;
        
        float div = 1.0f / (float)tileSize;

        for (int k = 0; k < totalTiles; k++)
        {
            int offset = k * 2;
            int x1 = idx[0, offset];
            int x2 = idx[0, offset+1];
            int y1 = idx[1, offset];
            int y2 = idx[1, offset+1];

            byte tl = data[To1DIndex(x1, y1, size.width)];
            byte tr = data[To1DIndex(x1, y2, size.width)];
            byte br = data[To1DIndex(x2, y2, size.width)];
            byte bl = data[To1DIndex(x2, y1, size.width)];

            for(int x = x1; x <= x2; x++)
            {
                for (int y = y1; y <= y2; y++)
                {
                    data[To1DIndex(x, y, size.width)] = LinearInterpolation(
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
    private int[] LocateWater(byte[] data, TextureSize size)
    {
        List<int> indexes = new List<int>(size.width * size.height / 3);

        for(int k = 0; k < data.Length; k++)
        {
            if (waterData[k] <= waterThreshold)
            {
                indexes.Add(k);
            }
        }

        return indexes.ToArray();
    }

    /// <summary>
    /// 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte LinearInterpolation(byte a, byte b, float t)
    {
        return System.Convert.ToByte((b - a) * t + a);
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
}
