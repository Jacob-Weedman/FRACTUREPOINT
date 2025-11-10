using UnityEngine;

public class DoubleCheck : MonoBehaviour
{
    public bool isInsideGround = false;
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground")) isInsideGround = true;
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground")) isInsideGround = false;
    }
}
