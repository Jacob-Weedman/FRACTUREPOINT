using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Designed by Jacob Weedman
//Place this script on any of the background elements you want paralax to apply to

public class Background : MonoBehaviour
{

    float length;
    float startposX;
    float startposY;
    GameObject camera;
    float LAYER_MODULATOR;
    float ParalaxStrength = 0.3f;
    float yOffset = 1f;
    public bool FixedOnCamera = false;
    public bool looping = false;

    void Awake()
    {
        startposX = transform.position.x;
        startposY = transform.position.y;

        length = GetComponentInChildren<SpriteRenderer>().bounds.size.x;
        LAYER_MODULATOR = transform.position.z;
        camera = GameObject.FindWithTag("MainCamera");
    }

    void Update()
    {
        float temp = (camera.transform.position.x * (1 - (LAYER_MODULATOR * ParalaxStrength * 0.1f)));
        float distanceX = (camera.transform.position.x * LAYER_MODULATOR * ParalaxStrength * 0.1f);
        float distanceY = (camera.transform.position.y * LAYER_MODULATOR * ParalaxStrength * 0.1f);
        
        if (FixedOnCamera)
        {
            transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, transform.position.z);
        }
        else
        {
            transform.position = new Vector3((startposX + distanceX), transform.position.y, transform.position.z);
        }

        if (looping == true)
        {
            if (temp > startposX + length)
            {
                startposX += length;
            }
            else if (temp < startposX - length)
            {
                startposX -= length;
            }
        }
    }
}