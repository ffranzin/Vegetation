using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreePool : MonoBehaviour
{

    public GameObject[] _treePool;
    public static List<Tree> treePool;

    public static int size
    {
        get
        {
            return treePool == null ? 0 : treePool.Count;
        }
    }



    private void Awake()
    {
        treePool = new List<Tree>();

        for (int i = 0; i < _treePool.Length; i++)
        {
            GameObject go = Instantiate(_treePool[i]);

            Tree t = go.GetComponent<Tree>();

            if (t == null) Debug.LogError("Missing Component.");

            t.myIndexInTreePool = i;

            treePool.Add(t);
        }
    }
    
}
