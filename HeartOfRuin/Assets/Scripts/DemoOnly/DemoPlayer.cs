using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class DemoPlayer : MonoBehaviour
{
    GameObject weaponObject;
    public CharacterStats playerStats;
    private void Start()
    {
        weaponObject = GameObject.Find("DemoSword");
        playerStats = GetComponent<CharacterStats>();
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

