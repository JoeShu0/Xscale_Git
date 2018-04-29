using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sub_Controller : MonoBehaviour {

    [HideInInspector] public Rigidbody RB;
    [HideInInspector] public CalculateSurfacePoint SurfacePointCal;
    [HideInInspector] public Sub_Movement SubMovement;

    public bool IsThrust = true;

    float Throttle = 0;
    float Elevator = 0;
    float Rudder = 0;
    int Ballast = 0;

    [Range(-2, 4)]
    int EngineGear = 0;
    //[Range(-3, 3)]
    //int RudderGear = 0;
    //[Range(-3, 3)]
    //int ElevatorGear = 0;
    //[Range(-3, 3)]
    //int Ballastlevel = 0;

    List<Transform> S_Points = new List<Transform>();

    // Use this for initialization
    void Start()
    {
        RB = GetComponent<Rigidbody>();
        SurfacePointCal = GetComponent<CalculateSurfacePoint>();
        SubMovement = GetComponent<Sub_Movement>();

        Transform[] Children = transform.GetComponentsInChildren<Transform>();

        for (int i = 0; i < Children.Length; i++)
        {
            if (Children[i].name.Contains("S_point"))
                S_Points.Add(Children[i]);
        }
    }

    private void Update()
    {

        RefreshMoveStatus();
        

    }

    void RefreshMoveStatus()
    {
        if (Input.GetKeyDown("q") && EngineGear < 4)
            EngineGear++;
        if (Input.GetKeyDown("z") && EngineGear >-2)
            EngineGear--;

        if (Input.GetKey("s"))
            Elevator = 1;
        else if (Input.GetKey("w"))
            Elevator = -1;
        else Elevator = 0;

        if (Input.GetKey("d"))
            Rudder = 1;
        else if (Input.GetKey("a"))
            Rudder = -1;
        else Rudder = 0;
        /*
        if (Input.GetKeyDown("s") && ElevatorGear < 3)
            ElevatorGear++;
        if (Input.GetKeyDown("w") && ElevatorGear > -3)
            ElevatorGear--;
        if (Input.GetKeyDown("d") && RudderGear < 3)
            RudderGear++;
        if (Input.GetKeyDown("a") && RudderGear >-3)
            RudderGear--;
            */
        if (Input.GetKeyDown("e") && Ballast < 3)
            Ballast++;
        //else if (Input.GetKeyDown("e") && Ballast == 3 && SubMovement.IsSurfaced == true)
            //Ballast++;
        if (Input.GetKeyDown("c") && Ballast > -3)
            Ballast--;


        if (Input.GetKeyDown("x"))
        {
            //EngineGear = 0;
            //RudderGear = 0;
            //ElevatorGear = 0;
            Ballast = 0;
        }

        if (EngineGear >= 0)
            Throttle = Mathf.Lerp(0, 1, (float)EngineGear / 4);
        else
            Throttle = -Mathf.Lerp(0, 0.4f, -(float)EngineGear / 4);

        //Rudder = Mathf.Lerp(-1, 1, (float)(RudderGear + 3) / 6);
        //Elevator = Mathf.Lerp(-1, 1, (float)(ElevatorGear + 3) / 6);
        //Ballast = Mathf.Lerp(-1, 1, (float)(Ballastlevel + 3) / 6);
        //if (Ballastlevel > 3) Ballast = 2;
        //Debug.Log("Throttle: " + Throttle + " Rudder: " + Rudder + " Elevator: " + Elevator + " Ballast: " + Ballast);
        PastMoveStausToSubMovement(SubMovement, Throttle, Rudder, Elevator, Ballast);
    }

    static void PastMoveStausToSubMovement(Sub_Movement SubM, float Throttle, float Rudder, float Elevator, int Ballast)
    {
        SubM.Throttle = Throttle;
        SubM.Rudder = Rudder;
        SubM.Elevator = Elevator;
        SubM.Ballast = Ballast;
        
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        
    }
}
