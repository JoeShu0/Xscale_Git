using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Sub_AnimationController : MonoBehaviour {


    public float m_Smoothing = 5f;
    public ControlSurface[] m_ControlSurfaces = null;

    public bool TorpHatchOpen = false;
    public bool VLSHatchOpen = false;
    public bool MastUp = false;

    private Animator m_Animator;
    private Sub_Movement m_Movement;
    // Use this for initialization
    void Start ()
    {
        m_Animator = GetComponent<Animator>();
        m_Movement = GetComponent<Sub_Movement>();

        foreach (var surface in m_ControlSurfaces)
        {
            surface.originalLocalRotation = surface.transform.localRotation;
        }
    }
	
	// Update is called once per frame
	void Update ()
    {

        UpdateAnimatorParameter();

        foreach (var surface in m_ControlSurfaces)
        {
            switch (surface.type)
            {

                case ControlSurface.Type.Elevator:
                    {
                        // Elevators rotate negatively around the x axis, according to the plane's pitch input
                        Quaternion rotation = Quaternion.Euler(surface.amount * -m_Movement.CurrentElevator, 0f, 0f);
                        RotateSurface(surface, rotation);
                        break;
                    }
                case ControlSurface.Type.Rudder:
                    {
                        // Rudders rotate around their y axis, according to the plane's yaw input
                        Quaternion rotation = Quaternion.Euler(0f, surface.amount * -m_Movement.CurrentRudder, 0f);
                        RotateSurface(surface, rotation);
                        break;
                    }
                case ControlSurface.Type.RuddervatorPositive:
                    {
                        float r = m_Movement.CurrentRudder + m_Movement.CurrentElevator;
                        Quaternion rotation = Quaternion.Euler(surface.amount * r, 0f, 0f);
                        RotateSurface(surface, rotation);
                        break;
                    }
                case ControlSurface.Type.RuddervatorNegative:
                    {
                        float r = -m_Movement.CurrentRudder + m_Movement.CurrentElevator;
                        Quaternion rotation = Quaternion.Euler(surface.amount * r, 0f, 0f);
                        RotateSurface(surface, rotation);
                        break;
                    }
            }
        }
    }

    private void UpdateAnimatorParameter()
    {
        if (TorpHatchOpen)
            m_Animator.SetBool("TorpHatchOpen", true);
        else
            m_Animator.SetBool("TorpHatchOpen", false);

        if (VLSHatchOpen)
            m_Animator.SetBool("VLSHatchOpen", true);
        else
            m_Animator.SetBool("VLSHatchOpen", false);

        if (MastUp)
            m_Animator.SetBool("MastUp", true);
        else
            m_Animator.SetBool("MastUp", false);
    }

    private void RotateSurface(ControlSurface surface, Quaternion rotation)
    {
        Quaternion target = surface.originalLocalRotation * rotation;

        surface.transform.localRotation = Quaternion.Slerp(surface.transform.localRotation, target, m_Smoothing * Time.deltaTime);
    }
    [Serializable]
    public class ControlSurface
    {
        public enum Type
        {
            AileronLeft,
            AileronRight,
            Elevator,
            Rudder,
            RuddervatorNegative,
            RuddervatorPositive,
        }

        public Transform transform;
        public float amount;
        public Type type;

        [HideInInspector] public Quaternion originalLocalRotation;
    }
}
