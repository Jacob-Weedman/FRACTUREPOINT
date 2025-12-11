using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text;

public class Sector1BossScript : MonoBehaviour
{

// Designed by Jacob Weedman

    GameObject Player;
    GameObject Barrel;
    GameObject Target;
    GameObject ChosenFireZone;
    GameObject Spotlight;
    GameObject Camera;
    GameObject BossProjectile;

    public float Phase = 0.5f;
    public float Health = 800;

    public float angle;

    bool canDeploy = true;
    public float DeployTimer = 4; // sec
    float CurrentDeployTimer; // sec
    public List<GameObject> DeployLocations;
    public List<GameObject> FireZones;

    public int Choice; //1 == shoot >1 == deploy
    public float InitialChoiceCountdown = 2f; // sec
    public float ChoiceCountdown; // sec
    float InitialBulletInterval = 0.01f;
    float BulletInterval = 0.01f;
    float InitialMoveCountdown = 2; //sec
    float MoveCountdown = 6; // sec

    float LaunchCooldown = 2f;

    bool canJump = true;
    float InitialJumpDelay = 1000;
    float JumpDelay = 1000;

    bool canShockwaveDamage = true;
    int ShockwaveDamage = 10;
    int WeaponDamage = 1;
    int WeaponRange = 50;
    int WeaponProjectileSpeed = 20;
    int WeaponRandomSpread = 8;

    float MovementSpeed = 2.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Camera = GameObject.FindGameObjectWithTag("MainCamera");
        BossProjectile = GameObject.Find("EnemyProjectile");
        Player = GameObject.FindWithTag("Player");
        Spotlight = transform.Find("SpotlightAxis").gameObject;
        Barrel = transform.Find("Barrel").gameObject;
        Target = GameObject.Find("BossTarget");
        Target.transform.position = transform.position;

        transform.Find("DamagerPiece").gameObject.GetComponent<GenericDamager>().LockDamage = true;

        foreach (Transform spawner in transform)
        {
            if (spawner.name == "SwitchbladeSpawner")
            {
                DeployLocations.Add(spawner.gameObject);
            }
        }

        FireZones = GameObject.FindGameObjectsWithTag("1").ToList();

        ChoiceCountdown = InitialChoiceCountdown;
        
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        Health = gameObject.GetComponent<GenericDestructable>().Health;

        GameObject.Find("HealthIndicator").transform.localScale = new Vector3(Health / 800 * 30, 1, 1);

        if (Phase == 1 && Choice > 1)
        {
            BackgroundMove();
        }

