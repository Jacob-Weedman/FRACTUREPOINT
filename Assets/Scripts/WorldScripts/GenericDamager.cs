using UnityEngine;

public class GenericDamager : MonoBehaviour
{
    // Designed by Jacob Weedman
    // Attatch to any game object you want to damage something

    public bool DamagePlayer = true;
    public bool DamageEnemy;
    public float Damage = 10;
    public float InitialDamageInterval = 0.5f; // sec
    public float DamageInterval;
    public bool CurrentlyColliding = false;
    public bool CanDamage = true;

    public bool LockDamage = false;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (CurrentlyColliding)
        {
            if (DamageInterval >= InitialDamageInterval)
            {
                CanDamage = true;
                DamageInterval = 0;
            }
            else
            {
                DamageInterval += Time.deltaTime;
            }
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (CanDamage == true && LockDamage == false)
        {
            if (collision.GetComponent<MasterEnemyAI>() && DamageEnemy == true)
            {
                CanDamage = false;
                collision.GetComponent<MasterEnemyAI>().Health -= Damage;
            }
            if (collision.gameObject.tag == "Player" && DamagePlayer == true)
            {
                CanDamage = false;
                GameObject.Find("GameData").GetComponent<GameData>().CurrentHealth -= Damage;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<MasterEnemyAI>() && DamageEnemy == true || collision.gameObject.tag == "Player" && DamagePlayer == true)
        {
            CurrentlyColliding = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<MasterEnemyAI>() && DamageEnemy == true || collision.gameObject.tag == "Player" && DamagePlayer == true)
        {
            CurrentlyColliding = false;
            DamageInterval = 0;
            CanDamage = true;
        }
    }
}
