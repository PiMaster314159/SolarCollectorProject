// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor;
// using System.IO;
// public class OLDHeatEquation : MonoBehaviour
// {
//     [SerializeField] double pointDistance;
//     [SerializeField] double thermalDiffusivity;
//     [SerializeField] double startingTemp;
//     [SerializeField] GameObject pointPrefab;
//     [SerializeField] float pointSizeFactor;
//     [SerializeField][VectorLabels("X", "Y", "Z", "Temp")] List<Vector4> startingPoints = new List<Vector4>();
//     [SerializeField] double timeStep;
//     [SerializeField] double maxTime;
//     [SerializeField] double printStep;
//     double elapsedTime = 0, printTime = 0;

//     double stepSizeX, stepSizeY;

//     GameObject[,,] points;//Create array of points in plane. This will be used later for simulation animation 
//         double[,,] temps; //Create array of temperatures assigned for points 
//     List<double[,,]> tempList = new List<double[,,]>(); //ArrayList of array of point temperatures at certain time 

//     bool tempsUpdated = true, animate = false;//See if all of temps updated before proceeding
//     void Awake(){
//         int layer_mask = LayerMask.GetMask("HeatedObject");
//         Vector3 meshSize = this.GetComponent<Collider>().bounds.size;
//         Vector3 startingPoint = new Vector3(transform.position.x - meshSize.x/2, transform.position.y + meshSize.y/2, transform.position.z + meshSize.z/2);
//         Vector3 endingPoint = new Vector3(transform.position.x + meshSize.x/2, transform.position.y - meshSize.y/2, transform.position.z - meshSize.z/2);

//         Debug.Log(((endingPoint.x-startingPoint.x)/pointDistance));
//         points = new GameObject[(int)Mathf.Abs((float)((endingPoint.x-startingPoint.x)/pointDistance))+1, (int)Mathf.Abs((float)((endingPoint.y-startingPoint.y)/pointDistance))+1, (int)Mathf.Abs((float)((endingPoint.z-startingPoint.z)/pointDistance))+1];
//         temps = new double[(int)Mathf.Abs((float)((endingPoint.x-startingPoint.x)/pointDistance))+1, (int)Mathf.Abs((float)((endingPoint.y-startingPoint.y)/pointDistance))+1, (int)Mathf.Abs((float)((endingPoint.z-startingPoint.z)/pointDistance))+1];

//         for(int i = 0; i < points.GetLength(0); i++){
//             for(int j = 0; j<points.GetLength(1); j++){
//                 for(int k = 0; k<points.GetLength(2); k++){
//                     Vector3 pos = new Vector3((float)(startingPoint.x + i*pointDistance), (float)(startingPoint.y - j*pointDistance), (float)(startingPoint.z - k*pointDistance));
//                     //Debug.Log(pos);
//                     Collider[] hitColliders = Physics.OverlapSphere(pos, 0,layer_mask);
//                     if(hitColliders.Length >0){
//                         //Debug.Log(hitColliders[0].gameObject.name);
//                         GameObject point = Instantiate(pointPrefab, pos, Quaternion.identity, transform);
//                         point.transform.localScale = new Vector3((float)pointDistance/(10*pointSizeFactor), (float)pointDistance/(10*pointSizeFactor), (float)pointDistance/(10*pointSizeFactor)
//                         );
//                         points[i,j,k] = point;
//                         temps[i,j,k] = startingTemp;
//                     } else {
//                         points[i,j,k] = null;
//                         temps[i,j,k] = double.MinValue;
//                     }
//                 }
//             }
//         }
//         foreach(Vector4 hotPoint in startingPoints){
//             temps[(int)hotPoint.x, (int)hotPoint.y,(int)hotPoint.z] = hotPoint.w;
//         }
//         Debug.Log(temps.GetLength(2));
//         //tempList.Add(temps);
//         Debug.Log("start update");
//         // Vector3 planeSize = this.GetComponent<Collider>().bounds.size; //get size of plane
//         // Vector3 origin = new Vector3(transform.position.x - planeSize.x / 2, transform.position.y, transform.position.z + planeSize.z / 2);//Shift point array origin to top left of object
//         // stepSizeX = (planeSize.x / (pointAmtX - 1));//Set distance away from one another that points are going to be placed
//         // stepSizeY = (planeSize.z / (pointAmtY - 1));
//         // points = new GameObject[pointAmtX, pointAmtY];
//         // temps = new double[pointAmtX, pointAmtY];
//         // tempsUpdated = true;
//         // for (int i = 0; i < pointAmtX; i++)//Instantiate points onto plane and assign to array position mirroring temperatures
//         // {
//         //     for (int j = 0; j < pointAmtY; j++)
//         //     {
//         //         Vector3 pointPosition = new Vector3(origin.x + (float)stepSizeX * i, transform.position.y, origin.z - (float)stepSizeY * j);
//         //         GameObject point = Instantiate(pointPrefab, pointPosition, Quaternion.identity, transform);
//         //         point.GetComponent<PointData>().temperature = 4;
//         //         point.transform.localScale = new Vector3((float)stepSizeX, (float)stepSizeX, (float)stepSizeX);
//         //         points[i,j] = point;
//         //         temps[i,j] = point.GetComponent<PointData>().temperature;
//         //     }
//         // }
//         // foreach(Vector3 hotPoint in startingPoints){
//         //     temps[(int)hotPoint.x, (int)hotPoint.y] = hotPoint.z;
//         // }
//     }

