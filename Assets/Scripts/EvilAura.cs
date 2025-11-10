using UnityEngine;
using System.Threading.Tasks;

//Designed by Jacob Weedman
// Attatch to "EvilAura" gameObject

public class EvilAura : MonoBehaviour
{
    public bool destroy = false;
    public float timer = 6;

    void Update()
    {
        if (destroy == true)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        { 
            //Damage
            GameObject.Find("GameData").GetComponent<GameData>().CurrentHealth -= 1;
        }
    }
}
