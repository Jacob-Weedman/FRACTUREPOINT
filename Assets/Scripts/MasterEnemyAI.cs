using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text;

//Designed by Jacob Weedman
//Use on any basic enemy
//Configure to your likeing 

public class MasterEnemyAI : MonoBehaviour
{

#region MISC VARIABLES (DO NOT CHANGE VALUES)
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
#endregion

#region CONDITIONS/GENERAL INFORMATION
    bool targetingPlayer = false;
    bool inFiringCycle = false;
    bool currentlyReloading = false;
    bool currentlyPatrolling;
    bool canJump = true; 
    bool canMove = true; // Prevent all movement
    bool canPursue = false; // Follow player
    bool canFire = false;
    bool canDash = true;
    bool currentlyMovingToNextPatrolTarget = false;
    float DesiredDistance;
    float MinumumDistance = 5f;
    int WeaponCurrentMagazineAmmount;
    public int NumberOfHitsPerMag = 0;
    bool canTeleport = true;
    float angle;

#endregion

#region ENEMY STATS (Changeable in Unity)
    public float Speed = 1f; // In relation to player's walking speed
    public float JumpHeight = 1f; // In relation to player's regular jump height
    public float Health = 100;
    public float PatrolDistance = 15;
    public int PatrolStallTime = 2000; //ms
    public int PlayerDetectionRange = 20;
    public float dashSpeed = 40f; // Strength of the dash
    public float HiddenTransparencyAmmount = 0.05f; // Strength of the cloaked transparency
    public int TeleportCooldown = 500; //ms
    
    // Capabiltiies
    public bool AbilityDash = false;
    public bool AbilityInvisible = false;
    public bool AbilityJump = false;
    public bool AbilityTeleport = false;
    public bool AbilityShootAndMove = false;
    public bool AbilityReloadAndMove = false;

#endregion

#region WEAPON STATS (CHANGEABLE in Unity)
    public string WeaponType = "RANGED"; // Options: "RANGED", "ROCKET", "PARTICLE", "MELEE", "GRENADE"
    public string ParticleWeaponType = "FIRE"; // Options: "FIRE", "ELECTRICITY". NOTE: only used if public variable WeaponType = "PARTICLE" 
    public int WeaponDamage = 5; // Damage per hit
    public int WeaponFireRate = 300; // Delay in time between attacks both melee and ranged
    public float WeaponRandomSpread = 7.5f; // Random direction of lanched projectiles
    public int WeaponRange = 15; // Maximum range of the projectile before it drops off
    public float WeaponProjectileSpeed = 40f; // Speed of launched projectiles
    public int WeaponMagazineSize = 20; // Number of shots the enemy will take before having to reload
    public int WeaponReloadTime = 3000; // Time it takes to reload the magazine
#endregion

#region COMPONENT REFERENCES
    Seeker seeker;
    Rigidbody2D rb;
#endregion

#region ONCE THE GAME STARTS
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
#endregion

#region INVOKE REPEATING METHODS
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

            if (AbilityTeleport)
            {
                Teleport();
            }    
        }
    }
#endregion

