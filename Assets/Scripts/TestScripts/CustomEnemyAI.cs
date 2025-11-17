using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text;

//Designed by Jacob Weedman
//V 0.0.3
//DO NOT USE THIS ON ANY ENEMIES

public class CustomEnemyAI : MonoBehaviour
{

    //MISC
    public GameObject target;
    public float nextWaypointDistance = 5;
    Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;
    GameObject projectile;
    GameObject player;
    public List<GameObject> SuitablePositions;
    public List<GameObject> AllPositions;
    public float DistanceFromPlayer;
    Vector2 StartPosition;
    public List<GameObject> PatrolPositions;
    public GameObject LastKnownPlayerLocation;

    //CONDITIONS/GENERAL INFORMATION
    public bool canJump = true;
    public bool canMove = true;
    public bool canPursue = true;
    public bool canFire = false;
    public bool currentlyReloading = false;
    public bool currentlyPatrolling;
    public bool currentlyMovingToNextPatrolTarget = false;
    public float DesiredDistance;
    public float MinumumDistance = 5f;
    public bool targetingPlayer = false;

    //ENEMY STATS (Changeable)
    public float Speed = 1; // In relation to player's walking speed
    public float JumpHeight = 1; // In relation to player's regular jump height
    public float Health = 100;
    public float PatrolDistance = 10;
    public int PatrolStallTime = 2000; //ms
    public int PlayerDetectionRange = 30;

    //WEAPON STATS (CHANGEABLE)
    public string WeaponType = "RANGED"; // RANGED, MELEE, PHYSICS
    public int WeaponDamage = 15; // Damage per hit
    public int WeaponFireRate = 250; // Delay in time between attacks both melee and ranged
    public float WeaponRandomSpread = 5f; // Random direction of lanched projectiles (DOES NOT APPLY TO MELEE ATTACKS)
    public int WeaponRange = 40; // Maximum range of the projectile before it drops off (DOES NOT APPLY TO MELEE ATTACKS)
    int WeaponMeleeRange = 5; // Minimum distance the enemy has to be away from the player in order to be able to hit the player (DOES NOT APPLY TO RANGED ATTACKS)
    public float WeaponProjectileSpeed = 30f; // Speed of launched projectiles (DOES NOT APPLY TO MELEE WEAPONS)
    public int WeaponMagazineSize = 20; // Number of shots the enemy will take before having to reload
    public int WeaponReloadTime = 5000; // Time it takes to reload the magazine
    public int WeaponCurrentMagazineAmmount; // Current number of ammo in the mag (DO NOT CHANGE THIS)

    Seeker seeker;
    Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        transform.Find("ReloadingIndicator").GetComponent<SpriteRenderer>().enabled = false;
        transform.Find("PursuingIndicator").GetComponent<SpriteRenderer>().enabled = false;

        StartPosition = transform.position;
        AllPositions = GameObject.FindGameObjectsWithTag("PossiblePositions").ToList();
        foreach (GameObject pos in AllPositions)
        {
            if (Vector2.Distance(pos.transform.position, StartPosition) <= PatrolDistance)
            {
                PatrolPositions.Add(pos);
            }
        }
        target = PatrolPositions[UnityEngine.Random.Range(0, PatrolPositions.Count)];

        projectile = GameObject.Find("TestBullet");
        player = GameObject.FindGameObjectWithTag("Player");
        LastKnownPlayerLocation = gameObject;

        WeaponCurrentMagazineAmmount = WeaponMagazineSize;
        DesiredDistance = WeaponRange;

        InvokeRepeating("UpdatePath", 0f, 0.1f);
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

