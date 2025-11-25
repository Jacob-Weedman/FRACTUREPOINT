using UnityEngine;

public class GenericDamage : MonoBehaviour
{

    // Designed by Jacob Weedman

    public float DamageAmmount = 10;
    public float DamageInterval = 10; //sec
    float CurrentDamageInterval;

    void Awake()
    {
        CurrentDamageInterval = DamageInterval;
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (CurrentDamageInterval == DamageInterval)
            {
                GameObject.Find("GameData").GetComponent<GameData>().CurrentHealth -= DamageAmmount;
            }
            else
            {
                CurrentDamageInterval -= Time.deltaTime;
            }
        }
        if (CurrentDamageInterval <= 0)
        {
            CurrentDamageInterval = DamageInterval;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        CurrentDamageInterval = DamageInterval;
    }

}
