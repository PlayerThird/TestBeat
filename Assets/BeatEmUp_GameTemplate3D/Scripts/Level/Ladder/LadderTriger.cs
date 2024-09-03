using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class LadderTriger : MonoBehaviour
{
    [SerializeField] public GameObject player;
    [SerializeField] public Animator anim;
    [SerializeField] public Transform startPoint;
    [SerializeField] public Transform endPoint;
    
    private GameObject[] Players;
    private GameObject playerinRange;
    public float ladderRange = 1;

    public GameObject FPC;
    public Transform positionStart;
    public Transform positionEnd;
    //private Animator anim;
    
    /*private void OnTriggerEnter(Collider other)
    {
        foreach(GameObject player in Players) {
            if(player == playerinRange) {
                float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

                //ladder in range
                if(distanceToPlayer < ladderRange && playerinRange == null) {
                    playerinRange = player;
                    player.SendMessage("LadderInRange", gameObject, SendMessageOptions.DontRequireReceiver);
                    return;

                }

                //ladder out of range
                if(distanceToPlayer > ladderRange && playerinRange != null) {
                    player.SendMessage("LadderOutRange", gameObject, SendMessageOptions.DontRequireReceiver);
                    playerinRange = null;
                }
            }
        }
    }*/

   
}
