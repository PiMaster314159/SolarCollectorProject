using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class HeatDispersion2D : MonoBehaviour
{
    //Control variables
    [SerializeField] int pointAmtX, pointAmtY; //Amount of points instantiated onto plane in the x and y directions
    [SerializeField] double thermalConductivity, density, specificHeatCapacity; //Object internal properties
    [SerializeField] GameObject pointPrefab;
    [SerializeField] double startTemp;
    [SerializeField][VectorLabels("X", "Y", "Temp")] List<Vector3> startingPoints = new List<Vector3>(); //Points on plane given some initial temperature
    [SerializeField] double timeStep; //How often should temps be updated
    [SerializeField] double maxTime; //How long simulation runs for
    [SerializeField] double printTimeStep; //How often should temp data be recorded

    //Other
    double elapsedTime = 0; //Current simulation run time
    double printTime = 0; //At what time is data being recorded for?
    double stepSizeX, stepSizeY; //How far are points apart
    GameObject[,] points;//Create array of points in plane. This will be used later for simulation animation 
    double[,] temps; //Create array of temperatures assigned for points 
    double thermalDiffusivity;
    List<double[,]> tempList = new List<double[,]>(); //ArrayList of array of point temperatures at certain time 
    bool isUpdating = false, simulationComplete = false;//Check to see if temperatures are currently updating

    //When simulation start, do this
    void Awake(){
        Debug.Log("go");
        Vector3 planeSize = this.GetComponent<MeshCollider>().bounds.size; //get size of plane
        Vector3 origin = new Vector3(transform.position.x - planeSize.x / 2, transform.position.y, transform.position.z + planeSize.z / 2);//Shift point array origin to top left of plane
        stepSizeX = (planeSize.x / (pointAmtX - 1));//Set distance away from one another that points are going to be placed
        stepSizeY = (planeSize.z / (pointAmtY - 1));
        points = new GameObject[pointAmtX, pointAmtY];//Set point array dimensions to amount of points on plane
        temps = new double[pointAmtX, pointAmtY];//Set temp array to same dimensions
        thermalDiffusivity = thermalConductivity/(density*specificHeatCapacity);//Set thermal diffusivity based on object properties
        for (int i = 0; i < pointAmtX; i++)//Instantiate points onto plane
        {
            for (int j = 0; j < pointAmtY; j++)
            {
                Vector3 pointPosition = new Vector3(origin.x + (float)stepSizeX * i, transform.position.y, origin.z - (float)stepSizeY * j);//point position
                GameObject point = Instantiate(pointPrefab, pointPosition, Quaternion.identity, transform);//Place point on plane
                point.transform.localScale = new Vector3((float)stepSizeX, (float)stepSizeX, (float)stepSizeX);//Set point size so that they touch one another
                points[i,j] = point;//Add point to point array
                temps[i,j] = startTemp;//set temp to initial temperature of plane
            }
        }
        foreach(Vector3 hotPoint in startingPoints){ //For every point that is heated, set its initial temperature to desired
            temps[(int)hotPoint.x, (int)hotPoint.y] = hotPoint.z;
        }
        double[,] initTemps = new double[temps.GetLength(0), temps.GetLength(1)];
        for(int i = 0; i<temps.GetLength(0); i++){
            for(int j = 0; j<temps.GetLength(1); j++){
                initTemps[i,j] = temps[i,j];
            }
        }
        tempList.Add(initTemps);
    }

    //At every frame, do this
    void Update(){
        if(!simulationComplete){
            if(elapsedTime < maxTime){
                if(!isUpdating){
                    updateTemps();
                }
            } else {
                printTempData();
            }
        }
    }

    void updateTemps(){
        isUpdating = true;
        double[,] newTemps = new double[temps.GetLength(0), temps.GetLength(1)];//create new temp array to get new temperatures of all points
        //loop through all temperatures/points in array list
        for(int i = 0; i<temps.GetLength(0); i++){
            for(int j = 0; j<temps.GetLength(1); j++){
                //if on boundry, use heat transfer equation. If not, use general heat equation
                double uxx = 0;
                if(i==0){
                    //heat transfer equation
                    double q = ((thermalConductivity * Mathf.Pow((float)stepSizeX, 2)) * ((float)(temps[i+1,j] - temps[i,j])))/stepSizeX;
                    uxx = q/(density * Mathf.Pow((float)stepSizeX, 3) * specificHeatCapacity * stepSizeX);
                } else if(i==temps.GetLength(0)-1){
                    double q = ((thermalConductivity * Mathf.Pow((float)stepSizeX, 2)) * ((float)(temps[i-1,j] - temps[i,j])))/stepSizeX;
                    uxx = q/(density * Mathf.Pow((float)stepSizeX, 3) * specificHeatCapacity * stepSizeX);
                } else {
                    //heat equatiton
                    uxx = thermalDiffusivity * (temps[i-1,j] - 2*temps[i,j] + temps[i+1,j])/Mathf.Pow((float)stepSizeX, 2);
                }
                double uyy = 0;
                if(j==0){
                    double q = ((thermalConductivity * Mathf.Pow((float)stepSizeY, 2)) * ((float)(temps[i,j+1] - temps[i,j])))/stepSizeY;
                    uyy = q/(density * Mathf.Pow((float)stepSizeY, 3) * specificHeatCapacity * stepSizeY);
                } else if(j==temps.GetLength(1)-1){
                    double q = ((thermalConductivity * Mathf.Pow((float)stepSizeY, 2)) * ((float)(temps[i,j-1] - temps[i,j])))/stepSizeY;
                    uyy = q/(density * Mathf.Pow((float)stepSizeY, 3) * specificHeatCapacity * stepSizeY);
                } else {
                    uyy = thermalDiffusivity * (temps[i,j-1] - 2*temps[i,j] + temps[i,j+1])/Mathf.Pow((float)stepSizeY, 2);
                }

                //Update temperature of object
                newTemps[i,j] = temps[i,j] + (uxx + uyy) * timeStep;
            }
        }
        tempList.Add(newTemps);//Add temps to list 
        //Update the current temperatues of the points
        for(int i = 0; i<temps.GetLength(0); i++){
            for(int j = 0; j<temps.GetLength(1); j++){
                temps[i,j] = newTemps[i,j];
            }
        }
        elapsedTime = System.Math.Round(elapsedTime + timeStep, 10);//Update simulation time
        if(elapsedTime*10 % 1 == 0){
            Debug.Log(elapsedTime);
        }

        isUpdating = false;//Allow for temps to be upated again
    }

    void printTempData(){
        Debug.Log("yes!");
        Debug.Log(tempList.Count);
        for(int i = 0; i<tempList.Count; i++){//If list time is on the print step, print
            if(System.Math.Round((i * timeStep)/printTimeStep, 10) % 1 == 0){
                WriteString("t= "+ (i*timeStep) + "\n" + tempArrToString(tempList[i]));
            }
        }
        printOthers();
        simulationComplete = true;
    }

    string tempArrToString(double[,] tempArr){//Create array in formation capable of being read by numpy
        string outStr = "[";
        for(int i = 0; i<tempArr.GetLength(0); i++){
            outStr+="[";
            for(int j=0; j<tempArr.GetLength(1); j++){
                outStr+=tempArr[i,j];
                if(j<tempArr.GetLength(1)-1)
                    outStr+=", ";
            }
            outStr+="]";
            if(i<tempArr.GetLength(0)-1)
                outStr+=", ";
        }
        outStr+="]";
        return outStr;
    }
    
    static void WriteString(string arr){
        string path = "Assets/heatDispersion2DPointData.txt";
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(arr);
        writer.Close();

        AssetDatabase.ImportAsset(path); 
        TextAsset asset = (TextAsset)Resources.Load("heatDispersion2DPointData");
    }

    void printOthers(){
        double[,] finalTemps = tempList[tempList.Count -1];
        double min = finalTemps[0,0], max = finalTemps[0,0];
        foreach(double temp in finalTemps){
            if(temp < min){
                min = temp;
            } else if(temp > max){
                max = temp;
            }
        }
        Debug.Log("range: " + (max-min));

        double total = 0;
        foreach(double temp in finalTemps)
            total+=temp;
        double mean = (total)/(finalTemps.GetLength(0) * temps.GetLength(1));
        Debug.Log("mean: " + mean);

        double totalMeanDiv = 0;
        foreach(double temp in finalTemps)
            totalMeanDiv += Mathf.Abs((float)(mean-temp));
        double meanDiv = totalMeanDiv/(finalTemps.GetLength(0) * temps.GetLength(1));
        Debug.Log("mean div: " + meanDiv);
    }
}
