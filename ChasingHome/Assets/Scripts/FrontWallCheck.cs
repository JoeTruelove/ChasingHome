using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrontWallCheck : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Ground"))
        {
            NewFPPlayerMove.frontWall = true;
            Debug.Log("FrontWAll Script Detected");
            
        }
    }
}
