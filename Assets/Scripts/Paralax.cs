using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Designed by Jacob Weedman
//Place this script on any of the background elements you want paralax to apply to
//V 0.0.4

public class Background : MonoBehaviour
{

    private float length;
    private float startposX;
    private float startposY;
    GameObject camera;
    private float LAYER_MODULATOR;
    private float ParalaxStrength = 0.3f;
    private float yOffset = 1f;
    private int MaxLayer = 10;
    public bool looping = false;

    void Start()
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
        //transform.position = new Vector3((startposX + distanceX) * -1, (startposY + distanceY + yOffset) * -1, transform.position.z);
        if (transform.position.z >= MaxLayer)
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