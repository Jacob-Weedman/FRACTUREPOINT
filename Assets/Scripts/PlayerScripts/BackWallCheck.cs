using UnityEngine;

public class BackWallCheck : MonoBehaviour
{
    public bool backTouchingWall;
    private playerController PlayerController;
    void Start()
    {
        PlayerController = GameObject.Find("Player").GetComponent<playerController>();
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer == 6 && PlayerController.isGrounded == false) PlayerController.Flip();
    }

}
