using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class PostEffectBase : MonoBehaviour {

    //Called when start
    protected void CheckResources()
    {
        bool isSupported = CheckSupport();

        if (isSupported == false)
            NotSupported();
    }

    protected bool CheckSupport()
    {
        if (SystemInfo.supportsImageEffects == false)
        {
            Debug.LogWarning("This platform does not support ImageEffect or RenderTexture!!");
            return false;
        }
        return true;
    }

    protected void NotSupported()
    {
        enabled = false;
    }

    protected Material CheckShaderAndCreateMaterial(Shader shader, Material material)
    {
        if (shader == null)
            return null;
        if (!shader.isSupported)
            return null;
        if(material == null)
        {
            material = new Material(shader);
            material.hideFlags = HideFlags.DontSave;
            if (material)
                return material;
            else
                return null;
        }
        else //if (shader.isSupported && material.shader == shader)
            return material;
    }

    // Use this for initialization
    protected void Start ()
    {
        CheckResources();
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
