// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor;
// using System.IO;

// // public enum Materials{
// //     aluminum = 0,
// //     ice = 1
// // }
// public class UpdateHeatEquation : MonoBehaviour
// {
//     [Header("Point Data")]
//     [SerializeField] GameObject pointPrefab;
//     [SerializeField] float pointSizeFactor;
//     [SerializeField] double pointDistance;
//     [Header("Object Data")]
//     [SerializeField] Materials materials;
//         [SerializeField] double startingTemp;
//     [Header("Data Tracker")]
//     //[SerializeField][VectorLabels("X", "Y", "Z", "Temp")] List<Vector4> startingPoints = new List<Vector4>();
//     [SerializeField] double timeStep;
//     [SerializeField] double maxTime;
//     [SerializeField] double printStep;
//     [Header("Heat Source Data")]
//     [SerializeField] GameObject heatSource;
//     [SerializeField] double mirrorSurfaceArea;
//     [SerializeField] double beamRadius;
//     [SerializeField] double intensity;
//     [SerializeField] Vector3 angVelocity;

//     double elapsedTime = 0, printTime = 0;

//     double stepSizeX, stepSizeY;

//     GameObject[,,] points;//Create array of points in plane. This will be used later for simulation animation 
//     double[,,] temps; //Create array of temperatures assigned for points
//     bool[,,] isHeated; 
//     List<double[,,]> tempList = new List<double[,,]>(); //ArrayList of array of point temperatures at certain time 
//     List<bool[,,]> heatList = new List<bool[,,]>();
//     double density, specificHeatCapacity, thermalConductivity, thermalDiffusivity;
//     double beamIntensity;

//     bool heatedUpdated = true, tempsUpdated = false, animate = false;//See if all of temps updated before proceeding
//     void Awake(){
//         if(materials == Materials.aluminum){
//             density = 2710;
//             specificHeatCapacity = 897;
//             thermalConductivity = 237;
//             thermalDiffusivity = thermalConductivity/(density*specificHeatCapacity);
//         }

//         beamIntensity = (intensity * mirrorSurfaceArea)/(Mathf.PI*Mathf.Pow((float)beamRadius, 2));
//         Debug.Log(beamIntensity);

//         int layer_mask = LayerMask.GetMask("HeatedObject");
//         Vector3 meshSize = this.GetComponent<Collider>().bounds.size;
//         Vector3 startingPoint = new Vector3(transform.position.x - meshSize.x/2, transform.position.y + meshSize.y/2, transform.position.z + meshSize.z/2);
//         Vector3 endingPoint = new Vector3(transform.position.x + meshSize.x/2, transform.position.y - meshSize.y/2, transform.position.z - meshSize.z/2);

//         Debug.Log(((endingPoint.x-startingPoint.x)/pointDistance));
//         points = new GameObject[(int)Mathf.Abs((float)((endingPoint.x-startingPoint.x)/pointDistance))+1, (int)Mathf.Abs((float)((endingPoint.y-startingPoint.y)/pointDistance))+1, (int)Mathf.Abs((float)((endingPoint.z-startingPoint.z)/pointDistance))+1];
//         temps = new double[points.GetLength(0), points.GetLength(1), points.GetLength(2)];
//         isHeated = new bool[points.GetLength(0), points.GetLength(1), points.GetLength(2)];
//         //Instantiate(pointPrefab, startingPoint, Quaternion.identity, transform);

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
//         // foreach(Vector4 hotPoint in startingPoints){
//         //     temps[(int)hotPoint.x, (int)hotPoint.y,(int)hotPoint.z] = hotPoint.w;
//         // }
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
//         if(elapsedTime <= maxTime && heatedUpdated){
//             updateHeat();
//         } else if(heatedUpdated){
//             StartCoroutine(writeHeats());
//         } else if(elapsedTime <= maxTime && tempsUpdated){
//             updateTemps();
//         } else if(tempsUpdated){
//             StartCoroutine(writeTemps());
//         }
//     }
// //
//     // void updateHeated(){
//     //     heatsUpdated = false;

