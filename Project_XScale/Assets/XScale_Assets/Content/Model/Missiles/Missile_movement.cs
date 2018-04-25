using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile_movement : MonoBehaviour {

    [HideInInspector] public Rigidbody RB;
    [HideInInspector] public CalculateSurfacePoint SurfacePointCal;
    public float Thrust = 10;

    public float ExplosionForce = 10000;
    //List<Transform> B_Points = new List<Transform>();
    Transform NozzlePoint;
    Transform RudderMesh, ElevatorMesh, Arm_point;
    Vector3 UnderwaterCenter;
    public float AirDrag = 1;
    public Vector3 COMoffset = new Vector3(0, 0, 0);
    public float ForwardDragFactor = 0.08f;
    public float SideDragFator = 1;
    public float UpwardDragFactor = 1f;
    public float SteerForce = 1;
    //float Rudder = 0, Elevator = 0;

    //public float BuoyancyFactor = 5;
    //float MAXBuoyancyPerPoint;
    //public float MAXBuoyancyDepth = 0.1f;

    //bool IsSetupComplete = false;
    //bool EnablePropeller = false;

    public Transform TargetTransform;
    Vector3 TargetPos;
    //Quaternion CurrentRotation;

    public GameObject ExplosionParticle;



    // Use this for initialization
    void Start()
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
            if (Children[i].name.Contains("MainNozzle"))
                NozzlePoint = Children[i];
        }
        RB.centerOfMass = COMoffset;
        //Debug.Log("Get "+B_Points.Count);
        //MAXBuoyancyPerPoint = RB.mass * BuoyancyFactor / B_Points.Count;

        //IsSetupComplete = true;

        //EnablePropeller = true;

        //CurrentRotation = transform.rotation;
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
        Vector3 FinalTrustVector = (NozzlePoint.forward - RencorrectVector * SteerForce - (Vector3.up*0.1f) / transform.position.y).normalized* Thrust;
        RB.AddForceAtPosition(FinalTrustVector, NozzlePoint.position);
        Debug.DrawLine(NozzlePoint.position, FinalTrustVector + NozzlePoint.position, Color.green);
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
}


