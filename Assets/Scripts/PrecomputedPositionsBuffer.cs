using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrecomputedPositionsBuffer : MonoBehaviour {

    static ComputeBuffer precomputedTileBufferL1;
    static ComputeBuffer precomputedTileBufferL2;
    static ComputeBuffer precomputedTileBufferL3;

    const int nPos = 100;

    static List<Vector2> positions = new List<Vector2>();

    static void GeneratePos(float minDist)
    {
        int noLoop = nPos * 5;

        minDist = minDist / 128f;

        while(positions.Count < nPos && noLoop > 0)
        {
            Vector2 pos = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
            
            noLoop--;

            if (positions.Exists(p => (p - pos).magnitude < minDist)) continue;

            positions.Add(pos);
        }
    }




    void Start () {

        positions.Clear();
        GeneratePos(10);
        precomputedTileBufferL1 = new ComputeBuffer(positions.Count, 8);
        precomputedTileBufferL1.SetData(positions);

        positions.Clear();
        GeneratePos(6);
        precomputedTileBufferL2 = new ComputeBuffer(positions.Count, 8);
        precomputedTileBufferL2.SetData(positions);
        
        positions.Clear();
        GeneratePos(3);
        precomputedTileBufferL3 = new ComputeBuffer(positions.Count, 8);
        precomputedTileBufferL3.SetData(positions);

        positions.Clear();

        Shader.SetGlobalBuffer("_GlobalPrecomputedPositionL1", precomputedTileBufferL1);
        Shader.SetGlobalBuffer("_GlobalPrecomputedPositionL2", precomputedTileBufferL2);
        Shader.SetGlobalBuffer("_GlobalPrecomputedPositionL3", precomputedTileBufferL3);
	}
	
}
