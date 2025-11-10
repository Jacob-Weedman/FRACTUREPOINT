using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Designed by Jacob Weedman
//Component of the "MainCamera" game object
//V 0.0.5

public class CameraMovement : MonoBehaviour
{

    GameObject target;
    GameObject player;
    GameObject TempPos;
    GameObject query;
    public bool TargetingPlayer = true;
    public bool LockX = false;
    public bool LockY = false;

    float CameraTweenAmmount = 0.005f;
    float CameraTweenRateX;
    float CameraTweenRateY;
    float MaxCameraTweenRate = 10f;
    float MinCameraTweenRate = 0.01f;
    public float MaxCameraDistance = 15f;
    public List<GameObject> InitialPanDestinations;


    void Start()
    {
        InitialPanDestinations = GameObject.FindGameObjectsWithTag("PanDestinations").ToList();

        target = GameObject.Find("CameraTarget");
        player = GameObject.FindWithTag("Player");

        transform.position = new Vector3(target.transform.position.x, target.transform.position.y, -10);
        //target.transform.position = new Vector2(player.transform.position.x, player.transform.position.y);
        //TempPos = InitialPanDestinations[UnityEngine.Random.Range(0, InitialPanDestinations.Count)];
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

        // Call for camera to be tweened
        //TweenX();
        //TweenY();

        // Move Camera
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.transform.position.x, target.transform.position.y, -10), Vector2.Distance(transform.position, target.transform.position) / 300);


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

    //Move Camera Smoothly
    void TweenX()
    {
        // Check if X value can be changed
        if (LockX == false)
            {
            // Determine if the camera is on the left or right of the target position
            if (transform.position.x > target.transform.position.x)
            {
                // Move camera left
                transform.position = new Vector3(transform.position.x - (CameraTweenAmmount * CameraTweenRateX), transform.position.y, transform.position.z);
            }
            if (transform.position.x < target.transform.position.x)
            {
                // Move camera left
                transform.position = new Vector3(transform.position.x + (CameraTweenAmmount * CameraTweenRateX), transform.position.y, transform.position.z);
            }
        }

    }
    void TweenY()
    {
        // Check if Y value can be changed
        if (LockY == false)
        {
            // Determine Up or Down
            if (transform.position.y > target.transform.position.y)
            {
                // Move camera Down
                transform.position = new Vector3(transform.position.x, transform.position.y - (CameraTweenAmmount * CameraTweenRateY), transform.position.z);
            }
            if (transform.position.y < target.transform.position.y)
            {
                // Move camera Up
                transform.position = new Vector3(transform.position.x, transform.position.y + (CameraTweenAmmount * CameraTweenRateY), transform.position.z);
            }
        }
    }
}
