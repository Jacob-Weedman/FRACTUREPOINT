using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text;

//Designed by Jacob Weedman
//Use on switchblade drone enemies

public class SwitchbladeAI : MonoBehaviour
{
    //MISC
    public GameObject target;
    public float nextWaypointDistance = 5;
    Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;
    GameObject projectile;
    GameObject player;
    GameObject barrel;
    public List<GameObject> AllPositions;
    public float DistanceFromPlayer;
    Vector2 StartPosition;
    public List<GameObject> PatrolPositions;
    public GameObject LastKnownPlayerLocation;

    //CONDITIONS/GENERAL INFORMATION
    public bool canMove = true;
    public bool canPursue = false;
    public bool canFire = true;
    public bool currentlyReloading = false;
    public bool currentlyPatrolling;
    public bool currentlyMovingToNextPatrolTarget = false;
    public float DesiredDistance = 15;
    int MinumumDistance = 10;
    public bool targetingPlayer = false;
    public bool inFiringCycle = false;

    //ENEMY STATS (Changeable)
    float Speed = 0.7f; // In relation to player's walking speed
    float Health = 100;
    float PatrolDistance = 10;
    int PatrolStallTime = 1500; //ms
    int PlayerDetectionRange = 25;
    int WeaponFireRate = 1000;
    int WeaponRange = 30;
    int WeaponDamage = 20;
    float dashSpeed = 40f;

    Seeker seeker;
    Rigidbody2D rb;

    //START OF THE GAME
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        transform.Find("PursuingIndicator").GetComponent<SpriteRenderer>().enabled = false;

        StartPosition = transform.position;
        AllPositions = GameObject.FindGameObjectsWithTag("PossiblePositions").ToList();

        target = Instantiate(GameObject.Find("FlyingTarget"), transform.position, Quaternion.identity);

        player = GameObject.FindGameObjectWithTag("Player");
        LastKnownPlayerLocation = null;

