using JetBrains.Annotations;
using System;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using static UnityEngine.Rendering.DebugUI;

public class playerMovement : MonoBehaviour
{

    private GroundCheck groundCheck; //Allows Use Of GroundCheck Script Variables

    private Rigidbody2D body;
    private BoxCollider2D hBox;

    private bool allowedToMove = true, allowedToJump = true, doubleJumpUnlocked = true; //Unlocking & Locking Abilities
    private bool isGrounded, canJumpAgain; //Physics Checks
    private float CoyoteTimer, BufferTimer, airjumpTurnTimer, turnaround; //Timers & Calculation Variables 
    private float moveSpeed = 10f, groundJumpSpeed = 18f, airJumpSpeed = 14f, turnResponsiveness = 4f; //Physics Values
    private float coyoteTimeAmount = .1f, airjumpTurnaroundTimeAmount = 1.5f, jumpBufferAmount = .2f; //Leniency Velues

    private KeyCode hop = KeyCode.W, crouch = KeyCode.S, left = KeyCode.A, right = KeyCode.D; //Controls


    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        hBox = transform.GetComponent<BoxCollider2D>();
        body.gravityScale = 4;
    }

    void Update()
    {


        TimerHandler();

        GroundCheck();
        
        Movement();

        
    }





    void GroundCheck() {
        
        groundCheck = GetComponentInChildren<GroundCheck>(); // Ground Check
        isGrounded = groundCheck.isGrounded;

        if (doubleJumpUnlocked && isGrounded) canJumpAgain = true; //Realoading Double Jump
    
    } //Ground & Double Jump Checks

    void TimerHandler() 
    {
        if (isGrounded && !(body.linearVelocity.y > 0)) CoyoteTimer = coyoteTimeAmount; //Coyote Time
        else CoyoteTimer -= Time.deltaTime;

        if (Input.GetKeyDown(hop)) BufferTimer = jumpBufferAmount; //Jump Buffering
        if (BufferTimer > 0) BufferTimer -= Time.deltaTime;

        if (airjumpTurnTimer > 0) airjumpTurnTimer -= Time.deltaTime; //Double Jump Physics
    } //Input Leniency & Special Input Time Windows

    void Movement() { 

        if (allowedToMove)
        {
            if (!(Input.GetKey(left) && Input.GetKey(right)))
            {

                if (!isGrounded) turnaround = turnResponsiveness * .75f;
                else turnaround = turnResponsiveness;

                if (Input.GetKey(left))
                { 
                        body.linearVelocity += new Vector2(-moveSpeed * turnaround * Time.deltaTime, 0);
                        body.linearVelocity = new Vector2(Mathf.Clamp(body.linearVelocity.x, -moveSpeed, moveSpeed), body.linearVelocity.y);
                }

                if (Input.GetKey(right))
                {
                        body.linearVelocity += new Vector2(moveSpeed * turnaround * Time.deltaTime, 0);
                        body.linearVelocity = new Vector2(Mathf.Clamp(body.linearVelocity.x, -moveSpeed, moveSpeed), body.linearVelocity.y);
                } 
            } //Walking

            if ( !(Input.GetKey(left) || Input.GetKey(right)) || (Input.GetKey(left) && Input.GetKey(right))) 
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x * .985f, body.linearVelocity.y);
            } //Custom friction
        } //Horizontal Movement & Physics


        if (allowedToJump)
        {
            if ((CoyoteTimer > 0 && BufferTimer > 0)) 
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, groundJumpSpeed);
                BufferTimer = 0;
                CoyoteTimer = 0;
            } //Normal jump is performed


            else if (canJumpAgain && Input.GetKeyDown(hop))
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, airJumpSpeed);
                canJumpAgain = false;

                airjumpTurnTimer = airjumpTurnaroundTimeAmount;
            } //Air jump is performed


            if (!Input.GetKey(hop) && body.linearVelocity.y > 0) 
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x, body.linearVelocity.y * .92f);
            } //Jump Released

        } //Vertical Movement & Physics

    } //Check Player Input
}
