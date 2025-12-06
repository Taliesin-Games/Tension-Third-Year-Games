using UnityEngine;
using UnityEngine.InputSystem;

public class Projectile : MonoBehaviour
{
    // Only GameObjects with a Rigidbody can be assigned as the projectile.
    [SerializeField] Rigidbody projectileBody;
    // Speed of the projectile when fired.
    // This is a public variable so it can be adjusted in the Unity Editor.
    [SerializeField] float speed = 4;
    [SerializeField] float lifeTime = 5;
    void Start()
    {
        // Deletes the projectile after 10 seconds, regardless
        // of whether it collided with anything. This prevents
        // instances from staying in the scene indefinitely.
        Destroy(gameObject, lifeTime);
    }
    // Update is called once per frame
    // This method checks for input and fires a projectile if the attack action is pressed.
    void Update()
    {
        projectileBody.linearVelocity = transform.forward * speed * Time.deltaTime;
    }

    void OnCollisionEnter()
    {
        Destroy(gameObject);
    }
}