        InvokeRepeating("UpdatePath", 0f, 0.1f);
        InvokeRepeating("PathfindingTimeout", 0f, 30);
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
        if (seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.transform.position, OnPathComplete);
        }
    }

    // Pathfiniding Timeout
    void PathfindingTimeout()
    {
        if (Vector2.Distance(transform.position, target.transform.position) > 0.5)
        {
            //target = gameObject;
            targetingPlayer = false;
            LastKnownPlayerLocation = null;
            MoveNextPatrol();
        }
    }

    //MAIN LOGIC
    void FixedUpdate()
    {
        // Death
        if (Health <= 0)
        {
            Explode();
        }

        // See where the enemy wants to go (DEBUGGING ONLY)
        if (target != null)
        {
            foreach (GameObject pos in AllPositions)
            {
                pos.GetComponent<SpriteRenderer>().enabled = false;
            }
            target.GetComponent<SpriteRenderer>().enabled = true;
        }

        //CHANGE ICONS
        if (targetingPlayer)
        {
            transform.Find("PursuingIndicator").GetComponent<SpriteRenderer>().enabled = true;
        }
        else
        {
            transform.Find("PursuingIndicator").GetComponent<SpriteRenderer>().enabled = false;
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

//DIVE BOMB
        // Check if enemy has line of sight on the player & if they are in the acceptable range               
        if (canFire == true && currentlyReloading == false && currentlyPatrolling == false)
        {
            if (DetermineLineOfSight(gameObject, player) == true && inFiringCycle == false && Vector2.Distance(transform.position, player.transform.position) <= WeaponRange)
            {
                UseWeapon();
            }
        }

        //MOVEMENT
        if (canMove == true)
        {
            Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
            Vector2 force = direction * Speed * 20;

            rb.AddForce(force);

            // A* logic
            float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

            if (distance < nextWaypointDistance)
            {
                currentWaypoint++;
            }
        }

        //PATROL
        // Enemy detects player
        if (DetermineLineOfSight(gameObject, player) == true && Vector2.Distance(transform.position, player.transform.position) <= PlayerDetectionRange)
        {
            currentlyPatrolling = false;
            targetingPlayer = true;
            canPursue = true;

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
                target.transform.position = LastKnownPlayerLocation.transform.position;
            }
            if (Vector2.Distance(transform.position, LastKnownPlayerLocation.transform.position) < 0.5 && DetermineLineOfSight(player, gameObject) == false)
            {
                targetingPlayer = false;
                LastKnownPlayerLocation = null;
            }
        }
        // Go back to patrol move
        else
        {
            LastKnownPlayerLocation = gameObject;
            currentlyPatrolling = true;
            targetingPlayer = false;


        }

        // Call patrol method
        if (currentlyPatrolling)
        {
            if (canMove == true && currentlyMovingToNextPatrolTarget == false)
            {
                MoveNextPatrol();
            }
        }

//TARGET DETERMINATION
        if (canPursue == true)
        {
            /*
            // If the player moves away (CHANGE TARGET)
            if (Vector2.Distance(target.transform.position, player.transform.position) > DesiredDistance)
            {
                ComputeClosestPositionToPlayer();
            }
            */

            //If the player is too close to the target (CHANGE TARGET)
            if (Vector2.Distance(target.transform.position, player.transform.position) < MinumumDistance)
            {
                ComputeClosestPositionToPlayer();
            }
            

            // If the player is not within line of sight of the desired position (CHANGE TARGET)
            if (DetermineLineOfSight(target, player) == false)
            {
                ComputeClosestPositionToPlayer();
            }

            // If the enemy reaches the target
            if (Vector2.Distance(target.transform.position, transform.position) <= 1)
            {
                if (DetermineLineOfSight(gameObject, player) == false) // If the enemy has LOS on the player
                {
                    ComputeClosestPositionToPlayer();
                }
            }
        }
    }

    //MISC METHODS
    // Attempt to explode on the player
    async Task UseWeapon()
    {
        inFiringCycle = true;

        await Task.Delay(3000); // Dash delay ms

        //Dash into player
        rb.linearVelocity = new Vector2(transform.position.x - player.transform.position.x, transform.position.y - player.transform.position.y).normalized * dashSpeed * -1;

        inFiringCycle = false;
    }

    // Part of the patrol cycle
    async Task MoveNextPatrol()
    {
        LastKnownPlayerLocation = gameObject;
        target.transform.position = transform.position;

        canPursue = false;
        currentlyMovingToNextPatrolTarget = true;

        //Find Random Position nearby
        target.transform.position = new Vector2((StartPosition.x + UnityEngine.Random.Range(-1 * PatrolDistance, PatrolDistance)), (StartPosition.y + UnityEngine.Random.Range(-1 * PatrolDistance, PatrolDistance)));
        await Task.Delay(PatrolStallTime + UnityEngine.Random.Range((PatrolStallTime * -1), PatrolStallTime));

        currentlyMovingToNextPatrolTarget = false;
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

    // Part of the pursue cycle
    void ComputeClosestPositionToPlayer()
    {
        canMove = true;
        // find a target in the air a set radius away from the player
        if (DetermineLineOfSight(player, gameObject))
        {
            target.transform.position = new Vector2(player.transform.position.x + UnityEngine.Random.Range(-MinumumDistance, MinumumDistance), player.transform.position.y + MinumumDistance);
        }
    }

    // Explode
    void Explode()
    {
        //Damage
        GameObject Explosion;
        Explosion = Instantiate(GameObject.Find("Explosion"), new Vector3(transform.position.x, transform.position.y, GameObject.Find("Explosion").transform.position.z), Quaternion.identity);
        Explosion.transform.rotation = Quaternion.Euler(Vector3.forward);

        //Set Variables
        Explosion.GetComponent<ParticleWeapon>().destroy = true;
        Explosion.GetComponent<ParticleWeapon>().opacity = true;
        Explosion.GetComponent<ParticleWeapon>().damageAmmount = WeaponDamage;

        //Delete Enemy
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Explode();
        }

        if (collision.gameObject.tag == "Ground")
        {
            if (collision.gameObject.layer == 6)
            {
                Explode();
            }
        }
    }
}