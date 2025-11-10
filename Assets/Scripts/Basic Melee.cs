using UnityEngine;

public class BasicMelee : MonoBehaviour
{

    public int damage = 1;
    void OnTriggerEnter2D(Collider2D collision) 
    {

        if (collision.CompareTag("Attackable"))
        {
            collision.GetComponent<EnemyHealth>()?.TakeDamage(damage);
            Debug.Log("hit an enemy");

        }
    
    }





}
