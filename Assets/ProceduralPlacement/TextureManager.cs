using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

[ExecuteInEditMode]
public class TextureManager : MonoBehaviour
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

    public string HeighmapPath;// = MapsPath + "heightmap_Rfloat.tif";
    public string WatermapPath;// = MapsPath + "watertmap_Rfloat.tif";

    [Space]
    public RawImage heightmapRawImg;
    public RawImage meanHeightRawImg;
    public RawImage relativeHeightRawImg;
    public RawImage slopeRawImg;
    public RawImage watermapRawImg;
    public RawImage waterSpreadRawImg;
    public RawImage humidityRawImg;

    public string MapsPath { get { return Application.dataPath + "/Resources/Maps/"; } }

    public int Width { get { return m_heightmapInfo.rasterSizeX; } }
    public int Height { get { return m_heightmapInfo.rasterSizeY; } }
    public int Resolution { get { return Width * Height; } }
    public int SizeInBytes { get { return Resolution * sizeof(float); } }
    public bool IsHeightDataLoaded { get { return HeightData != null && HeightData.Length > 0 && HeightData.Length == Resolution; } }
    public bool IsWaterDataLoaded { get { return WaterData != null && WaterData.Length > 0 && WaterData.Length == Resolution; } }
    public Vector2 Dimensions { get { return new Vector2(Width, Height); } }

    public float[] HeightData { get; private set; }
    public float[] WaterData { get; private set; }
    public RasterReader.RasterInfo HeightmapInfo { get { return m_heightmapInfo; } }
    public RasterReader.RasterInfo WatermapInfo { get { return m_watermapInfo; } }

    //private Texture2D heightMapTex;
    //private Texture2D waterMapTex;
    //private Texture2D waterSpreadTex;
    //private Texture2D meanHeightTex;
    //private Texture2D relativeHeightTex;
    //private Texture2D slopeTex;
    //private Texture2D humidityTex;

    //private float[] HeightData;
    //private float[] WaterData;
    private RasterReader.RasterInfo m_heightmapInfo;
    private RasterReader.RasterInfo m_watermapInfo;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public float[] LoadHeightData()
    {
        HeightData = RasterReader.ReadTIFF(HeighmapPath, out m_heightmapInfo);

        if (HeightData != null)
            Debug.Log("Height Data loaded.\n" + m_heightmapInfo.ToString());
        else
            Debug.LogError("Error loading Height Data.");

        return HeightData;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public float[] LoadWaterData()
    {
        WaterData = RasterReader.ReadTIFF(WatermapPath, out m_watermapInfo);

        if (WaterData != null)
            Debug.Log("Water Data loaded.\n" + m_watermapInfo.ToString());
        else
            Debug.LogError("Error loading Water Data.");

        return WaterData;
    }

    /// <summary>
    /// 
    /// </summary>
    public void CleanData()
    {
        HeightData = null;
        WaterData = null;

        m_heightmapInfo = default(RasterReader.RasterInfo);

        heightmapRawImg.texture = null;
        meanHeightRawImg.texture = null;
        relativeHeightRawImg.texture = null;
        slopeRawImg.texture = null;
        watermapRawImg.texture = null;
        waterSpreadRawImg.texture = null;
        humidityRawImg.texture = null;
    }

    #region TEXTURES UPDATE
    /// <summary>
    /// 
    /// </summary>
    public void UpdateHeightmapTexture(byte[] tmpBuffer = null)
    {
        UpdateTexture(HeightData, heightmapRawImg, tmpBuffer);
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateWatermapTexture(byte[] tmpBuffer = null)
    {
        if (!IsHeightDataLoaded)
        {
            Debug.LogError("Water Data not loaded.");
            return;
        }
        UpdateTexture(WaterData, watermapRawImg, tmpBuffer);
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateMeanHeightTexture(float[] meanHeightData, byte[] tmpBuffer = null)
    {
        UpdateTexture(meanHeightData, meanHeightRawImg, tmpBuffer);
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateRelativeHeightTexture(float[] relativeHeightData, byte[] tmpBuffer = null)
    {
        UpdateTexture(relativeHeightData, relativeHeightRawImg, tmpBuffer);
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateSlopeTexture(float[] slopeData, byte[] tmpBuffer = null)
    {
        UpdateTexture(slopeData, slopeRawImg, tmpBuffer);
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateWaterSpreadTexture(float[] waterSpreadData, byte[] tmpBuffer = null)
    {
        UpdateTexture(waterSpreadData, waterSpreadRawImg, tmpBuffer);
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateHumidityTexture(float[] humidityData, byte[] tmpBuffer = null)
    {
        UpdateTexture(humidityData, humidityRawImg, tmpBuffer);
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateAllTextures(float[] meanHeightData, float[] relativeHeightData, float[] slopeData,
        float[] waterSpreadData, float[] humidityData)
    {
        byte[] tmpBuffer = new byte[SizeInBytes];

        UpdateHeightmapTexture(tmpBuffer);
        UpdateWatermapTexture(tmpBuffer);
        UpdateMeanHeightTexture(meanHeightData, tmpBuffer);
        UpdateRelativeHeightTexture(relativeHeightData, tmpBuffer);
        UpdateSlopeTexture(slopeData, tmpBuffer);
        UpdateWaterSpreadTexture(waterSpreadData, tmpBuffer);
        UpdateHumidityTexture(humidityData, tmpBuffer);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="rawImage"></param>
    /// <param name="tmpBuffer"></param>
    private void UpdateTexture(float[] data, RawImage rawImage, byte[] tmpBuffer = null)
    {
        if (!IsHeightDataLoaded)
        {
            Debug.LogError("Height Data not loaded.");
            return;
        }

        if (data == null || data.Length < Resolution)
        {
            Debug.LogError((data == null ? "No" : "Not enough") + " data.");
            return;
        }

        if (tmpBuffer == null || tmpBuffer.Length != SizeInBytes)
        {
            tmpBuffer = new byte[SizeInBytes];
            Debug.LogWarning("Temporary buffer created.");
        }

        Buffer.BlockCopy(data, 0, tmpBuffer, 0, tmpBuffer.Length);
        Texture2D tex = new Texture2D(Width, Height, TextureFormat.RFloat, false, true);
        tex.LoadRawTextureData(tmpBuffer);
        tex.alphaIsTransparency = false;
        tex.Apply();
        rawImage.texture = tex;
    }

    #endregion

    public void SaveAllTextures(float[] meanHeightData, float[] relativeHeightData, float[] slopeData,
       float[] waterSpreadData, float[] humidityData)
    {
        int x = HeightmapInfo.rasterSizeX;
        int y = HeightmapInfo.rasterSizeY;

        RasterReader.WriteTIFF(meanHeightData, MapsPath + "Outputs/meanHeight_RFloat.tif", x, y);
        RasterReader.WriteTIFF(relativeHeightData, MapsPath + "Outputs/relativeHeight_RFloat.tif", x, y);
        RasterReader.WriteTIFF(slopeData, MapsPath + "Outputs/slope_RFloat.tif", x, y);
        RasterReader.WriteTIFF(waterSpreadData, MapsPath + "Outputs/waterSpread_RFloat.tif", x, y);
        RasterReader.WriteTIFF(humidityData, MapsPath + "Outputs/humidity_RFloat.tif", x, y);

    }
}