        switch (Phase)
        {
#region Phase1

        case 1: // Phase 1

            if (Health <= 400)
            {
                Health = 400;
                Phase = 1.5f;
                gameObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.up * 60;
            }

            if (ChosenFireZone)
            {
                // Angle spotlight towards fireing zone
                angle = Mathf.Atan2(ChosenFireZone.transform.position.y + 10 - Barrel.transform.position.y, ChosenFireZone.transform.position.x - Barrel.transform.position.x) * Mathf.Rad2Deg;
                angle += 90;
                // Rotate spotlight towards fireing zone
                Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
                Spotlight.transform.rotation = Quaternion.RotateTowards(Spotlight.transform.rotation, targetRotation, 200 * Time.deltaTime);
            }

            // Change attack speed
            if (Health >= 750)
            {
                DeployTimer = 4;
                InitialChoiceCountdown = 3.5f;
                LaunchCooldown = 1.75f;
            }
            else if (Health >= 700)
            {
                DeployTimer = 2;
                InitialChoiceCountdown = 3;
                LaunchCooldown = 1.50f;
            }
            else if (Health >= 650)
            {
                DeployTimer = 1;
                InitialChoiceCountdown = 2.5f;
                LaunchCooldown = 1.25f;
            }
            else if (Health >= 500)
            {
                DeployTimer = 0.5f;
                InitialChoiceCountdown = 1;
                LaunchCooldown = 0.75f;
            }

            

            ChoiceCountdown -= Time.deltaTime;
            if (ChoiceCountdown <= 0)
            {
                ChoiceCountdown = InitialChoiceCountdown;
                Choice = UnityEngine.Random.Range(1, 4);

                if (Choice == 1) // Find random firing zone
                {

                    foreach (GameObject location in FireZones) // Set all fire zones to invisible
                    {
                        //location.GetComponent<SpriteRenderer>().enabled = false;
                    }

                    ChosenFireZone = FireZones[UnityEngine.Random.Range(0, FireZones.Count())];
                    //ChosenFireZone.GetComponent<SpriteRenderer>().enabled = true;

                    Spotlight.transform.Find("Spotlight").gameObject.GetComponent<SpriteRenderer>().enabled = true;
                
                }
                else
                {
                    foreach (GameObject location in FireZones) // Set all fire zones to invisible
                    {
                        //location.GetComponent<SpriteRenderer>().enabled = false;
                    }

                    Spotlight.transform.Find("Spotlight").gameObject.GetComponent<SpriteRenderer>().enabled = false;
                }
            }

            MoveCountdown -= Time.deltaTime;
            if (Vector2.Distance(Target.transform.position, transform.position) <= 0.1)
            {
                if (MoveCountdown <= 0 )
                {
                MoveCountdown = InitialMoveCountdown;
                Target.transform.position = new Vector2(UnityEngine.Random.Range(-20, 13), UnityEngine.Random.Range(2, 9));
                }
            }
            else
            {
                MoveCountdown = InitialMoveCountdown;
            }

            if (Choice > 1) // Deploy
            {
                CurrentDeployTimer -= Time.deltaTime;
                if (CurrentDeployTimer <= 0)
                {
                    canDeploy = true;
                }

                if (canDeploy)
                {
                    CurrentDeployTimer = DeployTimer;
                    canDeploy = false;
                    Deploy();
                }
            }
            else // Shoot
            {
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(-3.5f, 6, transform.position.z), MovementSpeed * Time.deltaTime);

                BulletInterval -= Time.deltaTime;
                if (BulletInterval <= 0)
                {
                    BulletInterval = InitialBulletInterval;
                    Shoot();            
                }

            }
        break;
#endregion

#region Cutscene between 1 - 2

        case 1.5f: // Cutscene between plases 1 & 2
        gameObject.GetComponent<Rigidbody2D>().gravityScale = 3;
        gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
        transform.Find("DamagerPiece").gameObject.GetComponent<GenericDamager>().LockDamage = false;

        gameObject.layer = LayerMask.NameToLayer("FlyingEnemies");

        if (transform.Find("SpotlightAxis")){
            Spotlight.transform.Find("Spotlight").gameObject.GetComponent<SpriteRenderer>().enabled = false;
            Destroy(transform.Find("SpotlightAxis").gameObject);
        }

        foreach (Transform child in transform)
        {
            if (child.name != "DamagerPiece")
            {
                child.gameObject.layer = LayerMask.NameToLayer("FlyingEnemies");
            }
        }

        
        if (transform.position.y <= 1.5)
        {
            Phase = 2;
        }

        break;

#endregion

#region Phase2
            case 2: // Phase 2

                // Change attack speed
                if (Health >= 550)
                {
                    float InitialJumpDelay = 1000;
                }
                else if (Health >= 400)
                {
                    float InitialJumpDelay = 600;
                }
                else if (Health >= 300)
                {
                    float InitialJumpDelay = 300;
                }
                else if (Health >= 200)
                {
                    float InitialJumpDelay = 200;
                }

                if (Health <= 0)
                {
                    Health = 0;
                    Phase = 2.5f;
                }

                if (gameObject.GetComponent<Rigidbody2D>().linearVelocity.y < 0 && transform.position.y < 1)
                {
                    if (canShockwaveDamage)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            // Spawn Grunt
                            GameObject GrenadeInstance = Instantiate(GameObject.Find("EnemyGrenade").gameObject, transform.position, Quaternion.identity);
                            GrenadeInstance.GetComponent<Rigidbody2D>().gravityScale = 1.5f;
                            GrenadeInstance.GetComponent<EnemyGrenade>().enabled = true;
                            GrenadeInstance.GetComponent<EnemyGrenade>().ExplodeOnContact = true;
                            GrenadeInstance.GetComponent<EnemyGrenade>().Duration = UnityEngine.Random.Range(3, 8);
                            GrenadeInstance.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(UnityEngine.Random.Range(-25,25), UnityEngine.Random.Range(10,25));
                        }

