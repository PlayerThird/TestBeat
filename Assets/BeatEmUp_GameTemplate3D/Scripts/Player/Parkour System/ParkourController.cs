using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ParkourController : MonoBehaviour
{
    private EnvironmentScanner environmentScanner;

    private void Awake()
    {
        environmentScanner = GetComponent<EnvironmentScanner>();
    }

    private void Update()
    {
        var hitData = environmentScanner.ObstacleCheck();
        //Дэбаггер, пишем с какой объект мы видим в заданном слое
        if (hitData.forwardHitFound)
        {
            Debug.Log("Obstacle Found" + hitData.forwardHit.transform.name);
        }
    }
}
