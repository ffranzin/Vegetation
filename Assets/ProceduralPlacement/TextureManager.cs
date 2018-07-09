using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

//[ExecuteInEditMode]
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

        if (heightmapRawImg != null) heightmapRawImg.texture = null;
        if (meanHeightRawImg != null) meanHeightRawImg.texture = null;
        if (relativeHeightRawImg != null) relativeHeightRawImg.texture = null;
        if (slopeRawImg != null) slopeRawImg.texture = null;
        if (watermapRawImg != null) watermapRawImg.texture = null;
        if (waterSpreadRawImg != null) waterSpreadRawImg.texture = null;
        if (moistureRawImg != null) moistureRawImg.texture = null;

        if (m_heightMapTex != null) m_heightMapTex.Release();
        if (m_waterMapTex != null) m_waterMapTex.Release();
        if (m_waterSpreadTex != null) m_waterSpreadTex.Release();
        if (m_meanHeightTex != null) m_meanHeightTex.Release();
        if (m_relativeHeightTex != null) m_relativeHeightTex.Release();
        if (m_slopeTex != null) m_slopeTex.Release();
        if (m_moistureTex != null) m_moistureTex.Release();
        if (m_vPassTex != null) m_vPassTex.Release();
        if (m_hPassTex != null) m_hPassTex.Release();

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
            volumeDepth = 1,
            colorFormat = texFormat,
            depthBufferBits = 0,
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

        m_heightMapTex.filterMode = filterMode;
        m_waterMapTex.filterMode = filterMode;
        m_waterSpreadTex.filterMode = filterMode;
        m_meanHeightTex.filterMode = filterMode;
        m_relativeHeightTex.filterMode = filterMode;
        m_slopeTex.filterMode = filterMode;
        m_moistureTex.filterMode = filterMode;
        m_vPassTex.filterMode = filterMode;
        m_hPassTex.filterMode = filterMode;

        m_heightMapTex.Create();
        m_waterMapTex.Create();
        m_waterSpreadTex.Create();
        m_meanHeightTex.Create();
        m_relativeHeightTex.Create();
        m_slopeTex.Create();
        m_moistureTex.Create();
        m_vPassTex.Create();
        m_hPassTex.Create();

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

    public void SaveAllTextures(float[] meanHeightData, float[] relativeHeightData, float[] slopeData,
       float[] waterSpreadData, float[] humidityData)
    {
        Debug.LogWarning("Nope!");
        return;

        //int x = HeightmapInfo.rasterSizeX;
        //int y = HeightmapInfo.rasterSizeY;

        //RasterReader.WriteTIFF(meanHeightData, MapsPath + "Outputs/meanHeight_RFloat.tif", x, y);
        //RasterReader.WriteTIFF(relativeHeightData, MapsPath + "Outputs/relativeHeight_RFloat.tif", x, y);
        //RasterReader.WriteTIFF(slopeData, MapsPath + "Outputs/slope_RFloat.tif", x, y);
        //RasterReader.WriteTIFF(waterSpreadData, MapsPath + "Outputs/waterSpread_RFloat.tif", x, y);
        //RasterReader.WriteTIFF(humidityData, MapsPath + "Outputs/humidity_RFloat.tif", x, y);

    }

    //private void OnDestroy()
    //{
    //    ReleaseTextures();
    //}
}
