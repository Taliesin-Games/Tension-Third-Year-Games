using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class DodgeGhostEffect : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The SkinnedMeshRenderer of the character to create ghosts from")]
    [SerializeField] SkinnedMeshRenderer sourceRenderer; // the main character mesh
    [Tooltip("Material used for the ghost (should support _GhostAlpha)")]
    [SerializeField] Material ghostMaterial;// material used for the ghost (should support _GhostAlpha)
    [Tooltip("Reference to PlayerInput component for detecting dodge action")]
    [SerializeField] private PlayerInput playerInput; // Reference to PlayerInput component

    [Header("Settings")]
    [Tooltip("Number of ghosts to create during dodge")]
    [SerializeField] int ghostCount = 4; // number of ghosts to create
    [Tooltip("Duration of each ghost before it disappears")]
    [SerializeField] float ghostLifetime = 0.4f; // duration of each ghost
    [Tooltip("End distance of ghosts from player position")]
    [SerializeField] float radius = 1.5f; // End distance from center
    [Tooltip("If true, ghosts face the player; if false, they keep the character's rotation")]
    [SerializeField] bool faceCenter = true; // if true, ghosts face the player

    // Controls the "burst vs linger" shape
    [Tooltip("Higher values make the ghost burst out quickly and slow down at the end")]
    [Range(0.1f, 5f)] [SerializeField] float motionEase = 2.5f;  // higher = faster burst, slower end
    [Tooltip("Higher values make the ghost linger longer before fading out")]
    [Range(0.1f, 5f)] [SerializeField] float fadeEase = 1.5f;  // higher = longer linger before fade


    //TODO: Integrate with dodge action in player controller
    private void Update()
    {
        if (playerInput == null)
            return;
        if (playerInput.actions["Roll"].triggered)
        {
            CreateGhosts();
        }
    }


    //CALL THISMETHOD WHEN DODGING TO MAKE THE GHOSTS
    public void CreateGhosts()
    {
        for (int i = 0; i < ghostCount; i++)
            StartCoroutine(CreateAndFollowGhost(i));
    }


    // Creates a ghost that moves outward and fades over time
    private IEnumerator CreateAndFollowGhost(int index)
    {
        if (sourceRenderer == null || ghostMaterial == null)
            yield break;

        // Bake the current mesh state
        Mesh bakedMesh = new Mesh();
        sourceRenderer.BakeMesh(bakedMesh);

        //Create ghost GameObject
        GameObject ghost = new GameObject("Ghost_" + index);
        MeshFilter mf = ghost.AddComponent<MeshFilter>();
        MeshRenderer mr = ghost.AddComponent<MeshRenderer>();
        mf.sharedMesh = bakedMesh;

        //Set material of ghost
        Material matInstance = new Material(ghostMaterial);
        mr.material = matInstance;

        //Compute direction for this ghost
        float angle = (360f / ghostCount) * index;
        Quaternion rot = Quaternion.Euler(0, angle, 0);
        Vector3 dir = rot * Vector3.forward;

        float t = 0f;
        while (t < ghostLifetime)
        {
            // Update time
            t += Time.deltaTime;
            float normalizedTime = t / ghostLifetime;

            // Eased motion: start fast, end slow
            float motionT = 1f - Mathf.Pow(1f - normalizedTime, motionEase);
            Vector3 offset = dir * (radius * motionT);
            ghost.transform.position = transform.position + offset;

            // Face player or keep rotation
            if (faceCenter)
                ghost.transform.LookAt(transform.position);
            else
                ghost.transform.rotation = transform.rotation;

            // Eased fade: stay visible, then drop off
            float fadeT = Mathf.Pow(normalizedTime, fadeEase);
            float alpha = 1f - fadeT;
            matInstance.SetFloat("_GhostAlpha", alpha);

            yield return null;
        }

        Destroy(ghost);
    }
}

