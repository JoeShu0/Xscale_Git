using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torpedo_Movement : MonoBehaviour {

    public float PropellerSpeed = 500;
    public float Thrust = 10;
    public float SteerForce = 1;
    float ActiveThrust = 0;
    public float ExplosionForce = 10000;
    public float MinDepth = 2.5f;
    //public Collider ArmTrigger;

    [HideInInspector] public Rigidbody RB;
    [HideInInspector] public CalculateSurfacePoint SurfacePointCal;

    List<Transform> B_Points = new List<Transform>();
    List<Transform> S_point = new List<Transform>();
    Transform RudderMesh, ElevatorMesh, Arm_point , Hatch;
    Vector3 UnderwaterCenter;
    public float WaterDrag = 1;
    public Vector3 COMoffset = new Vector3(0, -1, 0);
    public float ForwardDragFactor = 0.08f;
    public float SideDragFator = 1;
    public float UpwardDragFactor = 1f;

    //float Rudder = 0, Elevator = 0;

    public float BuoyancyFactor = 5;
    float MAXBuoyancyPerPoint;
    public float MAXBuoyancyDepth = 0.1f;

    //bool IsSetupComplete = false;
    //bool EnablePropeller = false;

    public Transform TargetTransform;
    Vector3 TargetPos;
    //Quaternion CurrentRotation;

    public GameObject ExplosionParticle;
    public VFX_RefleSetup VFX_Setup;

    [HideInInspector] public bool M_ISActive = false;//是否处于激活状态，激活后能够单独计算动力，阻力
    [HideInInspector] public bool M_ISArmed = false;//是否会接触就爆炸
    bool M_ISHatchOpen = false;//
    bool M_ISEngineActive = true;//


    private bool IsInBarrel;//是否还在发射管当中，在发射管中时，运动方向会受到限制
    private float BarrelLength = 5;//发射管长度

    Vector3 InitialLocalPos;
    Quaternion InitialLocalRot;//记录初始的局部坐标

    public GameObject WeaponInSide;
    private GameObject WP;



    public enum Type
    {
        Torpedo,
        MissileLaunchTube,
    }

    public Type Torp_Type;

    // Use this for initialization
    private void Awake()
    {
        RB = GetComponent<Rigidbody>();

        SurfacePointCal = transform.gameObject.AddComponent<CalculateSurfacePoint>() as CalculateSurfacePoint;
        //RB = GetComponent<Rigidbody>();
        SurfacePointCal = GetComponent<CalculateSurfacePoint>();

        VFX_Setup = GetComponent<VFX_RefleSetup>();

        //遍历所有子对象并分类识别
        Transform[] Children = transform.GetComponentsInChildren<Transform>();
        for (int i = 0; i < Children.Length; i++)
        {
            if (Children[i].name.Contains("B_point"))
                B_Points.Add(Children[i]);
            if (Children[i].name.Contains("S_point"))
                S_point.Add(Children[i]);
            //if (Children[i].name.Contains("Rudder"))
            //RudderMesh = Children[i];
            //if (Children[i].name.Contains("Elevator"))
            //ElevatorMesh = Children[i];
            //if (Children[i].name.Contains("Arm_point"))
            //Arm_point = Children[i];
            if (Children[i].name.Contains("_Hatch"))
                Hatch = Children[i];
        }
        RB.centerOfMass = COMoffset;
        //Debug.Log("Get "+B_Points.Count);
        MAXBuoyancyPerPoint = RB.mass * BuoyancyFactor / B_Points.Count;

        InitialLocalPos = transform.localPosition;
        InitialLocalRot = transform.localRotation;//记录初始的局部坐标

        M_ISActive = false;
        M_ISArmed = false;
}

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Pos: " + transform.localPosition + "  Rot: " + transform.localRotation);
    }

    private void LateUpdate()
    {
        if (IsInBarrel)
            BarrelRestrict();//发射管中时，运动方向会受到限制
    }

    private void FixedUpdate()
    {
        if (M_ISActive == true)
        {
            if (SurfacePointCal.IsSetupComplete == false)
            {
                Debug.Log("Setting Up SurfacePointCalculator for " + transform.gameObject.name);
                return;
            }
            ActiveThrust = Thrust;
            if (SurfacePointCal.CalculateWaterPosition(S_point[0].position) < S_point[0].position.y)
                ActiveThrust = 0;
            switch (Torp_Type)
            {
                case Type.Torpedo:
                    {
                        if (TargetTransform == null)
                        {
                            TargetPos = new Vector3(transform.forward.x, 0, transform.forward.z) * 10 + transform.position;
                            if (TargetPos.y > -MinDepth)
                                TargetPos.y = -MinDepth;
                        }
                        else
                            TargetPos = TargetTransform.position;
                        Debug.DrawLine(transform.position, TargetPos, Color.cyan);
                        break;
                    }
                case Type.MissileLaunchTube:
                    {
                        TargetPos = new Vector3(transform.position.x ,0, transform.position.z);
                        Debug.DrawLine(transform.position, TargetPos, Color.cyan);
                        if (transform.position.y > 0 && M_ISHatchOpen == false)
                        {
                            OpenTube();
                            M_ISHatchOpen = true;

                            M_ISEngineActive = false;
                            Destroy(gameObject, 10);
                        }  
                        break;
                    }
            }


            AddDragBuoyancyForces();
            if(M_ISEngineActive == true)
                AddThrust();
        }

        //transform.localRotation = InitialLocalRot;
        //transform.localPosition = new Vector3(InitialLocalPos.x, InitialLocalPos.y, transform.position.z);
        //CurrentRotation = transform.rotation;
    }

    public void ActiveWeapon()
    {
        S_point[0].gameObject.SetActive(true);
        VFX_Setup.enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
        M_ISActive = true;
        IsInBarrel = true;
        //StartCoroutine(ArmCountDown());
        //transform.parent = null;
        switch (Torp_Type)
        {
            case Type.MissileLaunchTube:
                {
                    WP = Instantiate(WeaponInSide, transform.position, transform.rotation, transform);
                    //Torpedo_Movement TM = WP.GetComponent<Torpedo_Movement>();
                    //TM.DeactiveWeapon();
                    WP.SendMessage("DeactiveWeapon");
                    //WP.GetComponent<Missile_Movement>().TargetTransform = TargetTransform;
                    Debug.Log("Weapon: " + WeaponInSide.name + "Loaded ");
                    break;
                }
        }
    }

    public void DeactiveWeapon()
    {
        M_ISActive = false;
        GetComponent<Rigidbody>().isKinematic = true;
        S_point[0].gameObject.SetActive(false);
        VFX_Setup.enabled = false;
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

    void AddDragBuoyancyForces()
    {
        //******************************************Buoyancy&drag***************************************
        List<Vector3> UnderWaterParts = new List<Vector3>();
        for (int i = 0; i < B_Points.Count; i++)// add buoyancy force
        {
            float _CuS_point = SurfacePointCal.CalculateWaterPosition(B_Points[i].position);
            float _depth = _CuS_point - B_Points[i].position.y;
            float B = CalculateBuoyancy(_depth);
            if (B > 0)
            {
                UnderWaterParts.Add(B_Points[i].position);
                RB.AddForceAtPosition(new Vector3(0, B, 0), B_Points[i].position + new Vector3(0, Mathf.Clamp(_depth, 0, MAXBuoyancyDepth), 0) / 2f);
                Debug.DrawLine(B_Points[i].position + new Vector3(0, Mathf.Clamp(_depth, 0, MAXBuoyancyDepth), 0) / 2f, B_Points[i].position + new Vector3(0, Mathf.Clamp(_depth, 0, MAXBuoyancyDepth), 0) / 2f + new Vector3(0, B / 100, 0), Color.green);
            }
            if (UnderWaterParts.Count > 0)//add water Drag , Split into Three direction;
            {
                UnderwaterCenter = new Vector3(0, 0, 0);
                for (int n = 0; n < UnderWaterParts.Count; n++)
                    UnderwaterCenter += (B_Points[n].position);
                UnderwaterCenter /= UnderWaterParts.Count;
                //UnderwaterCenter -= transform.forward * 5f;

                Vector3 ForwardDrag = -transform.forward *
                    (Vector3.Dot(transform.forward, RB.velocity) * RB.velocity.magnitude * WaterDrag * ForwardDragFactor);
                //Debug.DrawLine(UnderwaterCenter, UnderwaterCenter + ForwardDrag,Color.red);
                Vector3 SideDrag = -transform.right *
                    (Vector3.Dot(transform.right, RB.velocity) * RB.velocity.magnitude * WaterDrag * SideDragFator);
                //Debug.DrawLine(UnderwaterCenter, UnderwaterCenter + SideDrag, Color.red);
                Vector3 UpwardDrag = -transform.up *
                    (Vector3.Dot(transform.up, RB.velocity) * RB.velocity.magnitude * WaterDrag * UpwardDragFactor);
                //Debug.DrawLine(UnderwaterCenter, UnderwaterCenter + UpwardDrag, Color.red);
                RB.AddForceAtPosition((SideDrag + UpwardDrag) * UnderWaterParts.Count, transform.position + RB.centerOfMass);//Do Not Use Forward Drag!!!! will Cause ship drift.
                RB.AddForceAtPosition(ForwardDrag * UnderWaterParts.Count, transform.position);//forward Drag must line up with the thrust!!
                //Debug.DrawLine(transform.position, transform.position + (SideDrag + UpwardDrag + ForwardDrag) * UnderWaterParts.Count/100f, Color.red);
            }
        }
    }

    void AddThrust()
    {
        //***************************************Thrust&rudder&elevator************************************************
        Vector3 RencorrectVector = (TargetPos - transform.position).normalized - transform.forward;
        RB.AddForceAtPosition((S_point[0].forward - RencorrectVector * SteerForce * 0.1f * RB.velocity.magnitude) * ActiveThrust, S_point[0].position);
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

    private void OnTriggerEnter(Collider other)
    {
        if (M_ISArmed == true)
        {
            switch (Torp_Type)
            {
                case Type.Torpedo:
                    {
                        Debug.Log(other.gameObject.name);
                        Explode();
                        break;
                    }
                case Type.MissileLaunchTube:
                    {
                        Debug.Log(other.gameObject.name);
                        //***********************action needed*****************
                        break;
                    }
            }
        }
        
    }

    public void Explode()
    {
        Debug.Log(transform.gameObject.name + " Explode!");
        Collider[] InRangeObjects = Physics.OverlapSphere(transform.position, 50f);
        //Debug.Log(InRangeObjects.Length);
        for (int i = 0; i < InRangeObjects.Length; i++)
        {
            Rigidbody EffectRB = InRangeObjects[i].GetComponentInParent<Rigidbody>();
            if(EffectRB != null)
            {
                //Debug.Log(EffectRB.gameObject.name);
                EffectRB.AddExplosionForce(ExplosionForce, transform.position, 50);
            }
        }
        ParticleSystem[] WaterTrail = GetComponentsInChildren<ParticleSystem>();
        foreach(var PS in WaterTrail)
        {
            ParticleSystem.EmissionModule EM = PS.emission;
            EM.rateOverTime = 0;
            Destroy(PS.gameObject, 10f);
            PS.transform.parent = null;
        }
        
        if (ExplosionParticle != null)
        { 
            GameObject ExplosionP =  Instantiate(ExplosionParticle, transform.position , transform.rotation);
            Destroy(ExplosionP, 3f);
        }
        //WaterTrail.emission.rateOverTime = 0;
        Destroy(transform.gameObject);
       
    }

    private void OpenTube()
    {
        Debug.Log("OpenTube HATCH");
        ParticleSystem[] WaterTrail = GetComponentsInChildren<ParticleSystem>();
        foreach (var PS in WaterTrail)
        {
            ParticleSystem.EmissionModule EM = PS.emission;
            EM.rateOverTime = 0;
            
            PS.transform.parent = null;
            Destroy(PS.gameObject, 10f);
        }
        Hatch.parent = null;
        Rigidbody HatchRB =  Hatch.gameObject.AddComponent<Rigidbody>() as Rigidbody;
        HatchRB.velocity = RB.velocity;
        HatchRB.mass = 0.02f;
        HatchRB.angularDrag = 0f;
        HatchRB.AddForce(transform.up * 5);
        HatchRB.AddTorque(-transform.right * 5000);
        Destroy(Hatch.gameObject, 3f);

        RB.mass = 0.85f * RB.mass;

        LaunchMissile();
    }

    void LaunchMissile()
    {
        WP.SendMessage("ActiveWeapon");
        WP.GetComponent<Missile_Movement>().TargetTransform = TargetTransform;
    }
}
