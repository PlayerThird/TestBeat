using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderEnd : MonoBehaviour
{
   [SerializeField] private GameObject player;
   private Animator anim;
   
   private void OnTriggerEnter(Collider other)
   {
      player = other.gameObject;
      anim = player.GetComponentInChildren<Animator>();
      if (player.GetComponent<UnitState>().currentState == UNITSTATE.UPSTAIRS)
      {
         GetComponentInParent<Ladder>().Exit();
         GetComponentInParent<Ladder>().ChangeBoolEnter();
      }
   }
}
