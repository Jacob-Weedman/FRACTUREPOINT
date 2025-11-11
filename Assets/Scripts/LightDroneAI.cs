using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text;

//Designed by Jacob Weedman
//V 0.0.1
//Use on light drones ranged enemies

public class LightDroneAI : MonoBehaviour
{
    //MISC
    GameObject target;
    float nextWaypointDistance = 5;
    Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;
    GameObject projectile;
    GameObject player;
    public List<GameObject> AllPositions;
    float DistanceFromPlayer;
    Vector2 StartPosition;
    GameObject LastKnownPlayerLocation;
    Transform DroneBayLocation;

    //CONDITIONS/GENERAL INFORMATION
    public bool canMove = true;
    public bool canPursue = false;
    public bool canFire = true;
    public bool currentlyReloading = false;
    public bool currentlyInDroneBay = true;
    public float DesiredDistance;
    public float MinumumDistance = 10f;
    public bool targetingPlayer = false;
    public bool inFiringCycle = false;
    public int WeaponCurrentMagazineAmmount;
    public int NumberOfHitsPerMag = 0;
    public int CurrentBatteryCapacity;
    public bool currentlyTravelingToDroneBay = false;

    //ENEMY STATS (Changeable)
    public float Speed = 0.7f; // In relation to player's walking speed
    public float Health = 100;
    public int PlayerDetectionRange = 25;
    public int MaxBatteryCapacity = 30; // Depletes one every second it is out of the bay
    public int RechargeTime = 6000; //ms

    //WEAPON STATS (CHANGEABLE)
    public int WeaponDamage = 5; // Damage per hit
    public int WeaponFireRate = 100; // Delay in time between attacks both melee and ranged
    public float WeaponRandomSpread = 5f; // Random direction of lanched projectiles (DOES NOT APPLY TO MELEE ATTACKS)
    public int WeaponRange = 30; // Maximum range of the projectile before it drops off (DOES NOT APPLY TO MELEE ATTACKS)
    public float WeaponProjectileSpeed = 30f; // Speed of launched projectiles (DOES NOT APPLY TO MELEE WEAPONS)
    public int WeaponMagazineSize = 50; // Number of shots the enemy will take before having to reload
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

        StartPosition = transform.position;
        AllPositions = GameObject.FindGameObjectsWithTag("PossiblePositions").ToList();

        target = Instantiate(GameObject.Find("FlyingTarget"), transform.position, Quaternion.identity);

        projectile = GameObject.Find("Projectile");
        player = GameObject.FindGameObjectWithTag("Player");
        LastKnownPlayerLocation = null;
        DroneBayLocation = transform.parent.transform;

        WeaponCurrentMagazineAmmount = WeaponMagazineSize;
        DesiredDistance = WeaponRange - 5;
        CurrentBatteryCapacity = MaxBatteryCapacity;

        InvokeRepeating("UpdatePath", 0f, 0.1f);
        InvokeRepeating("PathfindingTimeout", 0f, 30);
        InvokeRepeating("PathfindingTimeout", 0f, 1f);

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
            ReturnToDroneBay();
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

