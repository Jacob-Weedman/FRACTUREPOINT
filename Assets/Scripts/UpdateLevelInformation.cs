using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Designed by Jacob Weedman
//Belongs to "UpdateLevelInformation" GameObject within every single level
//V 0.0.1

public class UpdateLevelInformation : MonoBehaviour
{
    public string SectorThisBelongsTo; //Change this in unity
    public string LevelThisBelongsTo; //Change this in unity
    void Start()
    {
        GameObject.Find("GameData").GetComponent<GameData>().CurrentSector = SectorThisBelongsTo;
        GameObject.Find("GameData").GetComponent<GameData>().CurrentLevel = LevelThisBelongsTo;
    }
}
