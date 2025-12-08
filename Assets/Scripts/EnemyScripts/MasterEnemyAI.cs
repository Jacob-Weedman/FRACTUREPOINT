using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text;

// Designed by Jacob Weedman
// Use on any basic enemy
//
// Configure to your likeing 
// Not all options apply with every configuration
//
// PREREQUISITES
// 1. The "Seeker" script from the A* pathfinding library must be attatched to the same game object
// 2. A Rigidbody2D component must be attatched to the same game object
// 3. A _Collider2D component must be attatched to the same game object
// 4. Various game objects are required to be in the scene in order for this to function properly
// 5. Various children game objects are required in order for this to funciton properly

public class MasterEnemyAI : MonoBehaviour
{

    #region MISC VARIABLES (DO NOT CHANGE VALUES)
    GameObject target;
    GameObject Camera;
    float nextWaypointDistance = 5;
    Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;
    GameObject player;
    GameObject barrel;
    public List<GameObject> SuitablePositions;
    public List<GameObject> AllPositions;
    float DistanceFromPlayer;
    Vector2 StartPosition;
    public List<GameObject> PatrolPositions;
    public GameObject LastKnownPlayerLocation;
    Transform DroneBayLocation;
    #endregion

    #region CONDITIONS/GENERAL INFORMATION
    public bool targetingPlayer = false;
    public bool inFiringCycle = false;
    bool currentlyReloading = false;
    bool currentlyPatrolling;
    bool canJump = true;
    bool canMove = true; // Prevent all movement
    bool canPursue = false; // Follow player
    bool canFire = true;
    bool canDash = true;
    bool currentlyMovingToNextPatrolTarget = false;
    float DesiredDistance;
    public int WeaponCurrentMagazineAmmount;
    public int NumberOfHitsPerMag = 0;
    bool canTeleport = true;
    float angle;
    bool currentlyInDroneBay = false;
    int CurrentBatteryCapacity;
    bool currentlyTravelingToDroneBay = false;
    bool currentlyRecharging = false;
    bool primed = false; // For self-explosion

    #endregion

    #region ENEMY STATS (Changeable in Unity)
    public float Speed = 1f; // In relation to player's walking speed
    public float JumpHeight = 1f; // In relation to player's regular jump height
    public float Health = 100;
    public float MinumumDistance = 5f;
    public float PatrolDistance = 15;
    public int PatrolStallTime = 2000; //ms
    public int PlayerDetectionRange = 20;
    public float dashSpeed = 40f; // Strength of the dash
    public int dashDelay = 1000;
    public float HiddenTransparencyAmmount = 0.05f; // Strength of the cloaked transparency
    public int TeleportCooldown = 500; //ms
    public int MaxBatteryCapacity = 30; // Depletes one every second it is out of the bay
    public int RechargeTime = 6000; //ms

    // Capabiltiies
    public bool AbilityDash = false;
    public bool AbilityInvisible = false;
    public bool AbilityJump = false;
    public bool AbilityTeleport = false;
    public bool AbilityShootAndMove = false;
    public bool AbilityReloadAndMove = false;
    public bool AbilityMove = false;
    public bool AbilityDynamicRelocation = false;
    public bool AbilityExplodeOnContact = false;
    public bool AbilityExplodeNearPlayer = false;
    public bool AbilityExplodeOnDeath = false;

    #endregion

    #region WEAPON STATS (CHANGEABLE in Unity)

    public GameObject EnemyProjectile;
    public string EnemyType = "GROUND"; // Options: "GROUND", "AIR"
    public bool IsDrone = false;
    public string WeaponType = "RANGED"; // Options: "RANGED", "ROCKET", "PARTICLE", "MELEE", "GRENADE", "NONE"
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

    #region ONCE THE ENEMY IS SPAWNED IN
    void Awake()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        if (transform.Find("ReloadingIndicator")) // Ensures Reloading Indicator Exists
        {
            transform.Find("ReloadingIndicator").GetComponent<SpriteRenderer>().enabled = false;
        }
        if (transform.Find("PursuingIndicator")) // Ensures Pursuing Indicator Exists
        {
            transform.Find("PursuingIndicator").GetComponent<SpriteRenderer>().enabled = false;
        }

        StartPosition = transform.position;
        AllPositions = GameObject.FindGameObjectsWithTag("PossiblePositions").ToList();

        if (EnemyType == "GROUND") // Configure settings for Ground enemies
        {
            foreach (GameObject pos in AllPositions)
            {
                if (Vector2.Distance(pos.transform.position, StartPosition) <= PatrolDistance)
                {
                    PatrolPositions.Add(pos);
                }
            }

            gameObject.layer = LayerMask.NameToLayer("Enemies");

            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Enemies");
            }

