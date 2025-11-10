using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text;

//Designed by Jacob Weedman
//Use on Cloaker type enemies

public class CloakerAI : MonoBehaviour
{

//MISC
GameObject target;
    public float nextWaypointDistance = 5;
    Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;
    GameObject projectile;
    GameObject player;
    public List<GameObject> SuitablePositions;
    public List<GameObject> AllPositions;
    float DistanceFromPlayer;
    Vector2 StartPosition;
    public List<GameObject> PatrolPositions;
    GameObject LastKnownPlayerLocation;

//CONDITIONS/GENERAL INFORMATION
    bool canJump = true;
    bool canMove = true; // Prevent all movement
    bool canPursue = false; // Follow player
    bool canFire = false;
    bool currentlyReloading = false;
    bool targetingPlayer = false;
    bool inFiringCycle = false;
    bool canDash = true;

//ENEMY STATS (Changeable)
    public float Speed = 0.7f; // In relation to player's walking speed
    public float JumpHeight = 1.4f; // In relation to player's regular jump height
    public float Health = 15;
    public int PlayerDetectionRange = 25;
    public float HiddenTransparencyAmmount = 0.05f;
    public float dashSpeed = 40f; // Strength of the dash

//WEAPON STATS (CHANGEABLE)
    public int WeaponDamage = 40; // Damage per hit
    public int WeaponFireRate = 1500; // Delay in time between attacks both melee and ranged
    public float WeaponMeleeRange = 0.75f;

//REFERENCES
    Seeker seeker;
    Rigidbody2D rb;

//ONCE THE GAME STARTS
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        transform.Find("PursuingIndicator").GetComponent<SpriteRenderer>().enabled = false;

        StartPosition = transform.position;
        AllPositions = GameObject.FindGameObjectsWithTag("PossiblePositions").ToList();
        
        projectile = GameObject.Find("EnemyProjectile");
        player = GameObject.FindGameObjectWithTag("Player");
        LastKnownPlayerLocation = null;
        target = gameObject;

        InvokeRepeating("UpdatePath", 0f, 0.1f);
        InvokeRepeating("PathfindingTimeout", 0f, 10);

    }

    // When enemy has reached the next node
    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    // Select next node
    void UpdatePath()
    {
        if (seeker.IsDone() && target != null)
        {
            seeker.StartPath(rb.position, target.transform.position, OnPathComplete);
        }
    }

    // Pathfiniding Timeout
    void PathfindingTimeout()
    {
        if (Vector2.Distance(transform.position, target.transform.position) > 0.5)
        {
            LastKnownPlayerLocation = null;
            targetingPlayer = false;
            target = gameObject;
        }
    }

//MAIN LOGIC
    void FixedUpdate()
    {
        
//DEATH
        if (Health <= 0)
        {
            GameObject DeadBody;
            DeadBody = Instantiate(gameObject, transform.position, Quaternion.identity);
            Destroy(GameObject.Find(DeadBody.name).GetComponent<TestEnemyAIGroundRanged>());
            Destroy(GameObject.Find(DeadBody.name).GetComponent<Seeker>());

        foreach (Transform child in DeadBody.transform) {
            GameObject.Destroy(child.gameObject);
        }
            Destroy(gameObject);
        }

//ICONS
        if (targetingPlayer && gameObject.GetComponent<SpriteRenderer>().color.a == 1)
        {
            transform.Find("PursuingIndicator").GetComponent<SpriteRenderer>().enabled = true;
        }
        else
        {
            transform.Find("PursuingIndicator").GetComponent<SpriteRenderer>().enabled = false;
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f, HiddenTransparencyAmmount);
        }

//MISC PATHFINDING
        if (path == null)
            return;

        if (target == null)
            return;
        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

//ATTACK
        // Check if enemy has line of sight on the player & if they are in the acceptable range       
        if (canFire == true && currentlyReloading == false)
        {
            if (DetermineLineOfSight(gameObject, player) == true && Vector2.Distance(player.transform.position, transform.position) <= WeaponMeleeRange)
            {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f, 1f);
            UseWeapon();
            }
        }
//CALL DASH
        if (DetermineLineOfSight(gameObject, player) && targetingPlayer && canDash == true)
        {
            Dash();
        }
        
