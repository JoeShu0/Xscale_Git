using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sub_Movement : MonoBehaviour {

    [HideInInspector] public Rigidbody RB;
    [HideInInspector] public CalculateSurfacePoint SurfacePointCal;

    //Material _WaterMat;
    public WaterManager WaterM;

    public float BuoyancyPerPoint = 1;
    public float MAXBuoyancyDepth = 1;
    Vector3 ThrustDir = new Vector3(0, 0, 1);

    public float WaterDrag = 1;

    public Vector3 COMoffset = new Vector3(0,0,0);

    //public bool IsThrust = true;
    public float Thrust = 2000;

    public float ForwardDragFactor = 0.15f;
    public float SideDragFator = 1;
    public float UpwardDragFactor = 10f;

    public float[] BallastConfig = new float[11];
    
    [HideInInspector] public float Throttle = 0f;
    [HideInInspector] public float Rudder = 0f;
    [HideInInspector] public float Elevator = 0f;
    [HideInInspector] public int Ballast = 0;

    [HideInInspector] public float CurrentThrottle = 0f, CurrentRudder = 0, CurrentElevator = 0;

    public float BallastChangeSpeed = 10f;
    public float RudderChangeSpeed = 0.5f;
    public float ElevatorChangeSpeed = 0.5f;
    public float ThrottleChangeSpeed = 0.1f;

    List<Transform> B_Points = new List<Transform>();
    List<Transform> S_Points = new List<Transform>();
    List<Transform> Rudder_Points = new List<Transform>();
    List<Transform> Surface_Points = new List<Transform>();
    public Transform Ship_Front_Point;
    Vector3 UnderwaterCenter = new Vector3(0,0,0);

    [HideInInspector] public bool IsSetupComplete = false;
    public bool IsSurfaced = false;

    public float PropellerSpeed = 500f;

    void Start ()
    {
        //得到水面材质
        //MeshRenderer _WaterMeshRender = GameObject.FindWithTag("WaterSurface").GetComponent<MeshRenderer>();
        //_WaterMat = _WaterMeshRender.material;
        //得到waterManager
        WaterM = GameObject.FindWithTag("WaterSurface").GetComponent<WaterManager>();
        WaterM.WaterInterSubs.Add(transform);//在watermanager中注册该Sub
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
            if (Children[i].name.Contains("Surface_point"))
                Surface_Points.Add(Children[i]);
            if (Children[i].name.Contains("Ship_Front_Point"))
                Ship_Front_Point = Children[i];
        }

        //Debug.Log("Get "+B_Points.Count);

        RB.centerOfMass = COMoffset;
        IsSetupComplete = true;
    }

    private void Update()
    {
        UpdateAnimation();
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        LerpControl();

        if (SurfacePointCal.IsSetupComplete == false)
        {
            Debug.Log("Setting Up SurfacePointCalculator for " + transform.gameObject.name);
            return;
        }
        Debug.DrawLine(transform.position, transform.position + RB.velocity / 1f, Color.cyan);//Draw Speed
        Debug.DrawLine(transform.position + RB.centerOfMass + transform.right * 5f, transform.position + RB.centerOfMass - transform.right * 5f, Color.black);//Draw COM
        Debug.DrawLine(transform.position, new Vector3(transform.position.x, SurfacePointCal.CalculateWaterPosition(transform.position), transform.position.z), Color.cyan);//Draw Depth
        //****************************************************surfacing detection**************************************************************
        for (int p = 0; p < Surface_Points.Count; p++)//surfacing detection
        {
            if (SurfacePointCal.CalculateWaterPosition(Surface_Points[p].position) < Surface_Points[p].position.y)
            {
                IsSurfaced = true;
                break;
            }
            IsSurfaced = false;
        }
        
        //****************************************************Add Control Force**************************************************************
        if (S_Points.Count > 0)//ADD Power
        {
            if (CurrentThrottle != 0f)
                foreach (Transform p in S_Points)
                {
                    ThrustDir = (transform.forward - transform.right * CurrentRudder * 0.5f - transform.up * CurrentElevator * 0.5f).normalized;
                    //ThrustDir = Vector3.MoveTowards(ThrustDir, NewThrustDir, RudderChangeSpeed * Time.deltaTime);
                    RB.AddForceAtPosition(ThrustDir * Thrust * CurrentThrottle, p.position + RB.centerOfMass/2);
                    Debug.DrawLine(p.position, p.position + ThrustDir * CurrentThrottle * 10f);
                }
            else if (CurrentRudder != 0f)
            {
                RB.AddForceAtPosition(Vector3.Dot(transform.forward, RB.velocity) * -transform.right * CurrentRudder * 30f, Rudder_Points[0].position);
                //Debug.DrawLine(Rudder_Points[0].position, Rudder_Points[0].position + Vector3.Dot(transform.forward, RB.velocity) * transform.right * Rudder);
            }
            //---------------Control Ballast---------------
            float ChangeSpeed = BallastChangeSpeed * Time.deltaTime;

            BuoyancyPerPoint = Mathf.MoveTowards(BuoyancyPerPoint, BallastConfig[BallastConfig.Length / 2 + Ballast], ChangeSpeed);


        }
        //****************************************************Buoyancy & Drag**************************************************************
        List<Vector3> UnderWaterParts = new List<Vector3>();
        for (int i = 0; i < B_Points.Count; i++)// add buoyancy force
        {
            float _CuS_point = SurfacePointCal.CalculateWaterPosition(B_Points[i].position);
            float _depth = _CuS_point - B_Points[i].position.y;
            float B = CalculateBuoyancy(_depth);
            if (B > 0)
            {
                UnderWaterParts.Add(B_Points[i].position);
                RB.AddForceAtPosition(new Vector3(0, B, 0), B_Points[i].position + transform.up * Mathf.Clamp(_depth, 0, MAXBuoyancyDepth));// new Vector3(0,Mathf.Clamp(_depth,0, MAXBuoyancyDepth), 0)/2f);
                Debug.DrawLine(B_Points[i].position + new Vector3(0, Mathf.Clamp(_depth, 0, MAXBuoyancyDepth), 0) / 2f, B_Points[i].position + new Vector3(0, Mathf.Clamp(_depth, 0, MAXBuoyancyDepth), 0) / 2f + new Vector3(0, B/100, 0), Color.green);
            }
            if (UnderWaterParts.Count >0)//add water Drag , Split into Three direction;
            {
                UnderwaterCenter = new Vector3(0, 0, 0);
                for (int n = 0; n < UnderWaterParts.Count; n++)
                    UnderwaterCenter += (B_Points[n].position );
                UnderwaterCenter /= UnderWaterParts.Count;
                //UnderwaterCenter -= transform.forward * 5f;
               
                Vector3 ForwardDrag = -transform.forward * 
                    (Vector3.Dot(transform.forward , RB.velocity) * RB.velocity.magnitude * WaterDrag * ForwardDragFactor);
                //Debug.DrawLine(UnderwaterCenter, UnderwaterCenter + ForwardDrag,Color.red);
                Vector3 SideDrag = -transform.right *
                    (Vector3.Dot(transform.right, RB.velocity) * RB.velocity.magnitude * WaterDrag * SideDragFator);
                //Debug.DrawLine(UnderwaterCenter, UnderwaterCenter + SideDrag, Color.red);
                Vector3 UpwardDrag = -transform.up *
                    (Vector3.Dot(transform.up, RB.velocity) * RB.velocity.magnitude  * WaterDrag * UpwardDragFactor);
                //Debug.DrawLine(UnderwaterCenter, UnderwaterCenter + UpwardDrag, Color.red);
                RB.AddForceAtPosition( (SideDrag + UpwardDrag) * UnderWaterParts.Count,  transform.position + RB.centerOfMass);//Do Not Use Forward Drag!!!! will Cause ship drift.
                RB.AddForceAtPosition(ForwardDrag * UnderWaterParts.Count, transform.position);//forward Drag must line up with the thrust!!
                //Debug.DrawLine(transform.position, transform.position + (SideDrag + UpwardDrag + ForwardDrag) * UnderWaterParts.Count/100f, Color.red);
            }
        }
        
	}

    private void UpdateAnimation()
    {

        S_Points[0].Rotate(-Vector3.forward * Time.deltaTime * PropellerSpeed * CurrentThrottle);
    }

    private void LerpControl()
    {
        CurrentThrottle = Mathf.Lerp(CurrentThrottle, Throttle, ThrottleChangeSpeed * Time.deltaTime);
        CurrentRudder = Mathf.Lerp(CurrentRudder, Rudder, RudderChangeSpeed * Time.deltaTime);
        CurrentElevator = Mathf.Lerp(CurrentElevator, Elevator, ElevatorChangeSpeed * Time.deltaTime);
    }

    float CalculateBuoyancy(float D)
    {
        if (D <= 0)
            return 0;
        else if (D >= MAXBuoyancyDepth)
            return BuoyancyPerPoint;
        else
            return Mathf.Lerp(0, BuoyancyPerPoint, D / MAXBuoyancyDepth);
    }
}
