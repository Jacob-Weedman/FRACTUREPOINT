using UnityEngine;

// Designed by Jacob Weedman
// Attach to the grenade weapon prefab

public class EnemyGrenade : MonoBehaviour
{
    float duration;
    public int WeaponDamage;

    void Awake()
    {
        duration = 10;
        WeaponDamage = 20;
        GetComponent<Rigidbody2D>().gravityScale = 2;
    }

    void FixedUpdate()
    {
        if (duration <= 0)
        {
            Explode();
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

        //Delete Grenade
        Destroy(gameObject);
    }
}
