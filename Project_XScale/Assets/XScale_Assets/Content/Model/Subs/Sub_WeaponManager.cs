using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sub_WeaponManager : MonoBehaviour {

    List<Transform> VLS_LPs = new List<Transform>();
    List<Transform> Torp_LPs = new List<Transform>();

    public Transform Target;

    public GameObject[] AvailableTorpedos;

    // Use this for initialization
    void Start ()
    {
        Transform[] Children = transform.GetComponentsInChildren<Transform>();
        for (int i = 0; i < Children.Length; i++)
        {
            if (Children[i].name.Contains("VLS_LP"))
                VLS_LPs.Add(Children[i]);
            if (Children[i].name.Contains("Torpedo_LP"))
                Torp_LPs.Add(Children[i]);
        }

        Load_TorpLP(Torp_LPs[0], AvailableTorpedos[0]);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKey("f"))
            Fire_TorpLP(Torp_LPs[0]);
    }

    private void Load_TorpLP(Transform LP, GameObject Weapon)
    {
        GameObject WP = Instantiate(Weapon, LP.position, LP.rotation, LP);
        Torpedo_Movement TM = WP.GetComponent<Torpedo_Movement>();
        TM.DeactiveWeapon();
        Debug.Log("Weapon: " + Weapon.name + "Loaded in " + LP.name);
    }

    private void Fire_TorpLP(Transform LP)
    {
        GameObject WP = LP.GetChild(0).gameObject;
        Torpedo_Movement TM = WP.GetComponent<Torpedo_Movement>();
        TM.ActiveWeapon();
        WP.transform.position = LP.position;
        WP.transform.rotation = LP.rotation;
        TM.TargetTransform = Target;
    }
}
