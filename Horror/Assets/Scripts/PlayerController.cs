using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{    
    [SerializeField]
    private UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController playerController;
    public bool isAlive = true;
    public void KillPlayer(){
        playerController.enabled= false;
        isAlive = false;
    }
}
