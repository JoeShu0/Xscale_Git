using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterManager : MonoBehaviour {

    private Vector3 CurrentV;
    [HideInInspector]public PlayerController PlayerCon;
    [HideInInspector]public Transform FocusedObject;
    private Material _WaterMat;
    public Shader ReflFogShader;
    CameraController CC;
    float CameraWaterDepth;

    public float WaterInterRenderDist = 200f;

    public List<Transform> WaterInterShips = new List<Transform>();
    public List<Transform> WaterInterSubs = new List<Transform>();
    //private Vector4[] WaterInterExplosions = new Vector4[4];
    //private Vector4[] WaterInterExplosions = new Vector4[16];
    //private int WaterInterExplosionsPOINT = 0;
    // Use this for initialization
    void Start ()
    {
       
        //to setup water add tag WaterSuface,layer to water 
        transform.gameObject.AddComponent<CalculateSurfacePoint>();
        Reflection RefScript = transform.gameObject.AddComponent<Reflection>();//Reflection must be add behind the Cal ,Order must br correct!!
        RefScript.RefShader = ReflFogShader;

        PlayerCon = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        FocusedObject = PlayerCon.CurrentFocusedObject;

        MeshRenderer _WaterMeshRender = GameObject.FindWithTag("WaterSurface").GetComponent<MeshRenderer>();
        _WaterMat = _WaterMeshRender.material;

        //transform.gameObject.AddComponent<MaterialSetup>();

        CC = Camera.main.GetComponent<CameraController>();
        CameraWaterDepth = CC.CameraDepth;

        /*
        for (int i = 0; i < 4; i++)
            WaterInterExplosions[i] = new Vector4(0, 0, 0, 0);

        AddExpToWater(new Vector4(50, 50, 15, 15));
        */
    }
	
	// Update is called once per frame
	void Update ()
    {
       
        FocusedObject = PlayerCon.CurrentFocusedObject;

        Vector3 DampPosition = Vector3.SmoothDamp(transform.position, FocusedObject.position,ref CurrentV, 2f);
        transform.position = new Vector3(DampPosition.x,0,DampPosition.z);
        //将水面互动的参数传递给material
        Matrix4x4 ShipsRenderMatrix = Matrix4x4.zero;
        ShipsRenderMatrix = GetInRangeInterObject(WaterInterShips);
        _WaterMat.SetMatrix("_ShipsPosMatrix", ShipsRenderMatrix);
        Matrix4x4 SubsRenderMatrix = Matrix4x4.zero;
        SubsRenderMatrix = GetInRangeInterObject(WaterInterSubs);
        _WaterMat.SetMatrix("_SubsPosMatrix", SubsRenderMatrix);
        List<float> SubsDepth = new List<float>();
        SubsDepth = GetSubsDepth(WaterInterSubs);
        if(SubsDepth.Count != 0)
            _WaterMat.SetFloatArray("_SubDepthArray", SubsDepth);

        CameraWaterDepth = CC.CameraDepth;
        _WaterMat.SetFloat("_CameraDepth", CameraWaterDepth);
        //TickDownWaterInterExplosions();
        //_WaterMat.SetVectorArray("_ExplosionArray", WaterInterExplosions);
    }

    private Matrix4x4 GetInRangeInterObject(List<Transform> ObjectList)//检查哪些物件在水面互动范围内
    {
        Matrix4x4 RenderMatrix = Matrix4x4.identity;

        for (int i = 0 ,n = 0; i < ObjectList.Count && n < 4; i++)
        {
            Vector3 ShipFront;
            if (ObjectList[i].GetComponent<Ship_Movement>())
                ShipFront = ObjectList[i].GetComponent<Ship_Movement>().Ship_Front_Point.transform.position;
            else
                ShipFront = ObjectList[i].GetComponent<Sub_Movement>().Ship_Front_Point.transform.position;
            //Debug.Log(i + " " + n +" " + ObjectList.Count);
            if ((ShipFront - FocusedObject.position).magnitude < WaterInterRenderDist)
            {
                Rigidbody ObejctRB = ObjectList[i].GetComponent<Rigidbody>();
                RenderMatrix.SetRow(n, new Vector4(ShipFront.x, ShipFront.z, ObejctRB.velocity.x, ObejctRB.velocity.z));
                n++;
            }
        }
        return RenderMatrix;
    }

    private List<float> GetSubsDepth(List<Transform> SubList)
    {
        List<float> SubsDepth = new List<float>();
        for (int i = 0, n = 0; i < SubList.Count && n < 4; i++)
        {
            Vector3 SubFront = SubList[i].GetComponent<Sub_Movement>().Ship_Front_Point.transform.position;
            if ((SubFront - FocusedObject.position).magnitude < WaterInterRenderDist)
            {
                SubsDepth.Add( Mathf.Clamp01(- Mathf.Min(0, SubFront.y) / 10));
                n++;
            }
        }
        return SubsDepth;
    }
    /*
    private void TickDownWaterInterExplosions()
    {
        
        for (int i = 0; i < 4; i++)
        {
            if (WaterInterExplosions[i] == new Vector4(0, 0, 0, 0))
                continue;
            WaterInterExplosions[i] = new Vector4(WaterInterExplosions[i].x, WaterInterExplosions[i].y, WaterInterExplosions[i].z, WaterInterExplosions[i].w - Time.deltaTime);
            if (WaterInterExplosions[i].w < 0)
                WaterInterExplosions[i] = new Vector4(0, 0, 0, 0);
        }
    }

    public void AddExpToWater(Vector4 P)
    {
        float MinTime = 20;
        int MinNum = 4;
        for (int i = 0; i < 4; i++)
        {
            if (WaterInterExplosions[i] == new Vector4(0, 0, 0, 0))
            {
                WaterInterExplosions[i] = P;
                return;
            }
            else if(WaterInterExplosions[i].w < MinTime)
            {
                MinTime = WaterInterExplosions[i].w;
                MinTime = i;
            }
        }
        WaterInterExplosions[MinNum] = P;
     }
     */
}
