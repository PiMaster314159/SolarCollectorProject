using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastTest : MonoBehaviour
{
    [SerializeField] GameObject raycastOrigin;
    // Start is called before the first frame update
    void Start()
    {
        if(Physics.SphereCast(raycastOrigin.transform.position, 3, (transform.position-raycastOrigin.transform.position).normalized, out RaycastHit hit, 100))
            Debug.Log("yes");
        else
            Debug.Log("sad :(");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
