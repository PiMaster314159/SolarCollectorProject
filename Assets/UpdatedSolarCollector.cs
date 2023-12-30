using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class UpdatedSolarCollector : MonoBehaviour
{
    [Header("Point Data")]
    [SerializeField] GameObject pointPrefab;//object instantiated at different points
    [SerializeField] float pointSizeFactor;//Inverse proportional to collider size - bigger value, smaller point
    [SerializeField] double pointDistance; //How far each point is away from one another

    [Header("Object Data")]
    [SerializeField] Materials materials; //Instance of declared possible material array values
    [SerializeField] double startingTemp; //Temp all points are set to
    [SerializeField] Vector3 velocity; // Velocity of object in 3D space
    [SerializeField] Vector3 angVelocity; //Ang velocito of object
    [SerializeField] double objectDistanceFromEarth;

    [Header("Data Tracker")]
    [SerializeField] double posSimTimeStep; //Before beam is able to hit object, how often update position of object and collector
    [SerializeField] double timeStep; //Time at which temps and isHeated measured
    [SerializeField] double maxTime; //Time simulation runs for before automatically stopping
    [SerializeField] double printStep; //At what timeStep will data be printed?

    [Header("Solar Collector Data")]
    [SerializeField] GameObject heatSource; //Object in scene that acts as solar collector
    [SerializeField] double mirrorSurfaceArea; //Total surface area of solar collector mirrors
    [SerializeField] double beamRadius; //Radius of solar beam directed at object
    [SerializeField] double solarIntensity;
    [SerializeField] double collectorDistanceFromEarth;


    GameObject[,,] points;//Points where temperature is measured
    double[,,] temps; //Temps at all points
    List<double[,,]> tempList = new List<double[,,]>(); //Temp arrs at timeStep values

    bool[,,] isHeated; //Check to see if points heated by solar collector
    List<bool[,,]> heatList = new List<bool[,,]>(); //Is heated at timeStep values

    List<double> maxTempList = new List<double>();
    List<double> meanTempList = new List<double>();


    double elapsedTime = 0; //How long simulation has been running

    double density, specificHeatCapacity, thermalConductivity, thermalDiffusivity, fusionTemp, emissivity; //Values tweaked in Start() based on object material
    double beamIntensity; //Intensity of solar radiation in beam
    double maxTemp; //Highest temperature of point inside object
    double meanTemp;

    //bool heatedUpdated = true, tempsUpdated = false, animate = false; //values for proceeding to next step of simulation
    bool isUpdating;
    bool startSimulation = false, heatsWritten = true, tempsWritten = false;

    double  EARTH_RADIUS = 6371000, ORBIT_VELOCITY = 7018.528, KARMAN_LINE_HEIGHT = 100000,     STEFAN_BOLTZMANN_CONSTANT = 5.670374419 * Mathf.Pow(10, -8);

    double objectEarthAngle = 0, collectorEarthAngle = Mathf.PI;//Object and collector start on opposite sides of the planet

    double objectOrbitRadius, collectorOrbitRadius, objectAngVelocity, collectorAngVelocity;

    double totalTime = 0;

    Vector3 objectPosition, collectorPosition;
    bool run = true;

    string meanTempPath = "Assets/uMeanTemp.txt", maxTempPath = "Assets/TextFiles/maxTemp.txt";

    void Awake(){
        // Set object properties based on materials
        if(materials == Materials.aluminum){
            density = 2710;
            specificHeatCapacity = 897;
            thermalConductivity = 237;
            thermalDiffusivity = thermalConductivity/(density*specificHeatCapacity);
            fusionTemp = 933.15;
            emissivity = 0.4;
        } else if(materials == Materials.titanium){
            density = 4430;
            specificHeatCapacity = 526.3;
            thermalConductivity = 6.7;
            thermalDiffusivity = thermalConductivity/(density*specificHeatCapacity);
            fusionTemp = 1877.15;
            emissivity = 0.25;
        }
        beamIntensity = (solarIntensity * mirrorSurfaceArea)/(Mathf.PI*Mathf.Pow((float)beamRadius, 2));

        //Set angular velocities of object and collector around Earth
        objectOrbitRadius = EARTH_RADIUS+objectDistanceFromEarth;
        collectorOrbitRadius = EARTH_RADIUS+collectorDistanceFromEarth;
        objectAngVelocity = ORBIT_VELOCITY/(objectOrbitRadius);
        collectorAngVelocity = ORBIT_VELOCITY/(collectorOrbitRadius);
        objectPosition = new Vector3(-(float)objectOrbitRadius, 0,0);
        collectorPosition = new Vector3((float)collectorOrbitRadius, 0, 0);

        int layer_mask = LayerMask.GetMask("HeatedObject");//Get layer of space debris object
        Vector3 meshSize = GetComponent<Collider>().bounds.size; //Get size of cube surrounding mesh
        GetComponent<MeshRenderer>().enabled = false;
        Vector3 startingPoint = new Vector3(transform.position.x - meshSize.x/2, transform.position.y + meshSize.y/2, transform.position.z + meshSize.z/2); //point where instantiation begins
        Vector3 endingPoint = new Vector3(transform.position.x + meshSize.x/2, transform.position.y - meshSize.y/2, transform.position.z - meshSize.z/2); //point where instantiation ends



        points = new GameObject[(int)Mathf.Abs((float)((endingPoint.x-startingPoint.x)/pointDistance))+1,
            (int)Mathf.Abs((float)((endingPoint.y-startingPoint.y)/pointDistance))+1,
            (int)Mathf.Abs((float)((endingPoint.z-startingPoint.z)/pointDistance))+1];//Initialize 3D arr of points pointDistance away from one another, and following temp and isHeated arrs of same size
        temps = new double[points.GetLength(0), points.GetLength(1), points.GetLength(2)];
        isHeated = new bool[points.GetLength(0), points.GetLength(1), points.GetLength(2)];

        for(int i = 0; i < points.GetLength(0); i++){//Check if point is inside mesh at point. If yes, instantiate, set desired scale, and temperature to starting temp. If not, set point to null and temp to a placeholder value
            for(int j = 0; j<points.GetLength(1); j++){
                for(int k = 0; k<points.GetLength(2); k++){
                    Vector3 pos = new Vector3((float)(startingPoint.x + i*pointDistance), (float)(startingPoint.y - j*pointDistance), (float)(startingPoint.z - k*pointDistance));
                    Collider[] hitColliders = Physics.OverlapSphere(pos, 0,layer_mask);
                    if(hitColliders.Length >0){
                        GameObject point = Instantiate(pointPrefab, pos, Quaternion.identity, transform);
                        point.GetComponent<MeshRenderer>().enabled = false;
                        point.transform.localScale = new Vector3((float)(pointDistance/meshSize.x), (float)pointDistance/meshSize.y, (float)pointDistance/meshSize.y);
                        points[i,j,k] = point;
                        temps[i,j,k] = startingTemp;
                    } else {
                        points[i,j,k] = null;
                        temps[i,j,k] = double.MinValue;
                    }
                }
            }
        }
        Debug.Log("started");
    }

    void FixedUpdate(){
        if(run){
            if(!startSimulation){
                StartCoroutine(checkIsHittingObject());
                StartCoroutine(updatePositionsAndOrientation(posSimTimeStep));
            } else{
                if(elapsedTime < maxTime && maxTemp < fusionTemp){
                    if(!isUpdating){
                        isUpdating = true;
                        StartCoroutine(updatePointData());
                        StartCoroutine(checkIsHittingObject());
                    }
                } else if(elapsedTime >= maxTime || maxTemp > fusionTemp){
                    Debug.Log(maxTemp);
                    if(heatsWritten){
                        StartCoroutine(writeData());
                    } else if(tempsWritten){
                        StartCoroutine(writeTemps());
                    }
                    run = false;
                }
            }
        }
    }

    void updateHeat(){
        maxTemp = 0;
        meanTemp = 0;
        bool[,,] newIsHeated = new bool[isHeated.GetLength(0), isHeated.GetLength(1), isHeated.GetLength(2)];//Create new 3D bool array to check if objects are being heated at time

        //Simulate beam by creating raycasts in shape of cylendar pointed at center of solar collector. If hit point, check if raycast already hit the point. If point has not been hit before, then add to list of hit points.
        Vector3 normal = heatSource.transform.position - transform.position;
        List<GameObject> hitPoints = new List<GameObject>();
        for(float r = (float)beamRadius/10f; r<=beamRadius; r+=(float)beamRadius/20f){
            int count = 0;
            for(float theta = 0; theta<2*Mathf.PI; theta+=0.005f/r){
                double raycastXPos = r*Mathf.Cos(theta);
                double raycastYPos = r*Mathf.Sin(theta);
                Vector3 raycastCirc = new Vector3(0, (float)raycastYPos, (float)raycastXPos);
                Vector3 raycastStart = Vector3.ProjectOnPlane(raycastCirc, normal).normalized * raycastCirc.magnitude;
                ///Debug.DrawLine(raycastStart+heatSource.transform.position, transform.position+raycastStart, Color.green, (float) timeStep);
                if(Physics.Raycast(raycastStart+heatSource.transform.position, (transform.position-heatSource.transform.position).normalized, out RaycastHit hit, 300f, LayerMask.GetMask("Points"))){
                    GameObject hitPoint = hit.transform.gameObject;
                    if(hitPoints.FindIndex(HitPoint => HitPoint == hitPoint) == -1){
                        hitPoints.Add(hitPoint);
                    }
                }
                count++;
            }
        }
        //For every point in object, check if it has been hit. If so, change its respective value in bool Arr to true;
        for(int i = 0; i<points.GetLength(0); i++){
            for(int j = 0; j<points.GetLength(1); j++){
                for(int k = 0; k<points.GetLength(2); k++){
                    for(int l = 0; l<hitPoints.Count; l++){
                        if(hitPoints[l] == points[i,j,k]){
                            newIsHeated[i,j,k] = true;
                        }
                    }
                }
            }
        }
        heatList.Add(newIsHeated);

        updateTemps(newIsHeated);
    }
    void updateTemps(bool[,,] NewIsHeated){
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
                            uxx = ((thermalConductivity * Mathf.Pow((float)pointDistance, 2)) * ((float)(temps[i+1,j,k] - temps[i,j,k])))/(Mathf.Pow((float)pointDistance, 2) * (Mathf.Pow((float)pointDistance, 3) * density) * specificHeatCapacity);
                        } else if(i==temps.GetLength(0)-1 || temps[i+1,j,k]==double.MinValue){
                            uxx = ((thermalConductivity * Mathf.Pow((float)pointDistance, 2)) * ((float)(temps[i-1,j,k] - temps[i,j,k])))/(Mathf.Pow((float)pointDistance, 2) * (Mathf.Pow((float)pointDistance, 3) * density) * specificHeatCapacity);
                        } else if(temps[i-1,j,k] != double.MinValue && temps[i+1,j,k] != double.MinValue){
                            uxx = thermalDiffusivity * (temps[i-1,j,k] - 2*temps[i,j,k] + temps[i+1,j,k])/Mathf.Pow((float)pointDistance, 2);
                        }

                        //Assess change in temp in y-direction
                        double uyy = 0;
                        if((j!= 0 && j!=temps.GetLength(1)-1) && (temps[i,j-1,k] == double.MinValue && temps[i,j+1,k] == double.MinValue)){
                            uyy=0;
                        } else if(j==0 || temps[i,j-1,k]==double.MinValue){
                            uyy = ((thermalConductivity * Mathf.Pow((float)pointDistance, 2)) * ((float)(temps[i,j+1,k] - temps[i,j,k])))/(Mathf.Pow((float)pointDistance, 2) * (Mathf.Pow((float)pointDistance, 3) * density) * specificHeatCapacity);
                        } else if(j==temps.GetLength(1)-1 || temps[i,j+1,k]==double.MinValue){
                            uyy = ((thermalConductivity * Mathf.Pow((float)pointDistance, 2)) * ((float)(temps[i,j-1,k] - temps[i,j,k])))/(Mathf.Pow((float)pointDistance, 2) * (Mathf.Pow((float)pointDistance, 3) * density) * specificHeatCapacity);
                        } else if(temps[i,j-1,k] != double.MinValue && temps[i,j+1,k] != double.MinValue){
                            uyy = thermalDiffusivity * (temps[i,j-1,k] - 2*temps[i,j,k] + temps[i,j+1,k])/Mathf.Pow((float)pointDistance, 2);
                        }

                        //Assess change in temp in z-direction
                        double uzz = 0;
                        if((k!= 0 && k!=temps.GetLength(2)-1) && (temps[i,j,k-1] == double.MinValue && temps[i,j,k+1] == double.MinValue)){
                            uzz = 0;
                        } else if(k==0 || temps[i,j,k-1]==double.MinValue){
                            uzz = ((thermalConductivity * Mathf.Pow((float)pointDistance, 2)) * ((float)(temps[i,j,k+1] - temps[i,j,k])))/(Mathf.Pow((float)pointDistance, 2) * (Mathf.Pow((float)pointDistance, 3) * density) * specificHeatCapacity);
                        } else if(k==temps.GetLength(0)-1 || temps[i,j,k+1]==double.MinValue){
                            uzz = ((thermalConductivity * Mathf.Pow((float)pointDistance, 2)) * ((float)(temps[i,j,k-1] - temps[i,j,k])))/(Mathf.Pow((float)pointDistance, 2) * (Mathf.Pow((float)pointDistance, 3) * density) * specificHeatCapacity);
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


        for(int i = 0; i<temps.GetLength(0); i++){
            for(int j = 0; j<temps.GetLength(1); j++){
                for(int k = 0; k<temps.GetLength(2); k++){
                    // Update temperature of point if is being heated at this time
                    if(NewIsHeated[i,j,k]){
                        newTemps[i,j,k] += (((beamIntensity*3*Mathf.Pow((float)pointDistance, 2)))/(density * Mathf.Pow((float)pointDistance, 3) * specificHeatCapacity))*timeStep;
                    }
                }
            }
        }

        for(int i = 0; i<temps.GetLength(0); i++){
            for(int j = 0; j<temps.GetLength(1); j++){
                for(int k = 0; k<temps.GetLength(2); k++){
                    if(temps[i,j,k] != double.MinValue){
                        double oldTemp = newTemps[i,j,k];
                        //If point is along exterior of debris object, then decrease temperature based on the Stefan-Boltzmann law
                        if(((i!= 0 && i!=temps.GetLength(0)-1) && (temps[i-1,j,k] == double.MinValue && temps[i+1,j,k] == double.MinValue)) || ((i==0 || temps[i-1,j,k]==double.MinValue) || i==temps.GetLength(0)-1 || temps[i+1,j,k]==double.MinValue)){
                            newTemps[i,j,k] -= timeStep * (emissivity * STEFAN_BOLTZMANN_CONSTANT * (3*Mathf.Pow((float)pointDistance, 2) * Mathf.Pow((float)newTemps[i,j,k], 4)))/(density * Mathf.Pow((float)pointDistance, 3) * specificHeatCapacity);
                        } else if(((j!= 0 && j!=temps.GetLength(1)-1) && (temps[i,j-1,k] == double.MinValue && temps[i,j+1,k] == double.MinValue)) || ((j==0 || newTemps[i,j-1,k]==double.MinValue)) || (j==temps.GetLength(1)-1 || temps[i,j+1,k]==double.MinValue)){
                            newTemps[i,j,k] -= timeStep * (emissivity * STEFAN_BOLTZMANN_CONSTANT * (3*Mathf.Pow((float)pointDistance, 2) * Mathf.Pow((float)newTemps[i,j,k], 4)))/(density * Mathf.Pow((float)pointDistance, 3) * specificHeatCapacity);
                        } else if(((k!= 0 && k!=temps.GetLength(2)-1) && (temps[i,j,k-1] == double.MinValue && temps[i,j,k+1] == double.MinValue)) || (k==0 || temps[i,j,k-1]==double.MinValue) || (k==temps.GetLength(0)-1 || temps[i,j,k+1]==double.MinValue)){
                            newTemps[i,j,k] -= timeStep * (emissivity * STEFAN_BOLTZMANN_CONSTANT * (3*Mathf.Pow((float)pointDistance, 2) * Mathf.Pow((float)newTemps[i,j,k], 4)))/(density * Mathf.Pow((float)pointDistance, 3) * specificHeatCapacity);
                        }
                    }
                }
            }
        }  


        //Update temperature of points
        for(int i = 0; i<temps.GetLength(0); i++){
            for(int j = 0; j<temps.GetLength(1); j++){
                for(int k = 0; k<temps.GetLength(2); k++){
                    temps[i,j,k] = newTemps[i,j,k];
                }
            }
        }

        //Calculate/add measurable values to arrayLists for writing & Exportation
        foreach(double temp in newTemps){
            if(temp > maxTemp){
                maxTemp = temp;
            }
        }
        maxTempList.Add(maxTemp);

        int tCount = 0;
        int pointCount = 0;
        foreach(double temp in newTemps){
            if(temp != double.MinValue){
                if(temp!=300)
                    tCount++;
                meanTemp+=temp;
                pointCount++;
            }
        }
        meanTemp/=pointCount;
        meanTempList.Add(meanTemp);

        tempList.Add(newTemps);

        temps = newTemps;

        //Debug.Log(elapsedTime);
        if(elapsedTime%1==0){
            StartCoroutine(writeData());
            meanTempList = new List<double>();
            maxTempList = new List<double>();
            heatList = new List<bool[,,]>();
            tempList = new List<double[,,]>();
        }
        elapsedTime = System.Math.Round(elapsedTime + timeStep, 10); //Update time
    }

    IEnumerator updatePointData(){
        updateHeat();
        StartCoroutine(checkIsHittingObject());
        StartCoroutine(updatePositionsAndOrientation(timeStep));
        transform.Rotate(Mathf.Rad2Deg*(angVelocity*(float)timeStep), Space.Self);
        isUpdating = false;
        yield return null;
    }

    static void WriteString(string path, string arr)
    {
        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(arr);
        writer.Close();

        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(path);
        TextAsset asset = (TextAsset)Resources.Load(path);
    }

    IEnumerator writeTemps(){
        Debug.Log("started write");
        Debug.Log(elapsedTime);
        tempsWritten = false;
        Debug.Log("write complete");
        yield return null;
    }
    IEnumerator writeData(){
        Debug.Log("started write");
        heatsWritten = false;
        int count = 0;
        int printCount = 0;
        foreach(double MaxTemp in maxTempList){
            if(System.Math.Round((count * timeStep)/printStep, 10) % 1 == 0){
                WriteString(maxTempPath, ""+MaxTemp);
                printCount++;
            }
            count++;
        }
        count = 0;
        printCount = 0;
        foreach(double MeanTemp in meanTempList){
            if(System.Math.Round((count * timeStep)/printStep, 10) % 1 == 0){
                WriteString(meanTempPath, ""+MeanTemp);
                printCount++;
            }
            count++;
        }
        Debug.Log("write complete");
        yield return null;
    }

    private string arrToString(double[,,] tempArr){
        string printString = "{\n";
        for(int k = 0; k<tempArr.GetLength(2); k++){
            printString+="[";
            for(int i = 0; i<tempArr.GetLength(0); i++){
                for(int j = 0; j<tempArr.GetLength(1); j++){
                    printString += (" " + tempArr[i, j, k]);
                }
                printString += ("\n");
            }
            printString += ("]\n");
        }
        printString+="}\n\n\n";
        return printString;
    }
    private string arrToString(bool[,,] tempArr){
        string printString = "{\n";
        for(int k = 0; k<tempArr.GetLength(2); k++){
            printString+="[";
            for(int i = 0; i<tempArr.GetLength(0); i++){
                for(int j = 0; j<tempArr.GetLength(1); j++){
                    printString += (" " + tempArr[i, j, k]);
                }
                printString += ("\n");
            }
            printString += ("]\n");
        }
        printString+="}\n\n\n";
        return printString;
    }

    // Simulate orientation of objects to one another when orbiting earth.
    IEnumerator checkIsHittingObject(){
        //Find altitude of triangle with center of earth, object, and solar collector as vertices
        double distanceBetween = (objectPosition - collectorPosition).magnitude;
        double triArea = 0.25 * Mathf.Sqrt((float)((objectOrbitRadius+collectorOrbitRadius+distanceBetween) * (-objectOrbitRadius+collectorOrbitRadius+distanceBetween) * (objectOrbitRadius-collectorOrbitRadius+distanceBetween) * (objectOrbitRadius+collectorOrbitRadius-distanceBetween)));
        double triAltitude = 2*triArea/distanceBetween;
        //Test to see if Earth's atmosphere radius intersects beam. If no, then begin pointing beam at object
        if((EARTH_RADIUS+KARMAN_LINE_HEIGHT) < triAltitude){
            if(!startSimulation)
                //elapsedTime = 0;
            startSimulation = true;
        } else {
            totalTime +=elapsedTime;
            //elapsedTime = 0;
            startSimulation = false;
        }
        totalTime+=posSimTimeStep;//Update total time
        yield return null;
    }

    IEnumerator updatePositionsAndOrientation(double TimeStep){
        //Calculate how collector and object change position relative to Earth per timeStep, then change their simulated positions respectively
        double objectRads = objectAngVelocity*TimeStep;
        objectEarthAngle += objectRads;
        double collectorRads = -collectorAngVelocity*TimeStep;
        collectorEarthAngle += collectorRads;

        objectPosition = new Vector3((float)objectOrbitRadius*Mathf.Cos((float)objectEarthAngle), (float)objectOrbitRadius*Mathf.Sin((float)objectEarthAngle),0);
        collectorPosition = new Vector3((float)collectorOrbitRadius*Mathf.Cos((float)collectorEarthAngle), (float)collectorOrbitRadius*Mathf.Sin((float)collectorEarthAngle),0);

        //Given position data, get orientation to one another, then change position of solar collector in scene to model orientation
        Vector3 localDist = (collectorPosition - objectPosition).normalized;
        heatSource.transform.position = transform.position + localDist;
        yield return null;
    }
}
