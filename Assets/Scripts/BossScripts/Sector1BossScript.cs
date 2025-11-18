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

    public int Phase = 1;
    public float Health = 1000;

    bool canDeploy = true;
    public int DeployTimer = 4000; // ms
    public List<GameObject> DeployLocations;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player = GameObject.FindWithTag("Player");
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

        if (Health <= 500)
        {
            Phase = 2;
        }

        switch (Phase)
        {
            case 1:

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

                if (canDeploy)
                {
                    Deploy();
                }

                break;
            case 2:

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
}
