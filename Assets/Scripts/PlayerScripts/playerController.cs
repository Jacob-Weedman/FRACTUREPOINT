using System;
using System.Collections.Generic;
using UnityEngine;


public class playerController : MonoBehaviour
{

    #region Imported Variables
    private GroundCheck groundCheck;
    private DoubleCheck doubleCheck;
    private WallCheck wallCheck;
    private Rigidbody2D body;
    private Collider2D hBox;
    public GameObject attackHitbox;
    public GameObject gun;
    #endregion 
    #region Keybinds
    private KeyCode hop, up, down, left, right, dash, lightAttack, heavyAttack, ultimateAttack, forward;
    private String currentDirection;
    #endregion
    #region Timers
    private float coyoteTimeAmount = .1f, airjumpTurnaroundWindow = .1f, jumpBufferAmount = .2f, clickBufferAmount = .1f, wallStallAmount = .15f, dashWindowAmount = .25f, waveDashWindow = .1f, scrollSpeed = .15f;
    private float CoyoteTimer, jumpBufferTimer, clickBufferTimer, airjumpTurnTimer, wallStallTimer, dashCooldownTimer, dashLeftTimer, dashRightTimer, waveDashTimer, hitboxTimer, scrollTimer, reloadTimer;
    private float tempMaxSpeedTimer, dashCooldownAmount = 1;
    #endregion
    #region Player Booleans
    private bool allowedToWalk = true, allowedToJump = true, allowedToWallSlide = true, allowedToDash = true, speedClampingActive = true, canScroll = true;
    public bool isGrounded, isWallSlide, canJumpAgain, isFacingRight = true, canDash, dashingLeft, dashingRight, alreadyAirDashed, waveDashing = false;
    [SerializeField] private bool canDie = true;
    #endregion
    #region Player Floats
    private float defaultMaxSpeed = 10f, defaultMoveSpeed = 10f, defaultJumpPower = 30f, defaultDashPower = 40f, defaultTurnResponsiveness = 4f, defaultDashSpeed = 40f, defaultExtraJumps = 1f; //Permanent Upgrades Will Affect These
    private float tempMaxSpeed = 10f, tempMoveSpeed = 10f, tempJumpPower = 30f, tempDashPower = 40f, tempTurnResponsiveness = 4f, tempExtraJumps; //Temporary Upgrades/Debuffs Will Affect These
    private float velocityDirection = 1, turnaround;
    [SerializeField] private float playerHealth;
    #endregion
    #region Characters/Inventory Management
    private List<String> playerInventory;
    public int equippedIndex = 0;
    public String equippedItem, SelectedPlayer;
    #endregion


    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        hBox = transform.GetComponent<BoxCollider2D>();
        body.gravityScale = 4;

        #region Character Movesets
        switch (SelectedPlayer)
        {
            case "Cade":
                playerInventory = new List<String> { "StaffSwing", "Fireball", "DragonBreath" };
                break;

            case "Sloane":
                playerInventory = new List<String> { "Knife", "Grenade", "Rifle", "Shotgun" };
                break;

            case "Leo":
                playerInventory = new List<String> { "ChargeFist", "JunkToss", "Grabble" };
                break;

            case "Gamma":
                playerInventory = new List<String> { "ArmStab", "BlubberBomb", "BodyThrow" };
                break;

            default:
                playerInventory = new List<String> { "Melee", "Gun" };
                break;
        }
        equippedItem = playerInventory[equippedIndex];
        #endregion

        #region Character Controls
        
        switch (SelectedPlayer)
        {
            case "Cade":
            case "Sloane":
            default:
                hop = KeyCode.W;
                up = KeyCode.W;
                down = KeyCode.S; 
                left = KeyCode.A; 
                right = KeyCode.D;
                dash = KeyCode.LeftShift;
                break;

            case "Leo":
            case "Gamma":
                hop = KeyCode.Space;
                up = KeyCode.UpArrow;
                down = KeyCode.DownArrow; 
                left = KeyCode.LeftArrow; 
                right = KeyCode.RightArrow;
                dash = KeyCode.LeftShift;
                lightAttack = KeyCode.X;
                heavyAttack = KeyCode.C;
                break;
        }


        
        
        
        
