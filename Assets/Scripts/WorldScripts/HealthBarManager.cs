using UnityEngine;

public class HealthBarManager : MonoBehaviour
{
    // public Transform centerBarTransform;
    // public Transform rightEdgeTransform;
    public float idk = 0.045f;
    private float playerHealth;
    public float sliderValue = 100.0f;
    public GameObject centerBar;
    private Transform centerBarTransform;
    public GameObject rightBar;
    private Transform rightBarTransform;

    public SpriteRenderer centerBarRenderer;
    private Vector3 centerBarMaxSize;
    // private float centerBarWidth; 
    // public Transform centerBarPosition;

    // public SpriteRenderer rightEdgeRenderer;
    // private Vector3 rightEdgeStartPosition;

    // private float sliderScale = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        centerBarRenderer = centerBar.GetComponent<SpriteRenderer>();
        centerBarMaxSize = centerBarRenderer.size;
        // rightEdgeStartPosition = rightEdgeTransform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        playerHealth = GameObject.Find("GameData").GetComponent<GameData>().CurrentHealth;

        // add lerp function for health 
        float healthIncrement = (playerHealth - sliderValue) * 0.1f;

        sliderValue += healthIncrement;


        centerBarRenderer.size = new Vector2(centerBarMaxSize.x * sliderValue / 100.0f, centerBarMaxSize.y);


        // centerBarPosition = centerBar.GetComponent<Transform>();
        // centerBarPosition = new Vector3(centerBarPosition.x + 0.01, centerBarPosition.y, centerBarPosition.z);

        centerBarTransform = centerBar.GetComponent<Transform>();
        centerBarTransform.position += Vector3.right * healthIncrement * idk;
        rightBarTransform = rightBar.GetComponent<Transform>();
        rightBarTransform.position += Vector3.right * healthIncrement * 2 * idk;





            // OLD SYSTEM //

        // // Changes center bar width //
        // sliderScale = playerHealth / 100f;
        // centerBarWidth =  centerBarMaxSize.x * sliderScale;
        // centerBarRenderer.size = new Vector2(centerBarWidth, centerBarMaxSize.y);

        // // Move center bar to correct for size change //
        // float centerBarMovement = centerBarMaxSize.x * (sliderScale - 1.0f) * 1.16f;
        // centerBarTransform.position = new Vector3(centerBarStartPosition.x + centerBarMovement,centerBarStartPosition.y, centerBarStartPosition.z);


        // // Move right bar to 
        // float rightEdgeMovement = centerBarMaxSize.x * (sliderScale - 1.0f) * 1.16f * 2.18f;
        // rightEdgeTransform.position = new Vector3(rightEdgeStartPosition.x + rightEdgeMovement, rightEdgeStartPosition.y, rightEdgeStartPosition.y);

    }
}
