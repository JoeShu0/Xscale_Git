using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship_Movement : MonoBehaviour {

    [HideInInspector] public Rigidbody RB;
    [HideInInspector] public CalculateSurfacePoint SurfacePointCal;

    //Material _WaterMat;

    public float MAXBuoyancyPerPoint = 1;
    public float MAXBuoyancyDepth = 1;

    public float WaterDrag = 1;

    public Vector3 COMoffset = new Vector3(0,0,0);

    //public bool IsThrust = true;
    public float Thrust = 2000;

    public float ForwardDragFactor = 0.15f;
    public float SideDragFator = 1;
    public float UpwardDragFactor = 10f;

    public WaterManager WaterM;

    [HideInInspector] public float Throttle = 0f;
    [HideInInspector] public float Rudder = 0f;

    List<Transform> B_Points = new List<Transform>();
    List<Transform> S_Points = new List<Transform>();
    List<Transform> Rudder_Points = new List<Transform>();
    public Transform Ship_Front_Point;
    Vector3 UnderwaterCenter = new Vector3(0,0,0);

    [HideInInspector] public bool IsSetupComplete = false;

    void Start ()
    {
        //得到水面材质
        //MeshRenderer _WaterMeshRender = GameObject.FindWithTag("WaterSurface").GetComponent<MeshRenderer>();
        //_WaterMat = _WaterMeshRender.material;
        //得到waterManager
        WaterM = GameObject.FindWithTag("WaterSurface").GetComponent<WaterManager>();
        WaterM.WaterInterShips.Add(transform);//在watermanager中注册该ship
        //得到水面高度计算器
        SurfacePointCal = transform.gameObject.AddComponent<CalculateSurfacePoint>() as CalculateSurfacePoint;
        RB = GetComponent<Rigidbody>();
        SurfacePointCal = GetComponent<CalculateSurfacePoint>();

        //遍历所有子对象并分类识别
        Transform[] Children = transform.GetComponentsInChildren<Transform>();
        for (int i = 0; i < Children.Length; i++)
        {
            if (Children[i].name.Contains("B_point"))
                B_Points.Add(Children[i]);
            if (Children[i].name.Contains("S_point"))
                S_Points.Add(Children[i]);
            if (Children[i].name.Contains("Rudder_point"))
                Rudder_Points.Add(Children[i]);
            if (Children[i].name.Contains("Ship_Front_Point"))
                Ship_Front_Point = Children[i];
        }



        RB.centerOfMass = COMoffset;
        IsSetupComplete = true;
    }

    private void Update()
    {
       
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        Debug.DrawLine(transform.position, transform.position + RB.velocity/1f, Color.cyan);//Draw Speed
        Debug.DrawLine(transform.position + RB.centerOfMass + transform.right *5f, transform.position +RB.centerOfMass - transform.right * 5f, Color.black);//Draw Speed
        if (SurfacePointCal.IsSetupComplete == false)
        {
            Debug.Log("Setting Up SurfacePointCalculator for " + transform.gameObject.name);
            return;
        }
        if (S_Points.Count > 0)//ADD Power
        {
            if (Throttle != 0f)
                if(Throttle > 0f)
                    foreach (Transform p in S_Points)
                    {
                        Vector3 ThrustDir = (transform.forward - transform.right * Rudder * 0.5f).normalized;
                        RB.AddForceAtPosition(ThrustDir * Thrust * Throttle, p.position);
                        Debug.DrawLine(p.position, p.position + ThrustDir);
                    }
                else
                    foreach (Transform p in S_Points)
                    {
                        RB.AddForceAtPosition(transform.forward * Thrust * Throttle, p.position);
                        RB.AddForceAtPosition(Vector3.Dot(transform.forward, RB.velocity) * -transform.right * Rudder * 10f, Rudder_Points[0].position);
                        //RB.AddForceAtPosition(Vector3.Dot(transform.forward,RB.velocity) * transform.right * Rudder , p.position);
                    }
            else if (Rudder != 0f)
            {
                RB.AddForceAtPosition(Vector3.Dot(transform.forward, RB.velocity) * -transform.right * Rudder *30f, Rudder_Points[0].position);
                //Debug.DrawLine(Rudder_Points[0].position, Rudder_Points[0].position + Vector3.Dot(transform.forward, RB.velocity) * transform.right * Rudder);
            }
        }
        List<Vector3> UnderWaterParts = new List<Vector3>();
        for (int i = 0; i < B_Points.Count; i++)// add buoyancy force
        {
            float _CuS_point = SurfacePointCal.CalculateWaterPosition(B_Points[i].position);
            float _depth = _CuS_point - B_Points[i].position.y;
            float B = CalculateBuoyancy(_depth);
            if (B > 0)
            {
                UnderWaterParts.Add(B_Points[i].position);
                RB.AddForceAtPosition(new Vector3(0, B, 0), B_Points[i].position);
                Debug.DrawLine(B_Points[i].position, B_Points[i].position + new Vector3(0, B/100, 0), Color.green);
            }
            if (UnderWaterParts.Count >0)//add water Drag , Split into Three direction;
            {
                UnderwaterCenter = new Vector3(0, 0, 0);
                for (int n = 0; n < UnderWaterParts.Count; n++)
                    UnderwaterCenter += B_Points[n].position;
                UnderwaterCenter /= UnderWaterParts.Count;
                UnderwaterCenter -= transform.forward * 5f;
               
                Vector3 ForwardDrag = -transform.forward * 
                    (Vector3.Dot(transform.forward , RB.velocity) * RB.velocity.magnitude * WaterDrag * ForwardDragFactor);
                Debug.DrawLine(UnderwaterCenter, UnderwaterCenter + ForwardDrag,Color.red);
                Vector3 SideDrag = -transform.right *
                    (Vector3.Dot(transform.right, RB.velocity) * RB.velocity.magnitude * WaterDrag * SideDragFator);
                Debug.DrawLine(UnderwaterCenter, UnderwaterCenter + SideDrag, Color.red);
                Vector3 UpwardDrag = -transform.up *
                    (Vector3.Dot(transform.up, RB.velocity) * RB.velocity.magnitude  * WaterDrag * UpwardDragFactor*10);
                Debug.DrawLine(UnderwaterCenter, UnderwaterCenter + UpwardDrag, Color.red);
                RB.AddForceAtPosition( SideDrag + UpwardDrag,  UnderwaterCenter);//Do Not Use Forward Drag!!!! will Cause ship drift.
                //float DragFactor = Mathf.Abs(1 - Vector3.Dot(RB.velocity.normalized, transform.forward));
                //RB.AddForceAtPosition(RB.velocity * RB.velocity.magnitude * -1 * WaterDrag * DragFactor, UnderwaterCenter);
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