        #endregion

    }

    void Update()
    {
        StateCheck();

        if (canDie && playerHealth >= 0) { 
        TimerHandler();

        Movement();

        CheckFaceDirection();

        RangedCombat();
        }
    }


    void CheckFaceDirection()
    {
        if ((body.linearVelocity.x > 0 && !isFacingRight || body.linearVelocity.x < 0 && isFacingRight) && isGrounded) Flip();
    }
    public void Flip()
    {        
        if (forward == left) forward = right;
        else if (forward == right) forward = left;
        
        isFacingRight = !isFacingRight;
        velocityDirection *= -1;

        #region Change Player Orientation
            Vector3 currentScale = transform.localScale;
            currentScale.x *= -1;
            transform.localScale = currentScale;

            Vector3 gunScale = gun.transform.localScale;
            gunScale.x *= -1;
            gunScale.y *= -1;
            gun.transform.localScale = gunScale;

            #endregion
    }

    

    void StateCheck()
    {
        #region Ground Check
        groundCheck = GetComponentInChildren<GroundCheck>();
        doubleCheck = GetComponentInChildren<DoubleCheck>();

        if (groundCheck.isGrounded && !doubleCheck.isInsideGround) isGrounded = true;
        else isGrounded = false;
        #endregion

        #region Wall Check
        wallCheck = GetComponentInChildren<WallCheck>();
        if ((wallCheck.touchingWall) && !isGrounded && Input.GetKey(forward)) isWallSlide = true;
        else isWallSlide = false;
        #endregion

        #region Reload Air Jumps
        if (isGrounded && (tempExtraJumps < defaultExtraJumps))
            tempExtraJumps = defaultExtraJumps;

        if (tempExtraJumps > 0) canJumpAgain = true;
        else canJumpAgain = false;
        #endregion

        #region Reload Dash
        if (isGrounded) alreadyAirDashed = false;
        #endregion

        #region Health Update
        playerHealth = GameObject.Find("GameData").GetComponent<GameData>().CurrentHealth;
        #endregion

        #region Most Recent Direction Input
        if (Input.GetKey(up)) currentDirection = "up";
        else if (Input.GetKey(down)) currentDirection = "down";
        else if (Input.GetKey(left) || Input.GetKey(right))
        { 
            if (Input.GetKey(left) && left != forward || Input.GetKey(right) && right != forward) currentDirection = "backward";
            if (Input.GetKey(left) && left == forward || Input.GetKey(right) && right == forward) currentDirection = "forward";
        }
        else currentDirection = "neutral";
        #endregion

    }

    void TimerHandler()
    {
        #region Physics Timers

        #region Coyote Time
        if (isGrounded && !(body.linearVelocity.y > 0)) CoyoteTimer = coyoteTimeAmount;
        if (CoyoteTimer > 0) CoyoteTimer -= Time.deltaTime;
        #endregion
        #region Jump Buffering
        if (Input.GetKeyDown(hop)) jumpBufferTimer = jumpBufferAmount;
        if (jumpBufferTimer > 0) jumpBufferTimer -= Time.deltaTime;
        #endregion
        #region Window To Change Directions After Air Jump
        if (airjumpTurnTimer > 0) airjumpTurnTimer -= Time.deltaTime;
        #endregion
        #region Wall Stall
        if (!isWallSlide || body.linearVelocity.y >= 0) wallStallTimer = wallStallAmount;
        if (wallStallTimer > 0) wallStallTimer -= Time.deltaTime;
        #endregion
        #region WaveDashing
        if (isGrounded && waveDashing) waveDashTimer -= Time.deltaTime;
        if (waveDashTimer > 0) waveDashing = true;
        else waveDashing = false;
        #endregion
        #region Resetting Speed Clamping

        if (tempMaxSpeedTimer <= 0 && waveDashing) /*tempMaxSpeed = defaultDashSpeed / 2*/;
        else if (tempMaxSpeedTimer <= 0) tempMaxSpeed = defaultMaxSpeed;

        tempMaxSpeedTimer -= Time.deltaTime;

        body.linearVelocity = new Vector2(Mathf.Clamp(body.linearVelocity.x, -tempMaxSpeed, tempMaxSpeed), body.linearVelocity.y);

        #endregion

        #endregion

        #region Controller Timers

        #region Click Buffer & Reload Timers
        if (Input.GetMouseButtonDown(0)) clickBufferTimer = clickBufferAmount;
        if (clickBufferTimer > 0) clickBufferTimer -= Time.deltaTime;
        if (reloadTimer > 0) reloadTimer -= Time.deltaTime;
        #endregion
        #region Dash Cooldown      
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;
        if (dashCooldownTimer <= 0) canDash = true;
        else canDash = false;
        #endregion
        #region Dash Input Window
        /*
        if (dashLeftTimer > 0 && Input.GetKeyDown(left) && canDash && !alreadyAirDashed) dashingLeft = true;
        if (Input.GetKeyDown(left))
        {
            dashLeftTimer = dashWindowAmount;
            dashRightTimer = 0;
        }

        if (dashRightTimer > 0 && Input.GetKeyDown(right) && canDash && !alreadyAirDashed) dashingRight = true;
        if (Input.GetKeyDown(right))
        {
            dashRightTimer = dashWindowAmount;
            dashLeftTimer = 0;
        }

        if (dashLeftTimer > 0) dashLeftTimer -= Time.deltaTime;
        if (dashRightTimer > 0) dashRightTimer -= Time.deltaTime;
        
        */
        #endregion
        #region Attack Window
        if (hitboxTimer >= 0) hitboxTimer -= Time.deltaTime;
        if (hitboxTimer < 0) attackHitbox.SetActive(false);
        #endregion
        #region Inventory Scrolling
        if (scrollTimer > 0) scrollTimer -= Time.deltaTime;
        if (scrollTimer > 0) canScroll = false;
        else canScroll = true;
        #endregion

        #endregion

        
    }

    void Movement()
    {

        if (allowedToWalk)
        {
            #region Walking
            if (!(Input.GetKey(left) && Input.GetKey(right)))
            {
                #region Control Responsiveness
                if (!isGrounded) turnaround = tempTurnResponsiveness * .75f;
                else turnaround = tempTurnResponsiveness;
                if (airjumpTurnTimer > 0) turnaround = tempTurnResponsiveness * 5f;
                #endregion

                #region Moving Left
                if (Input.GetKey(left)) body.linearVelocity += new Vector2(-tempMoveSpeed * turnaround * Time.deltaTime, 0);
                #endregion 

                #region Moving Right
                if (Input.GetKey(right)) body.linearVelocity += new Vector2(tempMoveSpeed * turnaround * Time.deltaTime, 0);
                #endregion

            }
            #endregion

            #region Friction
            if (!(Input.GetKey(left) || Input.GetKey(right)) || (Input.GetKey(left) && Input.GetKey(right)))
            {
                float fric;
                if (Math.Abs(body.linearVelocity.x) > 5f) { fric = .97f; }
                else { fric = .92f; }

                body.linearVelocity = new Vector2(body.linearVelocity.x * fric, body.linearVelocity.y);

                if (Math.Abs(body.linearVelocity.x) < .4f) body.linearVelocity = new Vector2(0, body.linearVelocity.y);

            }
            #endregion
        }

        if (allowedToJump)
        {
            #region Normal Jumps
            if (CoyoteTimer > 0 && jumpBufferTimer > 0)
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, tempJumpPower);
                jumpBufferTimer = 0;
                CoyoteTimer = 0;
                if(waveDashTimer > 0) waveDashTimer = waveDashWindow;
            }
            #endregion

            #region Air Jumps
            else if (canJumpAgain && Input.GetKeyDown(hop) && !isWallSlide)
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, tempJumpPower * .8f);
                tempExtraJumps--;

                jumpBufferTimer = 0;

                airjumpTurnTimer = airjumpTurnaroundWindow;
            }
            #endregion

            #region Releasing Jump
            if (!Input.GetKey(hop) && body.linearVelocity.y > 0)
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, body.linearVelocity.y * .985f);
            }
            #endregion
        }

        if (allowedToWallSlide)
        {
            #region Wall Sliding Friction
            if (isWallSlide && wallStallTimer > 0 && body.linearVelocity.y <= 0)
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, body.linearVelocity.y * .95f);
                body.gravityScale = 0;

            }
            else if (isWallSlide && body.linearVelocity.y <= 0) body.gravityScale = 2;
            else body.gravityScale = 4;



            #endregion

            #region Wall Jumps
            if (isWallSlide && jumpBufferTimer > 0)
            {
                body.linearVelocity = new Vector2(tempMoveSpeed * -velocityDirection, .74f * tempJumpPower);
                body.linearVelocity = new Vector2(Mathf.Clamp(body.linearVelocity.x, -tempMaxSpeed, tempMaxSpeed), body.linearVelocity.y);
                jumpBufferTimer = 0;
            }
            #endregion
        }

        if (allowedToDash)
        {            

            #region Dashing
            if (Input.GetKeyDown(dash) && canDash && !alreadyAirDashed)
            {
                tempMaxSpeed = tempDashPower;
                tempMaxSpeedTimer = .2f;

                dashCooldownTimer = dashCooldownAmount;
                alreadyAirDashed = true;

                waveDashTimer = waveDashWindow;

                if (Input.GetKey(left) && Input.GetKey(right) == false) body.linearVelocity = new Vector2(-tempDashPower, body.linearVelocity.y);
                if (Input.GetKey(right) && Input.GetKey(left) == false) body.linearVelocity = new Vector2(tempDashPower, body.linearVelocity.y);
                if (Input.GetKey(up) && Input.GetKey(down) == false) body.linearVelocity = new Vector2(body.linearVelocity.x, tempDashPower);
                if (Input.GetKey(down) && Input.GetKey(up) == false) body.linearVelocity = new Vector2(body.linearVelocity.x, -tempDashPower);
            }
            #endregion

            #region Right Dash
            if (dashingRight)
            {
                tempMaxSpeed = tempDashPower;
                tempMaxSpeedTimer = .2f;
                body.linearVelocity = new Vector2(tempDashPower, 0);

                dashingRight = false;
                dashCooldownTimer = dashCooldownAmount;
                alreadyAirDashed = true;

                waveDashTimer = waveDashWindow;
            }
            #endregion
        }

    }

    public void RangedCombat()
    {

        #region Weapon Wheel
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0 && canScroll)
        {
            int scrollDirection = Math.Sign(scrollInput);
            int nextIndex = equippedIndex + scrollDirection;
            scrollTimer = scrollSpeed;

            equippedIndex = (nextIndex % playerInventory.Count + playerInventory.Count) % playerInventory.Count;
            equippedItem = playerInventory[equippedIndex];

            reloadTimer = 0;
        }
        #endregion

        #region Shooting

        if (clickBufferTimer > 0 && reloadTimer <= 0)
        {

            switch (equippedItem)
            {

                #region Cade's Weapons
                case "StaffSwing":
                    break;

                case "Fireball":
                    break;

                case "DragonBreath":
                    break;
                #endregion

                #region Sloane's Weapons
                case "Knife":
                    break;

                case "Grenade":
                    break;

                case "Rifle":
                    break;

                case "Shotgun":
                    break;
                #endregion

                #region Leo's Weapons
                case "ChargeFist":
                    break;

                case "JunkToss":
                    break;

                case "Grabble":
                    break;
                #endregion

                #region Gamma's Weapons
                case "ArmStab":
                    break;

                case "BlubberBomb":
                    break;

                case "BodyThrow":
                    break;
                #endregion


                #region Test Weapons
                case "Melee":
                    attackHitbox.SetActive(true);
                    hitboxTimer = .2f;
                    reloadTimer = .3f;
                    Debug.Log("swing");
                    break;

                case "Gun":
                    GetComponent<ShootScript>().Shoot("Bullet", 2000f, 15f, 25f, .25f, 3, .05f);
                    reloadTimer = .5f;
                    break;
                    #endregion
            }


        }
        #endregion

    }

    public void DirectionalCombat()
    {

        if (Input.GetKeyDown(lightAttack) && reloadTimer <= 0)
        {
            switch (currentDirection) {

                case ("up"): 



                    break;

                case ("down"):

                    break;

                case ("backward"):
                   




                    break;

                case ("forward"):

                    break;

                case ("right"):

                    break;



            }


        }





        if (Input.GetKeyDown(heavyAttack) && reloadTimer <= 0)
        {
            switch (currentDirection)
            {

                case ("up"):



                    break;

                case ("down"):

                    break;

                case ("left"):

                    break;

                case ("right"):

                    break;

            }


        }



    }









}
