using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public GameObject destination;
    
    void Awake()
    {
        destination = transform.Find("B").gameObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player");
        {
            other.transform.position=destination.transform.position;
        }
    }
}
