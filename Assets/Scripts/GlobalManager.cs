
using UnityEngine;

public class GlobalManager : MonoBehaviour {

    public static Atlas m_atlas;

    GameObject tmpDebudAtlas;
    bool tmpIsScaled = false;

	void Start () {
        m_atlas = new Atlas(RenderTextureFormat.ARGBFloat, FilterMode.Bilinear, 1024, 128, true);

        if (m_atlas == null) Debug.LogError("Atlas cannot be generated.");

        tmpDebudAtlas = GameObject.Find("Plane");
        tmpDebudAtlas.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", m_atlas.texture);

    }



    void Update () {

        if(Input.GetKeyDown(KeyCode.P))
        {
            if(!tmpIsScaled)
            {
                tmpDebudAtlas.transform.localPosition = new Vector3(-19, 7, 20);
                tmpDebudAtlas.transform.localScale = new Vector3(.7f, 1, .7f);
            }
            else
            {
                tmpDebudAtlas.transform.localPosition = new Vector3(0, 0, 35);
                tmpDebudAtlas.transform.localScale = new Vector3(4, 1, 4);
            }

            tmpIsScaled = !tmpIsScaled;
        }
        // if (m_atlas.IsFull()) Debug.LogError("ATLAS FULL");
    }
}
