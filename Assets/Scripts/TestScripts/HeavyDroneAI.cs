using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text;

//Designed by Jacob Weedman
//Use on heavy drone type enemies 

public class HeavyDroneAI : MonoBehaviour
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
    public List<GameObject> AllPositions;
    float DistanceFromPlayer;
    Vector2 StartPosition;
    GameObject LastKnownPlayerLocation;
    Transform DroneBayLocation;

    //CONDITIONS/GENERAL INFORMATION
    bool canMove = false;
    bool canPursue = false;
    bool canFire = true;
    bool currentlyReloading = false;
    bool currentlyInDroneBay = true;
    float DesiredDistance;
    float MinumumDistance = 10f;
    bool targetingPlayer = false;
    bool inFiringCycle = false;
    int WeaponCurrentMagazineAmmount;
    int CurrentBatteryCapacity;
    bool currentlyTravelingToDroneBay = false;
    bool currentlyRecharging = false;
    float angle;

    //ENEMY STATS (Changeable)
    public float Speed = 0.7f; // In relation to player's walking speed
    public float Health = 200;
    public int PlayerDetectionRange = 25;
    public int MaxBatteryCapacity = 30; // Depletes one every second it is out of the bay
    public int RechargeTime = 6000; //ms

    //WEAPON STATS (CHANGEABLE)
    public int WeaponDamage = 20; // Damage per hit
    public int WeaponFireRate = 2000; // Delay in time between attacks both melee and ranged
    public float WeaponRandomSpread = 5f; // Random direction of lanched projectiles (DOES NOT APPLY TO MELEE ATTACKS)
    public int WeaponRange = 30; // Maximum range of the projectile before it drops off (DOES NOT APPLY TO MELEE ATTACKS)
    public float WeaponProjectileSpeed = 30f; // Speed of launched projectiles (DOES NOT APPLY TO MELEE WEAPONS)
    public int WeaponMagazineSize = 12; // Number of shots the enemy will take before having to reload
    public int WeaponReloadTime = 5000; // Time it takes to reload the magazine


    Seeker seeker;
    Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        transform.Find("ReloadingIndicator").GetComponent<SpriteRenderer>().enabled = false;
        transform.Find("PursuingIndicator").GetComponent<SpriteRenderer>().enabled = false;
        transform.Find("BatteryIndicator").GetComponent<SpriteRenderer>().enabled = false;

        StartPosition = transform.position;
        AllPositions = GameObject.FindGameObjectsWithTag("PossiblePositions").ToList();

        target = Instantiate(GameObject.Find("FlyingTarget"), transform.position, Quaternion.identity);

        projectile = GameObject.Find("EnemyGrenade");
        player = GameObject.FindGameObjectWithTag("Player");
        barrel = transform.Find("Barrel").gameObject;
        LastKnownPlayerLocation = null;
        DroneBayLocation = transform.parent.transform;

        WeaponCurrentMagazineAmmount = WeaponMagazineSize;
        DesiredDistance = WeaponRange - 5;
        CurrentBatteryCapacity = MaxBatteryCapacity;

        InvokeRepeating("UpdatePath", 0f, 0.1f);
        InvokeRepeating("PathfindingTimeout", 0f, 30);
        InvokeRepeating("BatteryDrain", 0f, 1);

        Recharge();
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
            //ReturnToDroneBay();
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

