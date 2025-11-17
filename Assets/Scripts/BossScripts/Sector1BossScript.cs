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

    GameObject Player;

    public int Phase = 1;

    bool canDeploy = true;
    const int DEPLOYTIMER = 5000; // ms
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
    void Update()
    {
        switch (Phase)
        {
            case 1:
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
        Switchblade = Instantiate(GameObject.Find("SwitchbladeEnemy"), new Vector3(ChosenSpawner.transform.position.x, ChosenSpawner.transform.position.y, 0), Quaternion.identity);

        Switchblade.GetComponent<Seeker>().enabled = true;
        Switchblade.GetComponent<MasterEnemyAI>().enabled = true;

        Switchblade.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 10);

        await Task.Delay(DEPLOYTIMER);

        canDeploy = true;
    }
}
