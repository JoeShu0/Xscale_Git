using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogManager : MonoBehaviour {

    public List<Material> GlobalMatList = new List<Material>();

    public Color GlobalAirFogColor;
    public Color GlobalWaterFogColor;

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		Shader.SetGlobalColor("_AirFogColorDensity", GlobalAirFogColor.linear);
        Shader.SetGlobalColor("_WaterFogColorDensity", GlobalWaterFogColor.linear);
        Shader.SetGlobalFloat("_MainCameraHeight", Camera.main.transform.position.y);
    }
}
