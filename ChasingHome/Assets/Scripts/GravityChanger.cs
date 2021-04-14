using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityChanger : MonoBehaviour
{
    Vector3 gravity;
    public bool rightWall = false;
    public bool leftWall = false;
    public bool floor = true;
    public bool ceiling = false;
    void Start()
    {
        gravity = Physics.gravity;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ceiling = true;
            floor = false;
            rightWall = false;
            leftWall = false;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ceiling = false;
            floor = true;
            rightWall = false;
            leftWall = false;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ceiling = false;
            floor = false;
            rightWall = true;
            leftWall = false;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ceiling = false;
            floor = false;
            rightWall = false;
            leftWall = true;
        }
    }
    void FixedUpdate()
    {
        Physics.gravity = gravity;

        if (Input.GetKeyDown(KeyCode.UpArrow) && ceiling)
        {
            gravity.x = 0;
            gravity.y = 9.81f;
            gravity.z = 0;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) && floor)
        {
            gravity.x = 0;
            gravity.y = -9.81f;
            gravity.z = 0;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) && rightWall)
        {
            gravity.x = 0;
            gravity.y = 0;
            gravity.z = 9.81f;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) && leftWall)
        {
            gravity.x = 0;
            gravity.y = 0;
            gravity.z = -9.81f;
        }

        
    }
}
