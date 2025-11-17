using System.Collections;
using UnityEngine;
using Utils;
public class DemoEnemy : MonoBehaviour
{
    DamageStruct playerDamage = new DamageStruct
    {
        Physical = 10f,
        Magical = 5f,
        True = 0f,
        Fire = 0f,
        Lightning = 0f,
        Ice = 0f,
        Earth = 0f,
        Wind = 0f,
        Water = 0f,
        None = 0f
    };

    [SerializeField] ParticleSystem particles;
    [SerializeField] Vector3 moveOffset = Vector3.forward * 5;
    [SerializeField] float moveTime = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0)) 
        { 
            Destroy(gameObject);
            GameManager.Instance.OnEnemyDefeated();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) DamagePlayer();
        if (Input.GetKeyDown(KeyCode.Alpha2)) TestParticle();
    }

    void DamagePlayer()
    {
        Health health = FindFirstObjectByType<BMD.PlayerController>().GetComponent<Health>();
        if (health == null) return;

        health.TakeDamage(playerDamage);
        Debugger.Log("Damaged Player from DemoEnemy");
    }
    void TestParticle()
    {
        Debugger.Log("Playing particles");
        StartCoroutine(ReactivateAfterDelay());
    }

    IEnumerator ReactivateAfterDelay()
    {
        float endTime = Time.time + moveTime;

        particles.Play();
        while (endTime >= Time.time)
        {
            Vector3 pos = transform.position;
            pos += moveOffset * Time.deltaTime;
            transform.position = pos;
            yield return new WaitForEndOfFrame();
            
        }
        Debugger.Log("Particle coroutine finished");
    }

    
}
