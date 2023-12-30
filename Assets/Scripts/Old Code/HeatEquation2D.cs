using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatEquation2D : MonoBehaviour
{
[SerializeField] int pointAmtX, pointAmtY;
[SerializeField]enum objects{ice=0};
    [SerializeField] double thermalDiffusivity, thermalConductivity;
    [SerializeField] GameObject pointPrefab;
    [SerializeField][VectorLabels("X", "Y", "Temp")] List<Vector3> startingPoints = new List<Vector3>();
    [SerializeField] double timeStep;
    [SerializeField] double maxTime;
    [SerializeField] double printStep;

    double elapsedTime = 0, printTime = 0;

    double stepSizeX, stepSizeY;

    GameObject[,] points;//Create array of points in plane. This will be used later for simulation animation 
    double[,] temps; //Create array of temperatures assigned for points 
    bool[,] isHeated;
    List<double[,]> tempList = new List<double[,]>(); //ArrayList of array of point temperatures at certain time 
    List<bool[,]> heatList = new List<bool[,]>();


    bool heatedUpdated = true, tempsUpdated = true, animate = false;//See if all of temps updated before proceeding
    void Awake(){
        Vector3 planeSize = this.GetComponent<Collider>().bounds.size; //get size of plane
        Vector3 origin = new Vector3(transform.position.x - planeSize.x / 2, transform.position.y, transform.position.z + planeSize.z / 2);//Shift point array origin to top left of object
        stepSizeX = (planeSize.x / (pointAmtX - 1));//Set distance away from one another that points are going to be placed
        stepSizeY = (planeSize.z / (pointAmtY - 1));
        points = new GameObject[pointAmtX, pointAmtY];
        temps = new double[pointAmtX, pointAmtY];
        isHeated = new bool[pointAmtX, pointAmtY];
        for (int i = 0; i < pointAmtX; i++)//Instantiate points onto plane and assign to array position mirroring temperatures
        {
            for (int j = 0; j < pointAmtY; j++)
            {
                Vector3 pointPosition = new Vector3(origin.x + (float)stepSizeX * i, transform.position.y, origin.z - (float)stepSizeY * j);
                GameObject point = Instantiate(pointPrefab, pointPosition, Quaternion.identity, transform);
                point.GetComponent<PointData>().temperature = 4;
                point.transform.localScale = new Vector3((float)stepSizeX, (float)stepSizeX, (float)stepSizeX);
                points[i,j] = point;
                temps[i,j] = point.GetComponent<PointData>().temperature;
                isHeated[i,j] = false;
            }
        }
        foreach(Vector3 hotPoint in startingPoints){
            temps[(int)hotPoint.x, (int)hotPoint.y] = hotPoint.z;
        }
        Debug.Log("start");
    }

    void FixedUpdate(){
        if(elapsedTime <= maxTime && tempsUpdated){
            updateTemps();
        } else if(tempsUpdated){
            Debug.Log(tempList.Count);
            Debug.Log("done");
            printTempList();
        } else if(animate){
            Debug.Log("yes");
            animate = false;   
        }
    }

    void updateHeat(){

    }

    void updateTemps(){
        tempsUpdated = false;
        double[,] newTemps = new double[temps.GetLength(0), temps.GetLength(1)];
        for(int i = 0; i<temps.GetLength(0); i++){
                for(int j = 0; j<temps.GetLength(1); j++){
                    double uxx = 0;
                    if(i==0){
                        uxx = (double)((float)thermalConductivity * Mathf.Pow((float)stepSizeX/2, 2) * (float)(temps[i+1,j] - temps[i,j]))/stepSizeX;
                    } else if(i==temps.GetLength(0)-1){
                        uxx = (double)((float)thermalConductivity * Mathf.Pow((float)stepSizeX/2, 2) * (float)(temps[i-1,j] - temps[i,j]))/stepSizeX;
                    } else {
                        uxx = (temps[i-1,j] - 2*temps[i,j] + temps[i+1,j])/Mathf.Pow((float)stepSizeX, 2);
                    }
                    double uyy = 0;
                    if(j==0){
                        uyy = (double)((float)thermalConductivity * Mathf.Pow((float)stepSizeY/2, 2) * (float)(temps[i,j+1] - temps[i,j]))/stepSizeY;
                    } else if(j==temps.GetLength(1)-1){
                        uyy = (double)((float)thermalConductivity * Mathf.Pow((float)stepSizeY/2, 2) * (float)(temps[i,j-1] - temps[i,j]))/stepSizeY;
                    } else {
                        uyy = (temps[i,j-1] - 2*temps[i,j] + temps[i,j+1])/Mathf.Pow((float)stepSizeY, 2);
                    }
                    newTemps[i,j] = temps[i,j] + thermalDiffusivity * (uxx + uyy) * timeStep;
                    // // if(i==0){
                    // //     uxx = (-2*temps[i,j] + temps[i+1,j])/Mathf.Pow((float)stepSizeX, 2);
                    // // } else if(i==temps.GetLength(0)-1){
                    // //     uxx = (temps[i-1,j] - 2*temps[i,j])/Mathf.Pow((float)stepSizeX, 2);
                    // // } else {
                    // //     uxx = (temps[i-1,j] - 2*temps[i,j] + temps[i+1,j])/Mathf.Pow((float)stepSizeX, 2);
                    // // }
                    // // double uyy = 0;
                    // // if(j==0){
                    // //     uyy = (temps[i,j] -2*temps[i,j+1])/Mathf.Pow((float)stepSizeY, 2);
                    // // } else if(j==temps.GetLength(1)-1){
                    // //     uyy = (temps[i,j-1] - 2*temps[i,j])/Mathf.Pow((float)stepSizeY, 2);
                    // // } else {
                    // //     uyy = (temps[i,j-1] - 2*temps[i,j] + temps[i,j+1])/Mathf.Pow((float)stepSizeY, 2);
                    // // }
                    // // newTemps[i,j] = temps[i,j] + thermalDiffusivity * (uxx + uyy) * timeStep;
                }
            }
            elapsedTime = System.Math.Round(elapsedTime + timeStep, 10);
            Debug.Log(elapsedTime);
        for(int i = 0; i<temps.GetLength(0); i++){
            for(int j = 0; j<temps.GetLength(1); j++){
                temps[i,j] = newTemps[i,j];
            }
        }
        tempList.Add(newTemps);
        // Debug.Log(printTemps(temps));
        tempsUpdated = true;
    }
    void printTempList(){
        double ratio = System.Math.Round(printStep/timeStep, 5);
        tempsUpdated = false;
        for(int i = 0; i<tempList.Count; i++){
            if(i % (int) ratio == 0){
                Debug.Log("t = " + printTime + "\n" + printTemps(tempList[i]));
            }
            printTime = System.Math.Round(printTime + timeStep, 10);
        }
        animate = true;
    }
    private string printTemps(double[,] tempArr){
        string printString = "";
        for(int i = 0; i<tempArr.GetLength(0); i++){
            for(int j = 0; j<tempArr.GetLength(1); j++){
                printString = printString + (" " + tempArr[i, j]);
            }
            printString += ("\n");
        }
        return printString;
    }

    public void updatePoints(double[,] temps){
        for(int i = 0; i<temps.GetLength(0); i++){
            for(int j = 0; j<temps.GetLength(1); j++){
                points[i,j].GetComponent<PointData>().temperature = temps[i,j];
            }
        }
    }
    public void animatePoints(){
        foreach(GameObject point in points){
            point.GetComponent<PointData>().setColor();
        }
    }
}