//     //     heatsUpdated = true;
//     // }
//     void updateHeat(){
//         heatedUpdated = false;
//         bool[,,] newIsHeated = new bool[isHeated.GetLength(0), isHeated.GetLength(1), isHeated.GetLength(2)];
//         // for(int i = 0; i<newIsHeated.GetLength(0); i++){
//         //     for(int j = 0; j<newIsHeated.GetLength(1); j++){
//         //         for(int k = 0; k<newIsHeated.GetLength(2); k++){
//         //             newIsHeated[i,j,k] = false;
//         //         }
//         //     }
//         // }
//         Vector3 normal = heatSource.transform.position - transform.position;
//         List<GameObject> hitPoints = new List<GameObject>();
//         for(float r = 0.01f; r<=beamRadius; r+=0.01f){
//             for(float t = 0; t<2*Mathf.PI; t+=0.1f/r){
//                 Vector3 raycastCirc = new Vector3(r*Mathf.Cos(t), r*Mathf.Sin(t), 0);
//                 Vector3 raycastStart = Vector3.ProjectOnPlane(raycastCirc, normal);
//                 Debug.DrawLine(raycastStart+heatSource.transform.position, transform.position+raycastStart, Color.green, (float) timeStep);
//                 //Debug.DrawRay(raycastStart+heatSource.transform.position, (transform.position-heatSource.transform.position), Color.green, (float)timeStep);
//                 if(Physics.Raycast(raycastStart+heatSource.transform.position, (transform.position-heatSource.transform.position).normalized, out RaycastHit hit, 300f, LayerMask.GetMask("Points"))){
//                     GameObject hitPoint = hit.transform.gameObject;
//                     //Debug.Log(hitPoint);
//                     if(hitPoints.FindIndex(HitPoint => HitPoint == hitPoint) == -1){
//                         hitPoints.Add(hitPoint);
//                     }
//                 }
//             }
//         }
//         //Debug.Log(hitPoints.Count);
//         //Debug.Log(hitPoints.GetLength(0));
//         //Debug.Log(hitPoints.GetLength(0));
//         for(int i = 0; i<points.GetLength(0); i++){
//             for(int j = 0; j<points.GetLength(1); j++){
//                 for(int k = 0; k<points.GetLength(2); k++){
//                     for(int l = 0; l<hitPoints.Count; l++){
//                         if(hitPoints[l] == points[i,j,k]){
//                             newIsHeated[i,j,k] = true;
//                         }
//                     }
//                 }
//             }
//         }
//         bool[,,] updated2 = newIsHeated;
//         // Debug.Log(arrToString(newIsHeated));
//         elapsedTime = System.Math.Round(elapsedTime + timeStep, 10);
//         heatList.Add((bool[,,])newIsHeated.Clone());
//         Debug.Log(elapsedTime);
//         transform.eulerAngles += Mathf.Rad2Deg*(angVelocity*(float)timeStep);
//         heatedUpdated = true;
//     }
//     void updateTemps(){
//         tempsUpdated = false;
//         double[,,] newTemps = new double[temps.GetLength(0), temps.GetLength(1),temps.GetLength(2)];
//         for(int i = 0; i<temps.GetLength(0); i++){
//                 for(int j = 0; j<temps.GetLength(1); j++){
//                     for(int k = 0; k<temps.GetLength(2); k++){
//                         if(temps[i,j,k] != double.MinValue){
//                             //Debug.Log("i: " + i + " j: " + j + " k: " + k);
//                             double uxx = 0;
//                             if((i!= 0 && i!=temps.GetLength(0)-1) && (temps[i-1,j,k] == double.MinValue && temps[i+1,j,k] == double.MinValue)){
//                                 uxx = 0;
//                             }else if(i==0 || temps[i-1,j,k]==double.MinValue){
//                                 uxx = (double)(((float)thermalConductivity * Mathf.Pow((float)pointDistance/2, 2) * (float)(temps[i+1,j,k] - temps[i,j,k]))/pointDistance)/((density * Mathf.Pow((float)pointDistance/2, 3) * specificHeatCapacity));
//                             } else if(i==temps.GetLength(0)-1 || temps[i+1,j,k]==double.MinValue){
//                                 uxx = ((double)((float)thermalConductivity * Mathf.Pow((float)pointDistance/2, 2) * (float)(temps[i-1,j,k] - temps[i,j,k]))/pointDistance)/((density * Mathf.Pow((float)pointDistance/2, 3) * specificHeatCapacity));
//                             } else if(temps[i-1,j,k] != double.MinValue && temps[i+1,j,k] != double.MinValue){
//                                 uxx = (temps[i-1,j,k] - 2*temps[i,j,k] + temps[i+1,j,k])/Mathf.Pow((float)pointDistance, 2);
//                                 //Debug.Log((temps[i-1,j,k] - 2*temps[i,j,k] + temps[i+1,j,k]));
//                             }

