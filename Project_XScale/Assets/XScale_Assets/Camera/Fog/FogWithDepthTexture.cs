using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogWithDepthTexture : PostEffectBase
{
    //Material WaterMat;
    public float FogSmooth = 1;
    public bool isForMainCamera = true;
    public float RefCameraDepth = 0;
    private CameraController CameraCon;
    public Shader fogShader;
    private Material fogMaterial = null;
    public Material material
    {
        get
        {
            fogMaterial = CheckShaderAndCreateMaterial(fogShader, fogMaterial);
            return fogMaterial;
        }
    }

    private Camera MyCamera;
    public Camera M_camera
    {
        get
        {
            if (MyCamera == null)
            {
                MyCamera = GetComponent<Camera>();
            }
            return MyCamera;
        }
    }

    private Transform MyCameraTransform;
    public Transform cameraTransform
    {
        get
        {
            if (MyCameraTransform == null)
            {
                MyCameraTransform = M_camera.transform;
            }
            return MyCameraTransform;
        }
    }
    
    private Material MyWatermaterial;
    public Material WaterMat
    {
        get
        {
            if (MyWatermaterial == null)
            {
                MyWatermaterial = GameObject.FindWithTag("WaterSurface").GetComponent<MeshRenderer>().sharedMaterial; 
            }
            return MyWatermaterial;
        }
    }
    

    [Range(0.0f, 10.0f)]
    public float WaterfogDensity = 1.0f;
    [Range(0.0f, 10.0f)]
    public float AirFogDesity = 1.0f;

    public Color WaterfogColor = Color.white;
    public Color AirfogColor = Color.white;

    //public float fogStart = 0.0f;
    //public float fogEnd = 2.0f;

    private void OnEnable()
    {
        M_camera.depthTextureMode = DepthTextureMode.Depth;
        CameraCon = GetComponent<CameraController>();

        
    }

    private new void Start()
    {
        //MeshRenderer WaterRender = GameObject.FindWithTag("WaterSurface").GetComponent<MeshRenderer>();
        //WaterMat = WaterRender.material;
    }

    
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (material != null)
        {
            Matrix4x4 FrustumCorners = Matrix4x4.identity;
            Matrix4x4 MyProjection = Matrix4x4.identity;

            float fov = M_camera.fieldOfView;
            float near = M_camera.nearClipPlane;
            float far = M_camera.farClipPlane;
            float aspect = M_camera.aspect;

            float halfHeight = near * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            Vector3 toRight = cameraTransform.right * halfHeight * aspect;
            Vector3 toTop = cameraTransform.up * halfHeight;

            Vector3 topLeft = cameraTransform.forward * near + toTop - toRight;
            float scale = topLeft.magnitude / near;

            topLeft.Normalize();
            topLeft *= scale;

            Vector3 topRight = cameraTransform.forward * near + toTop + toRight;
            topRight.Normalize();
            topRight *= scale;

            Vector3 bottomLeft = cameraTransform.forward * near - toTop - toRight;
            bottomLeft.Normalize();
            bottomLeft *= scale;

            Vector3 bottomRight = cameraTransform.forward * near - toTop + toRight;
            bottomRight.Normalize();
            bottomRight *= scale;

            FrustumCorners.SetRow(0, bottomLeft);
            FrustumCorners.SetRow(1, bottomRight);
            FrustumCorners.SetRow(2, topRight);
            FrustumCorners.SetRow(3, topLeft);

            MyProjection.SetRow(0, new Vector3(cameraTransform.forward.x, cameraTransform.forward.y, cameraTransform.forward.z));
            MyProjection.SetRow(1, new Vector3(toRight.x, toRight.y, toRight.z));
            MyProjection.SetRow(2, new Vector3(toTop.x, toTop.y, toTop.z));
            MyProjection.SetRow(3, new Vector3(near, far, FogSmooth/100));

            Debug.Log(new Vector3(cameraTransform.forward.x, cameraTransform.forward.y, cameraTransform.forward.z));

            material.SetMatrix("_FrustumCornersRay", FrustumCorners);
            material.SetMatrix("_MyProjection", MyProjection);
            //material.SetMatrix("_ViewProjectionInverseMatrix", (M_camera.projectionMatrix * M_camera.worldToCameraMatrix).inverse);
            material.SetFloat("_FogSmooth", FogSmooth / 500f);
            material.SetFloat("_FogDensity", WaterfogDensity / 100f);
            material.SetFloat("_AirFogDensity", AirFogDesity / 100f);
            material.SetColor("_FogColor", WaterfogColor);
            material.SetColor("_AirFogColor", AirfogColor);
            if (isForMainCamera)
            {
                material.SetFloat("_CameraDepth", CameraCon.CameraDepth);
                if (WaterMat != null)
                {
                    WaterMat.SetMatrix("_FrustumCornersRay", FrustumCorners);
                    WaterMat.SetMatrix("_MyProjection", MyProjection);
                    WaterMat.SetFloat("_FogDensity", WaterfogDensity / 100f);
                    WaterMat.SetColor("_FogColor", WaterfogColor);
                    WaterMat.SetFloat("_CameraDepth", CameraCon.CameraDepth);
                    WaterMat.SetFloat("_FogSmooth", FogSmooth / 100f);
                }
            }
            else
            {
                material.SetFloat("_CameraDepth", RefCameraDepth);
            }
            //material.SetFloat("_FogStart", fogStart);
            //material.SetFloat("_FogEnd", fogEnd);
            
            Graphics.Blit(src, dest, material);
            //Graphics.Blit(src, dest, WaterMat);
        }
        else
            Graphics.Blit(src, dest);
    }
    

	
	// Update is called once per frame
	void Update ()
    {

    }
}