#region MAIN LOGIC
    void FixedUpdate()
    {
#region DEATH
        if (Health <= 0)
        {
            GameObject DeadBody;
            DeadBody = Instantiate(gameObject, transform.position, Quaternion.identity);
            Destroy(GameObject.Find(DeadBody.name).GetComponent<MasterEnemyAI>());
            Destroy(GameObject.Find(DeadBody.name).GetComponent<Seeker>());

        foreach (Transform child in DeadBody.transform) {
            GameObject.Destroy(child.gameObject);
        }
            Destroy(gameObject);
        }
#endregion

#region ICONS
        if (targetingPlayer && transform.Find("PursuingIndicator").GetComponent<SpriteRenderer>())
        {
            transform.Find("PursuingIndicator").GetComponent<SpriteRenderer>().enabled = true;
            if (AbilityInvisible)
            {
                Cloak(false);
            }
        }
        else
        {
            transform.Find("PursuingIndicator").GetComponent<SpriteRenderer>().enabled = false;
            if (AbilityInvisible)
            {
                Cloak(true);
            }
        }

        if (currentlyReloading && transform.Find("ReloadingIndicator").GetComponent<SpriteRenderer>())
        {
            transform.Find("ReloadingIndicator").GetComponent<SpriteRenderer>().enabled = true;
        }
        else
        {
            transform.Find("ReloadingIndicator").GetComponent<SpriteRenderer>().enabled = false;
        }

        //if (90 <= angle || angle <= 270) // FIX THIS
        //{
        //    barrel.transform.localScale = new Vector2(-barrel.transform.localScale.x, barrel.transform.localScale.y);
        //}
#endregion

#region MISC PATHFINDING (DONT CHANGE)
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
#endregion

#region WEAPONATTACK
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
#endregion

#region MOVEMENT

        // Call Dash
        if (DetermineLineOfSight(gameObject, player) && targetingPlayer && canDash == true && AbilityDash)
        {
            Dash();
        }

        // Regular Movement
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
#endregion

#region GROUND DETECTION
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
#endregion

#region PATROL & DETECTION    
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

                if (AbilityTeleport)
                {
                    Teleport();
                }    
                
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

#endregion

    #region TARGET DETERMINATION
        if (canPursue == true)
        {
            // Catch desired distance error
            if (DesiredDistance <= MinumumDistance)
            {
                DesiredDistance = MinumumDistance + 1;
            }

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

            if (canTeleport && AbilityTeleport)
            {
                if (Vector2.Distance(transform.position, target.transform.position) > 0.5)
                {
                    Teleport();
                }
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
    #endregion

    #endregion

    #region UseWeapon 
    // Use Weapon
    async Task UseWeapon()
    {
        if (AbilityShootAndMove == false)
        {
            canMove = false;
        }

        canFire = false;
        inFiringCycle = true;

        switch (WeaponType)
        {
            case "RANGED":
                // Create Projectile
                GameObject BulletInstance;
                BulletInstance = Instantiate(projectile, transform.position, Quaternion.LookRotation(transform.position - GameObject.FindGameObjectWithTag("Player").transform.position + new Vector3(UnityEngine.Random.Range((-1 * WeaponRandomSpread), WeaponRandomSpread), UnityEngine.Random.Range((-1 * WeaponRandomSpread), WeaponRandomSpread), 0)));
                BulletInstance.transform.parent = transform;

                // Send it on it's way
                BulletInstance.GetComponent<Rigidbody2D>().linearVelocity = BulletInstance.transform.forward * -1 * WeaponProjectileSpeed;
                BulletInstance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, BulletInstance.transform.forward) - 90));
                break;

            case "ROCKET":

                // Create Rocket
                GameObject Rocket;
                Rocket = Instantiate(GameObject.Find("EnemyRocket"), new Vector3(transform.position.x + UnityEngine.Random.Range(-0.5f, 0.5f), transform.position.y + UnityEngine.Random.Range(-0.5f, 0.5f), GameObject.Find("EnemyRocket").transform.position.z), Quaternion.identity);
                Rocket.transform.rotation = Quaternion.Euler(Vector3.forward);

                // Send it on its way
                Rocket.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(transform.position.x - player.transform.position.x + UnityEngine.Random.Range(-WeaponRandomSpread, WeaponRandomSpread), transform.position.y - player.transform.position.y + UnityEngine.Random.Range(-WeaponRandomSpread, WeaponRandomSpread)).normalized * WeaponProjectileSpeed * -1;
                Rocket.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, Rocket.transform.forward) - 90));

                // Variables
                Rocket.GetComponent<EnemyRocket>().WeaponDamage = WeaponDamage;

                break;

            case "PARTICLE":

                // Create particle
                GameObject ParticleWeapon;

                if (ParticleWeaponType == "ELECTRICITY") // Instantiate electrity object
                {
                    ParticleWeapon = Instantiate(GameObject.Find("EvilAura"), new Vector3(barrel.transform.position.x, barrel.transform.position.y, GameObject.Find("EvilAura").transform.position.z), Quaternion.identity);
                    ParticleWeapon.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(transform.position.x - player.transform.position.x + UnityEngine.Random.Range(-WeaponRandomSpread, WeaponRandomSpread), transform.position.y - player.transform.position.y + UnityEngine.Random.Range(-WeaponRandomSpread, WeaponRandomSpread)).normalized * 1.25f * WeaponRange * -1;

                    // Set variables
                    ParticleWeapon.GetComponent<EnemyParticleWeapon>().destroy = true;
                    ParticleWeapon.GetComponent<EnemyParticleWeapon>().opacity = true;
                    ParticleWeapon.GetComponent<EnemyParticleWeapon>().rotate = true;
                    ParticleWeapon.GetComponent<EnemyParticleWeapon>().damageAmmount = WeaponDamage;
                }
                if (ParticleWeaponType == "FIRE") // Instantiate fire object
                {
                    ParticleWeapon = Instantiate(GameObject.Find("Flame"), new Vector3(transform.position.x + UnityEngine.Random.Range(-0.5f, 0.5f), transform.position.y + UnityEngine.Random.Range(-0.5f, 0.5f), GameObject.Find("Flame").transform.position.z), Quaternion.identity);
                    ParticleWeapon.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(transform.position.x - player.transform.position.x + UnityEngine.Random.Range(-WeaponRandomSpread, WeaponRandomSpread), transform.position.y - player.transform.position.y + UnityEngine.Random.Range(-WeaponRandomSpread, WeaponRandomSpread)).normalized * 1.25f * WeaponRange * -1;

                    // Set variables
                    ParticleWeapon.GetComponent<EnemyParticleWeapon>().destroy = true;
                    ParticleWeapon.GetComponent<EnemyParticleWeapon>().opacity = true;
                    ParticleWeapon.GetComponent<EnemyParticleWeapon>().damageAmmount = WeaponDamage;
                }


                break;

            case "MELEE":

                GameObject.Find("GameData").GetComponent<GameData>().CurrentHealth -= WeaponDamage;

                break;

            case "GRENADE":

                break;

            default:
                Debug.Log("Invalid or no weapon seleted.");
                break;

        }

        WeaponCurrentMagazineAmmount--;
        await Task.Delay(WeaponFireRate);

        canFire = true;
        canMove = true;
        inFiringCycle = false;
    }
    #endregion
    
    #region MISC METHODS

    async Task Dash()
    {
        canDash = false;

        // Perform dash
        rb.linearVelocity = new Vector2(transform.position.x - player.transform.position.x, transform.position.y - player.transform.position.y).normalized * dashSpeed * -1;

        await Task.Delay(1000); // Dash delay ms

        canDash = true;
    }
    
    // Reload Weapon
    async Task ReloadWeapon()
    {
        canFire = false;

        if (AbilityReloadAndMove == false)
        {
            canMove = false;
        }

        //play reload animation
        currentlyReloading = true;
        await Task.Delay(WeaponReloadTime);
        WeaponCurrentMagazineAmmount = WeaponMagazineSize;
        currentlyReloading = false;

        if (NumberOfHitsPerMag / WeaponMagazineSize < 0.5)
        {
            DesiredDistance -= 5;
        }
        NumberOfHitsPerMag = 0;

        canFire = true;
        canMove = true;
    }

    // Part of the patrol cycle
    async Task MoveNextPatrol()
    {
        LastKnownPlayerLocation = null;
        DesiredDistance = WeaponRange - 5;

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
    
    // Teleport
    async Task Teleport()
    {
        canTeleport = false;
        transform.position = target.transform.position;
        await Task.Delay(TeleportCooldown + UnityEngine.Random.Range(0, TeleportCooldown / 2));
        canTeleport = true;
    }

    // Cloak
    void Cloak(bool state)
    {
        if (state)
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, HiddenTransparencyAmmount);
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        }
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

    // Perform jump
    async Task JumpMethod()
    {
        if (canJump == true && AbilityJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpHeight * 12);
            canJump = false;
            await Task.Delay(500);
        }
    }
}
#endregion
