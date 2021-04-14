using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public bool debugMode = false;
    public bool isGrounded;
    public float jumpHeight = 3f;
    public bool isMoving = false;
    private float yRot;
    public float speed = 1;
    public float horizontalSpeed = 1;
    public float maxSpeed = 1;
    public bool onWall = false;
    public float raycastLineDist = 1;
    public bool frontWall = false;
    public bool startMoving = false;
    public int timesJumped = 0;
    public int currentLevel = 0;
    public bool onWallGravity = true;
    public float onWallGravitySpeed = .001f;
    public bool deathAllowed = true;
    public List<GameObject> Spawns;

    private bool death = false;
    private bool justPortaled = false;
    private bool justLeftWalled = false;
    private bool justRightWalled = false;

    //private Animator anim;
    private Rigidbody rigidBody;
    public GameObject playerBody;

    // Use this for initialization
    void Start()
    {

        //anim = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        startMoving = false;

    }
    private void FixedUpdate()
    {

        //Raycast
        Vector3 forward = transform.TransformDirection(Vector3.forward) * raycastLineDist;
        Debug.DrawRay(transform.position, forward, Color.red);

        RaycastHit hit;
        
        if (Physics.Raycast(transform.position, forward, raycastLineDist))
        {
            print("hit!");
            frontWall = true;
        }
        else
        {
            frontWall = false;
        }

    }
    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.W))
        {
            startMoving = true;
        }

        //Camera Code
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, yRot, transform.localEulerAngles.z);

        isMoving = false;

        //Jumping code
        if ((Input.GetKeyDown(KeyCode.Space) && (isGrounded || (timesJumped < 2 && onWall)) && startMoving))
        {
            //transform.Translate(Vector3.up * jumpHeight);
            //rigidBody.AddForce(transform.TransformDirection(Vector3.up) * 5);
            rigidBody.AddForce(Vector3.up * jumpHeight, ForceMode.VelocityChange);
            isGrounded = false;
            timesJumped++;
        }

        //move character forward
        if (!frontWall && startMoving)
        {
            transform.Translate(Vector3.forward * Time.deltaTime * speed);
            playerBody.GetComponent<Animator>().SetBool("Running", true);
        }
        if(onWall)
        {
            Vector3 pos = new Vector3(transform.position.x, transform.position.y - onWallGravitySpeed, transform.position.z);
            transform.position = pos;
        }
        
        //Moving left and right
        if ((Input.GetAxisRaw("Horizontal") > 0.5f || Input.GetAxisRaw("Horizontal") < -0.5f) && startMoving)
        {
            //transform.Translate(Vector3.right * Input.GetAxis("Horizontal") * playerSpeed);
            rigidBody.velocity += transform.right * Input.GetAxisRaw("Horizontal") * horizontalSpeed;
            /*if(rigidBody.velocity.sqrMagnitude < maxSpeed && transform.right)
            {
                rigidBody.velocity += transform.right * Input.GetAxisRaw("Horizontal") * horizontalSpeed;
            }*/
            
            isMoving = true;
        }

        //Turn on or off onWallGravity
        if(Input.GetKeyDown(KeyCode.G))
        {
            onWallGravity = !onWallGravity;
        }

        //Reset Player Position
        if (Input.GetKeyDown(KeyCode.R))
        {
            changeLevel(currentLevel);
        }

        //Gravity Code
        /*if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            this.gameObject.transform.Rotate(0, 0, 180);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            this.gameObject.transform.Rotate(0, 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            this.gameObject.transform.Rotate(0, 0, 90);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            this.gameObject.transform.Rotate(0, 0, 270);
        }*/

        

        

        //wanim.SetBool("isMoving", isMoving);
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            changeLevel(0);
            currentLevel = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            changeLevel(1);
            currentLevel = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            changeLevel(2);
            currentLevel = 2;

        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            changeLevel(3);
            currentLevel = 3;
        }
    }

    public void changeLevel(int i)
    {
        GameObject o = Spawns[i];
        this.gameObject.transform.position = o.transform.position;
        death = false;
        startMoving = false;
        justPortaled = false;
        isGrounded = true;
        rigidBody.velocity = Vector3.zero;
        playerBody.GetComponent<Animator>().SetBool("Running", false);
        if(currentLevel == 0 || currentLevel == 3)
        {
            speed = 8;
        }
        if(currentLevel == 1)
        {
            speed = 12;
        }
        if(currentLevel == 2)
        {
            speed = 9;
        }
    }
    //consider what the object is collider with
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Ground")
        {
            isGrounded = true;
            timesJumped = 0;
            onWall = false;
            justLeftWalled = false;
            justRightWalled = false;
        }

        if (other.gameObject.tag == "LeftWall")
        {
            onWall = true;
            if(justRightWalled && timesJumped == 2)
            {
                timesJumped = 1;
            }
            justRightWalled = false;
            justLeftWalled = true;
        }
        
        if (other.gameObject.tag == "RightWall")
        {
            onWall = true;
            if (justLeftWalled && timesJumped == 2)
            {
                timesJumped = 1;
            }
            justLeftWalled = false;
            justRightWalled = true;
        }

        if (other.gameObject.tag == "Death")
        {
            if(deathAllowed)
            {
                death = true;
                changeLevel(currentLevel);
            }
        }

        if (other.gameObject.tag == "Portal")
        {
            currentLevel++;
            changeLevel(currentLevel);
        }
    }

    //consider when character is jumping .. it will exit collision.
    void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag == "Ground")
        {
            isGrounded = false;
        }

        if (other.gameObject.tag == "LeftWall")
        {
            onWall = false;
            justLeftWalled = true;
            justRightWalled = false;
        }

        if (other.gameObject.tag == "RightWall")
        {
            onWall = false;
            justRightWalled = true;
            justLeftWalled = false;
        }

        if (other.gameObject.tag == "Death")
        {
            death = false;
        }
    }

}
