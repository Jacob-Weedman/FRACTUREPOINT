using UnityEngine;

public class HealthBarManager : MonoBehaviour
{
    public Transform centerBarTransform;
    public Transform rightEdgeTransform;
    public float playerHealth = 100.0f;

    public SpriteRenderer centerBarRenderer;
    private Vector3 centerBarMaxSize;
    private float centerBarWidth; 
    private Vector3 centerBarStartPosition;

    public SpriteRenderer rightEdgeRenderer;
    private Vector3 rightEdgeStartPosition;

    private float sliderScale = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        centerBarMaxSize = centerBarRenderer.size;
        centerBarStartPosition = centerBarTransform.position;

        rightEdgeStartPosition = rightEdgeTransform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Changes center bar width //
        sliderScale = playerHealth / 100f;
        centerBarWidth =  centerBarMaxSize.x * sliderScale;
        centerBarRenderer.size = new Vector2(centerBarWidth, centerBarMaxSize.y);

        // Move center bar to correct for size change //
        float centerBarMovement = centerBarMaxSize.x * (sliderScale - 1.0f) * 1.16f;
        centerBarTransform.position = new Vector3(centerBarStartPosition.x + centerBarMovement,centerBarStartPosition.y, centerBarStartPosition.z);


        // Move right bar to 
        float rightEdgeMovement = centerBarMaxSize.x * (sliderScale - 1.0f) * 1.16f * 2.18f;
        rightEdgeTransform.position = new Vector3(rightEdgeStartPosition.x + rightEdgeMovement, rightEdgeStartPosition.y, rightEdgeStartPosition.y);

    }
}
