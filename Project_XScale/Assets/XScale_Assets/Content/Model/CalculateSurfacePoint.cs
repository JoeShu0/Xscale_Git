using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateSurfacePoint : MonoBehaviour {

    [HideInInspector] public MeshRenderer _WaterMeshRender;
    Material _WaterMat;

    float _Displacement, _PlaneSize,
         _MainTile01, _MovingSpeed, _MovingDirection,
         _SubTile01, _SubMS01, _SubMD01,_SubSt01,
         _SubTile02,_SubMS02,_SubMD02,_SubSt02;
    Texture2D _DispTex, _DispTex01, _DispTex02;

    Vector3 WaterSurfacePoint;

    [HideInInspector] public bool IsSetupComplete = false;

    void Start ()
    {
        _WaterMeshRender = GameObject.FindWithTag("WaterSurface").GetComponent<MeshRenderer>();
        _WaterMat = _WaterMeshRender.material;
        GetAllValFromMat();
        IsSetupComplete = true;
    }
	
	// Update is called once per frame
	void Update ()
    {
        GetAllValFromMat();
        //WaterSurfacePoint =  CalculateWaterPosition(transform.position);
        //Debug.DrawLine(transform.position, WaterSurfacePoint, Color.red);
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
        _SubMD01 = (float)_WaterMat.GetFloat("_SubMD01");
        _SubSt01 = (float)_WaterMat.GetFloat("_SubSt01");
        _SubTile02 = (float)_WaterMat.GetFloat("_SubTile02");
        _SubMS02 = (float)_WaterMat.GetFloat("_SubMS02");
        _SubMD02 = (float)_WaterMat.GetFloat("_SubMD02");
        _SubSt02 = (float)_WaterMat.GetFloat("_SubSt02");

        _DispTex = _WaterMat.GetTexture("_DispTex") as Texture2D;
        _DispTex01 = _WaterMat.GetTexture("_DispTex01") as Texture2D;
        _DispTex02 = _WaterMat.GetTexture("_DispTex02") as Texture2D;
    }

    public float CalculateWaterPosition(Vector3 p)
    {
        float C_time = Time.time;
        //float C_time = 10;

        float Mainoffset = (float)(_MovingSpeed * C_time * 0.001f);
        //float Suboffset01 = (float)(_SubMS01 * C_time * 0.001f);
        Vector2 Suboffset01 = new Vector2(_SubMS01 * C_time * 0.001f * Mathf.Sin(Mathf.Deg2Rad * _SubMD01), _SubMS01 * C_time * 0.001f * Mathf.Cos(Mathf.Deg2Rad * _SubMD01));
        //float Suboffset02 = (float)(_SubMS02 * C_time * 0.001f);
        Vector2 Suboffset02 = new Vector2(_SubMS02 * C_time * 0.001f * Mathf.Sin(Mathf.Deg2Rad * _SubMD02), _SubMS02 * C_time * 0.001f * Mathf.Cos(Mathf.Deg2Rad * _SubMD02));
        float Md = (_DispTex.GetPixelBilinear((p.x / _PlaneSize * _MainTile01 + 0.5f + Mainoffset), (p.z / _PlaneSize * _MainTile01  + 0.5f + Mainoffset)).r)* _Displacement;
        float Sd01 = (_DispTex01.GetPixelBilinear((p.x / _PlaneSize * _SubTile01 + 0.5f + Suboffset01.x), (p.z / _PlaneSize * _SubTile01  + 0.5f + Suboffset01.y)).r - 0.5f) * _Displacement;
        float Sd02 = (_DispTex02.GetPixelBilinear((p.x / _PlaneSize * _SubTile02  + 0.5f + Suboffset02.x), (p.z / _PlaneSize * _SubTile02  + 0.5f + Suboffset02.y)).r - 0.5f) * _Displacement;

        float d = (Md + Sd01 * _SubSt01 + Sd02 * _SubSt02) ;// + Sd01 * _SubSt01 + Sd02 * _SubSt02;
        //Debug.Log(d);
        return d;
    }
}
