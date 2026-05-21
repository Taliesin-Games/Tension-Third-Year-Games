using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Projectile : MonoBehaviour
{
    [SerializeField] Rigidbody projectileBody;
    [SerializeField] float speed = 4;
    [SerializeField] float lifeTime = 5;
    SpellBase spell;

    public void SetSpell(SpellBase inSpell) { spell = inSpell; }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        projectileBody.linearVelocity = transform.forward * speed * Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (gameObject.layer == LayerMask.NameToLayer("Projectiles")) return;

        Fireball fireball = (Fireball)spell;
        fireball.DealDamage(collision.gameObject);
        Destroy(gameObject);
    }
}