// MAIN LOGIC
    void FixedUpdate()
    {
//DEATH
        if (CurrentBatteryCapacity == 0)
        {
            Health = 0;
        }
        
        if (Health <= 0)
        {
            GameObject DeadBody;
            DeadBody = Instantiate(gameObject, transform.position, Quaternion.identity);
            DeadBody.GetComponent<Rigidbody2D>().gravityScale = 3;
            Destroy(GameObject.Find(DeadBody.name).GetComponent<LightDroneAI>());
            Destroy(GameObject.Find(DeadBody.name).GetComponent<Seeker>());

            foreach (Transform child in DeadBody.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            Destroy(gameObject);
        }

//Battery
        // Low battery return
        if (CurrentBatteryCapacity <= 10)
        {
            ReturnToDroneBay();
        }

        // Recharge battery & bay detection
        if (Vector2.Distance(transform.position, DroneBayLocation.position) <= 0.5)
        {
            currentlyInDroneBay = true;
            currentlyTravelingToDroneBay = false;
            if (currentlyRecharging == false && CurrentBatteryCapacity < 10)
            {
                Recharge();
            }
            else
            {
                canMove = false;
            }
        }
        else
        {
            //canMove = true;
            currentlyInDroneBay = false;
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

        if (currentlyReloading)
        {
            transform.Find("ReloadingIndicator").GetComponent<SpriteRenderer>().enabled = true;
        }
        else
        {
            transform.Find("ReloadingIndicator").GetComponent<SpriteRenderer>().enabled = false;
        }

        if (CurrentBatteryCapacity <= MaxBatteryCapacity / 10)
        {
            transform.Find("BatteryIndicator").GetComponent<SpriteRenderer>().enabled = true;
        }
        else
        {
            transform.Find("BatteryIndicator").GetComponent<SpriteRenderer>().enabled = false;
        }

        if (currentlyInDroneBay == false)
        {
            seeker.enabled = true;
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

//RANGED ATTACK
        // Check if enemy has line of sight on the player & if they are in the acceptable range

        if (WeaponCurrentMagazineAmmount > 0 && canFire == true && currentlyReloading == false && currentlyInDroneBay == false)
        {
            if (DetermineLineOfSight(gameObject, player) == true && inFiringCycle == false && Vector2.Distance(transform.position, player.transform.position) <= WeaponRange && Vector2.Distance(transform.position, player.transform.position) <= DesiredDistance)
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

//DETECTION
        // Enemy detects player
        if (DetermineLineOfSight(transform.parent.gameObject, player) && Vector2.Distance(DroneBayLocation.position, player.transform.position) <= PlayerDetectionRange && currentlyRecharging == false)
        {
            if (currentlyInDroneBay == true)
            {
                rb.linearVelocity = transform.parent.transform.up * 10;
            }
            canMove = true;
            targetingPlayer = true;
            canPursue = true;

            // Angle barrel towards player
            angle = Mathf.Atan2(player.transform.position.y - barrel.transform.position.y, player.transform.position.x - barrel.transform.position.x) * Mathf.Rad2Deg;

        }
        else if (DetermineLineOfSight(gameObject, player) == true && Vector2.Distance(transform.position, player.transform.position) <= PlayerDetectionRange)
        {
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

            // Reset barrel rotation
            angle = -90f;
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

            // Reset barrel rotation
            angle = -90f;
        }
        // Go back to drone bay
        else
        {
            LastKnownPlayerLocation = null;
            ReturnToDroneBay();
            targetingPlayer = false;

            // Reset barrel rotation
            angle = -90f;
        }

        // Rotate barrel towards player
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        barrel.transform.rotation = Quaternion.RotateTowards(barrel.transform.rotation, targetRotation, 100 * Time.deltaTime);


        if (currentlyTravelingToDroneBay == false)
        {
            //TARGET DETERMINATION

            // If enemy is at the target position
            if (Vector2.Distance(target.transform.position, transform.position) < 1.5)
            {
                ComputeClosestPositionToPlayer();
            }

            // If the player moves away (CHANGE TARGET)
            if (Vector2.Distance(target.transform.position, player.transform.position) > DesiredDistance)
            {
                ComputeClosestPositionToPlayer();
            }

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
    // Use Ranged Weapon
    async Task UseWeapon()
    {
        inFiringCycle = true;

        // Create Grenade
        GameObject GrenadeInstance;
        GrenadeInstance = Instantiate(projectile, transform.position, Quaternion.LookRotation(transform.position - GameObject.FindGameObjectWithTag("Player").transform.position + new Vector3(UnityEngine.Random.Range((-1 * WeaponRandomSpread), WeaponRandomSpread), UnityEngine.Random.Range((-1 * WeaponRandomSpread), WeaponRandomSpread), 0)));
        GrenadeInstance.GetComponent<EnemyGrenade>().WeaponDamage = WeaponDamage;

        // Send it on it's way
        GrenadeInstance.GetComponent<Rigidbody2D>().linearVelocity = GrenadeInstance.transform.forward * -1 * WeaponProjectileSpeed;
        GrenadeInstance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, GrenadeInstance.transform.forward) - 90));
        WeaponCurrentMagazineAmmount--;

        await Task.Delay(WeaponFireRate);

        inFiringCycle = false;
    }

    // Reload Weapon
    async Task ReloadWeapon()
    {
        canFire = false;

        //play reload animation
        currentlyReloading = true;
        await Task.Delay(WeaponReloadTime);
        WeaponCurrentMagazineAmmount = WeaponMagazineSize;
        currentlyReloading = false;

        canFire = true;
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
        // find a target in the air a set radius away from the player
        if (DetermineLineOfSight(player, gameObject))
        {
            canMove = true;
            target.transform.position = new Vector2((player.transform.position.x + UnityEngine.Random.Range(-MinumumDistance, MinumumDistance)), (player.transform.position.y + MinumumDistance + UnityEngine.Random.Range(-5, 0)));
        }
    }
}