    //Main logic
    void FixedUpdate()
    {
        if (Health <= 0)
        {
            Destroy(transform);
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

        // Change Icons
        if (targetingPlayer)
        {
            transform.Find("PursuingIndicator").GetComponent<SpriteRenderer>().enabled = true;
        }
        else
        {
            transform.Find("PursuingIndicator").GetComponent<SpriteRenderer>().enabled = false;
        }

        if (currentlyReloading)
        {
            transform.Find("ReloadingIndicator").GetComponent<SpriteRenderer>().enabled = true;
        }
        else
        {
            transform.Find("ReloadingIndicator").GetComponent<SpriteRenderer>().enabled = false;
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

        //MOVEMENT
        if (canMove == true)
        {
            canFire = false;

            Vector2 direction = (Vector2)path.vectorPath[currentWaypoint] - rb.position;
            float xDirection = path.vectorPath[currentWaypoint].x - rb.position.x;

            if (xDirection > 0) // Move right
            {
                rb.AddForce(new Vector2(Speed * 20, rb.linearVelocity.y));
            }

            if (xDirection < 0) // Move left
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

        //Detecting if the enemy has reached the ground
        if (GetComponentInChildren<GroundCheck>().isGrounded == true)
        {
            canJump = true;
        }
        else
        {
            canJump = false;
        }

        //PATROL
        if (DetermineLineOfSight(gameObject, player) == true && Vector2.Distance(transform.position, player.transform.position) <= PlayerDetectionRange)
        {
            //target = null;
            currentlyPatrolling = false;
            targetingPlayer = true;
            canPursue = true;
            foreach (GameObject pos in AllPositions)
            {
                if (Vector2.Distance(player.transform.position, pos.transform.position) < Vector2.Distance(player.transform.position, LastKnownPlayerLocation.transform.position))
                {
                    LastKnownPlayerLocation = pos;
                }
            }
        }
        else if (Vector2.Distance(transform.position, LastKnownPlayerLocation.transform.position) > 0.5)
        {
            canPursue = false;
            if (targetingPlayer == false)
            {
                target = LastKnownPlayerLocation;
            }
        }
        
        else
        {
            //target = PatrolPositions[UnityEngine.Random.Range(0, PatrolPositions.Count)];
            LastKnownPlayerLocation = gameObject;
            currentlyPatrolling = true;
            currentlyPatrolling = true;
            targetingPlayer = false;
        }

        if (currentlyPatrolling)
        {
            if (canMove == true && currentlyMovingToNextPatrolTarget == false)
            {
                MoveNextPatrol();
            }
        }
        else
        {
            //RANGED ATTACK
            if (WeaponType == "RANGED")
            {
                // Check if enemy has line of sight on the player
                // Does the ray intersect any ground objects
                //if (Physics2D.Raycast(EnemyRaycastStart, EnemyRaycastDirection, EnemyRaycastDistance, LayerMask.GetMask("Default")))
                if (DetermineLineOfSight(gameObject, player) == true && Vector2.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) <= WeaponRange)
                {// Line of sight
                 // Check to see if the enemy is within range of the player to begin shooting
                 //if (Vector2.Distance(rb.position, GameObject.FindGameObjectWithTag("Player").transform.position) <= WeaponRange)
                 //{
                    if (WeaponCurrentMagazineAmmount > 0 && canFire == true && currentlyReloading == false)
                    {
                        UseWeapon();
                    }
                    else if (WeaponCurrentMagazineAmmount == 0 && currentlyReloading == false)
                    {
                        ReloadWeapon();
                    }
                }
                else
                {
                    canMove = true;
                }
            }
        }


        if (canPursue == true)
        {
            //TARGET DETERMINATION
            // If the player moves away (CHANGE TARGET)
            
            ComputeClosestPositionToPlayer();

            if (Vector2.Distance(target.transform.position, player.transform.position) > WeaponRange - 2)
            {
                ComputeClosestPositionToPlayer();
                canFire = false;
                canMove = true;
            }

            //If the player is too close to the target (CHANGE TARGET)
            if (Vector2.Distance(target.transform.position, player.transform.position) < MinumumDistance)
            {
                ComputeClosestPositionToPlayer();
                canFire = false;
                canMove = true;
            }

            //If the target is on the other side of the player
            if (Vector2.Distance(transform.position, player.transform.position) < Vector2.Distance(target.transform.position, player.transform.position))
            {
                ComputeClosestPositionToPlayer();
                canFire = false;
                canMove = true;
            }

            // If the player is not within line of sight of the desired position (CHANGE TARGET)
            if (DetermineLineOfSight(target, player) == false)
            {
                ComputeClosestPositionToPlayer();
                canFire = false;
                canMove = true;
            }

            // If the enemy reaches the target
            if (Vector2.Distance(target.transform.position, transform.position) < 0.5)
            {
                if (DetermineLineOfSight(gameObject, player)) // If the enemy has LOS on the player
                {
                    if (currentlyReloading == false)
                    {
                        canFire = true;
                    }
                }
                else
                {
                    ComputeClosestPositionToPlayer();
                    canFire = false;
                    canMove = true;
                }
            }

            // If the enemy has not yet reached the target
            else
            {

            }
        }
    }

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
    // Use Ranged Weapon
    async Task UseWeapon()
    {
        canFire = false;
        canMove = false;

        // Create Projectile
        GameObject BulletInstance;
        BulletInstance = Instantiate(projectile, transform.position, Quaternion.LookRotation(transform.position - GameObject.FindGameObjectWithTag("Player").transform.position + new Vector3(UnityEngine.Random.Range((-1 * WeaponRandomSpread), WeaponRandomSpread), UnityEngine.Random.Range((-1 * WeaponRandomSpread), WeaponRandomSpread), 0)));
        BulletInstance.transform.parent = transform;

        // Generate random angle
        //Vector2 ExitVelocity = BulletInstance.transform.forward * new Vector2(UnityEngine.Random.Range((-1 * WeaponRandomSpread), WeaponRandomSpread), UnityEngine.Random.Range((-1 * WeaponRandomSpread), WeaponRandomSpread)).normalized;

        // Send it on it's way
        BulletInstance.GetComponent<Rigidbody2D>().linearVelocity = BulletInstance.transform.forward * -1 * WeaponProjectileSpeed;
        BulletInstance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, BulletInstance.transform.forward) - 90));
        WeaponCurrentMagazineAmmount--;

