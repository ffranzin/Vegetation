using UnityEngine;


public class TerrainManager : MonoBehaviour {

    static int SIZE = (int)Mathf.Pow(2f, 15f);

    public static readonly Vector3 TERRAIN_ORIGIN   = new Vector3(0, 0, 0);
    public static readonly Vector3 TERRAIN_END      = new Vector3(SIZE, 0, SIZE);
    
    public static double PIXEL_HEIGHT;
    public static double PIXEL_WIDTH;
    public static float TERRAIN_HEIGHT_MULTIPLIER = 80;
    public static float TERRAIN_HEIGHT_LAKE = -1;

    Material terrain;
    public Texture2D heightMap;
    static RasterReader.RasterInfo heightmapRaster; 
    
    static float[] m_heightMapGDAL;

    public static Texture2D m_heightMap;
    public static RenderTexture m_heightMapRT;
    private void Awake()
    {
        m_heightMap = heightMap;
        m_heightMapRT = new RenderTexture(m_heightMap.width, m_heightMap.height, 1, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        m_heightMapRT.useMipMap = false;
        m_heightMapRT.enableRandomWrite = true;
        m_heightMapRT.Create();
        Graphics.Blit(m_heightMap, m_heightMapRT);

        //m_heightMapGDAL = RasterReader.ReadTIFF(@"Assets\Materials_Textures\heightmap_Rfloat.tif", out heightmapRaster);

        terrain = gameObject.GetComponent<MeshRenderer>().materials[0];

       // terrain.SetTexture("_HeightMap", heightMap);
        
        PIXEL_WIDTH = TERRAIN_END.z / heightmapRaster.rasterSizeX;
        PIXEL_HEIGHT = TERRAIN_END.z / heightmapRaster.rasterSizeY;

        terrain.SetFloat("_terrainSize", (float)SIZE);

        Shader.SetGlobalFloat("TERRAIN_HEIGHT_MULTIPLIER", TERRAIN_HEIGHT_MULTIPLIER);

        Shader.SetGlobalTexture("_HeightMap", heightMap);
    }
}

