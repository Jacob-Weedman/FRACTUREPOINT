using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    private KeyCode hop = KeyCode.W, crouch = KeyCode.S, left = KeyCode.A, right = KeyCode.D, forward;
    #endregion
    #region Timers
    private float coyoteTimeAmount = .1f, airjumpTurnaroundWindow = .1f, jumpBufferAmount = .2f, wallStallAmount = .15f, dashWindowAmount = .25f, attackTimerAmount = .3f, scrollSpeed = .15f;
    private float CoyoteTimer, BufferTimer, airjumpTurnTimer, wallStallTimer, dashCooldownTimer, dashLeftTimer, dashRightTimer, attackTimer, scrollTimer;
    private float tempMaxSpeedTimer;
    [SerializeField] private float dashCooldownAmount = 3f;
    #endregion
    #region Player Booleans
    private bool allowedToWalk = true, allowedToJump = true, allowedToWallSlide = true, allowedToDash = true, speedClampingActive = true, canScroll = true;
    private bool isGrounded, isWallSlide, canJumpAgain, isFacingRight = true, canDash, dashingLeft, dashingRight, alreadyAirDashed;
    #endregion
    #region Player Floats
    private float defaultMaxSpeed = 10f, defaultMoveSpeed = 10f, defaultJumpPower = 18f, defaultDashPower = 100f, defaultTurnResponsiveness = 4f, defaultExtraJumps = 1f; //Permanent Upgrades Will Affect These
    private float tempMaxSpeed = 10f, tempMoveSpeed = 10f, tempJumpPower = 18f, tempDashPower = 100f, tempTurnResponsiveness = 4f, tempExtraJumps; //Temporary Upgrades/Debuffs Will Affect These
    private float velocityDirection = 1, turnaround;
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
        #endregion
    }

    void Update()
    {

        TimerHandler();

        StateCheck();

        Movement();

        CheckFaceDirection();

        Combat();

    }


    void CheckFaceDirection()
    {

        if (body.linearVelocity.x > 0 && !isFacingRight)
        {
            forward = right;
            Flip();
        }
        if (body.linearVelocity.x < 0 && isFacingRight)
        {
            forward = left;
            Flip();
        }

        void Flip()
        {
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
        if (wallCheck.touchingWall && !isGrounded && Input.GetKey(forward)) isWallSlide = true;
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
    }

    void TimerHandler()
    {
        #region Coyote Time
        if (isGrounded && !(body.linearVelocity.y > 0)) CoyoteTimer = coyoteTimeAmount;
        if (CoyoteTimer > 0) CoyoteTimer -= Time.deltaTime;
        #endregion
        #region Jump Buffering
        if (Input.GetKeyDown(hop)) BufferTimer = jumpBufferAmount;
        if (BufferTimer > 0) BufferTimer -= Time.deltaTime;
        #endregion
        #region Window To Change Directions After Air Jump
        if (airjumpTurnTimer > 0) airjumpTurnTimer -= Time.deltaTime;
        #endregion
        #region Wall Stall
        if (!isWallSlide || body.linearVelocity.y >= 0) wallStallTimer = wallStallAmount;
        if (wallStallTimer > 0) wallStallTimer -= Time.deltaTime;
        #endregion
        #region Dash Cooldown      
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;
        if (dashCooldownTimer <= 0) canDash = true;
        else canDash = false;
        #endregion
        #region Dash Input Window

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
        #endregion
        #region Attack Window
        if (attackTimer >= 0) attackTimer -= Time.deltaTime;
        if (attackTimer < 0) attackHitbox.SetActive(false);
        #endregion
        #region Inventory Scrolling
        if (scrollTimer > 0) scrollTimer -= Time.deltaTime;
        if (scrollTimer > 0) canScroll = false;
        else canScroll = true;
        #endregion

        #region Resetting Speed Clamping

        if (tempMaxSpeedTimer <= 0) tempMaxSpeed = defaultMaxSpeed;
        tempMaxSpeedTimer -= Time.deltaTime;

        body.linearVelocity = new Vector2(Mathf.Clamp(body.linearVelocity.x, -tempMaxSpeed, tempMaxSpeed), body.linearVelocity.y);

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
            if (CoyoteTimer > 0 && BufferTimer > 0)
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, tempJumpPower);
                BufferTimer = 0;
                CoyoteTimer = 0;
            }
            #endregion

            #region Air Jumps
            else if (canJumpAgain && Input.GetKeyDown(hop) && !isWallSlide)
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, tempJumpPower * .8f);
                tempExtraJumps--;

                BufferTimer = 0;

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
            if (isWallSlide && BufferTimer > 0)
            {
                body.linearVelocity = new Vector2(tempMoveSpeed * -velocityDirection, .74f * tempJumpPower);
                body.linearVelocity = new Vector2(Mathf.Clamp(body.linearVelocity.x, -tempMaxSpeed, tempMaxSpeed), body.linearVelocity.y);
                BufferTimer = 0;
            }
            #endregion
        }

        if (allowedToDash)
        {
            #region Left Dash
            if (dashingLeft)
            {
                tempMaxSpeed = tempDashPower;
                tempMaxSpeedTimer = .03f;
                body.linearVelocity = new Vector2(-tempDashPower, body.linearVelocity.y);

                dashingLeft = false;
                dashCooldownTimer = dashCooldownAmount;
                alreadyAirDashed = true;
            }
            #endregion

            #region Right Dash
            if (dashingRight)
            {
                tempMaxSpeed = tempDashPower;
                tempMaxSpeedTimer = .03f;
                body.linearVelocity = new Vector2(tempDashPower, 0);

                dashingRight = false;
                dashCooldownTimer = dashCooldownAmount;
                alreadyAirDashed = true;
            }
            #endregion
        }

    }

    public void Combat()
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
        }
        #endregion

        #region Using Weapons

        if (Input.GetMouseButtonDown(0))
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
                    attackTimer = attackTimerAmount;
                    Debug.Log("swing");
                    break;

                case "Gun":
                    GetComponent<ShootScript>().Shoot("Bullet", 40, 10);
                    break;
                    #endregion
            }


        }
        #endregion


    }



}
