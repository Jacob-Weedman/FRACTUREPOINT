using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class ShootScript: MonoBehaviour
{
    public Transform Gun, ShootPoint;
    public float BulletSpeed, BulletRange, BulletDamage, BulletUnloadSpeed, BulletMagSize, BulletReloadSpeed, BulletBurstAmount;
    private float BurstTimer, BurstCounter;
    private string ProjectileType;
    Vector2 direction;
    

    private void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        direction = mousePos - (Vector2)Gun.position;
        FaceMouse();


        BurstTimer -= Time.deltaTime;

        if (BulletBurstAmount > 0 && BurstTimer < 0)
        {
            GameObject BulletInstance = Instantiate(GameObject.Find(ProjectileType), ShootPoint.position, ShootPoint.rotation * Quaternion.Euler(0, 0, 90));
            BulletInstance.GetComponent<Rigidbody2D>().AddForce(BulletInstance.transform.up * -1 * BulletSpeed);

            BurstTimer = BurstCounter;           
            BulletBurstAmount--;
        }



    }

    void FaceMouse()
    {
        Gun.transform.right = direction;
    }

    public void Shoot(string projectile, float speed, float range, float damage, float unload, float burst = 1, float burstSpeed = 0)
    {
        ProjectileType = projectile;
        BulletRange = range;
        BulletDamage = damage;
        BulletSpeed = speed;
        BulletUnloadSpeed = unload;
        BulletBurstAmount = burst;
        BurstCounter = burstSpeed;
        BurstTimer = BurstCounter;
    }
}
