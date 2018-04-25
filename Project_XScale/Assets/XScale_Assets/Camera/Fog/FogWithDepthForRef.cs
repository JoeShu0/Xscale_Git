using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogWithDepthForRef : MonoBehaviour {

    Material FogMaterial;
    public Shader FogShader;

    RenderTexture AE_Reflection;
    public Material WaterMat;

    public float fogDensity;
    public Color fogColor;

    Camera refCamera;
    Transform cameraTransform;
    // Use this for initialization
    void Start ()
    {
        FogMaterial = new Material(FogShader);
        AE_Reflection = new RenderTexture(Mathf.FloorToInt(Camera.main.pixelWidth)/4,
                                             Mathf.FloorToInt(Camera.main.pixelHeight)/4, 24);
        refCamera = GetComponent<Camera>();

        refCamera.targetTexture = AE_Reflection;
        cameraTransform = refCamera.transform;
    }
	
	// Update is called once per frame
	void Update ()
    {
        
    }
    
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (FogMaterial != null)
        {
            Matrix4x4 FrustumCorners = Matrix4x4.identity;

            float fov = refCamera.fieldOfView;
            float near = refCamera.nearClipPlane;
            float far = refCamera.farClipPlane;
            float aspect = refCamera.aspect;

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

            FrustumCorners.SetRow(1, bottomLeft);
            FrustumCorners.SetRow(0, bottomRight);
            FrustumCorners.SetRow(2, topRight);
            FrustumCorners.SetRow(3, topLeft);


            //Debug.Log(new Vector3(cameraTransform.forward.x, cameraTransform.forward.y, cameraTransform.forward.z));

            FogMaterial.SetMatrix("_FrustumCornersRay", FrustumCorners);

            //material.SetMatrix("_ViewProjectionInverseMatrix", (M_camera.projectionMatrix * M_camera.worldToCameraMatrix).inverse);

            FogMaterial.SetFloat("_FogDensity", fogDensity / 100f);
            FogMaterial.SetColor("_FogColor", fogColor);


            Graphics.Blit(source, destination, FogMaterial);
        }
        else
            Graphics.Blit(source, destination);
    }
    
    private void OnPostRender()
    {
        WaterMat.SetTexture("_ReflecTexture", refCamera.targetTexture);
    }
}
