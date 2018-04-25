using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public Transform CurrentControlledObject;
    public Transform CurrentFocusedObject;
    private Cinemachine.CinemachineFreeLook CineFreeLookScript;
    // Use this for initialization
    void Start ()
    {
        GameObject FreeLookCAM = GameObject.Find("CM FreeLook1");
        CineFreeLookScript = FreeLookCAM.GetComponent<Cinemachine.CinemachineFreeLook>();
        if(CurrentControlledObject != null)
            InitializeController();
        //CineFreeLookScript.Follow = CurrentControlledObject.transform;
        //CineFreeLookScript.LookAt = CurrentControlledObject.transform;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void InitializeController()
    {
        CineFreeLookScript.Follow = CurrentControlledObject;
        CineFreeLookScript.LookAt = CurrentControlledObject;
        CurrentFocusedObject = CurrentControlledObject;
        Ship_Controller ShipCon = CurrentControlledObject.GetComponent<Ship_Controller>();
        Sub_Controller SubCon = CurrentControlledObject.GetComponent<Sub_Controller>();
        if (ShipCon)
            ShipCon.enabled = true;
        else if (SubCon)
            SubCon.enabled = true;
        else
            Debug.LogWarning ("Did not find Controller on CurrentControlledObject !");
    }

    void ChangeFocusedObject(Transform NewFocusedObject)
    {
        if (NewFocusedObject.gameObject == CurrentFocusedObject.gameObject)
            return;
        CineFreeLookScript.Follow = NewFocusedObject;
        CineFreeLookScript.LookAt = NewFocusedObject;
    }
}
