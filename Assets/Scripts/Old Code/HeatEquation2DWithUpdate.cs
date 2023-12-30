using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatEquation2DWithUpdate : MonoBehaviour
{

    [SerializeField] int pointAmtX, pointAmtY;
    [SerializeField] double thermalDiffusivity, density, specificHeatCapacity;
    [SerializeField] GameObject pointPrefab;
    [SerializeField][VectorLabels("X", "Y", "Temp")] List<Vector3> startingPoints = new List<Vector3>();
    [SerializeField] double timeStep;
    [SerializeField] double maxTime;
    [SerializeField] double printStep;
        [SerializeField] GameObject heatSource;
    [SerializeField] double intensity;
    [SerializeField] double beamRadius;
    double elapsedTime = 0, printTime = 0;

    double stepSizeX, stepSizeY;

    GameObject[,] points;//Create array of points in plane. This will be used later for simulation animation 
    double[,] temps; //Create array of temperatures assigned for points 
    bool[,] isHeated;
    List<double[,]> tempList = new List<double[,]>(); //ArrayList of array of point temperatures at certain time 
    List<bool[,]> heatList = new List<bool[,]>();


    bool heatedUpdated = true, tempsUpdated = false, animate = false;//See if all of temps updated before proceeding
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
    }

    void FixedUpdate(){
        if(elapsedTime <= maxTime && heatedUpdated){
            updateHeat();
        } else if(heatedUpdated){
            Debug.Log("start write");
            printHeatList();
        } else if(elapsedTime <= maxTime && tempsUpdated){
            updateTemps();
        } else if(tempsUpdated){
            printTempList();    
        }//else if(animate){
        //     Debug.Log("yes");
        //     animate = false;   
        // }
    }

    void updateHeat(){
        heatedUpdated = false;
        bool[,] newIsHeated = new bool[isHeated.GetLength(0), isHeated.GetLength(1)];
        RaycastHit[] hitPoints = Physics.SphereCastAll(heatSource.transform.position, 3, (transform.position-heatSource.transform.position).normalized, Mathf.Infinity, LayerMask.GetMask("Points"));
        //Debug.Log(hitPoints.GetLength(0));
        //Debug.Log(hitPoints.GetLength(0));
        for(int i = 0; i<points.GetLength(0); i++){
            for(int j = 0; j<points.GetLength(1); j++){
                foreach(RaycastHit hit in hitPoints){
                    if(hit.transform.gameObject == points[i,j]){
                        newIsHeated[i,j] = true;
                    }
                }
            }
        }
        elapsedTime = System.Math.Round(elapsedTime + timeStep, 10);
        //Debug.Log(elapsedTime);
        for(int i = 0; i<isHeated.GetLength(0); i++){
            for(int j = 0; j<isHeated.GetLength(1); j++){
                isHeated[i,j] = newIsHeated[i,j];
            }
        }
        heatList.Add(newIsHeated);
        heatedUpdated = true;
    }

    void updateTemps(){
        tempsUpdated = false;
        double[,] newTemps = new double[temps.GetLength(0), temps.GetLength(1)];
        for(int i = 0; i<temps.GetLength(0); i++){
            for(int j = 0; j<temps.GetLength(1); j++){
                double uxx = 0;
                if(i==0){
                    uxx = (temps[i,j] -2*temps[i+1,j] + temps[i+2, j])/Mathf.Pow((float)stepSizeX, 2);
                } else if(i==temps.GetLength(0)-1){
                    uxx = (temps[i-2,j] - 2*temps[i-1,j] + temps[i,j])/Mathf.Pow((float)stepSizeX, 2);
                } else {
                    uxx = (temps[i-1,j] - 2*temps[i,j] + temps[i+1,j])/Mathf.Pow((float)stepSizeX, 2);
                }
                double uyy = 0;
                if(j==0){
                    uyy = (temps[i,j] -2*temps[i,j+1] + temps[i,j+2])/Mathf.Pow((float)stepSizeY, 2);
                } else if(j==temps.GetLength(1)-1){
                    uyy = (temps[i,j-2] - 2*temps[i,j-1] + temps[i,j])/Mathf.Pow((float)stepSizeY, 2);
                } else {
                    uyy = (temps[i,j-1] - 2*temps[i,j] + temps[i,j+1])/Mathf.Pow((float)stepSizeY, 2);
                }
                newTemps[i,j] = temps[i,j] + thermalDiffusivity * (uxx + uyy) * timeStep;
                if(heatList[(int)(elapsedTime/timeStep)][i,j]){
                    //Debug.Log("yes");
                    newTemps[i,j] += ((intensity*6*Mathf.Pow((float)stepSizeX, 2)) * timeStep)/(density * Mathf.Pow((float)stepSizeX, 3) * specificHeatCapacity);
                }
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
        for(int i = 0; i<temps.GetLength(0); i++){
            for(int j = 0; j<temps.GetLength(1); j++){
                temps[i,j] = newTemps[i,j];
            }
        }
        //Debug.Log(elapsedTime);
        tempList.Add(newTemps);
        // Debug.Log(printTemps(temps));
        tempsUpdated = true;
    }
    void printHeatList(){
        double ratio = System.Math.Round(printStep/timeStep, 5);
        heatedUpdated = false;
        for(int i = 0; i<heatList.Count; i++){
            if(i % (int) ratio == 0){
                Debug.Log("t = " + printTime + "\n" + printBools(heatList[i]));
            }
            printTime = System.Math.Round(printTime + timeStep, 10);
        }
        tempsUpdated = true;
        elapsedTime = 0;
        printTime = 0;
        Debug.Log("Done");
        Debug.Log("Start translation");
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
        string printString = "[";
        for(int i = 0; i<tempArr.GetLength(0); i++){
            for(int j = 0; j<tempArr.GetLength(1); j++){
                printString = printString + (" " + tempArr[i, j]);
            }
            printString += ("\n");
        }
        return printString;
    }
    private string printBools(bool[,] heatArr){
                string printString = "";
        for(int i = 0; i<heatArr.GetLength(0); i++){
            for(int j = 0; j<heatArr.GetLength(1); j++){
                printString = printString + (" " + heatArr[i, j]);
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
