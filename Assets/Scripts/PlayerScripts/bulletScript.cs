using UnityEngine;

public class bulletScript : MonoBehaviour
{
    private float range;
    private Rigidbody2D body;
    Vector3 startPos;
    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        range = GetComponent<ShootScript>().BulletRange;
        startPos = transform.position;
    }
    void Update()
    {
        Vector3 currentPos = transform.position;
        if (Vector2.Distance(startPos, currentPos) > range) body.gravityScale = 4;

    }
}
