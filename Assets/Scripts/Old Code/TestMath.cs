using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMath : MonoBehaviour
{
    [SerializeField] GameObject other;
    [SerializeField] double beamRadius;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Mathf.Cos(Mathf.PI));
        Debug.Log(GetComponent<Collider>().bounds.size);
        Vector3 normal = transform.position - other.transform.position;
        Debug.Log(normal);
        Debug.DrawLine(transform.position, other.transform.position, Color.blue, 50);
        Vector3 movePos = new Vector3(0,2,0);
        Debug.Log(movePos);
        Debug.DrawLine(transform.position, movePos + transform.position, Color.blue, 50);
        Vector3 newPos = Vector3.ProjectOnPlane(movePos, normal);
        Debug.Log(newPos);
        Debug.DrawLine(transform.position, newPos+transform.position, Color.red, 50);
        for(float r = 0.01f; r<=beamRadius; r+=0.01f){
            for(float t = 0; t<2*Mathf.PI; t+=0.01f/r){
                Vector3 raycastCirc = new Vector3(r*Mathf.Cos(t), r*Mathf.Sin(t), 0);
                Vector3 raycastStart = Vector3.ProjectOnPlane(raycastCirc, normal);
                Debug.DrawLine(raycastStart+transform.position, raycastStart+other.transform.position, Color.green, 50);
            }
        }
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
