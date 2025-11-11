using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BMD
{
    [RequireComponent(typeof(Animator))] // Ensure that an Animator component is attached
    [RequireComponent(typeof(BMD.PlayerController))] // Ensure that a CharacterController component is attached
    public class Player : Character
    {

        // This class inherits from Character and can override methods or add new functionality specific to players
        protected override void Start()
        {
            base.Start(); // Call the base class Start method
            Debug.Log("Character Type: Player");
        }
    }

}