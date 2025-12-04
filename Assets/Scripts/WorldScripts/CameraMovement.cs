using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Designed by Jacob Weedman
//Component of the "MainCamera" game object

public class CameraMovement : MonoBehaviour
{

    GameObject target;
    GameObject player;
    GameObject TempPos;
    GameObject query;
    public bool TargetingPlayer = true;

    float CameraTweenAmmount = 0.005f;
    float CameraTweenRateX;
    float CameraTweenRateY;
    float MaxCameraTweenRate = 10f;
    float MinCameraTweenRate = 0.01f;
    public float MaxCameraDistance = 15f;
    public List<GameObject> InitialPanDestinations;
    Vector3 StartLocation;
    public float shakeDuration;
    public float shakeAmount;
    public float shakeInterval = 0.01f; //sec
    public float currentShakeInterval = 0.01f; // sec


    void Start()
    {
        InitialPanDestinations = GameObject.FindGameObjectsWithTag("PanDestinations").ToList();

        target = GameObject.Find("CameraTarget");
        player = GameObject.FindWithTag("Player");

        transform.position = new Vector3(target.transform.position.x, target.transform.position.y, -10);

        float distance = 1000000;
        foreach (GameObject query in InitialPanDestinations)
        {
            if (Vector2.Distance(query.transform.position, target.transform.position) < distance)
            {
                distance = Vector2.Distance(query.transform.position, target.transform.position);
                TempPos = query;
            }
        }

        if (TempPos == null)
        {
            TempPos = target;
        }

        target.transform.position = TempPos.transform.position;
    }
    
    void Update()
    {
        // Difference between camera and target; abs()
        CameraTweenRateX = (Math.Abs(target.transform.position.x - transform.position.x));
        CameraTweenRateY = (Math.Abs(target.transform.position.y - transform.position.y));

        // Ensure the Tween Rate stays between the Max and Min values
        if (CameraTweenRateX > MaxCameraTweenRate)
        {
            CameraTweenRateX = MaxCameraTweenRate;
        }
        if (CameraTweenRateX < MinCameraTweenRate)
        {
            transform.position = new Vector3(target.transform.position.x, transform.position.y, transform.position.z);
        }
        if (CameraTweenRateY > MaxCameraTweenRate)
        {
            CameraTweenRateY = MaxCameraTweenRate;
        }
        if (CameraTweenRateY < MinCameraTweenRate)
        {
            transform.position = new Vector3(transform.position.x, target.transform.position.y, transform.position.z);
        }

        // Camera Shake
        if (shakeDuration > 0) {
            if (currentShakeInterval <= 0)
            {
                currentShakeInterval = shakeInterval;

                transform.localPosition = new Vector3(UnityEngine.Random.Range(-shakeAmount, shakeAmount) + StartLocation.x, UnityEngine.Random.Range(-shakeAmount, shakeAmount) + StartLocation.y, -10);
            }
            else
            {
                currentShakeInterval -= Time.deltaTime;
            }
            shakeInterval += 0.15f * Time.deltaTime;
            shakeAmount -= 0.15f * Time.deltaTime;
            shakeDuration -= Time.deltaTime;

        } 
        else 
        {
            StartLocation = transform.position;
            
            shakeDuration = 0.0f;
            shakeAmount= 0.0f;
            shakeInterval = 0.01f;

            // Move Camera
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.transform.position.x, target.transform.position.y, -10), Vector2.Distance(transform.position, target.transform.position) / 100);
        }

        //Initial Pan
        if (InitialPanDestinations.Count > 0)
        {
            // Lock player movement
            if (Vector2.Distance(transform.position, TempPos.transform.position) < 0.5) // Camera at destination
            {
                InitialPanDestinations.Remove(TempPos);
                TempPos = player;
                float distance = 1000000;
                foreach (GameObject query in InitialPanDestinations)
                {
                    if (Vector2.Distance(query.transform.position, target.transform.position) < distance)
                    {
                        distance = Vector2.Distance(query.transform.position, target.transform.position);
                        TempPos = query;
                    }

                }
                target.transform.position = TempPos.transform.position;
            }
            else
            {
                target.transform.position = TempPos.transform.position;
            }
        }
        else
        {
            // Hide Level Start UI
            if (GameObject.Find("LevelName"))
            {
                Destroy(GameObject.Find("LevelName"));
            }
            if (GameObject.Find("BEGIN!"))
            {
                GameObject.Find("BEGIN!").transform.localScale = new Vector3(1, 1, 1);
            }
            if (Vector2.Distance(target.transform.position, transform.position) <= 0.5)
            {
                Destroy(GameObject.Find("BEGIN!"));
            }
            // Unlock player movement
            // Look ahead feature based on right click
            if (TargetingPlayer == true && Input.GetMouseButton(1) == true)
            {
                Vector3 pointer;
                int speed = 10;

                pointer = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                pointer.z = transform.position.z;
                target.transform.position = Vector3.MoveTowards(new Vector2(player.transform.position.x, player.transform.position.y + 5), pointer, MaxCameraDistance);
            }
            // Reset target to player position
            else if (TargetingPlayer == true && Input.GetMouseButton(1) == false)
            {  
                target.transform.position = new Vector2(player.transform.position.x, player.transform.position.y + 5);
            }
        }
    }

    public void shakeCamera(float duration, float intensity)
    {
        shakeDuration = duration;
        shakeAmount = intensity;
        shakeInterval = 0.01f;
    }

}
