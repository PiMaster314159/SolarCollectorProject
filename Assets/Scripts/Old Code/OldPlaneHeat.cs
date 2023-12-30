// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class PlaneHeatEquation : MonoBehaviour
// {
//     //How many points will be on the x and y axis of the object
//     [SerializeField] private int pointAmtX, pointAmtY;
//     //The lwh dimensions of each of the cubes representing points
//     [SerializeField] private float pointSize;
//     //Prefab instantiated onto object for heat values
//     [SerializeField] private GameObject pointPrefab;
//     [SerializeField] private double thermalDiffusivity;
//     [SerializeField][Range(0,1)] private double stepTime = 0.01;
//     private double eTime = 0;

//     private Collider plane;
//     private GameObject[,] pointArr;
//     private float stepSizeX, stepSizeY;
//     private bool isUpdated = true;


//     void Awake()
//     {
//         pointArr = new GameObject[pointAmtX, pointAmtY];    
//         Vector3 pointDimensions = new Vector3(pointSize, pointSize, pointSize);
//         //Set collider value to the plane's collider
//         plane = GetComponent<Collider>();
//         Vector3 planeSize = plane.bounds.size;
//         //Use collider size to establish origin in the top left corner of the plane
//         Vector3 originPosition = new Vector3(transform.position.x - planeSize.x / 2, transform.position.y, transform.position.z + planeSize.z / 2);
//         stepSizeX = (planeSize.x / (pointAmtX - 1));
//         stepSizeY = (planeSize.z / (pointAmtY - 1));
//         //On start of program, create points on cube where temperature is being measured
//         for (int i = 0; i < pointAmtX; i++)
//         {
//             for (int j = 0; j < pointAmtY; j++)
//             {
//                 Vector3 pointPosition = new Vector3(originPosition.x + stepSizeX * i, transform.position.y, originPosition.z - stepSizeY * j);
//                 GameObject point = Instantiate(pointPrefab, pointPosition, Quaternion.identity, transform);
//                 point.GetComponent<PointData>().temperature = 4;
//                 point.transform.localScale = new Vector3(stepSizeX, stepSizeX, stepSizeX);
//                 pointArr[i,j] = point;
//             }
//         }
//         ArrayList temps = new ArrayList();
//         Debug.Log(planeSize.x);
//         Debug.Log(stepSizeX);
//         pointArr[5,5].GetComponent<PointData>().temperature = 100;
//         printTemps();
//     }

//     void Update()
//     {
//         updateTemps();
//     }

//     private double getTemp(GameObject point){
//         return point.GetComponent<PointData>().temperature;
//     }
//     private void setTemp(GameObject point, float temp){
//         point.GetComponent<PointData>().temperature = temp;
//     }
//     private GameObject getPoint(int i, int j){
//         return pointArr[i,j];
//     }

//     private void updateTemps(){
//         if(isUpdated){
//             isUpdated = false;
//             for(int i = 0; i<pointArr.GetLength(0); i++){
//                 for(int j = 0; j<pointArr.GetLength(1); j++){
//                     GameObject point = pointArr[i,j];
//                     double uxx = 0;
//                     if(i==0){
//                         uxx = (getTemp(getPoint(i,j)) -2*getTemp(getPoint(i+1,j)) + getTemp(getPoint(i+2, j)))/Mathf.Pow(stepSizeX, 2);
//                     } else if(i==pointArr.GetLength(0)-1){
//                         uxx = (getTemp(getPoint(i-2,j)) - 2*getTemp(getPoint(i-1, j)) + getTemp(getPoint(i, j)))/Mathf.Pow(stepSizeX, 2);
//                     } else {
//                         uxx = (getTemp(getPoint(i-1,j)) - 2*getTemp(getPoint(i, j)) + getTemp(getPoint(i+1, j)))/Mathf.Pow(stepSizeX, 2);
//                     }
//                     double uyy = 0;
//                     if(j==0){
//                         uyy = (getTemp(getPoint(i,j)) -2*getTemp(getPoint(i,j+1)) + getTemp(getPoint(i, j+2)))/Mathf.Pow(stepSizeY, 2);
//                     } else if(j==pointArr.GetLength(1)-1){
//                         uyy = (getTemp(getPoint(i,j-2)) - 2*getTemp(getPoint(i, j-1)) + getTemp(getPoint(i, j)))/Mathf.Pow(stepSizeY, 2);
//                     } else {
//                         uyy = (getTemp(getPoint(i,j-1)) - 2*getTemp(getPoint(i, j)) + getTemp(getPoint(i, j+1)))/Mathf.Pow(stepSizeY, 2);
//                     }
//                     getPoint(i,j).GetComponent<PointData>().newTemp = getTemp(getPoint(i,j)) + thermalDiffusivity * (uxx + uyy) * stepTime;
//                 }
//             }
//             eTime = System.Math.Round(eTime + stepTime, 10);
//             foreach(GameObject point in pointArr){
//                 point.GetComponent<PointData>().updateTemp();
//                 point.GetComponent<PointData>().setColor();
//             }
//             printTemps();
//             isUpdated = true;
//         }
//     }
//     private void printTemps(){
//         if(eTime %01f == 0){
//             string printString = "";
//             for(int i = 0; i<pointArr.GetLength(0); i++){
//                 for(int j = 0; j<pointArr.GetLength(1); j++){
//                     printString = printString + (string.Format("{0} ", pointArr[i, j].GetComponent<PointData>().temperature));
//                 }
//                 printString += ("\n");
//             }
//             Debug.Log("t = " + eTime + "\n" + printString);
//         }
//     }
// }


// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class OldPlaneHeat : MonoBehaviour
// {
//     //How many points will be on the x and y axis of the object
//     [SerializeField] private int pointAmtX, pointAmtY;
//     //The lwh dimensions of each of the cubes representing points
//     [SerializeField] private float pointSize;
//     //Prefab instantiated onto object for heat values
//     [SerializeField] private GameObject pointPrefab;
//     [SerializeField] private float thermalDiffusivity;
//     /*[SerializeField][Range(0,1)]*/ private float stepTime = 0.01f;
//     private float eTime = 0f;

//     private Collider plane;
//     private GameObject[,] pointArr;
//     private float stepSizeX, stepSizeY;
//     private bool isUpdated = true;


//     void Awake()
//     {
//         pointArr = new GameObject[pointAmtX, pointAmtY];    
//         Vector3 pointDimensions = new Vector3(pointSize, pointSize, pointSize);
//         //Set collider value to the plane's collider
//         plane = GetComponent<Collider>();
//         Vector3 planeSize = plane.bounds.size;
//         //Use collider size to establish origin in the top left corner of the plane
//         Vector3 originPosition = new Vector3(transform.position.x - planeSize.x / 2, transform.position.y, transform.position.z + planeSize.z / 2);
//         stepSizeX = (planeSize.x / (pointAmtX - 1));
//         stepSizeY = (planeSize.z / (pointAmtY - 1));
//         //On start of program, create points on cube where temperature is being measured
//         for (int i = 0; i < pointAmtX; i++)
//         {
//             for (int j = 0; j < pointAmtY; j++)
//             {
//                 Vector3 pointPosition = new Vector3(originPosition.x + stepSizeX * i, transform.position.y, originPosition.z - stepSizeY * j);
//                 GameObject point = Instantiate(pointPrefab, pointPosition, Quaternion.identity, transform);
//                 point.GetComponent<PointData>().temperature = 4;
//                 pointArr[i,j] = point;
//             }
//         }
//         Debug.Log(planeSize.x);
//         Debug.Log(stepSizeX);
//         pointArr[5,5].GetComponent<PointData>().temperature = 100;
//         Debug.Log("Step time test: " + eTime);
//         eTime+=stepTime;
//         Debug.Log("Step time test: " + eTime);
//         eTime+=stepTime;
//         Debug.Log("Step time test: " + eTime);

//         printTemps();
//         updateTemps();
//         updateTemps();
//         updateTemps();
//         updateTemps();
//     }

//     void Update()
//     {
//         //updateTemps();
//         //pointArr[5,5].GetComponent<PointData>().temperature += 5*0.001f;
//     }

//     private float getTemp(GameObject point){
//         return point.GetComponent<PointData>().temperature;
//     }
//     private void setTemp(GameObject point, float temp){
//         point.GetComponent<PointData>().temperature = temp;
//     }
//     private GameObject getPoint(int i, int j){
//         return pointArr[i,j];
//     }

