using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFX_RefleSetup : MonoBehaviour {

    private Transform ThisParent;
    public GameObject ParticleObject;
    private GameObject Copy_ParticleObject;

    public Material PreRenderMAT, PostRenderMAT;

    ParticleSystemRenderer PreRender, PostRender;

    private Camera MainCam;

    // Use this for initialization
    void Start ()
    {
        ThisParent = transform;

        Copy_ParticleObject = Instantiate(ParticleObject, ParticleObject.transform.position, ParticleObject.transform.rotation, ThisParent);

        ParticleObject.layer = LayerMask.NameToLayer("PreRenderVFX");
        Copy_ParticleObject.layer = LayerMask.NameToLayer("Default");

        PreRender = ParticleObject.GetComponent<ParticleSystemRenderer>();
        PostRender = Copy_ParticleObject.GetComponent<ParticleSystemRenderer>();

        PreRender.sharedMaterial = PreRenderMAT;
        PostRender.sharedMaterial = PostRenderMAT;

        MainCam = Camera.main;
    }
	
	// Update is called once per frame
	void Update ()
    {
        
        if (MainCam.transform.position.y * ParticleObject.transform.position.y > 0)
        {
            ParticleObject.layer = LayerMask.NameToLayer("PreRenderVFX");
            Copy_ParticleObject.layer = LayerMask.NameToLayer("Default");
        }
        else
        {
            ParticleObject.layer = LayerMask.NameToLayer("Default");
            Copy_ParticleObject.layer = LayerMask.NameToLayer("PreRenderVFX");
        }
        
	}
}
