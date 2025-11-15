using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text;

//Designed by Jacob Weedman
//Use on rocket turrets

public class RocketTurretAI : MonoBehaviour
{

//MISC
    GameObject projectile;
    GameObject player;
    GameObject barrel;
    float DistanceFromPlayer;
    Vector2 StartPosition;

//CONDITIONS/GENERAL INFORMATION
    bool canFire = true;
    bool targetingPlayer = false;
    bool inFiringCycle = false;
    float angle;

//ENEMY STATS (Changeable)
    public float Health = 100;
    public int PlayerDetectionRange = 50;

//WEAPON STATS (CHANGEABLE)
    public int WeaponDamage = 30; // Damage per hit
    public int WeaponFireRate = 2500; // Delay in time between attacks both melee and ranged
    public float WeaponRandomSpread = 0.5f; // Random direction of lanched projectiles
    public int WeaponRange = 40; // Maximum range of the projectile before it drops off
    public float WeaponProjectileSpeed = 50f; // Speed of launched projectiles
    
//ONCE THE GAME STARTS
    void Start()
    {
        transform.Find("PursuingIndicator").GetComponent<SpriteRenderer>().enabled = false;

        projectile = GameObject.Find("EnemyRocketWeapon");
        player = GameObject.FindGameObjectWithTag("Player");
        barrel = transform.Find("Barrel").gameObject;

        canFire = true;
    }

//MAIN LOGIC
    void FixedUpdate()
    {
        DistanceFromPlayer = Vector2.Distance(transform.position, player.transform.position);

// Death
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

//RANGED ATTACK
        // Check if enemy has line of sight on the player & if they are in the acceptable range       
        if (canFire == true)
        {
            if (DetermineLineOfSight(gameObject, player) == true && Vector2.Distance(transform.position, player.transform.position) <= WeaponRange)
            {
                UseWeapon();
            }
        }

//PLAYER TARGETING
        if (DetermineLineOfSight(gameObject, player) == true && Vector2.Distance(transform.position, player.transform.position) <= WeaponRange + 10)
        {
            targetingPlayer = true;

            // Set angle to player
            angle = Mathf.Atan2(player.transform.position.y - barrel.transform.position.y, player.transform.position.x - barrel.transform.position.x) * Mathf.Rad2Deg;

        }
        else
        {
            // Reset barrel rotation
            angle = 90f;
        }
        
        // Rotate barrel towards player
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        barrel.transform.rotation = Quaternion.RotateTowards(barrel.transform.rotation, targetRotation, 100 * Time.deltaTime);

    }

    //MISC METHODS
    // Use Ranged Weapon
    async Task UseWeapon()
    {
        canFire = false;
        inFiringCycle = true;

        GameObject Rocket;
        Rocket = Instantiate(GameObject.Find("EnemyRocket"), new Vector3(transform.position.x + UnityEngine.Random.Range(-0.5f, 0.5f), transform.position.y + UnityEngine.Random.Range(-0.5f, 0.5f), GameObject.Find("EvilAura").transform.position.z), Quaternion.identity);
        Rocket.transform.rotation = Quaternion.Euler(Vector3.forward * UnityEngine.Random.Range(-90, 90));

        // Send it on its way
        Rocket.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(transform.position.x - player.transform.position.x + UnityEngine.Random.Range(-WeaponRandomSpread, WeaponRandomSpread), transform.position.y - player.transform.position.y + UnityEngine.Random.Range(-WeaponRandomSpread, WeaponRandomSpread)).normalized * WeaponProjectileSpeed * -1;
        Rocket.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, Rocket.transform.forward) - 90));

        // Variables
        Rocket.GetComponent<EnemyRocket>().WeaponDamage = WeaponDamage;

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