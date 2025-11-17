using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Designed by Jacob Weedman
//Belongs to the "WorldManager" GameObject in the "LevelSelector" scene
//V 0.0.1

public class WorldManager : MonoBehaviour
{

    string[] LevelsCompleted;
    string levelChosen;
    public string currentSector;
    void Update()
    {
        if (GameObject.Find("GameData").GetComponent<GameData>().CurrentSector == "0")
        {
            GameObject.Find("GameData").GetComponent<GameData>().CurrentSector = "1";
        }

        currentSector = GameObject.Find("CurrentSector").tag;
        levelChosen = tag;

        if ((currentSector != "Untagged") && (levelChosen != "Untagged"))
        {
            ChangeLevel();
        }
    }

    void ChangeLevel()
    {
        if (levelChosen == "test")
        {
            //SceneManager.LoadScene("WorkingPathfindingTest");
            SceneManager.LoadScene("Sector1Boss");
        }
        else
        {
            SceneManager.LoadScene("Sector" + currentSector + "Level" + levelChosen);
        }
    }

}   