    //Main logic
    void FixedUpdate()
    {
        // Death
        if (Health <= 0)
        {
            GameObject DeadBody;
            DeadBody = Instantiate(gameObject, transform.position, Quaternion.identity);
            DeadBody.GetComponent<Rigidbody2D>().gravityScale = 3;
            Destroy(GameObject.Find(DeadBody.name).GetComponent<TestEnemyAIAirRanged>());
            Destroy(GameObject.Find(DeadBody.name).GetComponent<Seeker>());

            foreach (Transform child in DeadBody.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            Destroy(gameObject);
        }

        if (CurrentBatteryCapacity <= MaxBatteryCapacity / 10)
        {
            ReturnToDroneBay();
        }

        // See where the enemy wants to go (DEBUGGING ONLY)
        /*
        if (target != null)
        {
            foreach (GameObject pos in AllPositions)
            {
                pos.GetComponent<SpriteRenderer>().enabled = false;
            }
            target.GetComponent<SpriteRenderer>().enabled = true;
        }
        */

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

        if (WeaponCurrentMagazineAmmount > 0 && canFire == true && currentlyReloading == false)
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
        if (DetermineLineOfSight(transform.parent.gameObject, player) && Vector2.Distance(DroneBayLocation.position, player.transform.position) <= PlayerDetectionRange)
        {
            currentlyInDroneBay = false;
            canMove = true;
            targetingPlayer = true;
            canPursue = true;
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
        // Go back to drone bay
        else
        {
            LastKnownPlayerLocation = gameObject;
            ReturnToDroneBay();
            targetingPlayer = false;
        }


        if (currentlyTravelingToDroneBay == false)
        {
            //TARGET DETERMINATION

            // If the player moves away (CHANGE TARGET)
            if (Vector2.Distance(target.transform.position, player.transform.position) > DesiredDistance)
            {
                ComputeClosestPositionToPlayer();
                canMove = true;
            }

            //If the player is too close to the target (CHANGE TARGET)
            if (Vector2.Distance(target.transform.position, player.transform.position) < MinumumDistance)
            {
                ComputeClosestPositionToPlayer();
                canMove = true;
            }

            // If the player is not within line of sight of the desired position (CHANGE TARGET)
            if (DetermineLineOfSight(target, player) == false)
            {
                ComputeClosestPositionToPlayer();
                canMove = true;
            }

            // If the enemy reaches the target
            if (Vector2.Distance(target.transform.position, transform.position) <= 1)
            {
                if (DetermineLineOfSight(gameObject, player)) // If the enemy has LOS on the player
                {

                }
                else
                {
                    ComputeClosestPositionToPlayer();
                    canMove = true;
                }
            }

            // If the enemy has not yet reached the target
            else
            {

            }
        }
    }

    // Use Ranged Weapon
    async Task UseWeapon() // Weapon for projectile based enemy
    {
        inFiringCycle = true;

        // Create Projectile
        GameObject BulletInstance;
        BulletInstance = Instantiate(projectile, transform.position, Quaternion.LookRotation(transform.position - GameObject.FindGameObjectWithTag("Player").transform.position + new Vector3(UnityEngine.Random.Range((-1 * WeaponRandomSpread), WeaponRandomSpread), UnityEngine.Random.Range((-1 * WeaponRandomSpread), WeaponRandomSpread), 0)));
        BulletInstance.transform.parent = transform;

        // Send it on it's way
        BulletInstance.GetComponent<Rigidbody2D>().linearVelocity = BulletInstance.transform.forward * -1 * WeaponProjectileSpeed;
        BulletInstance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, BulletInstance.transform.forward) - 90));
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

        NumberOfHitsPerMag = 0;

        canFire = true;
    }

    // Return To Drone Bay
    async Task ReturnToDroneBay()
    {
        if (currentlyTravelingToDroneBay == false)
        {
            currentlyTravelingToDroneBay = true;
            target.transform.position = DroneBayLocation.position;
            if (Vector2.Distance(transform.position, DroneBayLocation.position) <= 0.5)
            {
                transform.position = DroneBayLocation.position;
                currentlyTravelingToDroneBay = false;
                canMove = false;
                currentlyInDroneBay = true;
                seeker.enabled = false;
                await Task.Delay(RechargeTime);
                CurrentBatteryCapacity = MaxBatteryCapacity;
            }
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

    // Partof the pursue cycle
    void ComputeClosestPositionToPlayer()
    {
        // find a target in the air a set radius away from the player
        if (DetermineLineOfSight(player, gameObject))
        {
            target.transform.position = new Vector2((player.transform.position.x + UnityEngine.Random.Range(-MinumumDistance, MinumumDistance)), (player.transform.position.y + MinumumDistance + UnityEngine.Random.Range(-5, 0)));
        }
    }
}