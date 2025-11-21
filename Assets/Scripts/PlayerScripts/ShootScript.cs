using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ShootScript: MonoBehaviour
{
    public Transform Gun, ShootPoint;
    public float BulletSpeed, BulletRange, BulletDamage, BulletUnloadSpeed, BulletMagSize, BulletReloadSpeed, BulletBurstAmount;
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

    public void Shoot(string projectile, float speed, float range, float damage, float unload, float burst = 1)
    {
        BulletRange = range;
        BulletDamage = damage;
        BulletSpeed = speed;
        BulletUnloadSpeed = unload;
        BulletBurstAmount = burst;

        GameObject BulletInstance = Instantiate(GameObject.Find(projectile), ShootPoint.position, ShootPoint.rotation * Quaternion.Euler(0, 0, 90));
        BulletInstance.GetComponent<Rigidbody2D>().AddForce(BulletInstance.transform.up * -1 * BulletSpeed);

    }
}
