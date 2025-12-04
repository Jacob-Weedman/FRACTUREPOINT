using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;



    public float targetAlpha = 0.0f;
    public float alphaSpeed = 0.07f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer.color = new Color (1.0f, 1.0f, 1.0f, 0.0f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // update 
	float newColorAlpha = spriteRenderer.color.a + (targetAlpha - spriteRenderer.color.a) * alphaSpeed;
	spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, newColorAlpha);
    }

    void OnMouseOver()
    {
	targetAlpha = 1;
    }

    void OnMouseExit()
    {
	targetAlpha = 0;
    }

    void OnMouseDown()
    {
        SceneManager.LoadScene("LevelSelection");
    }
}
