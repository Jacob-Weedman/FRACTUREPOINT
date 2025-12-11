using UnityEngine;

// Designed by Jacob Weedman
// Attach to the grenade weapon prefab

public class EnemyGrenade : MonoBehaviour
{
    public float Duration = 3;
    public int WeaponDamage = 20;
    public bool ExplodeOnContact = false;

    void Awake()
    {
        GetComponent<Rigidbody2D>().gravityScale = 2;
    }

    void FixedUpdate()
    {
        if (Duration <= 0)
        {
            Explode();
        }
        Duration -= Time.deltaTime;
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

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (ExplodeOnContact)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("SolidGround") || collision.gameObject.layer == LayerMask.NameToLayer("Enemies") || collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Explode();
            }
        }
    }
}