                        // Damage Boss
                        gameObject.GetComponent<GenericDestructable>().Health -= ShockwaveDamage;

                        // Shake Camera
                        Camera.GetComponent<CameraMovement>().shakeCamera(0.8f, 0.5f);
                        canJump = true;
                        if (Player.transform.GetComponentInChildren<GroundCheck>().isGrounded == true)
                        {
                            // Shockwave Damage
                            GameObject.Find("GameData").GetComponent<GameData>().CurrentHealth -= ShockwaveDamage;
                        }
                    }
                    canShockwaveDamage = false;
                }

                if (canJump)
                {
                    Jump();
                }

                break;

#region Death Animation
    case 2.5f:
        Phase = 3;
        break;
#endregion

#region Death
    case 3:
        break;
#endregion
            
            default:
                break;
        }
#endregion

    }

    async Awaitable Deploy()
    {
        // Find Which module to deploy from
        GameObject ChosenSpawner = DeployLocations[UnityEngine.Random.Range(0, DeployLocations.Count())];

        GameObject Switchblade;
        Switchblade = Instantiate(GameObject.Find("SwitchbladeEnemy"), new Vector3(ChosenSpawner.transform.position.x, ChosenSpawner.transform.position.y, -1), Quaternion.identity);
        Switchblade.transform.parent = gameObject.transform;

        Switchblade.GetComponent<MasterEnemyAI>().enabled = true;
        Switchblade.GetComponent<MasterEnemyAI>().AbilityMove = false;
        Switchblade.GetComponent<MasterEnemyAI>().AbilityDash = false;

        await Awaitable.WaitForSecondsAsync(LaunchCooldown);

        Switchblade.GetComponent<Seeker>().enabled = true;
        Switchblade.GetComponent<MasterEnemyAI>().AbilityMove = true;
        Switchblade.GetComponent<MasterEnemyAI>().AbilityDash = true;

        // Launch
        Switchblade.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(UnityEngine.Random.Range(-15,15), 20);

        Switchblade.transform.parent = null;
    }

    async Awaitable Jump()
    {
        canJump = false;
        canShockwaveDamage = true;

        await Awaitable.WaitForSecondsAsync(1);

        // Jump Towards Player
        if (Player.transform.position.x - transform.position.x > 0) // Jump right
        {
            GetComponent<Rigidbody2D>().linearVelocity = new Vector2(15, 30);
        }
        else if (Player.transform.position.x - transform.position.x < 0) // Jump left
        {
            GetComponent<Rigidbody2D>().linearVelocity = new Vector2(-15, 30);
        }
        else // Jump straight up
        {
            GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, 30);
        }
        
    }

    void Shoot()
    {
        if (ChosenFireZone)
        {
        // Create Projectile
        GameObject BulletInstance;
        BulletInstance = Instantiate(BossProjectile, Barrel.transform.position, Quaternion.LookRotation(transform.position - ChosenFireZone.transform.position + new Vector3(UnityEngine.Random.Range((-1 * WeaponRandomSpread), WeaponRandomSpread), UnityEngine.Random.Range((-1 * WeaponRandomSpread), WeaponRandomSpread), 0)));
        //BulletInstance.transform.parent = transform;

        // Variables
        BulletInstance.GetComponent<EnemyProjectileScript>().WeaponDamage = WeaponDamage;
        BulletInstance.GetComponent<EnemyProjectileScript>().WeaponRange = WeaponRange;

        // Send it on it's way
        BulletInstance.GetComponent<Rigidbody2D>().linearVelocity = BulletInstance.transform.forward * -1 * WeaponProjectileSpeed;
        BulletInstance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, BulletInstance.transform.forward) - 90));
        }       
    }

    void BackgroundMove()
    {
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(Target.transform.position.x, Target.transform.position.y, transform.position.z), MovementSpeed * Time.deltaTime);
    }

}
