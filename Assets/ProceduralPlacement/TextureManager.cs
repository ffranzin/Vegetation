using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

//[ExecuteInEditMode]
public class TextureManager : MonoBehaviour
{
    public string heighmapPath; // = AtlasPath + "heightmap_Rfloat.tif";
    public string watermapPath; // = AtlasPath + "watermap_Rfloat.tif";
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

    public string AtlasPath { get { return /*Application.dataPath +*/ "Assets/Resources/Maps/"; } }

    public int Width { get { return m_heightmapInfo.rasterSizeX; } }
    public int Height { get { return m_heightmapInfo.rasterSizeY; } }
    public int AtlasResolution { get { return Width * Height; } }
    public int SizeInBytes { get { return AtlasResolution * sizeof(float); } }
    public bool IsHeightDataLoaded { get { return HeightData != null && HeightData.Length > 0 && HeightData.Length == AtlasResolution; } }
    public bool IsWaterDataLoaded { get { return WaterData != null && WaterData.Length > 0 && WaterData.Length == AtlasResolution; } }
    public Vector2Int AtlasDimensions { get { return new Vector2Int(Width, Height); } }
    public Vector2Int SplatDimensions { get; private set; }

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
        heighmapPath = string.IsNullOrEmpty(heighmapPath) ? (AtlasPath + "heightmap_Rfloat.tif") : heighmapPath;

        HeightData = RasterReader.ReadTIFF(heighmapPath, out m_heightmapInfo);

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
        watermapPath = string.IsNullOrEmpty(watermapPath) ? (AtlasPath + "watermap_Rfloat.tif") : watermapPath;

        WaterData = RasterReader.ReadTIFF(watermapPath, out m_watermapInfo);

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
        //HeightData = null;
        //WaterData = null;

        //m_heightmapInfo = default(RasterReader.RasterInfo);
        //m_watermapInfo = default(RasterReader.RasterInfo);

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

