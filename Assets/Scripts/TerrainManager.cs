using UnityEngine;


public class TerrainManager : MonoBehaviour {

    static int TERRAIN_SIZE = (int)Mathf.Pow(2f, 10f);

    public static readonly Vector3 TERRAIN_ORIGIN   = new Vector3(0, 0, 0);
    public static readonly Vector3 TERRAIN_END      = new Vector3(TERRAIN_SIZE, 0, TERRAIN_SIZE);
    
    public static double PIXEL_HEIGHT;
    public static double PIXEL_WIDTH;
    public static float TERRAIN_HEIGHT_MULTIPLIER = 100;
    public static float TERRAIN_HEIGHT_LAKE = -1;

    Material terrain;
    public Texture2D heightMap;
    public Texture2D waterMap;

    public static Texture2D m_heightMap;
    public static Texture2D m_waterMap;
    
    MoistureDistribuition moisture;
    
    private void Awake()
    {
        m_heightMap = heightMap;
        m_waterMap = waterMap;

        terrain = gameObject.GetComponent<MeshRenderer>().materials[0];

        PIXEL_WIDTH = TERRAIN_END.z / m_heightMap.width;
        PIXEL_HEIGHT = TERRAIN_END.z / m_heightMap.height;

        Shader.SetGlobalFloat("_globalTerrainSize", (float)TERRAIN_SIZE);

        Shader.SetGlobalFloat("_globalPixelSize", (float)PIXEL_WIDTH);

        Shader.SetGlobalFloat("TERRAIN_HEIGHT_MULTIPLIER", TERRAIN_HEIGHT_MULTIPLIER);

        terrain.SetTexture("_waterMaptmp", waterMap);

        Shader.SetGlobalTexture("_heightMapAux", m_heightMap);

        moisture = GameObject.Find("Calculator").GetComponent<MoistureDistribuition>();
        tex = new Texture2D(1024, 1024, TextureFormat.RFloat, false);
    }

    public Texture2D tex;
    public void Update()
    {
       // Graphics.CopyTexture(moisture.TexManager.m_heightMapTex, tex);
        //terrain.SetTexture("_debugMap", tex);
        //terrain.SetTexture("_slopeMapDebug", tex);
    }
}

