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

    public List<string> EnemyList;

    public float Phase = 0.5f;
    public float Health = 1200;

    bool canSpawn = true;
    public List<GameObject> SpawnLocations;

    float InitialSpawnCooldown = 5;
    float SpawnCooldown;


    void Awake()
    {
        SpawnLocations = GameObject.FindGameObjectsWithTag("1").ToList();
        
        //Add initial enemies to spawn
        EnemyList.Add("Brute");
        //EnemyList.Add("Grunt");
        //EnemyList.Add("Sniper");

        // Setup
        SpawnCooldown = InitialSpawnCooldown;
    }

    void FixedUpdate()
    {
        switch (Phase)
        {
#region Phase1

        case 1: // Phase 1

        SpawnCooldown -= Time.deltaTime;

        if (SpawnCooldown <= 0)
        {
            SpawnCooldown = InitialSpawnCooldown;
            SpawnEnemy();            
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

    async Task SpawnEnemy()
    {
        // Choose firing zone
        ChosenSpawnZone = SpawnLocations[UnityEngine.Random.Range(0, SpawnLocations.Count)];

        // Choose enemy to spawn
        GameObject ChosenEnemy = GameObject.Find("EnemiesToSpawn").transform.Find(EnemyList[UnityEngine.Random.Range(0, EnemyList.Count())]).gameObject;

        // Give life
        GameObject InstantiatedEnemy = Instantiate(ChosenEnemy, ChosenSpawnZone.transform.position, Quaternion.identity);
        await Task.Delay(1000);
        InstantiatedEnemy.GetComponent<Rigidbody2D>().gravityScale = 1.5f;
        InstantiatedEnemy.GetComponent<MasterEnemyAI>().enabled = true;

    }

}