//     private void updateTemps(){
//         if(isUpdated){
//             isUpdated = false;
//             for(int i = 0; i<pointArr.GetLength(0); i++){
//                 for(int j = 0; j<pointArr.GetLength(1); j++){
//                     GameObject point = pointArr[i,j];
//                     float uxx = 0;
//                     if(i==0){
//                         uxx = (getTemp(getPoint(i,j)) -2*getTemp(getPoint(i+1,j)) + getTemp(getPoint(i+2, j)))/Mathf.Pow(stepSizeX, 2);
//                     } else if(i==pointArr.GetLength(0)-1){
//                         uxx = (getTemp(getPoint(i-2,j)) - 2*getTemp(getPoint(i-1, j)) + getTemp(getPoint(i, j)))/Mathf.Pow(stepSizeX, 2);
//                     } else {
//                         // if(i==1 && j==1){
//                         //     Debug.Log("yes");
//                         // }
//                         uxx = (getTemp(getPoint(i-1,j)) - 2*getTemp(getPoint(i, j)) + getTemp(getPoint(i+1, j)))/Mathf.Pow(stepSizeX, 2);
//                     }
//                     float uyy = 0;
//                     if(j==0){
//                         uyy = (getTemp(getPoint(i,j)) -2*getTemp(getPoint(i,j+1)) + getTemp(getPoint(i, j+2)))/Mathf.Pow(stepSizeY, 2);
//                     } else if(j==pointArr.GetLength(1)-1){
//                         uyy = (getTemp(getPoint(i,j-2)) - 2*getTemp(getPoint(i, j-1)) + getTemp(getPoint(i, j)))/Mathf.Pow(stepSizeY, 2);
//                     } else {
//                         uyy = (getTemp(getPoint(i,j-1)) - 2*getTemp(getPoint(i, j)) + getTemp(getPoint(i, j+1)))/Mathf.Pow(stepSizeY, 2);
//                     }
//                     getPoint(i,j).GetComponent<PointData>().newTemp = getTemp(getPoint(i,j)) + thermalDiffusivity * (uxx + uyy) * stepTime;
//                     eTime += stepTime;
//                     // point.GetComponent<PointData>().newTemp = getTemp(point) + thermalDiffusivity *
//                     // (
//                     //     (i==0 ?
//                     //         (getTemp(getPoint(i,j)) - 2*getTemp(getPoint(i+1, j)) + getTemp(getPoint(i+2, j)))
//                     //         : (i==pointArr.GetLength(0)-1 ?
//                     //             (getTemp(getPoint(i-2,j)) - 2*getTemp(getPoint(i-1, j)) + getTemp(getPoint(i, j))) :
//                     //             (getTemp(getPoint(i-1,j)) - 2*getTemp(getPoint(i, j)) + getTemp(getPoint(i+1, j)))
//                     //         )
//                     //     )/(2*Mathf.Pow(stepSizeX, 2)) - 
//                     //     (j==0 ?
//                     //         (getTemp(getPoint(i,j)) - 2*getTemp(getPoint(i, j+1)) + getTemp(getPoint(i, j+2)))
//                     //         : (j==pointArr.GetLength(1)-1 ?
//                     //             (getTemp(getPoint(i,j-2)) - 2*getTemp(getPoint(i, j-1)) + getTemp(getPoint(i, j))) :
//                     //             (getTemp(getPoint(i,j-1)) - 2*getTemp(getPoint(i, j)) + getTemp(getPoint(i, j+1)))
//                     //         )
//                     //     )/(2*Mathf.Pow(stepSizeY, 2))
//                     // ) * Time.deltaTime;
//                     // point.GetComponent<PointData>().newTemp = getTemp(point) + thermalDiffusivity * 
//                     // (
//                     //     (i==0 ? //if boundry to left, do this
//                     //         (getTemp(pointArr[i,j]) - 2*getTemp(pointArr[i+1,j]) + getTemp(pointArr[i+2,j])/(2*Mathf.Pow(stepSizeX, 2)))
//                     //         : (
//                     //             i==pointArr.GetLength(0)-1 ? // else if boundry is to the right, do this
//                     //                 (getTemp(pointArr[i-2,j]) - 2*getTemp(pointArr[i-1,j]) + getTemp(pointArr[i,j])/(2*Mathf.Pow(stepSizeX, 2)))
//                     //             : //else
//                     //                 (getTemp(pointArr[i-1,j]) - 2*getTemp(pointArr[i,j]) + getTemp(pointArr[i+1,j])/(2*Mathf.Pow(stepSizeX, 2)))
//                     //         )
//                     //     ) +
//                     //     (j==0 ? //if boundry to above, do this
//                     //         (getTemp(pointArr[i,j]) - 2*getTemp(pointArr[i,j+1]) + getTemp(pointArr[i,j+2])/(2*Mathf.Pow(stepSizeY, 2)))
//                     //         : (
//                     //             j==pointArr.GetLength(1)-1 ? // else if boundry is below, do this
//                     //                 (getTemp(pointArr[i,j-2]) - 2*getTemp(pointArr[i,j-1]) + getTemp(pointArr[i,j])/(2*Mathf.Pow(stepSizeY, 2)))
//                     //             : //else
//                     //                 (getTemp(pointArr[i,j-1]) - 2*getTemp(pointArr[i,j]) + getTemp(pointArr[i,j+1])/(2*Mathf.Pow(stepSizeY, 2)))
//                     //         )
//                     //     )
//                     // ) * Time.deltaTime;
//                     //if(i==1 &&  j==1)
//                     //Debug.Log("i: " + i + " j: " + j + " newTemp: " + point.GetComponent<PointData>().newTemp);
//                 }
//             }
//             foreach(GameObject point in pointArr){
//                 point.GetComponent<PointData>().updateTemp();
//                 point.GetComponent<PointData>().setColor();
//             }
//             printTemps();
//             isUpdated = true;
//         }
//     }
//     private void printTemps(){
//         Debug.Log(eTime);
//         if(eTime %1f == 0){
//             string printString = "";
//             for(int i = 0; i<pointArr.GetLength(0); i++){
//                 for(int j = 0; j<pointArr.GetLength(1); j++){
//                     printString = printString + (string.Format("{0} ", pointArr[i, j].GetComponent<PointData>().temperature));
//                 }
//                 printString += ("\n");
//             }
//             Debug.Log("t = " + eTime + "\n" + printString);
//         }
//     }
// }
