using UnityEngine;
using System;

//Designed by Jacob Weedman

public class EnemyProjectileScript : MonoBehaviour
{
    Vector2 StartPosition;

    public int WeaponDamage;
    public int WeaponRange;
    
    void Awake()
    {
        StartPosition = transform.position;
        GetComponent<SpriteRenderer>().enabled = true;
    }

    void Update()
    {
        //transform.rotation = Quaternion.Euler(new Vector3(0, 0, GetComponent<Rigidbody2D>().angularVelocity));//FIX THIS

        if (Vector2.Distance(transform.position, StartPosition) > WeaponRange)
        {
            GetComponent<Rigidbody2D>().gravityScale = 0.5f;

        }

        if (transform.position.y <= -10000)
        {
            Destroy(gameObject);
        }
    }
    /*
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            //Damage
            GameObject.Find("GameData").GetComponent<GameData>().CurrentHealth -= WeaponDamage;

            //Send Hit Data (for repositioning)
            //if (GetComponentInParent<MasterEnemyAI>())
            //{
            //    GetComponentInParent<MasterEnemyAI>().NumberOfHitsPerMag += 1;
            //}

            Destroy(gameObject);
        }

        if (collision.gameObject.tag == "Ground")
        {
            if (collision.gameObject.layer == 6)
            {
                Destroy(gameObject);
            }
        }
    }
    */
}
