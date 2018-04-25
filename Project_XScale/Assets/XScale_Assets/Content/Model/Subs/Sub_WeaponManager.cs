using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sub_WeaponManager : MonoBehaviour {

    List<Transform> VLS_LPs = new List<Transform>();
    List<Transform> Torp_LPs = new List<Transform>();

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
		
	}

    private void Load_TorpLP(Transform LP, GameObject Weapon)
    {
        GameObject WP = Instantiate(Weapon, LP.position, LP.rotation, LP);
        WP.active = false;
        Torpedo_Movement TorpM = WP.GetComponent<Torpedo_Movement>();
        TorpM.enabled = false;
        Debug.Log("Weapon: " + Weapon.name + "Loaded in " + LP.name);
    }
}
