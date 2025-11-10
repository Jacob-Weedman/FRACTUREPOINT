using UnityEngine;

public class WallCheck : MonoBehaviour
{
    public bool touchingWall;
    public float wallStallTimer;

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Wall")) touchingWall = true;
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            touchingWall = false;
        }
    }

}
