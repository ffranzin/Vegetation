
using UnityEngine;


public class Tree : MonoBehaviour
{

    const float TERRAIN_MIN_HEIGHT = 0;
    const float TERRAIN_MAX_HEIGHT = 100;

    const float WORLD_MIN_TEMPERATURE = -20;
    const float WORLD_MAX_TEMPERATURE = 50;


    public float TreeGlobalHeightOccuranceProbability(float worldHeight)
    {
        worldHeight = Utils.Remap(worldHeight, TERRAIN_MIN_HEIGHT, TERRAIN_MAX_HEIGHT, 0f, 1f);
        return Mathf.Clamp01(globalHeightCurve.Evaluate(worldHeight));
    }


    public float TreeMoistureOccuranceProbability(float worldMoisture)
    {
        return Mathf.Clamp01(moistureCurve.Evaluate(worldMoisture));
    }


    public float TreeInclinationOccuranceProbability(float worldInclination)
    {
        return Mathf.Clamp01(inclinationCurve.Evaluate(worldInclination));
    }


    public float TreeTemperatureOccuranceProbability(float worldTemperature)
    {
        worldTemperature = temperatureCurve.Evaluate(worldTemperature);
        return Utils.Remap(worldTemperature, WORLD_MIN_TEMPERATURE, WORLD_MAX_TEMPERATURE, 0, 1);
    }

    public AnimationCurve globalHeightCurve;
    public AnimationCurve localHeightCurve;
    public AnimationCurve temperatureCurve;
    public AnimationCurve moistureCurve;
    public AnimationCurve inclinationCurve;
    public AnimationCurve slopeCurve;

    public GameObject[] models;

}
