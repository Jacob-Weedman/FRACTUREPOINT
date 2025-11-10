using UnityEngine;

public class EnemyHealth : MonoBehaviour
{

    public int maxHealth = 3, currentHealth = 3;
    public void TakeDamage(int damage) 
    {
    
        Debug.Log($"Enemy took  {damage} damage");

        currentHealth -= damage;

        if (currentHealth <= 0) 
        { 
        
             Destroy(gameObject);  
        
        }
    
    
    
    }
}
