using UnityEngine;
using System;

//Designed by Jacob Weedman
//V 0.0.2

public class ProjectileScript : MonoBehaviour
{
    Vector2 StartPosition;

    int WeaponDamage;
    int WeaponRange;
    
    void Start()
    {
        StartPosition = transform.position;
        GetComponent<SpriteRenderer>().enabled = true;
/*
        if (GetComponentInParent<GruntAI>())
        {
            WeaponDamage = GetComponentInParent<GruntAI>().WeaponDamage;
            WeaponRange = GetComponentInParent<GruntAI>().WeaponRange;
        }

        if (GetComponentInParent<SniperAI>())
        {
            WeaponDamage = GetComponentInParent<SniperAI>().WeaponDamage;
            WeaponRange = GetComponentInParent<SniperAI>().WeaponRange;
        }
        if (GetComponentInParent<RegularFlyingAI>())
        {
            WeaponDamage = GetComponentInParent<RegularFlyingAI>().WeaponDamage;
            WeaponRange = GetComponentInParent<RegularFlyingAI>().WeaponRange;
        }
        if (GetComponentInParent<LightDroneAI>())
        {
            WeaponDamage = GetComponentInParent<LightDroneAI>().WeaponDamage;
            WeaponRange = GetComponentInParent<LightDroneAI>().WeaponRange;
        }
*/
        if (GetComponentInParent<TestEnemyAIAirRanged>())
        {
            WeaponDamage = GetComponentInParent<TestEnemyAIAirRanged>().WeaponDamage;
            WeaponRange = GetComponentInParent<TestEnemyAIAirRanged>().WeaponRange;
        }

        if (GetComponentInParent<TestEnemyAIGroundRanged>())
        {
            WeaponDamage = GetComponentInParent<TestEnemyAIGroundRanged>().WeaponDamage;
            WeaponRange = GetComponentInParent<TestEnemyAIGroundRanged>().WeaponRange;
        }
        if (GetComponentInParent<TestEnemyAITurret>())
        {
            WeaponDamage = GetComponentInParent<TestEnemyAITurret>().WeaponDamage;
            WeaponRange = GetComponentInParent<TestEnemyAITurret>().WeaponRange;
        }
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
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            if (collision.gameObject.layer == 6)
            {
                Destroy(gameObject);
            }
        }
        if (collision.gameObject.tag == "Player")
        {
            //Damage
            GameObject.Find("GameData").GetComponent<GameData>().CurrentHealth -= WeaponDamage;

            //Send Hit Data (for repositioning)
            //if (GetComponentInParent<GruntAI>())
            //{
            //    GetComponentInParent<GruntAI>().NumberOfHitsPerMag += 1;
            //}

            if (GetComponentInParent<TestEnemyAIGroundRanged>())
            {
                WeaponDamage = GetComponentInParent<TestEnemyAIGroundRanged>().NumberOfHitsPerMag += 1;
            }
            Destroy(gameObject);
        }
    }
}
