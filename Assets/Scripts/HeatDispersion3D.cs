using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class HeatDispersion3D : MonoBehaviour
{
//Control variables
    [SerializeField] double pointDistance; //Amount of points instantiated onto plane in the x and y directions
    [SerializeField] double thermalConductivity, density, specificHeatCapacity; //Object internal properties
    [SerializeField] GameObject pointPrefab;
    [SerializeField] double startTemp;
    [SerializeField][VectorLabels("X", "Y", "Z", "Temp")] List<Vector4> startingPoints = new List<Vector4>(); //Points on plane given some initial temperature
    [SerializeField] double timeStep; //How often should temps be updated
    [SerializeField] double maxTime; //How long simulation runs for
    [SerializeField] double printStep; //How often should temp data be recorded

    //Other
    double elapsedTime = 0; //Current simulation run time
    double printTime = 0; //At what time is data being recorded for?
    GameObject[,,] points;//Create array of points in plane. This will be used later for simulation animation 
    double[,,] temps; //Create array of temperatures assigned for points 
    double thermalDiffusivity;
    List<double[,,]> tempList = new List<double[,,]>(); //ArrayList of array of point temperatures at certain time 
    bool isUpdating = false, simulationComplete = false;//Check to see if temperatures are currently updating

    //When simulation start, do this
    void Awake(){
        Debug.Log("go");

        int layer_mask = LayerMask.GetMask("Debris");//Get layer of space debris object
        Vector3 meshSize = GetComponent<Collider>().bounds.size; //Get size of cube surrounding mesh
        GetComponent<MeshRenderer>().enabled = false;
        Vector3 startingPoint = new Vector3(transform.position.x - meshSize.x/2, transform.position.y + meshSize.y/2, transform.position.z + meshSize.z/2); //point where instantiation begins
        Vector3 endingPoint = new Vector3(transform.position.x + meshSize.x/2, transform.position.y - meshSize.y/2, transform.position.z - meshSize.z/2); //point where instantiation ends
        
        thermalDiffusivity = thermalConductivity/(density*specificHeatCapacity);//Set thermal diffusivity based on object properties

        points = new GameObject[(int)Mathf.Abs((float)((endingPoint.x-startingPoint.x)/pointDistance))+1, 
                (int)Mathf.Abs((float)((endingPoint.y-startingPoint.y)/pointDistance))+1, 
                (int)Mathf.Abs((float)((endingPoint.z-startingPoint.z)/pointDistance))+1];//Initialize 3D arr of points pointDistance away from one another, and following temp arr of same size
        temps = new double[points.GetLength(0), points.GetLength(1), points.GetLength(2)];

        for(int i = 0; i < points.GetLength(0); i++){//Check if point is inside mesh at point. If yes, instantiate, set desired scale, and temperature to starting temp. If not, set point to null and temp to a placeholder value
            for(int j = 0; j<points.GetLength(1); j++){
                for(int k = 0; k<points.GetLength(2); k++){
                    Vector3 pos = new Vector3((float)(startingPoint.x + i*pointDistance), (float)(startingPoint.y - j*pointDistance), (float)(startingPoint.z - k*pointDistance));
                    Collider[] hitColliders = Physics.OverlapSphere(pos, 0,layer_mask);
                    if(hitColliders.Length >0){
                        GameObject point = Instantiate(pointPrefab, pos, Quaternion.identity, transform);
                        point.transform.localScale = new Vector3((float)(pointDistance/meshSize.x), (float)pointDistance/meshSize.y, (float)pointDistance/meshSize.y);
                        points[i,j,k] = point;
                        temps[i,j,k] = startTemp;
                    } else {
                        points[i,j,k] = null;
                        temps[i,j,k] = double.MinValue;
                    }
                }
            }
        }

        foreach(Vector4 hotPoint in startingPoints){ //For every point that is heated, set its initial temperature to desired
            temps[(int)hotPoint.x, (int)hotPoint.y, (int)hotPoint.z] = hotPoint.w;
        }
        double[,,] initTemps = new double[temps.GetLength(0), temps.GetLength(1), temps.GetLength(2)];
        for(int i = 0; i<temps.GetLength(0); i++){//Add initial temps to tempList
            for(int j = 0; j<temps.GetLength(1); j++){
                for(int k = 0; k<temps.GetLength(2); k++){
                    initTemps[i,j,k] = temps[i,j,k];
                }
            }
        }
        tempList.Add(initTemps);
        int count = 0;
        foreach(GameObject point in points){
            if(point != null)
                count++;
        }
        Debug.Log(count);
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

    async void updateTemps(){
        double[,,] newTemps = new double[temps.GetLength(0), temps.GetLength(1),temps.GetLength(2)]; //Create arr of new temperatures of all points (at end, will set actual point temps to this value)
        //Run through temp update cycle for all points
        for(int i = 0; i<temps.GetLength(0); i++){
                for(int j = 0; j<temps.GetLength(1); j++){
                    for(int k = 0; k<temps.GetLength(2); k++){
                        if(temps[i,j,k] != double.MinValue){//Update temperature if point is actually inside object
                            //Assess change in temp in x-direction
                            double uxx = 0;
                            if((i!= 0 && i!=temps.GetLength(0)-1) && (temps[i-1,j,k] == double.MinValue && temps[i+1,j,k] == double.MinValue)){
                                uxx = 0;
                            }else if(i==0 || temps[i-1,j,k]==double.MinValue){
                                double q = ((thermalConductivity * Mathf.Pow((float)pointDistance, 2)) * ((float)(temps[i+1,j,k] - temps[i,j,k])))/pointDistance;
                                uxx = q/(density * Mathf.Pow((float)pointDistance, 3) * specificHeatCapacity * pointDistance);
                            } else if(i==temps.GetLength(0)-1 || temps[i+1,j,k]==double.MinValue){
                                double q = ((thermalConductivity * Mathf.Pow((float)pointDistance, 2)) * ((float)(temps[i-1,j,k] - temps[i,j,k])))/pointDistance;
                                uxx = q/(density * Mathf.Pow((float)pointDistance, 3) * specificHeatCapacity * pointDistance);                           
                            } else if(temps[i-1,j,k] != double.MinValue && temps[i+1,j,k] != double.MinValue){
                                
                                uxx = thermalDiffusivity * (temps[i-1,j,k] - 2*temps[i,j,k] + temps[i+1,j,k])/Mathf.Pow((float)pointDistance, 2);
                            }

                            //Assess change in temp in y-direction
                            double uyy = 0;
                            if((j!= 0 && j!=temps.GetLength(1)-1) && (temps[i,j-1,k] == double.MinValue && temps[i,j+1,k] == double.MinValue)){
                                uyy=0;
                            } else if(j==0 || temps[i,j-1,k]==double.MinValue){
                                double q = ((thermalConductivity * Mathf.Pow((float)pointDistance, 2)) * ((float)(temps[i,j+1,k] - temps[i,j,k])))/pointDistance;
                                uyy = q/(density * Mathf.Pow((float)pointDistance, 3) * specificHeatCapacity * pointDistance); 
                            } else if(j==temps.GetLength(1)-1 || temps[i,j+1,k]==double.MinValue){
                                double q = ((thermalConductivity * Mathf.Pow((float)pointDistance, 2)) * ((float)(temps[i,j-1,k] - temps[i,j,k])))/pointDistance;
                                uyy = q/(density * Mathf.Pow((float)pointDistance, 3) * specificHeatCapacity * pointDistance); 
                            } else if(temps[i-1,j,k] != double.MinValue && temps[i+1,j,k] != double.MinValue){
                                uyy = thermalDiffusivity * (temps[i,j-1,k] - 2*temps[i,j,k] + temps[i,j+1,k])/Mathf.Pow((float)pointDistance, 2);
                            }
                            

                            //Assess change in temp in z-direction
                            double uzz = 0;
                            if((k!= 0 && k!=temps.GetLength(2)-1) && (temps[i,j,k-1] == double.MinValue && temps[i,j,k+1] == double.MinValue)){
                                uzz = 0;
                            } else if(k==0 || temps[i,j,k-1]==double.MinValue){
                                double q = ((thermalConductivity * Mathf.Pow((float)pointDistance, 2)) * ((float)(temps[i,j,k+1] - temps[i,j,k])))/pointDistance;
                                uzz = q/(density * Mathf.Pow((float)pointDistance, 3) * specificHeatCapacity * pointDistance); 
                            } else if(k==temps.GetLength(0)-1 || temps[i,j,k+1]==double.MinValue){
                                double q = ((thermalConductivity * Mathf.Pow((float)pointDistance, 2)) * ((float)(temps[i,j,k-1] - temps[i,j,k])))/pointDistance;
                                uzz = q/(density * Mathf.Pow((float)pointDistance, 3) * specificHeatCapacity * pointDistance); 
                            } else if(temps[i,j,k-1] != double.MinValue && temps[i,j,k+1] != double.MinValue){
                                uzz = thermalDiffusivity * (temps[i,j,k-1] - 2*temps[i,j,k] + temps[i,j,k+1])/Mathf.Pow((float)pointDistance, 2);
                            } 

                            // Set new temp of point based on heat equation
                            newTemps[i,j,k] = temps[i,j,k] + (uxx + uyy + uzz) * timeStep;

                        } else { //If point not in object, don't change placeholder value
                            newTemps[i,j,k] = double.MinValue;
                        }

                    }
                }
            }
            elapsedTime = System.Math.Round(elapsedTime + timeStep, 10); //Update time
        tempList.Add(newTemps);//Add temps to list 
        //Update the current temperatues of the points
        for(int i = 0; i<temps.GetLength(0); i++){
            for(int j = 0; j<temps.GetLength(1); j++){
                for(int k = 0; k<temps.GetLength(2); k++){
                    temps[i,j,k] = newTemps[i,j,k];
                }
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
            if(System.Math.Round((i * timeStep)/printStep, 10) % 1 == 0){
                WriteString("t= "+ (i*timeStep) + "\n" + tempArrToString(tempList[i]));
            }
        }
        printOthers();
        simulationComplete = true;
    }

    string tempArrToString(double[,,] tempArr){//Create array in formation capable of being read by numpy
        string outStr = "[";
        for(int i = 0; i<tempArr.GetLength(0); i++){
            outStr+="[";
            for(int j=0; j<tempArr.GetLength(1); j++){
                outStr+="[";
                for(int k = 0; k<tempArr.GetLength(2); k++){
                    if(tempArr[i,j,k] == double.MinValue){
                        outStr += "0";
                    } else {
                        outStr+=tempArr[i,j,k];
                    }
                    if(k<tempArr.GetLength(2)-1)
                        outStr+=", ";
                }
                outStr+="]";
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
        string path = "Assets/heatDispersion3DPointData.txt";
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(arr);
        writer.Close();

        AssetDatabase.ImportAsset(path); 
        TextAsset asset = (TextAsset)Resources.Load("heatDispersion3DPointData");
    }

        void printOthers(){
        double[,,] finalTemps = tempList[tempList.Count -1];
        double min = finalTemps[5,5,5], max = finalTemps[5,5,5];
        foreach(double temp in finalTemps){
            if(temp!=double.MinValue && temp < min){
                min = temp;
            } else if(temp > max){
                max = temp;
            }
        }
        Debug.Log("max: " + max);
        Debug.Log("min: " + min);
        Debug.Log("range: " + (max-min));

        double total = 0;
        foreach(double temp in finalTemps){
            if(temp != double.MinValue){
                total+=temp;
            }
        }
        double mean = (total)/(515);
        Debug.Log("mean: " + mean);

        double totalMeanDiv = 0;
        foreach(double temp in finalTemps){
            if(temp != double.MinValue){
                totalMeanDiv += Mathf.Abs((float)(mean-temp));
            }
        }
        double meanDiv = totalMeanDiv/(515);
        Debug.Log("mean div: " + meanDiv);
    }
}
