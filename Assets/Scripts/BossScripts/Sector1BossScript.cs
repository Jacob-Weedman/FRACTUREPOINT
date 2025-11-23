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

    public float Phase = 0.5f;
    public float Health = 1000;

    public float angle;

    bool canDeploy = true;
    public float DeployTimer = 4; // sec
    float CurrentDeployTimer; // sec
    public List<GameObject> DeployLocations;
    public List<GameObject> FireZones;

    public int Choice; // 0 == deploy, 1 == shoot
    float InititialChoiceCountdown = 5f; // sec
    float ChoiceCountdown; // sec
    float InitialBulletInterval = 0.01f;
    float BulletInterval = 0.01f;
    float InitialMoveCountdown = 2; //sec
    float MoveCountdown = 6; // sec

    int WeaponDamage = 3;
    int WeaponRange = 50;
    int WeaponProjectileSpeed = 20;
    int WeaponRandomSpread = 7;

    float MovementSpeed = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player = GameObject.FindWithTag("Player");
        Spotlight = transform.Find("SpotlightAxis").gameObject;
        Barrel = transform.Find("Barrel").gameObject;
        Target = GameObject.Find("BossTarget");
        Target.transform.position = transform.position;

        foreach (Transform spawner in transform)
        {
            if (spawner.name == "SwitchbladeSpawner")
            {
                DeployLocations.Add(spawner.gameObject);
            }
        }

        FireZones = GameObject.FindGameObjectsWithTag("1").ToList();

        ChoiceCountdown = InititialChoiceCountdown;
        
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        Health = gameObject.GetComponent<GenericDestructable>().Health;

        GameObject.Find("HealthIndicator").transform.localScale = new Vector3(Health / 1000 * 30, 1, 1);

        if (Phase == 1 && Choice == 0)
        {
            BackgroundMove();
        }

        switch (Phase)
        {
#region Phase1

            case 1: // Phase 1

            if (Health <= 500)
            {
                Health = 500;
                Phase = 1.5f;
                gameObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.up * 100;
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

            if (Health >= 900)
            {
                DeployTimer = 4;
                InititialChoiceCountdown = 5;
            }
            else if (Health >= 800)
            {
                DeployTimer = 2;
                InititialChoiceCountdown = 4;
            }
            else if (Health >= 700)
            {
                DeployTimer = 1;
                InititialChoiceCountdown = 2.5f;
            }
            else if (Health >= 600)
            {
                DeployTimer = 0.5f;
                InititialChoiceCountdown = 1;
            }

            

            ChoiceCountdown -= Time.deltaTime;
            if (ChoiceCountdown <= 0)
            {
                ChoiceCountdown = InititialChoiceCountdown;
                Choice = UnityEngine.Random.Range(0, 2);

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
                MoveCountdown = InitialMoveCountdown   ;
            }

            switch (Choice)
            {
                case 0: // deploy

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
                break;

                case 1:

                transform.position = Vector3.MoveTowards(transform.position, new Vector3(-3.5f, 6, transform.position.z), MovementSpeed * Time.deltaTime);

                BulletInterval -= Time.deltaTime;
                if (BulletInterval <= 0)
                    {
                        BulletInterval = InitialBulletInterval;
                        Shoot();            
                    }

                break;

                default:
                break;               
            }
                break;

#endregion

#region Cutscene between 1 - 2

    case 1.5f: // Cutscene between plases 1 & 2
        gameObject.GetComponent<Rigidbody2D>().gravityScale = 3;
        gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
        gameObject.layer = LayerMask.NameToLayer("FlyingEnemies");

        if (transform.position.y <= 1.5)
        {
            Phase = 2;
        }

        break;

#endregion

#region Phase2
            case 2: // Phase 2

                break;
            default:
                break;
        }
#endregion   
    }

    async Task Deploy()
    {
        // Find Which module to deploy from
        GameObject ChosenSpawner = DeployLocations[UnityEngine.Random.Range(0, DeployLocations.Count())];

        GameObject Switchblade;
        Switchblade = Instantiate(GameObject.Find("SwitchbladeEnemy"), new Vector3(ChosenSpawner.transform.position.x, ChosenSpawner.transform.position.y, -1), Quaternion.identity);
        Switchblade.transform.parent = gameObject.transform;

        Switchblade.GetComponent<MasterEnemyAI>().enabled = true;
        Switchblade.GetComponent<MasterEnemyAI>().AbilityMove = false;
        Switchblade.GetComponent<MasterEnemyAI>().AbilityDash = false;

        await Task.Delay(1000);

        Switchblade.GetComponent<Seeker>().enabled = true;
        Switchblade.GetComponent<MasterEnemyAI>().AbilityMove = true;
        Switchblade.GetComponent<MasterEnemyAI>().AbilityDash = true;

        Switchblade.transform.parent = null;
        Switchblade.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, 20);
    }

    void Shoot()
    {
        // Create Projectile
        GameObject BulletInstance;
        BulletInstance = Instantiate(GameObject.Find("EnemyProjectile"), Barrel.transform.position, Quaternion.LookRotation(transform.position - ChosenFireZone.transform.position + new Vector3(UnityEngine.Random.Range((-1 * WeaponRandomSpread), WeaponRandomSpread), UnityEngine.Random.Range((-1 * WeaponRandomSpread), WeaponRandomSpread), 0)));
        //BulletInstance.transform.parent = transform;

        // Variables
        BulletInstance.GetComponent<EnemyProjectileScript>().WeaponDamage = WeaponDamage;
        BulletInstance.GetComponent<EnemyProjectileScript>().WeaponRange = WeaponRange;

        // Send it on it's way
        BulletInstance.GetComponent<Rigidbody2D>().linearVelocity = BulletInstance.transform.forward * -1 * WeaponProjectileSpeed;
        BulletInstance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Vector2.SignedAngle(Vector2.right, BulletInstance.transform.forward) - 90));
                
    }

    void BackgroundMove()
    {
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(Target.transform.position.x, Target.transform.position.y, transform.position.z), MovementSpeed * Time.deltaTime);
    }

}
