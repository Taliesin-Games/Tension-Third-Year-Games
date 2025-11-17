using UnityEngine;

public class DemoParticles : MonoBehaviour
{
    [SerializeField] ParticleSystem[] particleSystems;

    ParticleSystem[] instantiatedParticles = new ParticleSystem[10];

    private void Start()
    {
        int i = 0;  
        foreach (var ps in particleSystems)
        {
            if (!ps) continue;
            instantiatedParticles[i] = Instantiate(ps, transform.position + Vector3.up, Quaternion.identity);
            instantiatedParticles[i].transform.parent = transform;
            i++;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && instantiatedParticles[0] != null) instantiatedParticles[0].Play();
        if (Input.GetKeyDown(KeyCode.Alpha2) && instantiatedParticles[1] != null) instantiatedParticles[1].Play();
        if (Input.GetKeyDown(KeyCode.Alpha3) && instantiatedParticles[2] != null) instantiatedParticles[2].Play();
        if (Input.GetKeyDown(KeyCode.Alpha4) && instantiatedParticles[3] != null) instantiatedParticles[3].Play();
        if (Input.GetKeyDown(KeyCode.Alpha5) && instantiatedParticles[4] != null) instantiatedParticles[4].Play();
        if (Input.GetKeyDown(KeyCode.Alpha6) && instantiatedParticles[5] != null) instantiatedParticles[5].Play();
        if (Input.GetKeyDown(KeyCode.Alpha7) && instantiatedParticles[6] != null) instantiatedParticles[6].Play();
        if (Input.GetKeyDown(KeyCode.Alpha8) && instantiatedParticles[7] != null) instantiatedParticles[7].Play();
        if (Input.GetKeyDown(KeyCode.Alpha9) && instantiatedParticles[8] != null) instantiatedParticles[8].Play();
        if (Input.GetKeyDown(KeyCode.Alpha0) && instantiatedParticles[9] != null) instantiatedParticles[9].Play();


    }
}
