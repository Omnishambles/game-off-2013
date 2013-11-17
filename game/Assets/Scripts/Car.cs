﻿using UnityEngine;
using System.Collections;

public class Car : MonoBehaviour
{
    private float _percentAcrossPath;
    private bool _isMoving;

    // Properties
    public Traffic Traffic
    {
        get;
        set;
    }

    public TrafficLane Lane
    {
        get;
        set;
    }

    public Car CarInFront
    {
        get;
        set;
    }

    public Car CarBehind
    {
        get;
        set;
    }

    public Vector3[] Path
    {
        get;
        set;
    }

    public static GameObject Create()
    {
        GameObject car = GameObject.CreatePrimitive(PrimitiveType.Cube);
        car.AddComponent<Car>();
        car.AddComponent<Rigidbody>();
        return car;
    }
    
    private void Start()
    {
        _percentAcrossPath = 0.02f;
        _isMoving = true;
    }
    
    private void FixedUpdate()
    {
        if (_isMoving)
        {
            float moveAmount = this.Traffic.MovementAmountForCar(this);
            if (moveAmount <= 0.0f)
            {
                return;
            }

            _percentAcrossPath += moveAmount;
            Vector3 pointOnPath = iTween.PointOnPath(Path, _percentAcrossPath);
            rigidbody.MovePosition(pointOnPath);
        }

        // Now check if we've moved past the end of the path.
        // This is when the car should move to the other intersection.
        // For now, just destroy it.
        if (_percentAcrossPath > 1.0f)
        {
            GameObject.Destroy(this.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        _isMoving = false;
    }
}