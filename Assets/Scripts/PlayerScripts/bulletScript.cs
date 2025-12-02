using Unity.VisualScripting;
using UnityEngine;

public class bulletScript : MonoBehaviour
{
    private ShootScript shootScript;
    private float range, damage, unload, burst;
    private Rigidbody2D body;
    Vector3 startPos;
    void Awake()
    {
        shootScript = GameObject.Find("Player").GetComponent<ShootScript>();
        body = GetComponent<Rigidbody2D>();
        range = shootScript.BulletRange;
        damage = shootScript.BulletDamage;
        unload = shootScript.BulletUnloadSpeed;
        burst = shootScript.BulletBurstAmount;

        startPos = transform.position;
    }
    void Update()
    {
        Vector3 currentPos = transform.position;

        if (Vector2.Distance(startPos, currentPos) > range) body.gravityScale = 1;
        if (currentPos.y < -1000) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Attackable"))
        {
            if (collision.GetComponent<MasterEnemyAI>())
            {
                collision.GetComponent<MasterEnemyAI>().Health -= damage;
                Destroy(gameObject);
            }

            if (collision.GetComponent<GenericDestructable>())
            {
                if (collision.GetComponent<GenericDestructable>().AttackableByPlayer) 
                {
                    collision.GetComponent<GenericDestructable>().Health -= damage;
                    Destroy(gameObject);

                }

            }
           
        }

        if (collision.gameObject.layer == 6) Destroy(gameObject);



    }


}