//     void FixedUpdate(){
//         if(elapsedTime <= maxTime && tempsUpdated){
//             updateTemps();
//         } else if(tempsUpdated){
//             Debug.Log(tempList.Count);
//             Debug.Log("done");
//             StartCoroutine(writeTemps());
//             tempsUpdated = false;
//         }
//     }

//     // void updateHeated(){
//     //     heatsUpdated = false;

//     //     heatsUpdated = true;
//     // }
    
//     void updateTemps(){
//         tempsUpdated = false;
//         double[,,] newTemps = new double[temps.GetLength(0), temps.GetLength(1),temps.GetLength(2)];
//         for(int i = 0; i<temps.GetLength(0); i++){
//                 for(int j = 0; j<temps.GetLength(1); j++){
//                     for(int k = 0; k<temps.GetLength(2); k++){
//                         if(temps[i,j,k] != double.MinValue){
//                             Debug.Log("i: " + i + " j: " + j + " k: " + k);
//                             double uxx = 0;
//                             if((i!= 0 && i!=temps.GetLength(0)-1) && (temps[i-1,j,k] == double.MinValue && temps[i+1,j,k] == double.MinValue)){
//                                 Debug.Log("4");
//                                 uxx = 0;
//                             }else if(i==0 || temps[i-1,j,k]==double.MinValue){
//                                 Debug.Log("1");
//                                 uxx = (temps[i,j,k] -2*temps[i+1,j,k] + temps[i+2, j,k])/Mathf.Pow((float)pointDistance, 2);
//                                 //Debug.Log((temps[i,j,k] -2*temps[i+1,j,k] + temps[i+2, j,k])/Mathf.Pow((float)pointDistance, 2));
//                             } else if(i==temps.GetLength(0)-1 || temps[i+1,j,k]==double.MinValue){
//                                 Debug.Log("2");
//                                 uxx = (temps[i-2,j,k] - 2*temps[i-1,j,k] + temps[i,j,k])/Mathf.Pow((float)pointDistance, 2);
//                                 //Debug.Log((temps[i-2,j,k] - 2*temps[i-1,j,k] + temps[i,j,k]));
//                             } else if(temps[i-1,j,k] != double.MinValue && temps[i+1,j,k] != double.MinValue){
//                                 Debug.Log("3");
//                                 uxx = (temps[i-1,j,k] - 2*temps[i,j,k] + temps[i+1,j,k])/Mathf.Pow((float)pointDistance, 2);
//                                 //Debug.Log((temps[i-1,j,k] - 2*temps[i,j,k] + temps[i+1,j,k]));
//                             }

