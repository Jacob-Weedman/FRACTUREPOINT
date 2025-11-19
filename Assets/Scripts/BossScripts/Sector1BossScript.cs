using UnityEngine;
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

    public int Phase = 1;
    public float Health = 1000;

    bool canDeploy = true;
    public int DeployTimer = 4000; // ms
    public List<GameObject> DeployLocations;

    public int Choice; // 0 == deploy, 1 == shoot
    float InititialChoiceCountdown = 5; // sec
    float ChoiceCountdown = 5; // sec
    float InitialBulletInterval = 0.1f;
    float BulletInterval = 0.1f;
    float InitialMoveCountdown = 2; //sec
    float MoveCountdown = 6; // sec

    int WeaponDamage = 7;
    int WeaponRange = 50;
    int WeaponProjectileSpeed = 40;
    int WeaponRandomSpread = 5;

    float MovementSpeed = 2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player = GameObject.FindWithTag("Player");
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
    }


    // Update is called once per frame
    void FixedUpdate()
    {

        if (Phase == 1)
        {
            BackgroundMove();
        }

        if (Health <= 500)
        {
            Phase = 2;
        }

        switch (Phase)
        {
            case 1: // Phase 1

                if (Health >= 900)
            {
                DeployTimer = 4000;
            }
            else if (Health >= 800)
            {
                DeployTimer = 2000;
            }
            else if (Health >= 700)
            {
                DeployTimer = 1000;
            }
            else if (Health >= 600)
            {
                DeployTimer = 50;
            }

            ChoiceCountdown -= Time.deltaTime;
            if (ChoiceCountdown <= 0)
            {
                ChoiceCountdown = InititialChoiceCountdown;
                Choice = UnityEngine.Random.Range(0, 2);
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
                if (canDeploy)
                {
                    Deploy();
                }
                break;

                case 1:
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
            case 2: // Phase 2

                break;
            default:
                break;
        }
            
    }

    async Task Deploy()
    {
        canDeploy = false;

        // Find Which module to deploy from
        GameObject ChosenSpawner = DeployLocations[UnityEngine.Random.Range(0, DeployLocations.Count() - 1)];

        GameObject Switchblade;
        Switchblade = Instantiate(GameObject.Find("SwitchbladeEnemy"), new Vector3(ChosenSpawner.transform.position.x, ChosenSpawner.transform.position.y, -1), Quaternion.identity);

        Switchblade.GetComponent<Seeker>().enabled = true;
        Switchblade.GetComponent<MasterEnemyAI>().enabled = true;

        Switchblade.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, 5);

        await Task.Delay(DeployTimer);

        canDeploy = true;
    }

    void Shoot()
    {
        // Create Projectile
        GameObject BulletInstance;
        BulletInstance = Instantiate(GameObject.Find("EnemyProjectile"), Barrel.transform.position, Quaternion.LookRotation(transform.position - GameObject.FindGameObjectWithTag("Player").transform.position + new Vector3(UnityEngine.Random.Range((-1 * WeaponRandomSpread), WeaponRandomSpread), UnityEngine.Random.Range((-1 * WeaponRandomSpread), WeaponRandomSpread), 0)));
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
