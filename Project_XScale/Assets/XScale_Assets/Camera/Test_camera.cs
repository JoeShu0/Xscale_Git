using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_camera : MonoBehaviour {

    Camera testcam;
    public Material watermat;
	// Use this for initialization
	void Start ()
    {
        testcam = gameObject.AddComponent<Camera>();
        testcam.depthTextureMode = DepthTextureMode.Depth;
        testcam.enabled = false;

        RenderTexture Test_render = new RenderTexture(Mathf.FloorToInt(Camera.main.pixelWidth),
                                             Mathf.FloorToInt(Camera.main.pixelHeight), 16);
        testcam.targetTexture = Test_render;

        //watermat = GameObject.FindWithTag("WaterSurface").GetComponent<MeshRenderer>().sharedMaterial;
    }
	
	// Update is called once per frame
	void Update ()
    {
        testcam.Render();
        watermat.SetTexture("_ReflecDepthTexture", testcam.targetTexture);

    }
}
