using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneHeatEquation : MonoBehaviour
{
    //How many points will be on the x and y axis of the object
    [SerializeField] private int pointAmtX, pointAmtY;
    //The lwh dimensions of each of the cubes representing points
    [SerializeField] private float pointSize;
    //Prefab instantiated onto object for heat values
    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private double thermalDiffusivity;
    [SerializeField][Range(0,1)] private double stepTime = 0.01;
    [SerializeField][Min(0)] float maxSimTime;

    private double eTime = 0;

    private Collider plane;
    private GameObject[,] pointArr;
    private float stepSizeX, stepSizeY;
    private bool isUpdated = true;


    void Awake()
    {
        pointArr = new GameObject[pointAmtX, pointAmtY];    
        Vector3 pointDimensions = new Vector3(pointSize, pointSize, pointSize);
        //Set collider value to the plane's collider
        plane = GetComponent<Collider>();
        Vector3 planeSize = plane.bounds.size;
        //Use collider size to establish origin in the top left corner of the plane
        Vector3 originPosition = new Vector3(transform.position.x - planeSize.x / 2, transform.position.y, transform.position.z + planeSize.z / 2);
        stepSizeX = (planeSize.x / (pointAmtX - 1));
        stepSizeY = (planeSize.z / (pointAmtY - 1));
        //On start of program, create points on cube where temperature is being measured
        for (int i = 0; i < pointAmtX; i++)
        {
            for (int j = 0; j < pointAmtY; j++)
            {
                Vector3 pointPosition = new Vector3(originPosition.x + stepSizeX * i, transform.position.y, originPosition.z - stepSizeY * j);
                GameObject point = Instantiate(pointPrefab, pointPosition, Quaternion.identity, transform);
                point.GetComponent<PointData>().temperature = 4;
                point.transform.localScale = new Vector3(stepSizeX, stepSizeX, stepSizeX);
                pointArr[i,j] = point;
            }
        }
        ArrayList temps = new ArrayList();
        pointArr[5,5].GetComponent<PointData>().temperature = 10;
        printTemps();
    }

    void Update()
    {
        updateTemps();
    }

    private double getTemp(GameObject point){
        return point.GetComponent<PointData>().temperature;
    }
    private void setTemp(GameObject point, float temp){
        point.GetComponent<PointData>().temperature = temp;
    }
    private GameObject getPoint(int i, int j){
        return pointArr[i,j];
    }

    private void updateTemps(){
        if(isUpdated){
            isUpdated = false;
            for(int i = 0; i<pointArr.GetLength(0); i++){
                for(int j = 0; j<pointArr.GetLength(1); j++){
                    GameObject point = pointArr[i,j];
                    double uxx = 0;
                    if(i==0){
                        uxx = (getTemp(getPoint(i,j)) -2*getTemp(getPoint(i+1,j)) + getTemp(getPoint(i+2, j)))/Mathf.Pow(stepSizeX, 2);
                    } else if(i==pointArr.GetLength(0)-1){
                        uxx = (getTemp(getPoint(i-2,j)) - 2*getTemp(getPoint(i-1, j)) + getTemp(getPoint(i, j)))/Mathf.Pow(stepSizeX, 2);
                    } else {
                        uxx = (getTemp(getPoint(i-1,j)) - 2*getTemp(getPoint(i, j)) + getTemp(getPoint(i+1, j)))/Mathf.Pow(stepSizeX, 2);
                    }
                    double uyy = 0;
                    if(j==0){
                        uyy = (getTemp(getPoint(i,j)) -2*getTemp(getPoint(i,j+1)) + getTemp(getPoint(i, j+2)))/Mathf.Pow(stepSizeY, 2);
                    } else if(j==pointArr.GetLength(1)-1){
                        uyy = (getTemp(getPoint(i,j-2)) - 2*getTemp(getPoint(i, j-1)) + getTemp(getPoint(i, j)))/Mathf.Pow(stepSizeY, 2);
                    } else {
                        uyy = (getTemp(getPoint(i,j-1)) - 2*getTemp(getPoint(i, j)) + getTemp(getPoint(i, j+1)))/Mathf.Pow(stepSizeY, 2);
                    }
                    getPoint(i,j).GetComponent<PointData>().newTemp = getTemp(getPoint(i,j)) + thermalDiffusivity * (uxx + uyy) * stepTime;
                }
            }
            eTime = System.Math.Round(eTime + stepTime, 10);
            foreach(GameObject point in pointArr){
                point.GetComponent<PointData>().updateTemp();
                point.GetComponent<PointData>().setColor();
            }
            printTemps();
            isUpdated = true;
        }
    }
    private void printTemps(){
        if(eTime %01f == 0){
            string printString = "";
            for(int i = 0; i<pointArr.GetLength(0); i++){
                for(int j = 0; j<pointArr.GetLength(1); j++){
                    printString = printString + (string.Format("{0} ", pointArr[i, j].GetComponent<PointData>().temperature));
                }
                printString += ("\n");
            }
            Debug.Log("t = " + eTime + "\n" + printString);
        }
    }
}
