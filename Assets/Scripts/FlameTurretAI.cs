using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text;

//Designed by Jacob Weedman
//Use on Flamethrower turrets

public class FlameTurretAI : MonoBehaviour
{

//MISC
    GameObject projectile;
    GameObject player;
    GameObject barrel;
    float DistanceFromPlayer;
    Vector2 StartPosition;

//CONDITIONS/GENERAL INFORMATION
    bool canFire = true;
    bool currentlyReloading = false;
    bool targetingPlayer = false;
    bool inFiringCycle = false;
    int WeaponCurrentMagazineAmmount;
    float angle;

//ENEMY STATS (Changeable)
    public float Health = 100;
    public int PlayerDetectionRange = 40;

//WEAPON STATS (CHANGEABLE)
    public int WeaponDamage = 1; // Damage per hit
    public int WeaponFireRate = 50; // Delay in time between attacks both melee and ranged
    public float WeaponRandomSpread = 2.5f; // Random direction of lanched projectiles
    public int WeaponRange = 20; // Maximum range of the projectile before it drops off
    public float WeaponProjectileSpeed = 20f; // Speed of launched projectiles
    public int WeaponMagazineSize = 80; // Number of shots the enemy will take before having to reload
    public int WeaponReloadTime = 10000; // Time it takes to reload the magazine
    
//ONCE THE GAME STARTS
    void Start()
    {
        transform.Find("ReloadingIndicator").GetComponent<SpriteRenderer>().enabled = false;
        transform.Find("PursuingIndicator").GetComponent<SpriteRenderer>().enabled = false;

        projectile = GameObject.Find("EnemyProjectile");
        player = GameObject.FindGameObjectWithTag("Player");
        barrel = transform.Find("Barrel").gameObject;

        WeaponCurrentMagazineAmmount = WeaponMagazineSize;

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

        if (currentlyReloading)
        {
            transform.Find("ReloadingIndicator").GetComponent<SpriteRenderer>().enabled = true;
        }
        else
        {
            transform.Find("ReloadingIndicator").GetComponent<SpriteRenderer>().enabled = false;
        }

//RANGED ATTACK
        // Check if enemy has line of sight on the player & if they are in the acceptable range       
        if (WeaponCurrentMagazineAmmount > 0 && canFire == true && currentlyReloading == false)
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

        GameObject Flame;
        Flame = Instantiate(GameObject.Find("Flame"), new Vector3(transform.position.x + UnityEngine.Random.Range(-0.5f, 0.5f), transform.position.y + UnityEngine.Random.Range(-0.5f, 0.5f), GameObject.Find("EvilAura").transform.position.z), Quaternion.identity);
        Flame.transform.rotation = Quaternion.Euler(Vector3.forward * UnityEngine.Random.Range(-90, 90));

        // Send it on its way
        Flame.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(transform.position.x - player.transform.position.x + UnityEngine.Random.Range(-WeaponRandomSpread, WeaponRandomSpread), transform.position.y - player.transform.position.y + UnityEngine.Random.Range(-WeaponRandomSpread, WeaponRandomSpread)).normalized * 3 * WeaponRange * -1;

        //Set Variables
        Flame.GetComponent<ParticleWeapon>().destroy = true;
        Flame.GetComponent<ParticleWeapon>().opacity = true;
        Flame.GetComponent<ParticleWeapon>().destroyOnCollide= true;
        Flame.GetComponent<ParticleWeapon>().damageAmmount = WeaponDamage;

        await Task.Delay(WeaponFireRate);

        canFire = true;
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