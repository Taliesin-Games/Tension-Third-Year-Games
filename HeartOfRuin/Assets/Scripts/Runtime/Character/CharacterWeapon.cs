using Unity.Cinemachine;
using UnityEngine;

public class CharacterWeapon : MonoBehaviour
{
    #region Configuration
    [SerializeField] float velocityDamageThreshold = 1f;
    #endregion

    #region Cached References
    Character character;
    Rigidbody rb;
    #endregion

    #region Runtime Variables
    Vector3 lastPosition;
    Vector3 velocity;
    #endregion

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (!rb) Debug.LogError($"Rigidbody not found on {name}");

        character = GetComponentInParent<Character>();
        if (!character) Debug.LogError($"Character not found for {name}");

    }

    void Update()
    {
        velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (character == null) return;
        
        // Check layer is enemy layer
        if ((other.gameObject.layer) != 7) return;  // Layer 7 is Enemy

        if (velocity.magnitude > velocityDamageThreshold)
        {
            character.HitWithWeapon(other.gameObject);
        }
    }
}