//MOVEMENT
        if (canMove == true)
        {
            canFire = false;

            Vector2 direction = (Vector2)path.vectorPath[currentWaypoint] - rb.position;

            if (direction.x > 0) // Move right
            {
                rb.AddForce(new Vector2(Speed * 20, rb.linearVelocity.y));
            }

            if (direction.x < 0) // Move left
            {
                rb.AddForce(new Vector2(-1 * Speed * 20, rb.linearVelocity.y));
            }

            if (direction.y > 1f) // Wants to jump
            {
                JumpMethod();
            }

            // A* logic
            float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

            if (distance < nextWaypointDistance)
            {
                currentWaypoint++;
            }
        }

//GROUND DETECTION
        //Detecting if the enemy has reached the ground
        if (GetComponentInChildren<GroundCheck>().isGrounded == true && rb.linearVelocity.y == 0)
        {
            canJump = true;
            if (inFiringCycle == false)
            {
                canFire = true;
            }
        }
        else
        {
            canJump = false;
            canFire = false;
        }

//DETECTION    
        // Enemy detects player
        if (DetermineLineOfSight(gameObject, player) == true && Vector2.Distance(transform.position, player.transform.position) <= PlayerDetectionRange)
        {
            targetingPlayer = true;
            canPursue = true;

            target = player;
            // Get the last know player location by finding which of the positions is closest to the player
            if (LastKnownPlayerLocation == null)
            {
                LastKnownPlayerLocation = gameObject;
            }
            foreach (GameObject pos in AllPositions)
            {
                if (Vector2.Distance(player.transform.position, pos.transform.position) < Vector2.Distance(player.transform.position, LastKnownPlayerLocation.transform.position))
                {
                    LastKnownPlayerLocation = pos;
                }
            }

        }
        // Player has broken line of sight and the enemy will attempt to move to the last known location
        else if (LastKnownPlayerLocation != null)
        {
            if (Vector2.Distance(transform.position, LastKnownPlayerLocation.transform.position) > 0.5)
            {
                canPursue = false;
                target = LastKnownPlayerLocation;
            }
            if (Vector2.Distance(transform.position, LastKnownPlayerLocation.transform.position) < 0.5 && DetermineLineOfSight(player, gameObject) == false)
            {
                targetingPlayer = false;
                LastKnownPlayerLocation = null;
            }


        }
        
//TARGET DETERMINATION
        if (canPursue == true)
        {
            // If the enemy reaches the target
            if (Vector2.Distance(target.transform.position, transform.position) <= 1 && DetermineLineOfSight(gameObject, player))
            {
                if (canFire == false && inFiringCycle == false)
                {
                    canFire = true;
                }
            }
        }
    }

//MISC METHODS
    // Perform jump
    async Task JumpMethod()
    {
        if (canJump == true)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpHeight * 12);
            canJump = false;
            await Task.Delay(500);
        }
    }

    // Dash
    async Task Dash()
    {
        canDash = false;

        // Perform dash
        rb.linearVelocity = new Vector2(transform.position.x - player.transform.position.x, transform.position.y - player.transform.position.y).normalized * dashSpeed * -1;

        await Task.Delay(1000); // Dash delay ms

        canDash = true;
    }
    
    // Use Ranged Weapon
    async Task UseWeapon()
    {
        canFire = false;
        inFiringCycle = true;

        // melee attack
        //Damage
        GameObject.Find("GameData").GetComponent<GameData>().CurrentHealth -= WeaponDamage;

        await Task.Delay(WeaponFireRate);

        canFire = true;
        inFiringCycle = false;
    }

    // General Utility
    bool DetermineLineOfSight(GameObject object1, GameObject object2)
    {
        Vector3 RaycastStart = object1.transform.position;
        Vector3 RaycastDirection = (object2.transform.position - object1.transform.position).normalized;
        float RaycastDistance = Vector3.Distance(object2.transform.position, object1.transform.position);

        if (Physics2D.Raycast(RaycastStart, RaycastDirection, RaycastDistance, LayerMask.GetMask("SolidGround")) == false)
        {
            Debug.DrawRay(RaycastStart, RaycastDirection * RaycastDistance);
            return true;
        }
        else
        {
            return false;
        }
    }
}