        await Task.Delay(WeaponFireRate);

        canFire = true;
        canMove = true;
    }

    // Reload Weapon
    async Task ReloadWeapon()
    {
        canFire = false;
        canMove = false;
        //play reload animation
        currentlyReloading = true;
        await Task.Delay(WeaponReloadTime);
        WeaponCurrentMagazineAmmount = WeaponMagazineSize;
        currentlyReloading = false;
        canFire = true;
        canMove = true;
    }

    // Part of the patrol cycle
    async Task MoveNextPatrol()
    {
        canPursue = false;
        currentlyMovingToNextPatrolTarget = true;

        //Find Random Position nearby
        if (Vector2.Distance(transform.position, target.transform.position) <= 0.5 || PatrolPositions.Contains(target) == false)
        {
            target = PatrolPositions[UnityEngine.Random.Range(0, PatrolPositions.Count)];
            await Task.Delay(PatrolStallTime);
        }

        canPursue = true;
        currentlyMovingToNextPatrolTarget = false;

    }

    // General Utility
    bool DetermineLineOfSight(GameObject object1, GameObject object2)
    {
        Vector3 RaycastStart = object1.transform.position;
        Vector3 RaycastDirection = (object2.transform.position - object1.transform.position).normalized;
        float RaycastDistance = Vector3.Distance(object2.transform.position, object1.transform.position);

        if (Physics2D.Raycast(RaycastStart, RaycastDirection, RaycastDistance, LayerMask.GetMask("Default")) == false)
        {
            Debug.DrawRay(RaycastStart, RaycastDirection * RaycastDistance);
            return true;
        }
        else
        {
            return false;
        }
    }

    // Partof the pursue cycle
    void ComputeClosestPositionToPlayer()
    {
        SuitablePositions.Clear();
        foreach (GameObject query in AllPositions)
        {
            // Check the distance of the position
            if (Vector2.Distance(query.transform.position, player.transform.position) <= DesiredDistance && Vector2.Distance(query.transform.position, player.transform.position) >= MinumumDistance)
            {
                // Check line of sight of the position

                if (DetermineLineOfSight(query, player))
                {
                    SuitablePositions.Add(query);
                }
            }
        }

        if (SuitablePositions.Count > 0)
        {
            target = SuitablePositions[UnityEngine.Random.Range(0, SuitablePositions.Count)];
            foreach (GameObject pos in SuitablePositions)
            {
                //Find the point that is closest to the enemy
                if (Vector2.Distance(transform.position, pos.transform.position) < Vector2.Distance(transform.position, target.transform.position))
                {
                    if (Vector2.Distance(player.transform.position, pos.transform.position) < Vector2.Distance(player.transform.position, target.transform.position))
                    {
                        target = pos;
                    }
                }
            }
        }
    }
}