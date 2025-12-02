using UnityEngine;

public class BasicMelee : MonoBehaviour
{

    public float damage = 15;
    void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Attackable"))
        {
            if (collision.GetComponent<MasterEnemyAI>())
            {
                collision.GetComponent<MasterEnemyAI>().Health -= damage;
            }

            if (collision.GetComponent<GenericDestructable>())
            {
                if (collision.GetComponent<GenericDestructable>().AttackableByPlayer)
                {
                    collision.GetComponent<GenericDestructable>().Health -= damage;

                }
            }
        }
    }
    
}