    public void InitTextures(Vector2Int atlasMapsSize, Vector2Int splatsMapsSize)
    {
        //ReleaseTextures();

        SplatDimensions = splatsMapsSize;

        RenderTextureDescriptor mainDescriptor = new RenderTextureDescriptor()
        {
            width = atlasMapsSize.x,
            height = atlasMapsSize.y,
            colorFormat = texFormat,
            depthBufferBits = 0,
            volumeDepth = 1,
            msaaSamples = 1,
            enableRandomWrite = true,
            autoGenerateMips = false,
            useMipMap = false,
            dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
        };

        RenderTextureDescriptor auxDescriptor = new RenderTextureDescriptor()
        {
            width = splatsMapsSize.x,
            height = splatsMapsSize.y,
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

        //m_heightMapTex.antiAliasing = 0;
        //m_waterMapTex.antiAliasing = 0;
        //m_waterSpreadTex.antiAliasing = 0;
        //m_meanHeightTex.antiAliasing = 0;
        //m_relativeHeightTex.antiAliasing = 0;
        //m_slopeTex.antiAliasing = 0;
        //m_moistureTex.antiAliasing = 0;
        //m_vPassTex.antiAliasing = 0;
        //m_hPassTex.antiAliasing = 0;

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
            Texture tempHeightTex = MakeTexture(HeightData, atlasMapsSize);
            Graphics.Blit(tempHeightTex, m_heightMapTex);
            Debug.Log("Height data loaded to the GPU.");
        }
        else
            Debug.LogError("Height data not loaded to the GPU.");

        if (IsWaterDataLoaded)
        {
            Texture tempWaterTex = MakeTexture(WaterData, atlasMapsSize);
            Graphics.Blit(tempWaterTex, m_waterMapTex);
            Debug.Log("Water data loaded to the GPU.");
        }
        else
            Debug.LogError("Water data not loaded to the GPU.");

        if (heightmapRawImg != null) heightmapRawImg.texture = m_heightMapTex;
        if (meanHeightRawImg != null) meanHeightRawImg.texture = m_meanHeightTex;
        if (relativeHeightRawImg != null) relativeHeightRawImg.texture = m_relativeHeightTex;
        if (slopeRawImg != null) slopeRawImg.texture = m_slopeTex;
        if (watermapRawImg != null) watermapRawImg.texture = m_waterMapTex;
        if (waterSpreadRawImg != null) waterSpreadRawImg.texture = m_waterSpreadTex;
        if (moistureRawImg != null) moistureRawImg.texture = m_moistureTex;

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

    /// <summary>
    /// Saves all the textures to files (except de HeightMap and WaterMap).
    /// </summary>
    public void SaveAllTextures(string sufix)
    {
        int x = SplatDimensions.x;
        int y = SplatDimensions.y;

        RenderTexture tmpRT = new RenderTexture(x, y, 1, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        tmpRT.Create();

        Texture2D tmp2DTex = new Texture2D(x, y, TextureFormat.RGBAFloat, false, true);

        RenderTexture currentActiveRT = RenderTexture.active;

        RenderTexture.active = tmpRT;

        if (m_meanHeightTex != null)
        {
            SaveRenderTexture(m_meanHeightTex, AtlasPath + "Outputs/meanHeight_RFloat" + sufix + ".tif", tmpRT, tmp2DTex);
        }
        if (m_relativeHeightTex != null)
        {
            SaveRenderTexture(m_relativeHeightTex, AtlasPath + "Outputs/relativeHeight_RFloat" + sufix + ".tif", tmpRT, tmp2DTex);
        }
        if (m_slopeTex != null)
        {
            SaveRenderTexture(m_slopeTex, AtlasPath + "Outputs/slope_RFloat" + sufix + ".tif", tmpRT, tmp2DTex);
        }
        if (m_waterSpreadTex != null)
        {
            SaveRenderTexture(m_waterSpreadTex, AtlasPath + "Outputs/waterSpread_RFloat" + sufix + ".tif", tmpRT, tmp2DTex);
        }
        if (m_moistureTex != null)
        {
            SaveRenderTexture(m_moistureTex, AtlasPath + "Outputs/moisture_RFloat" + sufix + ".tif", tmpRT, tmp2DTex);
        }

        RenderTexture.active = currentActiveRT;

        tmpRT.Release();
    }

    float[] Byte2FloatArray(byte[] bytes, int size)
    {
        float[] results = new float[size];
        for (int i = 0; i < results.Length; i++)
        {
            //int byteIndex = i * 4;
            byte[] localBytes = new byte[] { bytes[i], bytes[i + 1], bytes[i + 2], bytes[i + 3] }; // converts 4 bytes to a float
            results[i] = System.BitConverter.ToSingle(localBytes, 0);
            
        }
        return results;
    }



    /// <summary>
    /// Extracts a single channel from RGBAFloat texture.
    /// </summary>
    /// <param name="tex"></param>
    /// <param name="channel"> 0 1 2 3 <=> R G B A</param>
    /// <returns></returns>
    private float[] GetChannel(Texture2D tex, int channel)
    {
        if (tex.format != TextureFormat.RGBAFloat)
        {
            Debug.LogError("[GetChannel] TextureFormat must be RGBAFloat.");
            return null;
        }

        float[] data = Byte2FloatArray(tex.GetRawTextureData(), tex.width * tex.height);
        float[] singleChannel = new float[data.Length / 4];



        for (int i = channel; i < data.Length; i += 4)
        {
            singleChannel[i / 4] = data[i];
        }

        Array.Reverse(singleChannel);

        for (int i = 0; i < tex.height; i++)
        {
            Array.Reverse(singleChannel, i * tex.width, tex.width);
        }

        return singleChannel;
    }
    
    private void SaveRenderTexture(RenderTexture RT, string outputPath, RenderTexture tmpRT, Texture2D tmp2DTex)
    {
        int x = RT.width;
        int y = RT.height;

        Graphics.Blit(RT, tmpRT);

        tmp2DTex.ReadPixels(new Rect(0, 0, x, y), 0, 0);
        tmp2DTex.Apply();

        RasterReader.WriteTIFF(GetChannel(tmp2DTex, 0), outputPath, x, y);
    }
}
