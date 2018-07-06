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
    public RenderTextureFormat texFormat = RenderTextureFormat.RFloat;
    public FilterMode filterMode = FilterMode.Bilinear;
    [Space]
    public RawImage heightmapRawImg;
    public RawImage meanHeightRawImg;
    public RawImage relativeHeightRawImg;
    public RawImage slopeRawImg;
    public RawImage watermapRawImg;
    public RawImage waterSpreadRawImg;
    public RawImage moistureRawImg;

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

    public RenderTexture m_heightMapTex { get; private set; }
    public RenderTexture m_waterMapTex { get; private set; }
    public RenderTexture m_waterSpreadTex { get; private set; }
    public RenderTexture m_meanHeightTex { get; private set; }
    public RenderTexture m_relativeHeightTex { get; private set; }
    public RenderTexture m_slopeTex { get; private set; }
    public RenderTexture m_moistureTex { get; private set; }

    public RenderTexture m_vPassTex { get; private set; }
    public RenderTexture m_hPassTex { get; private set; }

    private RasterReader.RasterInfo m_heightmapInfo;
    private RasterReader.RasterInfo m_watermapInfo;

    private byte[] auxTmpBuffer;

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
    public void ReleaseTextures()
    {
        HeightData = null;
        WaterData = null;

        m_heightmapInfo = default(RasterReader.RasterInfo);
        m_watermapInfo = default(RasterReader.RasterInfo);

        heightmapRawImg.texture = null;
        meanHeightRawImg.texture = null;
        relativeHeightRawImg.texture = null;
        slopeRawImg.texture = null;
        watermapRawImg.texture = null;
        waterSpreadRawImg.texture = null;
        moistureRawImg.texture = null;

        m_heightMapTex.Release();
        m_waterMapTex.Release();
        m_waterSpreadTex.Release();
        m_meanHeightTex.Release();
        m_relativeHeightTex.Release();
        m_slopeTex.Release();
        m_moistureTex.Release();
        m_vPassTex.Release();
        m_hPassTex.Release();

        m_heightMapTex = null;
        m_waterMapTex = null;
        m_waterSpreadTex = null;
        m_meanHeightTex = null;
        m_relativeHeightTex = null;
        m_slopeTex = null;
        m_moistureTex = null;
        m_vPassTex = null;
        m_hPassTex = null;

        Debug.Log("Data cleaned.");
    }

    public void InitTextures(Vector2 mainMapsSize, Vector2 auxMapsSize)
    {
        RenderTextureDescriptor mainDescriptor = new RenderTextureDescriptor()
        {
            width = (int)mainMapsSize.x,
            height = (int)mainMapsSize.y,
            colorFormat = texFormat,
            depthBufferBits = 0,
            volumeDepth = 1,
            msaaSamples = 1,
            enableRandomWrite = false,
            autoGenerateMips = false,
            useMipMap = false,
            dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
        };

        RenderTextureDescriptor auxDescriptor = new RenderTextureDescriptor()
        {
            width = (int)auxMapsSize.x,
            height = (int)auxMapsSize.y,
            colorFormat = texFormat,
            depthBufferBits = 0,
            volumeDepth = 1,
            msaaSamples = 1,
            enableRandomWrite = true,
            autoGenerateMips = false,
            useMipMap = false,
            dimension = UnityEngine.Rendering.TextureDimension.Tex2D
        };

        m_heightMapTex = new RenderTexture(mainDescriptor);
        m_waterMapTex = new RenderTexture(mainDescriptor);
        m_meanHeightTex = new RenderTexture(auxDescriptor);
        m_relativeHeightTex = new RenderTexture(auxDescriptor);
        m_slopeTex = new RenderTexture(auxDescriptor);
        m_moistureTex = new RenderTexture(auxDescriptor);
        m_waterSpreadTex = new RenderTexture(auxDescriptor);
        m_vPassTex = new RenderTexture(auxDescriptor);
        m_hPassTex = new RenderTexture(auxDescriptor);

        m_heightMapTex.Create();
        m_waterMapTex.Create();
        m_waterSpreadTex.Create();
        m_meanHeightTex.Create();
        m_relativeHeightTex.Create();
        m_slopeTex.Create();
        m_moistureTex.Create();
        m_vPassTex.Create();
        m_hPassTex.Create();

        m_heightMapTex.SetGlobalShaderProperty("TexHeight");
        m_waterMapTex.SetGlobalShaderProperty("TexWater");
        m_waterSpreadTex.SetGlobalShaderProperty("TexWSpread");
        m_meanHeightTex.SetGlobalShaderProperty("TexMeanH");
        m_relativeHeightTex.SetGlobalShaderProperty("TexRelativeH");
        m_slopeTex.SetGlobalShaderProperty("TexSlope");
        m_moistureTex.SetGlobalShaderProperty("TexMoisture");
        m_vPassTex.SetGlobalShaderProperty("TexVPass");
        m_hPassTex.SetGlobalShaderProperty("TexHPass");

        if (IsHeightDataLoaded)
        {
            Texture tempHeightTex = MakeTexture(HeightData, mainMapsSize);
            Graphics.Blit(tempHeightTex, m_heightMapTex);
            Debug.Log("Height data loaded to the GPU.");
        }
        else
            Debug.LogError("Height data not loaded to the GPU.");

        if (IsWaterDataLoaded)
        {
            Texture tempWaterTex = MakeTexture(WaterData, mainMapsSize);
            Graphics.Blit(tempWaterTex, m_waterMapTex);
            Debug.Log("Water data loaded to the GPU.");
        }
        else
            Debug.LogError("Water data not loaded to the GPU.");

        heightmapRawImg.texture = m_heightMapTex;
        meanHeightRawImg.texture = m_meanHeightTex;
        relativeHeightRawImg.texture = m_relativeHeightTex;
        slopeRawImg.texture = m_slopeTex;
        watermapRawImg.texture = m_waterMapTex;
        waterSpreadRawImg.texture = m_waterSpreadTex;
        moistureRawImg.texture = m_moistureTex;

        Debug.Log("RenderTextures initialized.");
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
        UpdateTexture(humidityData, moistureRawImg, tmpBuffer);
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
            if (auxTmpBuffer != null && auxTmpBuffer.Length == SizeInBytes)
            {
                tmpBuffer = auxTmpBuffer;
            }
            else
            {
                tmpBuffer = auxTmpBuffer = new byte[SizeInBytes];
                //tmpBuffer = new byte[SizeInBytes];
                Debug.LogWarning("Temporary buffer created.");
            }
        }

        m_heightMapTex = new RenderTexture(1, 1, 1, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);

        Buffer.BlockCopy(data, 0, tmpBuffer, 0, tmpBuffer.Length);
        Texture2D tex = new Texture2D(Width, Height, TextureFormat.RFloat, false, true);
        tex.LoadRawTextureData(tmpBuffer);
        tex.filterMode = filterMode;
        tex.alphaIsTransparency = false;
        tex.Apply();
        rawImage.texture = tex;
    }

    private Texture2D MakeTexture(float[] data, Vector2 size)
    {
        if (data == null)
        {
            Debug.LogError("No data to make texture.");
            return null;
        }

        byte[] tmpBuffer = new byte[data.Length * sizeof(float)];
        Buffer.BlockCopy(data, 0, tmpBuffer, 0, tmpBuffer.Length);

        Texture2D tex = new Texture2D((int)size.x, (int)size.y, TextureFormat.RFloat, false, true);
        tex.LoadRawTextureData(tmpBuffer);
        tex.filterMode = filterMode;
        tex.alphaIsTransparency = false;
        tex.Apply();

        return tex;
    }

    #endregion

    public void SaveAllTextures(float[] meanHeightData, float[] relativeHeightData, float[] slopeData,
       float[] waterSpreadData, float[] humidityData)
    {
        Debug.LogWarning("Nope!");
        return;

        int x = HeightmapInfo.rasterSizeX;
        int y = HeightmapInfo.rasterSizeY;

        RasterReader.WriteTIFF(meanHeightData, MapsPath + "Outputs/meanHeight_RFloat.tif", x, y);
        RasterReader.WriteTIFF(relativeHeightData, MapsPath + "Outputs/relativeHeight_RFloat.tif", x, y);
        RasterReader.WriteTIFF(slopeData, MapsPath + "Outputs/slope_RFloat.tif", x, y);
        RasterReader.WriteTIFF(waterSpreadData, MapsPath + "Outputs/waterSpread_RFloat.tif", x, y);
        RasterReader.WriteTIFF(humidityData, MapsPath + "Outputs/humidity_RFloat.tif", x, y);

    }

    private void OnDestroy()
    {
        ReleaseTextures();
    }
}
