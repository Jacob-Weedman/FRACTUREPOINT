using UnityEngine;

public class WallCheck : MonoBehaviour
{
    public bool touchingWall;
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer == 6) touchingWall = true;
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.layer == 6) touchingWall = false;
    }

}
