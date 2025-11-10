using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Designed by Jacob Weedman
//Belongs to the "GameData" GameObject
//V 0.0.1

public class GameData : MonoBehaviour
{
    public static GameData Instance;

    // Game Variables
    public string CharacterName; //
    public string CurrentSector;
    public string CurrentLevel;
    public int MaxHealth;
    public int CurrentHealth;
    public int CurrentExp;
    public int LevelsCompleted;
    public int BossesDefeated;
    public int EnemiesKilled;

    //Changable setting while playing
    public static float CameraOffset = 5f;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        //Initialize Variables
        CharacterName = null; //
        CurrentSector = "0";
        CurrentLevel = "0";
        MaxHealth = 100;
        CurrentHealth = 100;
        CurrentExp = 0;
        LevelsCompleted = 0;
        BossesDefeated = 0;
        EnemiesKilled = 0;
    }
}
