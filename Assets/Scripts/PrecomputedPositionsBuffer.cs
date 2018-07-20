using System.Collections;
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
        
        //Debug.Log("POSITIONS GENERATED : " + positions.Count);
        //SpawnAll(positions, boundSize);
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