//                             double uyy = 0;
//                             if((j!= 0 && j!=temps.GetLength(1)-1) && (temps[i,j-1,k] == double.MinValue && temps[i,j+1,k] == double.MinValue)){
//                                 Debug.Log("4");
//                                 uyy=0;
//                             } else if(j==0 || temps[i,j-1,k]==double.MinValue){
//                                 Debug.Log("1");
//                                 uyy = (temps[i,j,k] -2*temps[i,j+1,k] + temps[i,j+2,k])/Mathf.Pow((float)pointDistance, 2);
//                             } else if(j==temps.GetLength(1)-1 || temps[i,j+1,k]==double.MinValue){
//                                 Debug.Log("2");
//                                 uyy = (temps[i,j-2,k] - 2*temps[i,j-1,k] + temps[i,j,k])/Mathf.Pow((float)pointDistance, 2);
//                             } else if(temps[i,j-1,k] != double.MinValue && temps[i,j+1,k] != double.MinValue){
//                                 Debug.Log("3");
//                                 uyy = (temps[i,j-1,k] - 2*temps[i,j,k] + temps[i,j+1,k])/Mathf.Pow((float)pointDistance, 2);
//                             } 

//                             double uzz = 0;
//                             if((k!= 0 && k!=temps.GetLength(2)-1) && (temps[i,j,k-1] == double.MinValue && temps[i,j,k+1] == double.MinValue)){
//                                 Debug.Log("4");
//                                 uzz = 0;
//                             } else if(k==0 || temps[i,j,k-1]==double.MinValue){
//                                 Debug.Log("1");
//                                 uzz = (temps[i,j,k] -2*temps[i,j,k+1] + temps[i,j,k+2])/Mathf.Pow((float)pointDistance, 2);
//                             } else if(k==temps.GetLength(0)-1 || temps[i,j,k+1]==double.MinValue){
//                                 Debug.Log("2");
//                                 uzz = (temps[i,j,k-2] - 2*temps[i,j,k-1] + temps[i,j,k])/Mathf.Pow((float)pointDistance, 2);
//                             } else if(temps[i,j,k-1] != double.MinValue && temps[i,j,k+1] != double.MinValue){
//                                 Debug.Log("3");
//                                 uzz = (temps[i,j,k-1] - 2*temps[i,j,k] + temps[i,j,k+1])/Mathf.Pow((float)pointDistance, 2);
//                             } 
//                             newTemps[i,j,k] = temps[i,j,k] + thermalDiffusivity * (uxx + uyy + uzz) * timeStep;
//                         } else {
//                             newTemps[i,j,k] = double.MinValue;
//                         }
//                     }
//                 }
//             }
//             elapsedTime = System.Math.Round(elapsedTime + timeStep, 10);
//         for(int i = 0; i<temps.GetLength(0); i++){
//             for(int j = 0; j<temps.GetLength(1); j++){
//                 for(int k = 0; k<temps.GetLength(2); k++){
//                     temps[i,j,k] = newTemps[i,j,k];
//                 }
//             }
//         }
//         tempList.Add(newTemps);
//         //Debug.Log("1");
//         tempsUpdated = true;
//     }

//     static void WriteString(string arr)
//     {
//         string path = "Assets/test.txt";

//         //Write some text to the test.txt file
//         StreamWriter writer = new StreamWriter(path, true);
//         writer.WriteLine(arr);
//         writer.Close();

//         //Re-import the file to update the reference in the editor
//         AssetDatabase.ImportAsset(path); 
//         TextAsset asset = (TextAsset)Resources.Load("test");

//         //Print the text from the file
//         //Debug.Log(asset.text);
//     }

//     private string arrToString(double[,,] tempArr){
//         string printString = "{\n";
//         for(int k = 0; k<tempArr.GetLength(2); k++){
//             printString+="[";
//             for(int i = 0; i<tempArr.GetLength(0); i++){
//                 for(int j = 0; j<tempArr.GetLength(1); j++){
//                     printString += (" " + tempArr[i, j, k]);
//                 }
//                 printString += ("\n");
//             }
//             printString += ("]\n");
//         }
//         printString+="}\n\n\n";
//         return printString;
//     }

//     // public void updatePoints(double[,] temps){
//     //     for(int i = 0; i<temps.GetLength(0); i++){
//     //         for(int j = 0; j<temps.GetLength(1); j++){
//     //             points[i,j].GetComponent<PointData>().temperature = temps[i,j];
//     //         }
//     //     }
//     // }
//     public void animatePoints(){
//         foreach(GameObject point in points){
//             point.GetComponent<PointData>().setColor();
//         }
//     }

//     IEnumerator writeTemps(){
//         Debug.Log("started write");
//         foreach(double[,,] temps in tempList){
//             WriteString(arrToString(temps));
//             yield return null;
//         }
//         Debug.Log("write complete");
//     }
// }