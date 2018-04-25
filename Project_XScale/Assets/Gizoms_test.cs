using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gizoms_test : MonoBehaviour {

    Rigidbody RB;
    LineRenderer LR;
    public Vector3 InitialVelocity;
    public int linecount;

    private Vector3 InitialPosition;
    
    // Use this for initialization
	void Start ()
    {
        RB = GetComponent<Rigidbody>();
        RB.velocity = InitialVelocity;

        LR = GetComponent<LineRenderer>();
        LR.positionCount = linecount;

        //InitialPosition = transform.position;
    }
	
	// Update is called once per frame
	void Update ()
    {
        //Vector3 MForward = RB.velocity.normalized*5;
        //Debug.DrawRay(transform.position, MForward, Color.red,1);
    }

    private void LateUpdate()
    {
        DrawPredictionLine(transform.position, RB.velocity);
        //DrawPredictionLine(InitialPosition, InitialVelocity);
    }

    private void DrawPredictionLine(Vector3 pos , Vector3 vel)
    {
        Vector3 Cupos = pos;
        Vector3 CuVel = vel;
        for (int i = 0; i < linecount; i++)
        {
            LR.SetPosition(i, Cupos);
            CuVel += Physics.gravity * Time.fixedDeltaTime;
            Cupos += CuVel * Time.fixedDeltaTime;
        }
    }
}
 

