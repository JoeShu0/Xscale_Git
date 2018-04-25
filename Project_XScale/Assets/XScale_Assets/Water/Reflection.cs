using UnityEngine;
using System.Collections;

public class Reflection : MonoBehaviour {

	public Transform Panel;
	public Camera RefCamera;
	public Material RefMat;
    public Shader RefShader;
    public FogWithDepthTexture  C_FogScript;
    private Material FogMaterial;
    //[HideInInspector] public CalculateSurfacePoint SurfacePointCal;
    [HideInInspector] public CameraController CameraCon;

    RenderTexture RelDepthTexture;
    RenderTexture reflecTexture;

    private FogWithDepthForRef RefPostEffctScript;

    float m_finalClipPlaneOffset = 0.0f;
    // Use this for initialization
    void Start ()
    {
        Panel = transform;

        Camera.main.depthTextureMode = DepthTextureMode.Depth;
        //SurfacePointCal = transform.GetComponent<CalculateSurfacePoint>() as CalculateSurfacePoint;
        CameraCon = Camera.main.GetComponent<CameraController>();

        if (null == RefCamera)
		{
			GameObject go = new GameObject();
			go.name = "refCamera";
			RefCamera = go.AddComponent<Camera>();
            RefCamera.depthTextureMode = DepthTextureMode.Depth;

            RefCamera.CopyFrom(Camera.main);
			RefCamera.enabled = false;
			RefCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Water"));
            RefCamera.cullingMask |=  1 << LayerMask.NameToLayer("PreRenderVFX");
        }
		if(null == RefMat)
		{
			RefMat = this.GetComponent<Renderer>().sharedMaterial;
		}
        /*
        ReflDepthShader = Camera.main.GetComponent<WaterManager>().ReflDepthShader;
        if (ReflDepthShader == null)
            Debug.LogWarning("No ReflDepthShader On watermanager");
            */
        
		reflecTexture = new RenderTexture(Mathf.FloorToInt(Camera.main.pixelWidth),
		                                     Mathf.FloorToInt(Camera.main.pixelHeight), 16);
        RefCamera.targetTexture = reflecTexture;
        /*
        RelDepthTexture = new RenderTexture(Mathf.FloorToInt(Camera.main.pixelWidth),
                                             Mathf.FloorToInt(Camera.main.pixelHeight), 16 , RenderTextureFormat.Depth);
        reflecTexture.hideFlags = HideFlags.DontSave;
        //RefCamera.SetTargetBuffers(reflecTexture.colorBuffer, RelDepthTexture.depthBuffer);
        
        */


        //Find out the reflection plane: position and normal in world space
        Vector3 pos = transform.position;
        Vector3 normal = -transform.up;
        //Setup oblique projection matrix so that near plane is our reflection plane.
        //This way we clip everything below/above it for free.
        Vector4 clipPlane = CameraSpacePlane(RefCamera, pos, normal, 1.0f);
        Camera cam = Camera.main;
        Matrix4x4 projection = cam.projectionMatrix;
        CalculateObliqueMatrix(ref projection, clipPlane);
        RefCamera.projectionMatrix = projection;
        /*
        FogMaterial = new Material(RefShader);
        RefPostEffctScript = RefCamera.gameObject.AddComponent<FogWithDepthForRef>();
        RefPostEffctScript.FogShader = RefShader;
        RefPostEffctScript.WaterMat = RefMat;

        C_FogScript = Camera.main.GetComponent<FogWithDepthTexture>();
        RefPostEffctScript.fogColor = C_FogScript.WaterfogColor;
        RefPostEffctScript.fogDensity = C_FogScript.WaterfogDensity;
        */
    }


    // Update is called once per frame
    void Update ()
    {
        /*
        MyFogscript.fogColor = C_FogScript.fogColor;
        MyFogscript.fogDensity = C_FogScript.fogDensity;

        MyFogscript.RefCameraDepth = -Camera.main.GetComponent<CameraController>().CameraDepth;
        */
        //float waterHeight = SurfacePointCal.CalculateWaterPosition(Camera.main.transform.position);
        //Debug.Log(waterHeight > Camera.main.transform.position.y);
        if (!CameraCon.IsSurfaced)
        {
            RenderReflecCam(1);
            //Debug.Log("Below");
        }
        else
            RenderReflecCam(-1);
        //RenderReflecCam(-1);

    }

