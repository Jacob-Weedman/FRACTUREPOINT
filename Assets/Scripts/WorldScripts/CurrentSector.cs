using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Designed by Jacob Weedman
//Belongs to the "CurrentSector" GameObject in the "LevelSelector" scene
//V 0.0.1

public class CurrentSector : MonoBehaviour
{
    void Update()
    {
        if (GameObject.Find("GameData").GetComponent<GameData>().CurrentSector != "0")
        {
            tag = GameObject.Find("GameData").GetComponent<GameData>().CurrentSector;
        }
    }
}
