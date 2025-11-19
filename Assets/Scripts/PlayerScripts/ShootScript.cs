using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ShootScript: MonoBehaviour
{
    public Transform Gun, ShootPoint;
    public GameObject Bullet;
    public float BulletSpeed, BulletRange;
    Vector2 direction;

    private void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        direction = mousePos - (Vector2)Gun.position;
        FaceMouse();

    }

    void FaceMouse()
    {
        Gun.transform.right = direction;
    }

    public void Shoot(string projectile, float speed, float range)
    {
        BulletSpeed = speed;
        BulletRange = range;

        GameObject BulletInstance = Instantiate(GameObject.Find(projectile), ShootPoint.position, ShootPoint.rotation * Quaternion.Euler(0, 0, 90));
        BulletInstance.GetComponent<Rigidbody2D>().AddForce(BulletInstance.transform.up * -1 * BulletSpeed);

    }
}
