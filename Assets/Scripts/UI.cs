using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour {


    public static bool UI_enableViewVegLevel1 = true;
    public static bool UI_enableViewVegLevel2 = true;
    public static bool UI_enableViewVegLevel3 = true;


    public static float UI_viewRangeVegL1;
    public static float UI_viewRangeVegL2;
    public static float UI_viewRangeVegL3;

    
    static Slider vr_l1;
    static Slider vr_l2;
    static Slider vr_l3;

    static Toggle en_l1;
    static Toggle en_l2;
    static Toggle en_l3;

    static Text FPS;
    static Text usedNodes;
    static Text camSpeed;

    static Text amountVegL1;
    static Text amountVegL2;
    static Text amountVegL3;

    static Text viewrangeTextL1;
    static Text viewrangeTextL2;
    static Text viewrangeTextL3;


    public static void UpdateAmount(int[] amount)
    {
        return;
        amountVegL1.text = "INSTANCES VEGETATION L1 : " + amount[0]; 
        amountVegL2.text = "INSTANCES VEGETATION L2 : " + amount[1]; 
        amountVegL3.text = "INSTANCES VEGETATION L3 : " + amount[2]; 
    }

    public Vector3 lastCamPos;
    public float lastDeltaTime;
    
    private void Start()
    {
        return;
        vr_l1 = GameObject.Find("SliderViewRangeL1").GetComponent<Slider>();
        vr_l2 = GameObject.Find("SliderViewRangeL2").GetComponent<Slider>();
        vr_l3 = GameObject.Find("SliderViewRangeL3").GetComponent<Slider>();

        FPS = GameObject.Find("FPS").GetComponent<Text>();

        camSpeed = GameObject.Find("CamSpeed").GetComponent<Text>();

        usedNodes = GameObject.Find("UsedNodes").GetComponent<Text>();

        amountVegL1 = GameObject.Find("AmountL1").GetComponent<Text>();
        amountVegL2 = GameObject.Find("AmountL2").GetComponent<Text>();
        amountVegL3 = GameObject.Find("AmountL3").GetComponent<Text>();

        viewrangeTextL1 = GameObject.Find("ViewRangeTextL1").GetComponent<Text>();
        viewrangeTextL2 = GameObject.Find("ViewRangeTextL2").GetComponent<Text>();
        viewrangeTextL3 = GameObject.Find("ViewRangeTextL3").GetComponent<Text>();


        //en_l1 = GameObject.Find("EnableL1").GetComponent<Toggle>();
        //en_l2 = GameObject.Find("EnableL2").GetComponent<Toggle>();
        //en_l3 = GameObject.Find("EnableL3").GetComponent<Toggle>();
        
    }


    

    void LateUpdate()
    {
        return;
        UI_viewRangeVegL1 = vr_l1.value;
        UI_viewRangeVegL2 = vr_l2.value;
        UI_viewRangeVegL3 = vr_l3.value;

        viewrangeTextL1.text = "VIEW RANGE L1 (" + UI_viewRangeVegL1 + "m)";
        viewrangeTextL2.text = "VIEW RANGE L1 (" + UI_viewRangeVegL2 + "m)";
        viewrangeTextL3.text = "VIEW RANGE L1 (" + UI_viewRangeVegL3 + "m)";


        usedNodes.text = "USED NODES : " + (NodePool.NODE_POOL_SIZE - NodePool.freeNodes);
        FPS.text = "FPS : " + 0;
        camSpeed.text = "CAMERA SPEED : " + 0;



        //UI_enableViewVegLevel1 = en_l1.isOn;
        //UI_enableViewVegLevel2 = en_l2.isOn;
        //UI_enableViewVegLevel3 = en_l3.isOn;
    }
    
}
