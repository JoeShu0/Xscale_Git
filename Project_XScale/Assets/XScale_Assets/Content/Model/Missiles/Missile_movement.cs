using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile_Movement : MonoBehaviour {

    [HideInInspector] public Rigidbody RB;
    [HideInInspector] public CalculateSurfacePoint SurfacePointCal;
    public float Thrust = 10;

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

    Vector3 InitialLocalPos;
    Quaternion InitialLocalRot;//记录初始的局部坐标

    public Transform TargetTransform;
    Vector3 TargetPos;
    //Quaternion CurrentRotation;

    public GameObject ExplosionParticle;


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
            if (Children[i].name.Contains("_Booter"))
                Boosters.Add(Children[i]);
        }
        RB.centerOfMass = COMoffset;
    }

    // Use this for initialization
    void Start()
    {
        
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

        if(transform.position.y <= 0)
            Explode();

        AddForces();
    }


     

    void AddForces()
    {
        

                Vector3 ForwardDrag = -transform.forward *
                    (Vector3.Dot(transform.forward, RB.velocity) * RB.velocity.magnitude * AirDrag * ForwardDragFactor);
                //Debug.DrawLine(UnderwaterCenter, UnderwaterCenter + ForwardDrag,Color.red);
                Vector3 SideDrag = -transform.right *
                    (Vector3.Dot(transform.right, RB.velocity) * RB.velocity.magnitude * AirDrag * SideDragFator);
                //Debug.DrawLine(UnderwaterCenter, UnderwaterCenter + SideDrag, Color.red);
                Vector3 UpwardDrag = -transform.up *
                    (Vector3.Dot(transform.up, RB.velocity) * RB.velocity.magnitude * AirDrag * UpwardDragFactor);
                //Debug.DrawLine(UnderwaterCenter, UnderwaterCenter + UpwardDrag, Color.red);
                RB.AddForceAtPosition((SideDrag + UpwardDrag) , transform.position - transform.forward *0.1f);//Do Not Use Forward Drag!!!! will Cause ship drift.
                RB.AddForceAtPosition(ForwardDrag , transform.position - transform.forward * 2);//forward Drag must line up with the thrust!!
                //Debug.DrawLine(transform.position, transform.position + (SideDrag + UpwardDrag + ForwardDrag) * UnderWaterParts.Count/100f, Color.red);
   
        //***************************************Thrust&rudder&elevator************************************************
        Vector3 RencorrectVector = (TargetPos - transform.position).normalized - transform.forward;
        Vector3 FinalTrustVector = (S_point[0].forward - RencorrectVector * SteerForce - (Vector3.up*0.1f) / transform.position.y).normalized* Thrust;
        RB.AddForceAtPosition(FinalTrustVector, S_point[0].position);
        Debug.DrawLine(S_point[0].position, FinalTrustVector + S_point[0].position, Color.green);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        Explode();
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
        S_point[0].gameObject.SetActive(true);
        //VFX_Setup.enabled = true;
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
        S_point[0].gameObject.SetActive(false);
       // VFX_Setup.enabled = false;
    }
}


