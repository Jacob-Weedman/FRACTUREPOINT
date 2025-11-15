using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Designed by Jacob Weedman
// Part of the "GAMESCRIPTS" required prefab

public class PossiblePositionsSetup : MonoBehaviour
{
    public List<GameObject> groundList;

    void Awake()
    {
        //Setup List
        groundList = GameObject.FindGameObjectsWithTag("Ground").ToList();

        //Iterate through each ground object
        foreach (GameObject ground in groundList)
        {
            float RemainingWidth = ground.transform.localScale.x;

            while (RemainingWidth > (-1 * ground.transform.localScale.x))
            {
                GameObject position;
                position = Instantiate(GameObject.Find("PossiblePosition"), new Vector3((ground.transform.position.x + (GameObject.Find("PossiblePosition").transform.localScale.x / 4) - (RemainingWidth / 2)), (ground.transform.position.y + (GameObject.Find("PossiblePosition").transform.localScale.y / 2) + (ground.transform.localScale.y / 2)), ground.transform.position.z), ground.transform.rotation);
                position.transform.parent = ground.transform;
                RemainingWidth -= GameObject.Find("PossiblePosition").transform.localScale.x * 2;
            }
        }
    }
}