//                             double uyy = 0;
//                             if((j!= 0 && j!=temps.GetLength(1)-1) && (temps[i,j-1,k] == double.MinValue && temps[i,j+1,k] == double.MinValue)){
//                                 uyy=0;
//                             } else if(j==0 || temps[i,j-1,k]==double.MinValue){
//                                 uyy = (double)(((float)thermalConductivity * Mathf.Pow((float)pointDistance/2, 2) * (float)(temps[i,j+1,k] - temps[i,j,k]))/pointDistance)/((density * Mathf.Pow((float)pointDistance/2, 3) * specificHeatCapacity));
//                             } else if(j==temps.GetLength(1)-1 || temps[i,j+1,k]==double.MinValue){
//                                 uyy = (double)(((float)thermalConductivity * Mathf.Pow((float)pointDistance/2, 2) * (float)(temps[i,j-1,k] - temps[i,j,k]))/pointDistance)/((density * Mathf.Pow((float)pointDistance/2, 3) * specificHeatCapacity));
//                             } else if(temps[i,j-1,k] != double.MinValue && temps[i,j+1,k] != double.MinValue){
//                                 uyy = (temps[i,j-1,k] - 2*temps[i,j,k] + temps[i,j+1,k])/Mathf.Pow((float)pointDistance, 2);
//                             } 

//                             double uzz = 0;
//                             if((k!= 0 && k!=temps.GetLength(2)-1) && (temps[i,j,k-1] == double.MinValue && temps[i,j,k+1] == double.MinValue)){
//                                 uzz = 0;
//                             } else if(k==0 || temps[i,j,k-1]==double.MinValue){
//                                 uzz = (double)(((float)thermalConductivity * Mathf.Pow((float)pointDistance/2, 2) * (float)(temps[i,j,k+1] - temps[i,j,k]))/pointDistance)/((density * Mathf.Pow((float)pointDistance/2, 3) * specificHeatCapacity));
//                             } else if(k==temps.GetLength(0)-1 || temps[i,j,k+1]==double.MinValue){
//                                 uzz = (double)(((float)thermalConductivity * Mathf.Pow((float)pointDistance/2, 2) * (float)(temps[i,j,k-1] - temps[i,j,k]))/pointDistance)/((density * Mathf.Pow((float)pointDistance/2, 3) * specificHeatCapacity));
//                             } else if(temps[i,j,k-1] != double.MinValue && temps[i,j,k+1] != double.MinValue){
//                                 uzz = (temps[i,j,k-1] - 2*temps[i,j,k] + temps[i,j,k+1])/Mathf.Pow((float)pointDistance, 2);
//                             } 
//                             newTemps[i,j,k] = temps[i,j,k] + thermalDiffusivity * (uxx + uyy + uzz) * timeStep;
//                         } else {
//                             newTemps[i,j,k] = double.MinValue;
//                         }

//                         if(heatList[(int)(elapsedTime/timeStep)][i,j,k]){
//                             newTemps[i,j,k] += ((beamIntensity*3*Mathf.Pow((float)pointDistance/2, 2)) * timeStep)/(density * Mathf.Pow((float)pointDistance/2, 3) * specificHeatCapacity);
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

//     void printHeatList(){
//         // double ratio = System.Math.Round(printStep/timeStep, 5);
//         // heatedUpdated = false;
//         // for(int i = 0; i<heatList.Count; i++){
//         //     if(i % (int) ratio == 0){
//         //         Debug.Log("t = " + printTime + "\n" + printBools(heatList[i,j]));
//         //     }
//         //     printTime = System.Math.Round(printTime + timeStep, 10);
//         // }
//         tempsUpdated = true;
//         elapsedTime = 0;
//         printTime = 0;
//         Debug.Log("Done");
//         Debug.Log("Start translation");
//     }
//     private string printBools(bool[,] heatArr){
//         string printString = "";
//         for(int i = 0; i<heatArr.GetLength(0); i++){
//             for(int j = 0; j<heatArr.GetLength(1); j++){
//                 printString = printString + (" " + heatArr[i, j]);
//             }
//             printString += ("\n");
//         }
//         return printString;
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
//     private string arrToString(bool[,,] tempArr){
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
//         tempsUpdated = false;
//         Debug.Log("started write");
//         foreach(double[,,] temps in tempList){
//                 WriteString(arrToString(temps));
//                 yield return null;
//         }
//         Debug.Log("write complete");
//     }
//     IEnumerator writeHeats(){
//         heatedUpdated = false;
//         Debug.Log("started write");
//         // foreach(bool[,,] isHeat in heatList){
//         //     WriteString(arrToString(isHeat));
//         //     yield return null;
//         // }
//         elapsedTime = 0;
//         printTime = 0;
//         tempsUpdated = true;
//         Debug.Log("write complete");
//         yield return null;
//     }
// }
