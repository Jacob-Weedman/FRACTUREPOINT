using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text;

//Designed by Jacob Weedman
//Use on Rocketman type enemies

public class RocketmanAI : MonoBehaviour
{

//MISC
    GameObject target;
    float nextWaypointDistance = 5;
    Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;
    GameObject projectile;
    GameObject player;
    GameObject barrel;
    public List<GameObject> SuitablePositions;
    public List<GameObject> AllPositions;
    float DistanceFromPlayer;
    Vector2 StartPosition;
    public List<GameObject> PatrolPositions;
    public GameObject LastKnownPlayerLocation;

//CONDITIONS/GENERAL INFORMATION
    bool canJump = true; 
    bool canMove = true; // Prevent all movement
    bool canPursue = false; // Follow player
    bool canFire = false;
    bool currentlyReloading = false;
    bool currentlyPatrolling;
    bool currentlyMovingToNextPatrolTarget = false;
    float DesiredDistance;
    float MinumumDistance = 5f;
    bool targetingPlayer = false;
    bool inFiringCycle = false;
    int WeaponCurrentMagazineAmmount;
    float angle;

//ENEMY STATS (Changeable)
    public float Speed = 0.8f; // In relation to player's walking speed
    public float JumpHeight = 0.8f; // In relation to player's regular jump height
    public float Health = 30;
    public float PatrolDistance = 15;
    public int PatrolStallTime = 2000; //ms
    public int PlayerDetectionRange = 40;

//WEAPON STATS (CHANGEABLE)
    public int WeaponDamage = 30; // Damage per hit
    public int WeaponFireRate = 2500; // Delay in time between attacks both melee and ranged
    public float WeaponRandomSpread = 1.5f; // Random direction of lanched projectiles
    public int WeaponRange = 30; // Maximum range of the projectile before it drops off
    public float WeaponProjectileSpeed = 40f; // Speed of launched projectiles
    public int WeaponMagazineSize = 1; // Number of shots the enemy will take before having to reload
    public int WeaponReloadTime = 5000; // Time it takes to reload the magazine
    
//REFERENCES
    Seeker seeker;
    Rigidbody2D rb;

//ONCE THE GAME STARTS
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

        projectile = GameObject.Find("EnemyProjectile");
        player = GameObject.FindGameObjectWithTag("Player");
        barrel = transform.Find("Barrel").gameObject;
        LastKnownPlayerLocation = null;

        WeaponCurrentMagazineAmmount = WeaponMagazineSize;
        DesiredDistance = WeaponRange - 5;

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
            target = gameObject;
            MoveNextPatrol();
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

        //if (90 <= angle || angle <= 270)
        //{
        //    barrel.transform.localScale = new Vector2(-barrel.transform.localScale.x, barrel.transform.localScale.y);
        //}

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

//RANGED ATTACK
        // Check if enemy has line of sight on the player & if they are in the acceptable range       
        if (WeaponCurrentMagazineAmmount > 0 && canFire == true && currentlyReloading == false && currentlyPatrolling == false)
        {
            if (DetermineLineOfSight(gameObject, player) == true && Vector2.Distance(transform.position, player.transform.position) <= WeaponRange && Vector2.Distance(transform.position, player.transform.position) <= DesiredDistance)
            {
            UseWeapon();
            }
        }
        else if (WeaponCurrentMagazineAmmount == 0 && currentlyReloading == false)
        {
            ReloadWeapon();
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

//PATROL & DETECTION    
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

            // Angle barrel towards player
            angle = Mathf.Atan2(player.transform.position.y - barrel.transform.position.y, player.transform.position.x - barrel.transform.position.x) * Mathf.Rad2Deg;

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

            // Reset barrel rotation
            angle = 0f;

        }
        // Go back to patrol move
        else
        {
            currentlyPatrolling = true;
            targetingPlayer = false;
            if (canMove == true && currentlyMovingToNextPatrolTarget == false)
            {
                MoveNextPatrol();
            }

            // Reset barrel rotation
            angle = 0f;
        }
        
        // Rotate barrel towards player
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        barrel.transform.rotation = Quaternion.RotateTowards(barrel.transform.rotation, targetRotation, 200 * Time.deltaTime);
        
//TARGET DETERMINATION
        if (canPursue == true)
        {
            // If the player moves away (CHANGE TARGET)
            if (Vector2.Distance(target.transform.position, player.transform.position) > DesiredDistance)
            {
                ComputeClosestPositionToPlayer();
            }

            // If the player is too close to the target (CHANGE TARGET)
            if (Vector2.Distance(target.transform.position, player.transform.position) < MinumumDistance)
            {
                ComputeClosestPositionToPlayer();
            }

            // If the target is on the other side of the player (CHANGE TARGET)
            if (Vector2.Distance(transform.position, player.transform.position) < Vector2.Distance(target.transform.position, player.transform.position))
            {
                ComputeClosestPositionToPlayer();
            }

            // If the player is not within line of sight of the desired position (CHANGE TARGET)
            if (DetermineLineOfSight(target, player) == false)
            {
                ComputeClosestPositionToPlayer();
            }

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
    // Use Ranged Weapon
    async Task UseWeapon()
    {
        canFire = false;
        canMove = false;
        inFiringCycle = true;

        WeaponCurrentMagazineAmmount--;

        GameObject Rocket;
        Rocket = Instantiate(GameObject.Find("EnemyRocket"), new Vector3(transform.position.x + UnityEngine.Random.Range(-0.5f, 0.5f), transform.position.y + UnityEngine.Random.Range(-0.5f, 0.5f), GameObject.Find("EnemyRocket").transform.position.z), Quaternion.identity);
        Rocket.transform.rotation = Quaternion.Euler(Vector3.forward * UnityEngine.Random.Range(-90, 90));

        // Send it on its way
        Rocket.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(transform.position.x - player.transform.position.x + UnityEngine.Random.Range(-WeaponRandomSpread, WeaponRandomSpread), transform.position.y - player.transform.position.y + UnityEngine.Random.Range(-WeaponRandomSpread, WeaponRandomSpread)).normalized * WeaponProjectileSpeed * -1;
        Rocket.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, Rocket.transform.forward) - 90));

        // Variables
        Rocket.GetComponent<EnemyRocket>().WeaponDamage = WeaponDamage;

        await Task.Delay(WeaponFireRate);

        canFire = true;
        canMove = true;
        inFiringCycle = false;
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
        LastKnownPlayerLocation = null;

        canPursue = false;
        currentlyMovingToNextPatrolTarget = true;

        //Find Random Position nearby
        if (Vector2.Distance(transform.position, target.transform.position) <= 0.5 || PatrolPositions.Contains(target) == false)
        {
            target = PatrolPositions[UnityEngine.Random.Range(0, PatrolPositions.Count)];
            await Task.Delay(PatrolStallTime + UnityEngine.Random.Range((PatrolStallTime * -1), PatrolStallTime));
        }

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
        canFire = false;
        canMove = true;
        
        SuitablePositions.Clear();
        foreach (GameObject query in AllPositions)
        {
            // Check the distance of the position
            if (Vector2.Distance(query.transform.position, player.transform.position) < DesiredDistance && Vector2.Distance(query.transform.position, player.transform.position) >= MinumumDistance)
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
                    target = pos;
                }
            }
        }
    }
}