using UnityEngine;

// Designed by Jacob Weedman
// Attach to the rocket weapon prefab

public class EnemyRocket : MonoBehaviour
{
    float duration;
    public int WeaponDamage;

    void Awake()
    {
        duration = 30;
    }

    void FixedUpdate()
    {
        if (duration <= 0)
        {
            GetComponent<Rigidbody2D>().gravityScale = 3;
        }
        duration -= Time.deltaTime;
    }

    void Explode()
    {
        //Create Explosion
        GameObject Explosion;
        Explosion = Instantiate(GameObject.Find("Explosion"), new Vector3(transform.position.x, transform.position.y, GameObject.Find("Explosion").transform.position.z), Quaternion.identity);
        Explosion.transform.rotation = Quaternion.Euler(Vector3.forward);

        //Set Variables
        Explosion.GetComponent<EnemyParticleWeapon>().timer = 3;
        Explosion.GetComponent<EnemyParticleWeapon>().destroy = true;
        Explosion.GetComponent<EnemyParticleWeapon>().opacity = true;
        Explosion.GetComponent<EnemyParticleWeapon>().damageAmmount = WeaponDamage;

        //Delete Rocket
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            if (collision.gameObject.layer == 6)
            {
                Explode();
            }
        }
        if (collision.gameObject.tag == "Player")
        {
            Explode();
        }
    }
}
