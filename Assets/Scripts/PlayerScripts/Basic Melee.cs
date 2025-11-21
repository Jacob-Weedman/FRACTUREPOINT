using UnityEngine;

public class BasicMelee : MonoBehaviour
{

    public float damage = 15;
    void OnTriggerEnter2D(Collider2D collision) 
    {

        if (collision.CompareTag("Attackable"))
        {
            collision.GetComponent<MasterEnemyAI>().Health -= damage;
            Debug.Log("hit an enemy");

        }
    
    }





}
