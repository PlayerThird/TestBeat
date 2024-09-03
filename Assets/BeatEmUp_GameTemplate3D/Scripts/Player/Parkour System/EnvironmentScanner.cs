using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentScanner : MonoBehaviour
{
    [SerializeField] Vector3 forwardRayOffset = new Vector3(0, 0.25f, 0);
    [SerializeField] private float forwardRayLenght = 0.8f;
    [SerializeField] private LayerMask obstacleLayer;
    
    public ObstacleHitData ObstacleCheck()
    {
        var hitData = new ObstacleHitData();
        
        var forwardOrigin = transform.position + forwardRayOffset;
        //Проверяем с помощью линии, есть ли впереди препятствие
        hitData.forwardHitFound = Physics.Raycast(forwardOrigin,  transform.right * (-1f), 
            out hitData.forwardHit, forwardRayLenght, obstacleLayer);
        //Дэбаггер, рисуем визульную линию проверки, если ничего: то голубой, если есть: фиолет
        Debug.DrawRay(forwardOrigin, transform.right * (-1f), 
            (hitData.forwardHitFound)? Color.magenta : Color.cyan);
        
        return hitData;
    }
}

public struct ObstacleHitData
{
    public bool forwardHitFound;
    public RaycastHit forwardHit;
}