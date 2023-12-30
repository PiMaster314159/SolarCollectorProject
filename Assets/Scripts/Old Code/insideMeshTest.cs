using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class insideMeshTest : MonoBehaviour
{
    [SerializeField] Vector3 offsetFromCenter;
    MeshCollider col;
    // Start is called before the first frame update
    void Awake()
    {
        MeshCollider col = this.GetComponent<MeshCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] hitColliders = Physics.OverlapSphere(offsetFromCenter, 0f);
        if(hitColliders.Length >0){
            Debug.Log("yes");
        }
        // Physics.queriesHitBackfaces= true;
        // //Debug.Log(inMesh(this.GetComponent<MeshCollider>(), offsetFromCenter));
        // Debug.Log(checkIfInside(offsetFromCenter));
    }
    // private bool inObject(){
    //     SphereCollider col = this.GetComponent<SphereCollider>();
    //     Vector3 testPoint = offsetFromCenter;
    //     RaycastHit[] hit = Physics.RaycastAll(testPoint, transform.forward, 100f);
    //     int count = 0;
    //     Debug.Log(hit.Length);
    //     foreach(RaycastHit h in hit){
    //         if(h.collider == col)
    //             count++;
    //     }
    //     //Debug.Log(count);
    //     return count%2==1;
    // }

    bool inMesh(MeshCollider col, Vector3 point){
        if (!col.bounds.Contains(point))
            return false;
        Physics.queriesHitBackfaces = true;
        RaycastHit[] hits = new RaycastHit[10];
        int num = Physics.RaycastNonAlloc(offsetFromCenter, Vector3.up, hits, 100f);
        Debug.Log(num);
        return true;
    }

    bool checkIfInside(Vector3 point) {

        Vector3 direction = new Vector3(0, 1, 0);

        if(Physics.Raycast(point, direction, Mathf.Infinity) &&
            Physics.Raycast(point, -direction, Mathf.Infinity)) {
                return true;
        }

        else return false;
}
}
