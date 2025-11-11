using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BMD
{
    [RequireComponent(typeof(BMD.EnemyController))] // Ensure that a CharacterController component is attached
    public class Enemy : Character
    {
        // This class inherits from Character and can override methods or add new functionality specific to enbemies
        protected override void Start()
        {
            base.Start(); // Call the base class Start method
            Debug.Log("Character Type: Enemy");
        }
    }

}