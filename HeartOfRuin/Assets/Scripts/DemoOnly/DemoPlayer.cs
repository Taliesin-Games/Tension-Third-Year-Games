using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class DemoPlayer : MonoBehaviour
{
    GameObject weaponObject;
    public PlayerStats playerStats;
    private void Start()
    {
        weaponObject = GameObject.Find("DemoSword");
        playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (weaponObject != null)
        {
            //rotate weapon around player
            weaponObject.transform.RotateAround(transform.position, Vector3.up, -30 * Time.deltaTime);
        }
    }
}

