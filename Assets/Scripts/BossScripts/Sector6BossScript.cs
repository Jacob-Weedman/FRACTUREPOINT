using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text;

public class Sector6BossScript : MonoBehaviour
{

// Designed by Jacob Weedman

    GameObject Player;
    GameObject ChosenSpawnZone;
    GameObject Camera;

    public Dictionary<string, int> EnemyDict = new Dictionary<string, int>();
    public List<string> EnemyList;

    public float Phase = 1f;
    public float Health = 1200;

    bool canSpawn = true;
    public List<GameObject> SpawnLocations;

    float InitialSpawnCooldown = 10;
    public float SpawnCooldown;


    void Awake()
    {
        // Add spawn locations to list
        SpawnLocations = GameObject.FindGameObjectsWithTag("1").ToList();

        // Setup
        SpawnCooldown = InitialSpawnCooldown;
    }

    void FixedUpdate()
    {
        // Health
        GameObject.Find("HealthIndicator").transform.localScale = new Vector3(Health / 1200 * 30, 1, 1);

        if (Health >= 1100)
        {
            InitialSpawnCooldown = 9;
            EnemyDict["Grunt"] = 1;
        }
        else if (Health >= 1000)
        {
            InitialSpawnCooldown = 8;
            EnemyDict["Grunt"] = 2;
            EnemyDict["Sniper"] = 1;
        }
        else if (Health >= 900)
        {
            InitialSpawnCooldown = 7;
            EnemyDict["Grunt"] = 5;
            EnemyDict["Sniper"] = 2;
            EnemyDict["Brute"] = 1;
        }
        else if (Health >= 800)
        {
            InitialSpawnCooldown = 6;
            EnemyDict["Grunt"] = 6;
            EnemyDict["Sniper"] = 3;
            EnemyDict["Brute"] = 2;
            EnemyDict["Warper"] = 1;
        }
        else if (Health >= 700)
        {
            InitialSpawnCooldown = 5;
            EnemyDict["Grunt"] = 3;
            EnemyDict["Sniper"] = 4;
            EnemyDict["Brute"] = 3;
            EnemyDict["Warper"] = 3;
        }
        else if (Health >= 600)
        {
            InitialSpawnCooldown = 4;
            EnemyDict["Sniper"] = 1;
            EnemyDict["Warper"] = 1;
            EnemyDict["Grunt"] = 0;
            EnemyDict["Brute"] = 0;
        }

        switch (Phase)
        {
#region Phase1

        case 1: // Phase 1

        // Decriment Health Automatically
        Health -= Time.deltaTime * 5;

        SpawnCooldown -= Time.deltaTime;

        if (SpawnCooldown <= 0)
        {
            SpawnCooldown = InitialSpawnCooldown;
            SpawnEnemy();
            Debug.Log("Spawned Enemy");
        }

        break; 
#endregion

#region Cutscene between 1 - 2

        case 1.5f: // Cutscene between plases 1 & 2

        break;

#endregion

#region Phase2
        case 2: // Phase 2

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

    async Awaitable SpawnEnemy()
    {
        // Put enemies into a list
        EnemyList.Clear();
        foreach (KeyValuePair<string, int> entry in EnemyDict)
        {
           int EndAmmount = entry.Value;
           for (int j = 0; j < EndAmmount; j++)
            {
                EnemyList.Add(entry.Key);
            }
        }
        // Choose firing zone
        ChosenSpawnZone = SpawnLocations[UnityEngine.Random.Range(0, SpawnLocations.Count)];

        // Choose enemy to spawn
        GameObject ChosenEnemy = GameObject.Find("EnemiesToSpawn").transform.Find(EnemyList[UnityEngine.Random.Range(0, EnemyList.Count())]).gameObject;

        // Give life
        GameObject InstantiatedEnemy = Instantiate(ChosenEnemy, ChosenSpawnZone.transform.position, Quaternion.identity);
        await Awaitable.WaitForSecondsAsync(1000/1000);
        InstantiatedEnemy.GetComponent<Rigidbody2D>().gravityScale = 1.5f;
        InstantiatedEnemy.GetComponent<MasterEnemyAI>().enabled = true;
        InstantiatedEnemy.GetComponent<MasterEnemyAI>().AbilityPlayerESP = true;

    }

}
