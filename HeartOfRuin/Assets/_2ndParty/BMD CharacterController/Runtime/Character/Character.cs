using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BMD
{
    [RequireComponent(typeof(BMD.CharacterController))] // Ensure that a CharacterController component is attached
    public abstract class Character : MonoBehaviour
    {
        [SerializeField] string characterName = "Dan";
        // Start is called before the first frame update
        protected virtual void Start()
        {
            Debug.Log("Character Name: " + characterName);
        }

        // Update is called once per frame
        protected virtual void Update()
        {

        }
    }
}