            if (PatrolPositions.Count() > 0) // Catches error when the game object starts outside of the game area
            {
                target = PatrolPositions[UnityEngine.Random.Range(0, PatrolPositions.Count)];
            }
            else
            {
                target = null;
            }
        }

        if (EnemyType == "AIR") // Configure settings for Air enemies
        {
            gameObject.layer = LayerMask.NameToLayer("FlyingEnemies");

            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("FlyingEnemies");
            }

            GetComponent<Rigidbody2D>().gravityScale = 0;
            target = Instantiate(GameObject.Find("FlyingTarget"), transform.position, Quaternion.identity);
        }

        Camera = GameObject.FindGameObjectWithTag("MainCamera");
        player = GameObject.FindGameObjectWithTag("Player");

        if (transform.Find("Barrel")) // Ensures barrel exists
        {
            barrel = transform.Find("Barrel").gameObject;
        }
        else
        {
            barrel = null;
        }

        LastKnownPlayerLocation = null;

        WeaponCurrentMagazineAmmount = WeaponMagazineSize;
        DesiredDistance = WeaponRange - 5;

        // Set default EnemyProjectile if it starts as NULL (DEV forgot to change it lmao)
        if (EnemyProjectile == null)
        {
            EnemyProjectile = GameObject.Find("GameObjectFolder").transform.Find("EnemyProjectile").gameObject;
        }


        InvokeRepeating("UpdatePath", 0f, 0.1f);
        InvokeRepeating("PathfindingTimeout", 0f, 10);

        if (IsDrone == true)
        {
            InvokeRepeating("BatteryDrain", 0f, 1);
            CurrentBatteryCapacity = MaxBatteryCapacity;
            DroneBayLocation = transform.parent.transform;
        }
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
        if (target != null && Vector2.Distance(transform.position, target.transform.position) > 0.5 || LastKnownPlayerLocation == null)
        {
            //target = gameObject;
            MoveNextPatrol();

            if (AbilityTeleport && canTeleport)
            {
                Teleport();
            }
        }
    }

    // Battery Drain
    void BatteryDrain()
    {
        if (currentlyInDroneBay == false)
        {
            CurrentBatteryCapacity -= 1;
        }
    }

    #endregion

    #region MAIN LOGIC
    void FixedUpdate()
    {
        #region DEATH
        if (Health <= 0)
        {
            if (AbilityExplodeNearPlayer == false && AbilityExplodeOnContact == false && AbilityExplodeOnDeath == false)
            {
            // Create carcas
            GameObject DeadBody;
            DeadBody = Instantiate(gameObject, transform.position, Quaternion.identity);
            DeadBody.GetComponent<Rigidbody2D>().gravityScale = 1.5f;
            Destroy(GameObject.Find(DeadBody.name).GetComponent<MasterEnemyAI>());
            Destroy(GameObject.Find(DeadBody.name).GetComponent<Seeker>());

            foreach (Transform child in DeadBody.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            Destroy(gameObject);
            }
            else if (AbilityExplodeOnDeath == true)
            {
                Explode();
            }
        }

        // Explode when close to the player
        if (AbilityExplodeNearPlayer && primed && Vector2.Distance(player.transform.position, transform.position) <= 2)
        {
            Explode();
        }

        #endregion

        #region ICONS

        // See where the enemy wants to go (DEBUGGING ONLY)
        
        if (target != null)
        {
            foreach (GameObject pos in AllPositions)
            {
                pos.GetComponent<SpriteRenderer>().enabled = false;
            }
            target.GetComponent<SpriteRenderer>().enabled = true;
        }
        

        if (targetingPlayer && transform.Find("PursuingIndicator"))
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

        if (currentlyReloading && transform.Find("ReloadingIndicator"))
        {
            transform.Find("ReloadingIndicator").GetComponent<SpriteRenderer>().enabled = true;
        }
        else if (transform.Find("ReloadingIndicator"))
        {
            transform.Find("ReloadingIndicator").GetComponent<SpriteRenderer>().enabled = false;
        }

        //if (90 <= angle || angle <= 270) // FIX THIS
        //{
        //    barrel.transform.localScale = new Vector2(-barrel.transform.localScale.x, barrel.transform.localScale.y);
        //}
        #endregion

        #region MISC PATHFINDING
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
        if (WeaponCurrentMagazineAmmount > 0 && canFire == true && currentlyReloading == false && WeaponType != "NONE")
        {
            if (DetermineLineOfSight(gameObject, player) == true && Vector2.Distance(transform.position, player.transform.position) <= WeaponRange)
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

        // Ground Movement
        if (canMove == true && EnemyType == "GROUND" && AbilityMove)
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
        // Air Movement
        if (canMove == true && EnemyType == "AIR" && AbilityMove)
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


        #endregion

        #region GROUND DETECTION
        //Detecting if the enemy has reached the ground
        if (EnemyType == "GROUND" && AbilityJump) // Makes sure the enemy can jump before calculating
        {
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
        }
        #endregion

        #region PATROL & DETECTION

        // Check if the enemy is at the target location, used for moving to the next patrol target
        if (Vector2.Distance(transform.position, target.transform.position) <= 1)
        {
            currentlyMovingToNextPatrolTarget = false;
        }

        // Enemy detects player
        if (DetermineLineOfSight(gameObject, player) == true && Vector2.Distance(transform.position, player.transform.position) <= PlayerDetectionRange)
        {
            currentlyPatrolling = false;
            targetingPlayer = true;
            canPursue = true;
            primed = true;
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
            if (barrel != null) // Prevents Error
            {
                angle = Mathf.Atan2(player.transform.position.y - barrel.transform.position.y, player.transform.position.x - barrel.transform.position.x) * Mathf.Rad2Deg;
            }

        }
        // Player has broken line of sight and the enemy will attempt to move to the last known location
        else if (LastKnownPlayerLocation != null)
        {
            primed = false;
            if (Vector2.Distance(transform.position, LastKnownPlayerLocation.transform.position) > 0.5)
            {
                canPursue = false;
                target = LastKnownPlayerLocation;

                if (AbilityTeleport && canTeleport)
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
            //LastKnownPlayerLocation = gameObject;
            currentlyPatrolling = true;
            targetingPlayer = false;

            if (IsDrone)
            {
                ReturnToDroneBay();
            }
            else if (canMove == true && currentlyMovingToNextPatrolTarget == false)
            {
                MoveNextPatrol();
            }

            // Reset barrel rotation
            angle = 0f;
        }

        // Rotate barrel towards player
        if (barrel != null) // PRevents error
        {
            Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
            barrel.transform.rotation = Quaternion.RotateTowards(barrel.transform.rotation, targetRotation, 200 * Time.deltaTime);
        }

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
            //if (Vector2.Distance(transform.position, player.transform.position) < Vector2.Distance(target.transform.position, player.transform.position))
            //{
            //    ComputeClosestPositionToPlayer();
            //}

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
                if (EnemyType == "AIR")
                {
                    ComputeClosestPositionToPlayer();
                }
                if (canTeleport && AbilityTeleport)
                {
                    ComputeClosestPositionToPlayer();
                    Teleport();
                }
            }
        }
    }
    #endregion

    #endregion

    #region USE WEAPON METHOD
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
                BulletInstance = Instantiate(EnemyProjectile, barrel.transform.position, Quaternion.LookRotation(transform.position - GameObject.FindGameObjectWithTag("Player").transform.position + new Vector3(UnityEngine.Random.Range((-1 * WeaponRandomSpread), WeaponRandomSpread), UnityEngine.Random.Range((-1 * WeaponRandomSpread), WeaponRandomSpread), 0)));
                //BulletInstance.transform.parent = transform;

                // Variables
                BulletInstance.GetComponent<EnemyProjectileScript>().WeaponDamage = WeaponDamage;
                BulletInstance.GetComponent<EnemyProjectileScript>().WeaponRange = WeaponRange;

                // Send it on it's way
                BulletInstance.GetComponent<Rigidbody2D>().linearVelocity = BulletInstance.transform.forward * -1 * WeaponProjectileSpeed;
                BulletInstance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, BulletInstance.transform.forward) - 90));
                break;

            case "ROCKET":

                // Create Rocket
                GameObject Rocket;
                Rocket = Instantiate(EnemyProjectile, barrel.transform.position, Quaternion.LookRotation(transform.position - GameObject.FindGameObjectWithTag("Player").transform.position + new Vector3(UnityEngine.Random.Range((-1 * WeaponRandomSpread), WeaponRandomSpread), UnityEngine.Random.Range((-1 * WeaponRandomSpread), WeaponRandomSpread), 0)));

                // Variables
                Rocket.GetComponent<EnemyRocket>().WeaponDamage = WeaponDamage;
                Rocket.GetComponent<EnemyRocket>().duration = 30;

                // Send it on its way
                Rocket.GetComponent<Rigidbody2D>().linearVelocity = Rocket.transform.forward * -1 * WeaponProjectileSpeed;
                Rocket.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, Rocket.transform.forward) - 90));
                
                break;

            case "PARTICLE":

                // Create particle
                GameObject ParticleWeapon;

                ParticleWeapon = Instantiate(EnemyProjectile, new Vector3(barrel.transform.position.x + UnityEngine.Random.Range(-0.5f, 0.5f), barrel.transform.position.y + UnityEngine.Random.Range(-0.5f, 0.5f), EnemyProjectile.transform.position.z), Quaternion.identity);
                ParticleWeapon.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(transform.position.x - player.transform.position.x + UnityEngine.Random.Range(-WeaponRandomSpread, WeaponRandomSpread), transform.position.y - player.transform.position.y + UnityEngine.Random.Range(-WeaponRandomSpread, WeaponRandomSpread)).normalized * 1.25f * WeaponRange * -1;

                // Set variables
                ParticleWeapon.GetComponent<EnemyParticleWeapon>().destroy = true;
                ParticleWeapon.GetComponent<EnemyParticleWeapon>().opacity = true;
                ParticleWeapon.GetComponent<EnemyParticleWeapon>().damageAmmount = WeaponDamage;

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


        await Task.Delay(dashDelay); // Dash delay ms

        // Flash Red
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f, 1f);

        await Task.Delay(100);

        gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);

        await Task.Delay(100);

        gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f, 1f);

        await Task.Delay(100);

        gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);

        await Task.Delay(100);

        // Perform dash
        rb.linearVelocity = new Vector2(transform.position.x - player.transform.position.x, transform.position.y - player.transform.position.y).normalized * dashSpeed * -1;

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

        if (NumberOfHitsPerMag / WeaponMagazineSize < 0.5 && AbilityDynamicRelocation)
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

        if (EnemyType == "GROUND")
        {
            //Find Random Position nearby
            if (Vector2.Distance(transform.position, target.transform.position) <= 0.5 || PatrolPositions.Contains(target) == false)
            {
                target = PatrolPositions[UnityEngine.Random.Range(0, PatrolPositions.Count)];
                await Task.Delay(PatrolStallTime);
            }
        }

        if (EnemyType == "AIR")
        {
            //Find Random Position nearby
            target.transform.position = new Vector2((StartPosition.x + UnityEngine.Random.Range(-1 * PatrolDistance, PatrolDistance)), (StartPosition.y + UnityEngine.Random.Range(-1 * PatrolDistance, PatrolDistance)));
            await Task.Delay(PatrolStallTime);
        }

    }

    // Teleport
    async Task Teleport()
    {
        canTeleport = false;
        transform.position = target.transform.position;
        await Task.Delay(TeleportCooldown);
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
        canMove = true;

        if (EnemyType == "GROUND")
        {

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
                
                if (AbilityTeleport == false)
                {
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

        if (EnemyType == "AIR")
        {
            //if (DetermineLineOfSight(player, gameObject))
            //{
                target.transform.position = new Vector2((player.transform.position.x + UnityEngine.Random.Range(-MinumumDistance, MinumumDistance)), (player.transform.position.y + MinumumDistance + UnityEngine.Random.Range(-5, 0)));
            //}
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

    // Return To Drone Bay
    async Task ReturnToDroneBay()
    {
        if (currentlyTravelingToDroneBay == false && currentlyInDroneBay == false)
        {
            currentlyTravelingToDroneBay = true;
            target.transform.position = DroneBayLocation.position;
        }
    }

    // Recharge
    async Task Recharge()
    {
        currentlyRecharging = true;
        canMove = false;
        canFire = false;
        seeker.enabled = false;
        transform.position = DroneBayLocation.position;
        rb.linearVelocity = Vector3.zero;
        currentlyTravelingToDroneBay = false;
        await Task.Delay(RechargeTime);
        CurrentBatteryCapacity = MaxBatteryCapacity;
        currentlyRecharging = false;
        canFire = true;
    }

    // Explode
    void Explode()
    {
        // Shake Camera
        Camera.GetComponent<CameraMovement>().shakeCamera(0.8f, 0.5f);

        //  Create Explosion
        GameObject Explosion;
        Explosion = Instantiate(GameObject.Find("Explosion"), new Vector3(transform.position.x, transform.position.y, GameObject.Find("Explosion").transform.position.z), Quaternion.identity);
        Explosion.transform.rotation = Quaternion.Euler(Vector3.forward);

        // Set Variables
        Explosion.GetComponent<EnemyParticleWeapon>().destroy = true;
        Explosion.GetComponent<EnemyParticleWeapon>().opacity = true;
        Explosion.GetComponent<EnemyParticleWeapon>().timer = 3;
        Explosion.GetComponent<EnemyParticleWeapon>().damageAmmount = WeaponDamage;

        // Destroy Flying Target
        if (EnemyType == "AIR")
        {
            Destroy(target);
        }

        // Destroy GameObject
        Destroy(gameObject);
    }

    // On contact
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (primed && AbilityExplodeOnContact)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("SolidGround") || collision.gameObject.layer == LayerMask.NameToLayer("Enemies") || collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Explode();
            }
        }
    }
}
#endregion