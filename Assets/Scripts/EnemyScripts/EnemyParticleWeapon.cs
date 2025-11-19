using UnityEngine;
using System.Threading.Tasks;

//Designed by Jacob Weedman
// Attatch to particle weapons

public class EnemyParticleWeapon : MonoBehaviour
{
    public bool destroy = false;
    public bool rotate = false;
    public bool opacity = false;
    public bool destroyOnCollide = false;
    public bool damageEnemies;
    public int damageAmmount;
    public float timer;
    float startTime;

    void Awake()
    {
        startTime = timer;
    }

    void FixedUpdate()
    {
        // Rotation
        if (rotate)
        {
            transform.rotation = Quaternion.Euler(Vector3.forward * UnityEngine.Random.Range(-90, 90));
        }

        // Opacity
        if (opacity)
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, (timer / startTime));
        }

        // Destruction Timer
        if (destroy)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                Destroy(gameObject);
            }
        }

        // Remove Collider To Prevent Multiple Damage Occurances
        if (timer/startTime <= 0.99)
        {
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            //Damage
            GameObject.Find("GameData").GetComponent<GameData>().CurrentHealth -= damageAmmount;
        }

        if (damageEnemies)
        {
            if (collision.gameObject.layer == 8 || collision.gameObject.layer == 11)
            {
                if (collision.GetComponent<MasterEnemyAI>())
                {
                    collision.GetComponent<MasterEnemyAI>().Health -= damageAmmount;
                }
            }
        }
        
        if (destroyOnCollide)
        {
            if (collision.gameObject.tag == "Ground")
            {
                if (collision.gameObject.layer == 6)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
