using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEMP_LTube : MonoBehaviour {

    public bool load = false;
    public bool Launch = false;
    public GameObject[] Weapons;
    public int weapon_index = 0;
    // Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (load == true)
        {
            Load_LP(transform, Weapons[weapon_index]);
            load = false;
        }
        if (Launch == true)
        {
            Fire_LP(transform);
            Launch = false;
        }

	}

    private void Load_LP(Transform LP, GameObject Weapon)
    {
        GameObject WP = Instantiate(Weapon, LP.position, LP.rotation, LP);
        WP.SendMessage("DeactiveWeapon");
        //Missile_Movement MM = WP.GetComponent<Missile_Movement>();
        //MM.DeactiveWeapon();
        Debug.Log("Weapon: " + Weapon.name + "Loaded in " + LP.name);
    }

    private void Fire_LP(Transform LP)
    {
        GameObject WP = LP.GetChild(0).gameObject;
        WP.SendMessage("ActiveWeapon");
        //Torpedo_Movement TM = WP.GetComponent<Torpedo_Movement>();
        //TM.ActiveWeapon();
        WP.transform.position = LP.position;
        WP.transform.rotation = LP.rotation;
        WP.GetComponent<Rigidbody>().AddForce(transform.forward*5000);
        //WP.GetComponent<Rigidbody>().velocity = RB.velocity;
        //TM.TargetTransform = Target;
    }
}
