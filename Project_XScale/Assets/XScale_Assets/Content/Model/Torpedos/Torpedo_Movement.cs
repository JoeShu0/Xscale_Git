﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torpedo_Movement : MonoBehaviour {

    public float PropellerSpeed = 500;
    public float Thrust = 10;
    public float SteerForce = 1;
    float ActiveThrust = 0;
    public float ExplosionForce = 10000;
    //public Collider ArmTrigger;

    [HideInInspector] public Rigidbody RB;
    [HideInInspector] public CalculateSurfacePoint SurfacePointCal;

    List<Transform> B_Points = new List<Transform>();
    List<Transform> S_point = new List<Transform>();
    Transform RudderMesh, ElevatorMesh, Arm_point;
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
        }
        RB.centerOfMass = COMoffset;
        //Debug.Log("Get "+B_Points.Count);
        MAXBuoyancyPerPoint = RB.mass * BuoyancyFactor / B_Points.Count;

        //IsSetupComplete = true;

        //EnablePropeller = true;

        //CurrentRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {




    }

    private void FixedUpdate()
    {
        if (SurfacePointCal.IsSetupComplete == false)
        {
            Debug.Log("Setting Up SurfacePointCalculator for " + transform.gameObject.name);
            return;
        }

        if (TargetTransform == null)
            TargetPos = new Vector3(transform.forward.x, 0, transform.forward.z) * 10 + transform.position;
        else
            TargetPos = TargetTransform.position;
        Debug.DrawLine(transform.position, TargetPos, Color.cyan);

        ActiveThrust = Thrust;
        if (SurfacePointCal.CalculateWaterPosition(S_point[0].position) < S_point[0].position.y)
            ActiveThrust = 0;
        AddForces();

        //CurrentRotation = transform.rotation;
    }

    void AddForces()
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
        //***************************************Thrust&rudder&elevator************************************************
        Vector3 RencorrectVector = (TargetPos - transform.position).normalized - transform.forward;
        RB.AddForceAtPosition((S_point[0].forward - RencorrectVector * SteerForce*0.1f * RB.velocity.magnitude) * ActiveThrust, S_point[0].position);
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
            if(EffectRB != null)
            {
                Debug.Log(EffectRB.gameObject.name);
                EffectRB.AddExplosionForce(ExplosionForce, transform.position, 50);
            }
        }
        ParticleSystem WaterTrail = GetComponentInChildren<ParticleSystem>();
        ParticleSystem.EmissionModule EM =  WaterTrail.emission;
        EM.rateOverTime = 0;
        Destroy(WaterTrail.gameObject, 3f);
        WaterTrail.transform.parent = null;

        if (ExplosionParticle != null)
        { 
            GameObject ExplosionP =  Instantiate(ExplosionParticle, transform.position , transform.rotation);
            Destroy(ExplosionP, 3f);
        }
        //WaterTrail.emission.rateOverTime = 0;
        Destroy(transform.gameObject);
    }
}
