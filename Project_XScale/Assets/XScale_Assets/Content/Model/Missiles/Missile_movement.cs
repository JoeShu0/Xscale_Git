using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile_Movement : MonoBehaviour {

    [HideInInspector] public Rigidbody RB;
    [HideInInspector] public CalculateSurfacePoint SurfacePointCal;
    public float Thrust = 10;
    public float BoosterActiveTime = 4;

    public float ExplosionForce = 10000;
    //List<Transform> B_Points = new List<Transform>();
    List<Transform> S_point = new List<Transform>();
    List<Transform> Boosters = new List<Transform>();
    Transform RudderMesh, ElevatorMesh, Arm_point;
    Vector3 UnderwaterCenter;
    public float AirDrag = 1;
    public Vector3 COMoffset = new Vector3(0, 0, 0);
    public float ForwardDragFactor = 0.08f;
    public float SideDragFator = 1;
    public float UpwardDragFactor = 1f;
    public float SteerForce = 1;

    [HideInInspector] public bool M_ISActive = false;//是否处于激活状态，激活后能够单独计算动力，阻力
    [HideInInspector] public bool M_ISArmed = false;//是否会接触就爆炸

    private bool IsInBarrel;//是否还在发射管当中，在发射管中时，运动方向会受到限制
    private float BarrelLength = 5;//发射管长度
    public bool M_ISEngineActive = true;//

    Vector3 InitialLocalPos;
    Quaternion InitialLocalRot;//记录初始的局部坐标

    public Transform TargetTransform;
    Vector3 TargetPos;
    //Quaternion CurrentRotation;

    public GameObject ExplosionParticle;
    public VFX_RefleSetup VFX_Setup;

    public SteerType M_SteerType;

    float EngineBurnTime = 0;

    public enum SteerType
    {
        AirDynamic,
        VectorThrust,
    }

    private void Awake()
    {
        SurfacePointCal = transform.gameObject.AddComponent<CalculateSurfacePoint>() as CalculateSurfacePoint;
        RB = GetComponent<Rigidbody>();
        SurfacePointCal = GetComponent<CalculateSurfacePoint>();
        //遍历所有子对象并分类识别
        Transform[] Children = transform.GetComponentsInChildren<Transform>();
        for (int i = 0; i < Children.Length; i++)
        {
            //if (Children[i].name.Contains("Rudder"))
            //RudderMesh = Children[i];
            //if (Children[i].name.Contains("Elevator"))
            //ElevatorMesh = Children[i];
            if (Children[i].name.Contains("S_Point"))
                S_point.Add(Children[i]);
            if (Children[i].name.Contains("_Booster"))
                Boosters.Add(Children[i]);
        }
        RB.centerOfMass = COMoffset;

        VFX_Setup = GetComponent<VFX_RefleSetup>();

        InitialLocalPos = transform.localPosition;
        InitialLocalRot = transform.localRotation;//记录初始的局部坐标
    }

    // Use this for initialization
    void Start()
    {
        //float EngineBurnTime = 0;
    }
    private void Update()
    {

        if (IsInBarrel)
            BarrelRestrict();//发射管中时，运动方向会受到限制

        EngineBurnTime += Time.deltaTime;
        if (EngineBurnTime > BoosterActiveTime && M_ISEngineActive == true)
            DetachBooster();
        

    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        if (SurfacePointCal.IsSetupComplete == false)
        {
            Debug.Log("Setting Up SurfacePointCalculator for " + transform.gameObject.name);
            return;
        }

        if (TargetTransform == null)
            TargetPos = transform.forward * 10 + transform.position;
        else
            TargetPos = TargetTransform.position;
        Debug.DrawLine(transform.position, TargetPos, Color.cyan);

        

        if (M_ISActive)
        {
            AddForces_Drag();
            AddForces_Thrust();
            if (transform.position.y <= 0)
                Explode();
        }
        
    }

    void AddForces_Drag()
    {

        /*
        float ForwardVel = Vector3.Dot(transform.forward, RB.velocity);
        Vector3 ForwardDrag = -transform.forward * ((ForwardVel * RB.velocity.magnitude) * AirDrag * ForwardDragFactor);
        float SideVel = Vector3.Dot(transform.right, RB.velocity);
        Vector3 SideDrag = -transform.right * ((SideVel * RB.velocity.magnitude) * AirDrag * SideDragFator);
        float UpwardVel = Vector3.Dot(transform.up, RB.velocity);
        Vector3 UpwardDrag = -transform.up * ((UpwardVel* RB.velocity.magnitude)  * AirDrag * UpwardDragFactor);
        //Debug.DrawLine(UnderwaterCenter, UnderwaterCenter + UpwardDrag, Color.red);
        RB.AddForceAtPosition((SideDrag + ForwardDrag) , transform.position - transform.forward *1f);//Do Not Use Forward Drag!!!! will Cause ship drift.
        RB.AddForceAtPosition(ForwardDrag , transform.position - transform.forward * 1f);//forward Drag must line up with the thrust!!
        //Debug.DrawLine(transform.position, transform.position + (SideDrag + UpwardDrag + ForwardDrag) * UnderWaterParts.Count/100f, Color.red);
        */
        Vector3 ForwardDrag = -transform.forward *
                    (Vector3.Dot(transform.forward, RB.velocity) * RB.velocity.magnitude * AirDrag * ForwardDragFactor);
        //Debug.DrawLine(UnderwaterCenter, UnderwaterCenter + ForwardDrag,Color.red);
        Vector3 SideDrag = -transform.right *
            (Vector3.Dot(transform.right, RB.velocity) * RB.velocity.magnitude * AirDrag * SideDragFator);
        //Debug.DrawLine(UnderwaterCenter, UnderwaterCenter + SideDrag, Color.red);
        Vector3 UpwardDrag = -transform.up *
            (Vector3.Dot(transform.up, RB.velocity) * RB.velocity.magnitude * AirDrag * UpwardDragFactor);
        //Debug.DrawLine(UnderwaterCenter, UnderwaterCenter + UpwardDrag, Color.red);
        RB.AddForceAtPosition((SideDrag + UpwardDrag), transform.position - transform.forward * 0.1f);//Do Not Use Forward Drag!!!! will Cause ship drift.
        RB.AddForceAtPosition(ForwardDrag, transform.position - transform.forward * 2);//forward Drag must line up with the thrust!!
                                                                                       //Debug.DrawLine(transform.position, transform.position + (SideDrag + UpwardDrag + ForwardDrag) * UnderWaterParts.Count/100f, Color.red);
    }

    void AddForces_Thrust()
    {
        //***************************************Thrust&rudder&elevator************************************************
        switch (M_SteerType)
        {
            case SteerType.AirDynamic:
                {
                    Vector3 RencorrectVector = (TargetPos - transform.position).normalized - transform.forward;
                    RencorrectVector = transform.InverseTransformVector(RencorrectVector);
                    RencorrectVector.z = 0f;
                    RencorrectVector = transform.TransformVector(RencorrectVector);

                    if (M_ISEngineActive)
                    {
                        Vector3 TrustVector = S_point[0].forward * Thrust;
                        RB.AddForceAtPosition(TrustVector, S_point[0].position);
                        Debug.DrawLine(S_point[0].position, TrustVector + S_point[0].position, Color.green);
                    }
                    else
                    {
                        Vector3 TrustVector = S_point[0].forward * Thrust * 0.5f;
                        RB.AddForceAtPosition(TrustVector, S_point[0].position);
                        Debug.DrawLine(S_point[0].position, TrustVector + S_point[0].position, Color.red);
                    }
                        
                    float ForwardAirSpeed = Vector3.Dot(RB.velocity, transform.forward);
                    RB.AddForceAtPosition((-RencorrectVector - (Vector3.up * 0.1f) / transform.position.y) * ForwardAirSpeed * 0.6f, S_point[0].position);
                    
                    break;
                }
            case SteerType.VectorThrust:
                {
                    Vector3 RencorrectVector = (TargetPos - transform.position).normalized - transform.forward;
                    Vector3 FinalTrustVector = (S_point[0].forward - RencorrectVector * SteerForce - (Vector3.up * 0.1f) / transform.position.y).normalized * Thrust;
                    RB.AddForceAtPosition(FinalTrustVector, S_point[0].position);
                    Debug.DrawLine(S_point[0].position, FinalTrustVector + S_point[0].position, Color.green);
                    break;
                }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(M_ISArmed)
        { 
            Debug.Log(other.gameObject.name + "IN CONTACT WITH " + gameObject.name);
            Explode();
        }
    }

    public void Explode()
    {
        Collider[] InRangeObjects = Physics.OverlapSphere(transform.position, 50f);
        Debug.Log(InRangeObjects.Length);
        for (int i = 0; i < InRangeObjects.Length; i++)
        {
            Rigidbody EffectRB = InRangeObjects[i].GetComponentInParent<Rigidbody>();
            if (EffectRB != null)
            {
                Debug.Log(EffectRB.gameObject.name);
                EffectRB.AddExplosionForce(ExplosionForce, transform.position, 50);
            }
        }
        ParticleSystem[] WaterTrail = GetComponentsInChildren<ParticleSystem>();
        foreach (var PS in WaterTrail)
        {
            ParticleSystem.EmissionModule EM = PS.emission;
            EM.rateOverTime = 0;
            Destroy(PS.gameObject, 3f);
            PS.transform.parent = null;
        }

        if (ExplosionParticle != null)
        {
            GameObject ExplosionP = Instantiate(ExplosionParticle, transform.position, transform.rotation);
            Destroy(ExplosionP, 3f);
        }
        //WaterTrail.emission.rateOverTime = 0;
        Destroy(transform.gameObject);
    }


    public void ActiveWeapon()
    {
        foreach (var S in S_point)
        {
            S.gameObject.SetActive(true);
        }
        S_point[0].gameObject.SetActive(true);
        VFX_Setup.enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
        M_ISActive = true;
        IsInBarrel = true;
        //StartCoroutine(ArmCountDown());
        //transform.parent = null;
    }

    public void DeactiveWeapon()
    {
        M_ISActive = false;
        GetComponent<Rigidbody>().isKinematic = true;
        foreach (var S in S_point)
        {
            S.gameObject.SetActive(false);
        }
        VFX_Setup.enabled = false;
    }

    private void DetachBooster()
    {
        M_ISEngineActive = false;
        Boosters[0].parent = null;
        Boosters[0].gameObject.AddComponent<Rigidbody>();
        Boosters[0].GetComponent<Rigidbody>().velocity = RB.velocity;
        Destroy(Boosters[0].gameObject , 3f);
    }

    private void BarrelRestrict()
    {
        transform.localRotation = InitialLocalRot;
        transform.localPosition = new Vector3(InitialLocalPos.x, InitialLocalPos.y, transform.localPosition.z);

        if (transform.localPosition.z > BarrelLength)//脱离发射管，激活武器，不再限制方向，从发射车辆上面脱离
        {
            M_ISArmed = true;
            IsInBarrel = false;
            transform.parent = null;
        }
    }
}


