using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewFPPlayerMove : MonoBehaviour
{

    //Wallrunning
    public LayerMask whatIsWall;
    public float wallrunForce, maxWallrunTime, maxWallSpeed;
    bool isWallRight, isWallLeft;
    bool isWallRunning;
    public float maxWallRunCameraTilt, wallRunCameraTilt;
    public float timesJumped = 0;
    public bool onWall = false;
    private bool justRightWalled = false;
    private bool justLeftWalled = false;
    static int deaths = 0;
    public bool gameOver = false;
    public bool dennisChangesEnabled = false;

    private void WallRunInput() //make sure to call in void Update
    {
        //Wallrun
        if (Input.GetKey(KeyCode.D) && isWallRight) StartWallrun();
        if (Input.GetKey(KeyCode.A) && isWallLeft) StartWallrun();
    }
    private void StartWallrun()
    {
        rb.useGravity = false;
        isWallRunning = true;
        allowDashForceCounter = false;

        if (rb.velocity.magnitude <= maxWallSpeed)
        {
            rb.AddForce(orientation.forward * wallrunForce * Time.deltaTime);

            //Make sure char sticks to wall
            if (isWallRight)
                rb.AddForce(orientation.right * wallrunForce / 5 * Time.deltaTime);
            else
                rb.AddForce(-orientation.right * wallrunForce / 5 * Time.deltaTime);
        }
    }
    private void StopWallRun()
    {
        isWallRunning = false;
        rb.useGravity = true;
    }
    private void CheckForWall() //make sure to call in void Update
    {
        isWallRight = Physics.Raycast(transform.position, orientation.right, 1f, whatIsWall);
        isWallLeft = Physics.Raycast(transform.position, -orientation.right, 1f, whatIsWall);

        //leave wall run
        if (!isWallLeft && !isWallRight) StopWallRun();
        //reset double jump (if you have one :D)
        if (isWallLeft || isWallRight) doubleJumpsLeft = startDoubleJumps;
    }


    /// <summary>
    /// Wall run done, here comes the rest of the movement script
    /// </summary>


    //Assingables
    public Transform playerCam;
    public Transform orientation;

    //Other
    private Rigidbody rb;

    //Rotation and look
    private float xRotation;
    private float sensitivity = 50f;
    private float sensMultiplier = 1f;

    //Movement
    public float moveSpeed = 4500;
    public float maxSpeed = 20;
    private float startMaxSpeed;
    public bool grounded;
    public LayerMask whatIsGround;

    public float counterMovement = 0.175f;
    private float threshold = 0.01f;
    public float maxSlopeAngle = 35f;

    //Crouch & Slide
    private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale;
    public float slideForce = 400;
    public float slideCounterMovement = 0.2f;
    public float crouchGravityMultiplier;

    //Jumping
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    public float jumpForce = 550f;

    public int startDoubleJumps = 1;
    int doubleJumpsLeft;

    //Input
    public float x, y;
    bool jumping, sprinting, crouching;

    //AirDash
    public float dashForce;
    public float dashCooldown;
    public float dashTime;
    bool allowDashForceCounter;
    bool readyToDash;
    int wTapTimes = 0;
    Vector3 dashStartVector;

    //RocketBoost
    public float maxRocketTime;
    public float rocketForce;
    bool rocketActive, readyToRocket;
    bool alreadyInvokedRockedStop;
    float rocketTimer;

    //Sliding
    private Vector3 normalVector = Vector3.up;

    //SonicSpeed
    public float maxSonicSpeed;
    public float sonicSpeedForce;
    public float timeBetweenNextSonicBoost;
    float timePassedSonic;

    //flash
    public float flashCooldown, flashRange;
    public int maxFlashesLeft;
    bool alreadySubtractedFlash;
    public int flashesLeft = 3;

    //Climbing
    public float climbForce, maxClimbSpeed;
    public LayerMask whatIsLadder;
    bool alreadyStoppedAtLadder;

    public GameObject playerBody;
    public float speed;
    public List<GameObject> Spawns;
    public List<GameObject> Portals;

    private bool death = false;
    public bool startMoving = false;
    public bool justPortaled = false;
    public bool deathAllowed = true;
    public float raycastLineDist = 1;
    public static bool frontWall = false;
    public Camera camera;

    public int currentLevel = 0;
    public Canvas gameOverUI;
    public AudioSource aSource;
    public List<AudioClip> music;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startMaxSpeed = maxSpeed;
        LockCursor();
        PauseScript.GameIsPaused = false;
        Time.timeScale = 1f;
    }

    void Start()
    {
        playerScale = transform.localScale;

        ChangeLevel(currentLevel);
        aSource = GetComponent<AudioSource>();
        if (aSource == null) aSource = gameObject.AddComponent<AudioSource>();
        //jumpSound = Resources.Load<AudioClip>("jsound");

    }



    private void FixedUpdate()
    {


        //Raycast
        RaycastHit hit;
        Vector3 forward = transform.TransformDirection(Vector3.forward) * .5f;
        Ray ray = new Ray(transform.position, transform.forward * 40);
        //Debug.DrawRay(transform.position, forward, Color.red);
        //Debug.DrawRay(transform.position, -Vector3.up, Color.red);

        if (Physics.Raycast(ray, out hit, .5f))
        {
            if (hit.collider.CompareTag("RightWall") || hit.collider.CompareTag("LeftWall") || hit.collider.CompareTag("Ground"))
            {
                print("hit! " + hit.collider.tag);
                frontWall = true;
                // rb.velocity = Vector3.zero;
                playerBody.GetComponent<Animator>().SetBool("FrontWall", true);
                if (!dennisChangesEnabled)
                {
                    ChangeLevel(currentLevel);
                }
            }
        }
        else
        {
            playerBody.GetComponent<Animator>().SetBool("FrontWall", false);
            frontWall = false;
        }

        // Handle player velocity
        if (dennisChangesEnabled)
        {
            if (!frontWall && startMoving)
            {
                Vector3 newVelocity = rb.velocity;
                newVelocity.z = Time.fixedDeltaTime * speed;

                if (onWall)
                {
                    newVelocity.y = 0;
                    //Debug.Log("is On");
                }

                rb.velocity = newVelocity;
            }
        }
        else
        {
            //transform.Translate(Vector3.forward * Time.deltaTime * speed);
        }
        Debug.Log(grounded);
        Debug.Log(frontWall);
        if(frontWall)
        {
            Vector3 newVelocity = rb.velocity;
            newVelocity.z = Time.fixedDeltaTime * speed;
            newVelocity.y = -35;
            newVelocity.x = -2;
            playerBody.GetComponent<Animator>().SetBool("FrontWall", true);
            grounded = false;
            rb.velocity = newVelocity;
        }

        Ray checkGround = new Ray(transform.position, -transform.up);
        RaycastHit gHit;

        if (Physics.Raycast(checkGround, out gHit, 1))
        {
            if(gHit.collider.CompareTag("Ground"))
            {
                grounded = true;
                Debug.Log("i'm grounded");
            }
        }
        else
        {
            grounded = false;
        }


        if (grounded && frontWall)
        {
            if (Physics.Raycast(checkGround, out gHit, 1))
            {
                grounded = true;
                onWall = false;
                Debug.Log("i'm grounded");
            }
        }

        //if(!grounded && playerBody.GetComponent<Animator>().GetBool("")


        /*if (Physics.Raycast(transform.position, forward, 40))
        {
            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Ground"))
            {

            }
            print("hit! " + hit.collider.tag);
            frontWall = true;
            rb.velocity = Vector3.zero;

        }
        else
        {
            frontWall = false;
        }*/

    }

    private void Update()
    {
        if (startMoving && !gameOver)
        {
            MyInput();
            WallRunInput();
            Movement();
        }

        if (this.transform.position.y < -15)
        {
            ChangeLevel(currentLevel);
        }

        //Look();
        CheckForWall();
        SonicSpeed();

        //Debug.Log(rb.velocity);
        //Debug.Log(rb.useGravity);
        if (Input.GetKeyDown(KeyCode.W))
        {
            startMoving = true;
            grounded = true;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ChangeLevel(currentLevel);
            deaths++;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeLevel(0);
            currentLevel = 0;
            aSource.volume = .2f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeLevel(1);
            currentLevel = 1;
            aSource.volume = .2f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeLevel(2);
            currentLevel = 2;
            aSource.volume = .1f;

        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ChangeLevel(3);
            currentLevel = 3;
            aSource.volume = .05f;
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            gameOver = true;
        }

        if (!frontWall && startMoving)
        {
            // if (!dennisChangesEnabled)
            // {
            //     transform.Translate(Vector3.forward * Time.deltaTime * speed);
            // }

            if (grounded)
            {
                playerBody.GetComponent<Animator>().SetBool("Running", true);
            }

        }
        if (PauseScript.GameIsPaused && Cursor.visible == false)
        {
            UnlockCursor();
        }
        else if (!PauseScript.GameIsPaused && Cursor.visible == true)
        {
            LockCursor();
        }
        /*if(!grounded)
        {
            playerBody.GetComponent<Animator>().SetBool("Running", false);
            if(startMoving)
            {
                playerBody.GetComponent<Animator>().SetBool("Jumping", true);
            }
            
        }*/
        if (currentLevel == 4)
        {
            gameOver = true;
        }
        if (gameOver)
        {

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    
    public void QuitGame()
    {
        Application.Quit();
    }
    public void ChangeLevel(int i)
    {

        GameObject o = Spawns[i];
        rb.velocity = Vector3.zero;
        transform.position = o.transform.position;
        death = false;
        startMoving = false;
        justPortaled = false;
        grounded = true;



        timesJumped = 0;
        //PortalAnimator(i);


        ChangeMusic(music[i]);



        playerBody.GetComponent<Animator>().SetBool("Running", false);
        playerBody.GetComponent<Animator>().SetBool("Jumping", false);
        playerBody.GetComponent<Animator>().SetBool("RightWall", false);
        playerBody.GetComponent<Animator>().SetBool("LeftWall", false);
        // if (currentLevel == 0 || currentLevel == 3)
        // {
        //     speed = 8;
        // }
        // if (currentLevel == 1)
        // {
        //     speed = 8;
        // }
        // if (currentLevel == 2)
        // {
        //     speed = 8;
        // }

        if (i == 4)
        {
            gameOver = true;
        }
    }

    public void PortalAnimator(int i)
    {
        GameObject o = Portals[i];
        if (i != 0)
        {
            GameObject e = Portals[i - 1];
            e.GetComponent<Animator>().SetBool("IsOn", false);
        }

        o.GetComponent<Animator>().SetBool("IsOn", true);



    }

    /// <summary>
    /// Find user input. Should put this in its own class but im lazy
    /// </summary>
    private void MyInput()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        jumping = Input.GetButton("Jump");
        //crouching = Input.GetKey(KeyCode.LeftShift);

        //Crouching
        /*if (Input.GetKeyDown(KeyCode.LeftShift))
            StartCrouch();
        if (Input.GetKeyUp(KeyCode.LeftShift))
            StopCrouch();*/

        //Double Jumping


        if (Input.GetButtonDown("Jump") && ((grounded || (timesJumped < 2 && onWall)) && startMoving))
        {
            Jump();
            playerBody.GetComponent<Animator>().SetBool("Jumping", true);
            doubleJumpsLeft--;
            timesJumped++;
            //aSource.PlayOneShot(jumpSound);

            if (dennisChangesEnabled)
            {
                onWall = false;
            }
        }

        //Dashing
        /*if (Input.GetKeyDown(KeyCode.W) && wTapTimes <= 1)
        {
            wTapTimes++;
            Invoke("ResetTapTimes", 0.3f);
        }
        if (wTapTimes == 2 && readyToDash) Dash();*/

        //SideFlash
        if (Input.GetKeyDown(KeyCode.Mouse1) && flashesLeft > 0 && x > 0) SideFlash(true);
        if (Input.GetKeyDown(KeyCode.Mouse1) && flashesLeft > 0 && x < 0) SideFlash(false);

        //RocketFlight
        if (Input.GetKeyDown(KeyCode.LeftControl) && readyToRocket)
        {
            //Dampens velocity
            rb.velocity = rb.velocity / 3;
        }
        if (Input.GetKey(KeyCode.LeftControl) && readyToRocket)
            StartRocketBoost();

        //Climbing
        if (Physics.Raycast(transform.position, orientation.forward, 1, whatIsLadder) && y > .9f)
            Climb();
        else alreadyStoppedAtLadder = false;
    }

    private void ResetTapTimes()
    {
        wTapTimes = 0;
    }

    private void StartCrouch()
    {
        transform.localScale = crouchScale;
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        if (rb.velocity.magnitude > 0.5f)
        {
            if (grounded)
            {
                rb.AddForce(orientation.transform.forward * slideForce);
            }
        }
    }

    private void StopCrouch()
    {
        transform.localScale = playerScale;
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
    }

    private void Movement()
    {
        //Extra gravity
        //Needed that the Ground Check works better!
        float gravityMultiplier = 10f;

        if (crouching) gravityMultiplier = crouchGravityMultiplier;

        rb.AddForce(Vector3.down * Time.deltaTime * gravityMultiplier);

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(x, y, mag);

        //If holding jump && ready to jump, then jump
        if (readyToJump && jumping && grounded && !rocketActive) Jump();

        //ResetStuff when touching ground
        if (grounded)
        {
            readyToDash = true;
            readyToRocket = true;
            doubleJumpsLeft = startDoubleJumps;
        }

        //Set max speed
        float maxSpeed = this.maxSpeed;

        //If sliding down a ramp, add force down so player stays grounded and also builds speed
        if (crouching && grounded && readyToJump)
        {
            rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (x > 0 && xMag > maxSpeed) x = 0;
        if (x < 0 && xMag < -maxSpeed) x = 0;
        if (y > 0 && yMag > maxSpeed) y = 0;
        if (y < 0 && yMag < -maxSpeed) y = 0;

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;

        // Movement in air
        if (!grounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }

        // Movement while sliding
        if (grounded && crouching) multiplierV = 0f;

        //Apply forces to move player
        //rb.AddForce(orientation.transform.forward * y * moveSpeed * Time.deltaTime * multiplier * multiplierV);
        rb.AddForce(orientation.transform.right * x * moveSpeed * Time.deltaTime * multiplier);
    }

    private void Jump()
    {
        if (grounded)
        {
            readyToJump = false;

            //Add jump forces
            rb.AddForce(Vector2.up * jumpForce * 1.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);

            //If jumping while falling, reset y velocity.
            Vector3 vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.velocity.y > 0)
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

            Invoke(nameof(ResetJump), jumpCooldown);
        }
        if (!grounded)
        {
            readyToJump = false;

            //Add jump forces
            rb.AddForce(orientation.forward * jumpForce * 1f);
            rb.AddForce(Vector2.up * jumpForce * 1.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);

            //Reset Velocity
            rb.velocity = Vector3.zero;

            //Disable dashForceCounter if doublejumping while dashing
            allowDashForceCounter = false;

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //Walljump
        if (isWallRunning)
        {
            readyToJump = false;

            //normal jump
            if (isWallLeft && !Input.GetKey(KeyCode.D) || isWallRight && !Input.GetKey(KeyCode.A))
            {
                rb.AddForce(Vector2.up * jumpForce * 1.5f);
                rb.AddForce(normalVector * jumpForce * 0.5f);
            }

            //sidwards wallhop
            if (isWallRight || isWallLeft && Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) rb.AddForce(-orientation.up * jumpForce * 1f);
            if (isWallRight && Input.GetKey(KeyCode.A)) rb.AddForce(-orientation.right * jumpForce * 3.2f);
            if (isWallLeft && Input.GetKey(KeyCode.D)) rb.AddForce(orientation.right * jumpForce * 3.2f);


            //Always add forward force
            rb.AddForce(orientation.forward * jumpForce * 1f);

            //Disable dashForceCounter if doublejumping while dashing
            allowDashForceCounter = false;

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    /*private void Dash()
    {
        //saves current velocity
        dashStartVector = orientation.forward;

        allowDashForceCounter = true;

        readyToDash = false;
        wTapTimes = 0;

        //Deactivate gravity
        rb.useGravity = false;

        //Add force
        rb.velocity = Vector3.zero;
        rb.AddForce(orientation.forward * dashForce);

        Invoke("ActivateGravity", dashTime);
    }*/
    private void ActivateGravity()
    {
        rb.useGravity = true;

        //Counter currentForce
        if (allowDashForceCounter)
        {
            rb.AddForce(dashStartVector * -dashForce * 0.5f);
        }
    }
    private void SonicSpeed()
    {
        //If running builds up speed
        if (grounded && y >= 0.99f)
        {
            timePassedSonic += Time.deltaTime;
        }
        else
        {
            timePassedSonic = 0;
            maxSpeed = startMaxSpeed;
        }

        if (timePassedSonic >= timeBetweenNextSonicBoost)
        {
            if (maxSpeed <= maxSonicSpeed)
            {
                maxSpeed += 5;
                rb.AddForce(orientation.forward * Time.deltaTime * sonicSpeedForce);
            }
            timePassedSonic = 0;
        }
    }
    private void SideFlash(bool isRight)
    {
        RaycastHit hit;
        //Flash Right
        if (Physics.Raycast(orientation.position, orientation.right, out hit, flashRange) && isRight)
        {
            transform.position = hit.point;
        }
        else if (!Physics.Raycast(orientation.position, orientation.right, out hit, flashRange) && isRight)
            transform.position = new Vector3(transform.position.x + flashRange, transform.position.y, transform.position.z);

        //Flash Left
        if (Physics.Raycast(orientation.position, -orientation.right, out hit, flashRange) && !isRight)
        {
            transform.position = hit.point;
        }
        else if (!Physics.Raycast(orientation.position, -orientation.right, out hit, flashRange) && !isRight)
            transform.position = new Vector3(transform.position.x - flashRange, transform.position.y, transform.position.z);

        //Dampen falldown
        Vector3 vel = rb.velocity;
        if (rb.velocity.y < 0.5f && !alreadyStoppedAtLadder)
        {
            rb.velocity = new Vector3(vel.x, 0, vel.z);
        }

        flashesLeft--;
        if (!alreadySubtractedFlash)
        {
            Invoke("ResetFlash", flashCooldown);
            alreadySubtractedFlash = true;
        }
    }
    private void ResetFlash()
    {
        alreadySubtractedFlash = false;
        Invoke("ResetFlash", flashCooldown);

        if (flashesLeft < maxFlashesLeft)
            flashesLeft++;
    }
    private void StartRocketBoost()
    {
        if (!alreadyInvokedRockedStop)
        {
            Invoke("StopRocketBoost", maxRocketTime);
            alreadyInvokedRockedStop = true;
        }

        rocketTimer += Time.deltaTime;

        rocketActive = true;

        //Disable dashForceCounter if doublejumping while dashing
        allowDashForceCounter = false;

        /*Boost all Forces
        Vector3 vel = velocityToBoost;
        Vector3 velBoosted = vel * rocketBoostMultiplier;
        rb.velocity = velBoosted;
        */

        //Boost forwards and upwards
        rb.AddForce(orientation.forward * rocketForce * Time.deltaTime * 1f);
        rb.AddForce(Vector3.up * rocketForce * Time.deltaTime * 2f);

    }
    private void StopRocketBoost()
    {
        alreadyInvokedRockedStop = false;
        rocketActive = false;
        readyToRocket = false;

        if (rocketTimer >= maxRocketTime - 0.2f)
        {
            rb.AddForce(orientation.forward * rocketForce * -.2f);
            rb.AddForce(Vector3.up * rocketForce * -.4f);
        }
        else
        {
            rb.AddForce(orientation.forward * rocketForce * -.2f * rocketTimer);
            rb.AddForce(Vector3.up * rocketForce * -.4f * rocketTimer);
        }

        rocketTimer = 0;
    }
    private void Climb()
    {
        //Makes possible to climb even when falling down fast
        Vector3 vel = rb.velocity;
        if (rb.velocity.y < 0.5f && !alreadyStoppedAtLadder)
        {
            rb.velocity = new Vector3(vel.x, 0, vel.z);
            //Make sure char get's at wall
            alreadyStoppedAtLadder = true;
            rb.AddForce(orientation.forward * 500 * Time.deltaTime);
        }

        //Push character up
        if (rb.velocity.magnitude < maxClimbSpeed)
            rb.AddForce(orientation.up * climbForce * Time.deltaTime);

        //Doesn't Push into the wall
        if (!Input.GetKey(KeyCode.S)) y = 0;
    }

    private float desiredX;
    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;

        //Find current look rotation
        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        //Rotate, and also make sure we dont over- or under-rotate.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Perform the rotations
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, wallRunCameraTilt);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);

        //While Wallrunning
        //Tilts camera in .5 second
        if (Mathf.Abs(wallRunCameraTilt) < maxWallRunCameraTilt && isWallRunning && isWallRight)
            wallRunCameraTilt += Time.deltaTime * maxWallRunCameraTilt * 2;
        if (Mathf.Abs(wallRunCameraTilt) < maxWallRunCameraTilt && isWallRunning && isWallLeft)
            wallRunCameraTilt -= Time.deltaTime * maxWallRunCameraTilt * 2;

        //Tilts camera back again
        if (wallRunCameraTilt > 0 && !isWallRight && !isWallLeft)
            wallRunCameraTilt -= Time.deltaTime * maxWallRunCameraTilt * 2;
        if (wallRunCameraTilt < 0 && !isWallRight && !isWallLeft)
            wallRunCameraTilt += Time.deltaTime * maxWallRunCameraTilt * 2;
    }
    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded || jumping) return;

        //Slow down sliding
        if (crouching)
        {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }

        //Counter movement
        if (Mathf.Abs(mag.x) > threshold && Mathf.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Mathf.Abs(mag.y) > threshold && Mathf.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }

        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > maxSpeed)
        {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    /// </summary>
    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;

    /// <summary>
    /// Handle ground detection
    /// </summary>
    /*private void OnCollisionStay(Collision other)
    {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal))
            {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }*/

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Ground")
        {
            grounded = true;
            playerBody.GetComponent<Animator>().SetBool("Jumping", false);
            timesJumped = 0;
        }

        if (other.gameObject.tag == "LeftWall")
        {
            onWall = true;
            if (justRightWalled && timesJumped == 2)
            {
                timesJumped = 1;
            }
            playerBody.GetComponent<Animator>().SetBool("LeftWall", true);
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
            playerBody.GetComponent<Animator>().SetBool("RightWall", true);
            justLeftWalled = false;
            justRightWalled = true;
        }

        if (other.gameObject.tag == "Death")
        {
            if (deathAllowed)
            {
                death = true;
                ChangeLevel(currentLevel);
                deaths++;
            }
        }

        if (other.gameObject.tag == "Portal")
        {
            Debug.Log("just hit Portal");
            if (currentLevel == 3)
            {
                gameOver = true;
            }
            if(currentLevel != 3)
            {
                currentLevel++;
                ChangeLevel(currentLevel);
            }
        }
    }

    public void ChangeMusic(AudioClip music)
    {
        aSource.Stop();
        aSource.clip = music;
        aSource.Play();
    }

    void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag == "Ground")
        {
            grounded = false;
        }

        if (other.gameObject.tag == "LeftWall")
        {
            if (dennisChangesEnabled)
            {
                onWall = false;
            }
            //StopWallRun();
            ActivateGravity();
            justLeftWalled = true;
            justRightWalled = false;
            playerBody.GetComponent<Animator>().SetBool("LeftWall", false);
        }

        if (other.gameObject.tag == "RightWall")
        {
            if (dennisChangesEnabled)
            {
                onWall = false;
            }
            //StopWallRun();
            ActivateGravity();
            justRightWalled = true;
            justLeftWalled = false;
            playerBody.GetComponent<Animator>().SetBool("RightWall", false);
        }


        /*if (other.gameObject.tag == "Death")
        {
            death = false;
        }*/
    }

    private void StopGrounded()
    {
        grounded = false;
    }
}
