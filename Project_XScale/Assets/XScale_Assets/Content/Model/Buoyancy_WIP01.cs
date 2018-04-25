using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buoyancy_WIP01 : MonoBehaviour {

    public Rigidbody RB;
    public CalculateSurfacePoint SPointCal;

    public float MAXBuoyancyPerPoint = 1;
    public float MAXBuoyancyDepth = 1;

    public float WaterDrag = 1;

    public Vector3 COMoffset = new Vector3(0,0,0);

    public bool IsThrust = true;
    public float Thrust = 50;

    List<Transform> B_Points = new List<Transform>();
    List<Transform> S_Points = new List<Transform>();
    Vector3 UnderwaterCenter = new Vector3(0,0,0);

    void Start ()
    {
        RB = GetComponent<Rigidbody>();
        SPointCal = GetComponent<CalculateSurfacePoint>();

        Transform[] Children = transform.GetComponentsInChildren<Transform>();

        for (int i = 0; i < Children.Length; i++)
        {
            if (Children[i].name.Contains("B_point"))
                B_Points.Add(Children[i]);
            if (Children[i].name.Contains("S_point"))
                S_Points.Add(Children[i]);
        }

        Debug.Log(B_Points.Count);

        RB.centerOfMass = COMoffset;
    }

    private void Update()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        if (IsThrust == true && S_Points.Count > 0)
            foreach (Transform p in S_Points)
            {
                Vector3 ThrustDir = (transform.forward - transform.right * Input.GetAxis("Horizontal") * 0.1f).normalized;
                RB.AddForceAtPosition(ThrustDir * Thrust * Input.GetAxis("Vertical"), p.position);
                Debug.DrawLine(p.position, p.position + ThrustDir * Thrust, Color.green);
            }

        List<Vector3> UnderWaterParts = new List<Vector3>();
        for (int i = 0; i < B_Points.Count; i++)// add buoyancy force
        {
            float _CuS_point = SPointCal.CalculateWaterPosition(B_Points[i].position);
            float _depth = _CuS_point - B_Points[i].position.y;
            float B = CalculateBuoyancy(_depth);
            if (B > 0)
            {
                UnderWaterParts.Add(B_Points[i].position);
                RB.AddForceAtPosition(new Vector3(0, B, 0), B_Points[i].position);
                Debug.DrawLine(B_Points[i].position, B_Points[i].position + new Vector3(0, B, 0), Color.green);
            }
            if (UnderWaterParts.Count >0)//add water Drag , counter to the V Lerp angle between V and Forward??
            {
                UnderwaterCenter = new Vector3(0, 0, 0);
                for (int n = 0; n < UnderWaterParts.Count; n++)
                    UnderwaterCenter += B_Points[n].position;
                UnderwaterCenter /= UnderWaterParts.Count;
                //Debug.Log(UnderwaterCenter);
                float DragFactor = Mathf.Abs(1 - Vector3.Dot(RB.velocity.normalized, transform.forward));
                RB.AddForceAtPosition(RB.velocity * RB.velocity.magnitude * -1 * WaterDrag * DragFactor, UnderwaterCenter);
            }
        }
        
	}

    float CalculateBuoyancy(float D)
    {
        if (D <= 0)
            return 0;
        else if (D >= MAXBuoyancyDepth)
            return MAXBuoyancyPerPoint;
        else
            return Mathf.Lerp(0, MAXBuoyancyPerPoint, D / MAXBuoyancyDepth);
    }
}
