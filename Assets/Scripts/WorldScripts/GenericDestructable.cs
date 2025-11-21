using UnityEngine;

public class GenericDestructable : MonoBehaviour
{

    // Designed by Jacob Weedman

    public string mode = "DESTROY"; // "DESTROY". "DISABLE"
    public bool AttackableByPlayer = true;
    public float Health = 100;

    // Update is called once per frame
    void Update()
    {
        if (Health <= 0)
        {
            switch (mode)
            {
                case "DESTROY":
                Destroy(gameObject);
                    break;
                case "DISABLE":
                gameObject.tag = "Destroyed";
                    break;
                default:
                    break;
            }
        }
    }
}
