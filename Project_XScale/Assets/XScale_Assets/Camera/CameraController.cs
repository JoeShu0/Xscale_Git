using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public bool IsSurfaced = true;

    //public Color FogColor = new Color(0, 0.4f, 0.7f, 0.6f);
    //public float FogDensity = 0.02f;
    public float CameraDepth = 0;
    public Light[] SceneLights;
    public float[] SceneLightsIntensity;





    CalculateSurfacePoint SPC;
    // Use this for initialization
    void Start ()
    {
        
        SPC = transform.gameObject.AddComponent<CalculateSurfacePoint>() as CalculateSurfacePoint;
        SceneLights = GameObject.FindObjectsOfType(typeof(Light)) as Light[];
        SceneLightsIntensity = new float[SceneLights.Length];
        for (int i = 0; i < SceneLights.Length; i++)
            SceneLightsIntensity[i] = SceneLights[i].intensity;


        //CameraDepth = SPC.CalculateWaterPosition(transform.position) - transform.position.y;
}
	
	// Update is called once per frame
	void Update ()
    {
        bool WasSurfaced = IsSurfaced;
        CameraDepth = SPC.CalculateWaterPosition(transform.position) - transform.position.y;
        if (CameraDepth > 0)
        {
            IsSurfaced = false;
            UpdateLighting();
        }
        else
            IsSurfaced = true;

        if (WasSurfaced == true && IsSurfaced == false)
            CameraGoDown();
        else if (WasSurfaced == false && IsSurfaced == true)
            CameraComeUp();


    }
    void UpdateLighting()
    {
        for (int i = 0; i < SceneLights.Length; i++)
        {
            if(CameraDepth > 0)
            {
                //SceneLights[i].intensity = Mathf.Lerp(SceneLightsIntensity[i] * 0.9f, 0, CameraDepth / 50);//直接修改了场景灯光，需要改方法
            }
            //SceneLights[i].intensity = Mathf.Lerp(SceneLightsIntensity[i], 0, CameraDepth / 50);//直接修改了场景灯光，需要改方法
        }
            
    }

    void CameraGoDown()
    {

    }
    void CameraComeUp()
    {

    }
}
