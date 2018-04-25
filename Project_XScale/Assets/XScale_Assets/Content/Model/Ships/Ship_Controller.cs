using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship_Controller : MonoBehaviour {

    [HideInInspector]public Rigidbody RB;
    [HideInInspector]public CalculateSurfacePoint SurfacePointCal;
    [HideInInspector] public Ship_Movement ShipMovement;

    public bool IsThrust = true;
    //public float Thrust = 0;

    List<Transform> S_Points = new List<Transform>();

    // Use this for initialization
    void Start ()
    {
        RB = GetComponent<Rigidbody>();
        SurfacePointCal = GetComponent<CalculateSurfacePoint>();
        ShipMovement = GetComponent<Ship_Movement>();

        Transform[] Children = transform.GetComponentsInChildren<Transform>();

        for (int i = 0; i < Children.Length; i++)
        {
            if (Children[i].name.Contains("S_point"))
                S_Points.Add(Children[i]);
        }
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        if (!ShipMovement.IsSetupComplete)
        {
            Debug.Log("Setting Up ShipMovement For ShipController");
            return;
        }
        ShipMovement.Throttle = Input.GetAxis("Vertical");
        
        ShipMovement.Rudder = Input.GetAxis("Horizontal");
        //Debug.Log(ShipMovement.Throttle +" "+ ShipMovement.Rudder);
    }
}
