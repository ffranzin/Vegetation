
using System.Collections.Generic;
using UnityEngine;

public class PrecomputedPositionsBuffer : MonoBehaviour {
    
    public static void GeneratePos(List<Vector2> positions, int boundSize, float minDist, float border = 3, int nPos = 1000)
    {
        int noLoop = nPos * 5;

        minDist = minDist / boundSize;

        border = border / boundSize;

        while(positions.Count < nPos && noLoop > 0)
        {
            Vector2 pos = new Vector2(Random.Range(border, 1f- border), Random.Range(border, 1f - border));
            
            noLoop--;

            if (positions.Exists(p => (p - pos).magnitude < minDist)) continue;

            positions.Add(pos);
            
        }
        
        Debug.Log("POSITIONS GENERATED : " + positions.Count);
        //SpawnAll(positions, boundSize);
    }

    /// <summary>
    /// ///////////////////poisson
    /// </summary>

    static float r = 10;
    static int k = 30;
    static float w;
    static int size = 512;

    static Vector2[] grid;
    static List<Vector2> active = new List<Vector2>();
    public static List<Vector2> poissonPosition = new List<Vector2>();

    static int cols, rows;

    static void Step0()
    {
        w = r / Mathf.Sqrt(2f);

        cols = Mathf.FloorToInt(size / w);
        rows = Mathf.FloorToInt(size / w);

        grid = new Vector2[(int)((rows + 1) * (cols + 1))];

        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                grid[i + j * cols] = Vector2.one * Mathf.Infinity;
        Step1();
    }

    static void Step1()
    {
        float xRandom = Random.Range(0, (float)size);
        float yRandom = Random.Range(0, (float)size);

        int x = Mathf.FloorToInt(xRandom / w);
        int y = Mathf.FloorToInt(yRandom / w);

        Vector2 pos = new Vector2(xRandom, yRandom);

        grid[x + y * cols] = pos;
        active.Add(pos);
        Step2();
    }

    static void Step2()
    {
        if (active.Count == 0) return;

        Vector2 pos = active[0];

        for (int i = 0; i < k; i++)
        {
            Vector2 rand = pos + Random.insideUnitCircle * Random.Range(r, 2 * r);

            if (rand.x > size || rand.y > size || rand.x < 0 || rand.y < 0) continue;

            int rCol = Mathf.FloorToInt(rand.x / w);
            int rRow = Mathf.FloorToInt(rand.y / w);

            bool areOk = true;

            for (int m = -1; m <= 1; m++)
            {
                for (int n = -1; n <= 1; n++)
                {
                    int index = (rRow + m) + (rCol + n) * cols;
                    if (index < 0 || index >= grid.Length) continue;

                    Vector2 neighboor = grid[index];

                    if ((neighboor - rand).magnitude < r)
                    {
                        areOk = false;
                        break;
                    }
                }
            }
            if (areOk)
            {
                grid[rRow + rCol * cols] = rand;
                poissonPosition.Add(rand);
                active.Add(rand);
            }
        }

        active.RemoveAt(0);
        Step2();
    }


    public static List<Vector2> Poisson(int boundSize, float minDist, float border = 3, int nPos = 1500)
    {
        poissonPosition.Clear();

       // minDist = minDist / boundSize;

       // border = border / boundSize;
        
        Step0();
        Debug.Log("asdas" + poissonPosition.Count);
        return poissonPosition;
    }


        public static void SpawnAll(List<Vector2> positions, int boundSize)
    {
        foreach(Vector2 p in positions)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            Vector3 pos = new Vector3(p.x, 0, p.y) * boundSize;

            go.transform.position = pos;
        }

        Bounds b = new Bounds(new Vector3(1, 0, 1) * boundSize / 2, Vector3.one * boundSize);

        Utils.ShowBoundLines(b, Color.black, 10000);
        Debug.Log("POSITIONS GENERATED : " + positions.Count);
    }
}
