using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouyancyTest : MonoBehaviour {

    [HideInInspector] public MeshRenderer _WaterMeshRender;
    Material _WaterMat;

    float _Displacement, _PlaneSize,
         _MainTile01, _MovingSpeed, _MovingDirection,
         _SubTile01, _SubMS01, _SubMD01,_SubSt01,
         _SubTile02,_SubMS02,_SubMD02,_SubSt02;
    Texture2D _DispTex, _DispTex01, _DispTex02;

    Vector3 WaterSurfacePoint;

    public float test01 =1;
    public float test02 = 0;

    void Start ()
    {
        _WaterMeshRender = GameObject.FindWithTag("WaterSurface").GetComponent<MeshRenderer>();
        _WaterMat = _WaterMeshRender.material;
    }
	
	// Update is called once per frame
	void Update ()
    {
        //_Displacement = _WaterMat.GetFloat("_Displacement");
        GetAllValFromMat();
        WaterSurfacePoint =  CalculateWaterPosition(transform.position);
        Debug.DrawLine(transform.position, WaterSurfacePoint, Color.red);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(WaterSurfacePoint,0.1f);
    }

    void GetAllValFromMat()
    {
        _Displacement = (float)_WaterMat.GetFloat("_Displacement");
        _PlaneSize = (float)_WaterMat.GetFloat("_PlaneSize");
        _MainTile01 = (float)_WaterMat.GetFloat("_MainTile01");
        _MovingSpeed = (float)_WaterMat.GetFloat("_MovingSpeed");
        //_MovingDirection = (float)_WaterMat.GetFloat("_MovingDirection");
        _SubTile01 = (float)_WaterMat.GetFloat("_SubTile01");
        _SubMS01 = (float)_WaterMat.GetFloat("_SubMS01");
       // _SubMD01 = (float)_WaterMat.GetFloat("_SubMD01");
        _SubSt01 = (float)_WaterMat.GetFloat("_SubSt01");
        _SubTile02 = (float)_WaterMat.GetFloat("_SubTile02");
        _SubMS02 = (float)_WaterMat.GetFloat("_SubMS02");
       // _SubMD02 = (float)_WaterMat.GetFloat("_SubMD02");
        _SubSt02 = (float)_WaterMat.GetFloat("_SubSt02");

        _DispTex = _WaterMat.GetTexture("_DispTex") as Texture2D;
        _DispTex01 = _WaterMat.GetTexture("_DispTex01") as Texture2D;
        _DispTex02 = _WaterMat.GetTexture("_DispTex02") as Texture2D;
    }

    Vector3 CalculateWaterPosition(Vector3 p)
    {
        float C_time = Time.time;
        //float C_time = 10;

        float Mainoffset = (float)(_MovingSpeed * C_time * 0.001f);
        float Suboffset01 = (float)(_SubMS01 * C_time * 0.001f);
        float Suboffset02 = (float)(_SubMS02 * C_time * 0.001f);
        float Md = _DispTex.GetPixelBilinear((p.x / _PlaneSize * _MainTile01 *0.01f + 0.5f + Mainoffset), (p.z / _PlaneSize * _MainTile01 *0.01f + 0.5f + Mainoffset)).grayscale* _Displacement;
        float Sd01 = (_DispTex01.GetPixelBilinear((p.x / _PlaneSize * _SubTile01 *0.01f + 0.5f + Suboffset01), (p.z / _PlaneSize * _SubTile01 * 0.01f + 0.5f + Suboffset01)).r - 0.5f) * _Displacement;
        float Sd02 = (_DispTex02.GetPixelBilinear((p.x / _PlaneSize * _SubTile02 *0.01f + 0.5f + Suboffset02), (p.z / _PlaneSize * _SubTile02 * 0.01f + 0.5f + Suboffset02)).r - 0.5f) * _Displacement;

        float d = (Md + Sd01 * _SubSt01 + Sd02 * _SubSt02) * 100f;// + Sd01 * _SubSt01 + Sd02 * _SubSt02;
        //Debug.Log(d);
        return new Vector3(p.x, d, p.z);
    }
}
