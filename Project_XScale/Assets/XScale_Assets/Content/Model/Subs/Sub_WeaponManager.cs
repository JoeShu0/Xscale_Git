using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sub_WeaponManager : MonoBehaviour {

    List<Transform> VLS_LPs = new List<Transform>();
    List<Transform> Torp_LPs = new List<Transform>();

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
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