    void RenderReflecCam(float sideSign)
    {
        Vector3 pos = transform.position;
        Vector3 normal = -transform.up;

        //Setup oblique projection matrix so that near plane is our reflection plane.
        //This way we clip everything below/above it for free.
        Vector4 clipPlane = CameraSpacePlane(RefCamera, pos, normal, sideSign);
        Camera cam = Camera.main;
        Matrix4x4 projection = cam.projectionMatrix;
        CalculateObliqueMatrix(ref projection, clipPlane);
        RefCamera.projectionMatrix = projection;
    }

    public void OnWillRenderObject()
	{
		RenderRefection();
	}
	void RenderRefection()
	{
		Vector3 normal = Panel.up;
		float d = -Vector3.Dot (normal, Panel.position);
		Matrix4x4 refMatrix = new Matrix4x4();
		refMatrix.m00 = 1-2*normal.x*normal.x;
		refMatrix.m01 = -2*normal.x*normal.y;
		refMatrix.m02 = -2*normal.x*normal.z;
		refMatrix.m03 = -2*d*normal.x;

		refMatrix.m10 = -2*normal.x*normal.y;
		refMatrix.m11 = 1-2*normal.y*normal.y;
		refMatrix.m12 = -2*normal.y*normal.z;
		refMatrix.m13 = -2*d*normal.y;

		refMatrix.m20 = -2*normal.x*normal.z;
		refMatrix.m21 = -2*normal.y*normal.z;
		refMatrix.m22 = 1-2*normal.z*normal.z;
		refMatrix.m23 = -2*d*normal.z;

		refMatrix.m30 = 0;
		refMatrix.m31 = 0;
		refMatrix.m32 = 0;
		refMatrix.m33 = 1;

		RefCamera.worldToCameraMatrix = Camera.main.worldToCameraMatrix * refMatrix;
		RefCamera.transform.position = refMatrix.MultiplyPoint(Camera.main.transform.position);

		Vector3 forward = Camera.main.transform.forward;
		Vector3 up = Camera.main.transform.up;
		forward = refMatrix.MultiplyPoint (forward);
		//up = refMatrix.MultiplyPoint (up);
		//Quaternion refQ = Quaternion.LookRotation (forward, up);
		//RefCamera.transform.rotation = refQ;
		RefCamera.transform.forward = forward;
		
		GL.invertCulling = true;
		RefCamera.Render();
		GL.invertCulling = false;

        //OnRenderImage(null, reflecTexture);
        //RefCamera.targetTexture.wrapMode = TextureWrapMode.Repeat;
        RefMat.SetTexture("_ReflecTexture", reflecTexture);//set the texture name for shader
        //RefMat.SetTexture("_ReflecDepthTexture", RelDepthTexture);//set the texture name for shader
    }

    //Given position/normal of the plane, calculates plane in camera space.
    private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = pos + normal * m_finalClipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    //Adjusts the given projection matrix so that near plane is the given clipPlane
    //clipPlane is given in camera space. See article in Game Programming Gems 5 and
    //http://aras-p.info/texts/obliqueortho.html
    private static void CalculateObliqueMatrix(ref Matrix4x4 projection, Vector4 clipPlane)
    {
        Vector4 q = projection.inverse * new Vector4(
            sgn(clipPlane.x),
            sgn(clipPlane.y),
            1.0f,
            1.0f
            );
        Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));
        //third row = clip plane - fourth row
        projection[2] = c.x - projection[3];
        projection[6] = c.y - projection[7];
        projection[10] = c.z - projection[11];
        projection[14] = c.w - projection[15];
    }

    private static float sgn(float a)
    {
        if (a > 0.0f) return 1.0f;
        if (a < 0.0f) return -1.0f;
        return 0.0f;
    }
}
