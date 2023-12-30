using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointData : MonoBehaviour
{
    [SerializeField] private double maxTemp, minTemp;
    private MeshRenderer _meshRenderer;
    private Gradient gradient;
    GradientColorKey[] colorKey;
    GradientAlphaKey[] alphaKey;
    public double temperature;
    public double newTemp;
    public bool isPointIsHeated = false;


    void Awake(){
        _meshRenderer = GetComponent<MeshRenderer>();
        gradient = new Gradient();

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        colorKey = new GradientColorKey[2];
        colorKey[0].color = Color.blue;
        colorKey[0].time = 0.0f;
        colorKey[1].color = Color.red;
        colorKey[1].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 0.0f;
        alphaKey[1].time = 1.0f;

        gradient.SetKeys(colorKey, alphaKey);
        temperature = 0;
    }

    
    public void setColor(){
        double tempTemp = temperature;
        // if(tempTemp < minTemp)
        //     tempTemp = minTemp;
        // else if(tempTemp > maxTemp)
        //     tempTemp = maxTemp;
        
        Color pointColor = gradient.Evaluate((float)((tempTemp-minTemp)/(maxTemp-minTemp)));
        //Debug.Log(pointColor);
        _meshRenderer.material.color = pointColor;
    }

    public void updateTemp(){
        temperature = newTemp;
    }

    void onTriggerEnter(Collider other){
        Debug.Log("hit");
        isPointIsHeated = true;
    }
}
