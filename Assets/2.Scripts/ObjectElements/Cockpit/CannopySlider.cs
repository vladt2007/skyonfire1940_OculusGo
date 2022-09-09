﻿using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
public class CannopySlider : CockpitTwoHands
{
    Vector3 openedPos;
    public BailOut bailOut;
    public HydraulicSystem cannopy;
    public float maxDistance = 0.5f;

    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        openedPos = defaultPos + transform.parent.InverseTransformDirection(-transform.forward * maxDistance);
    }
    protected override void VRInteraction(Vector3 gripPos,Quaternion gripRot)
    {
        Vector3 localPos = transform.parent.InverseTransformPoint(gripPos);
        float input = Mathv.InverseLerpVec3(defaultPos, openedPos, localPos);
        cannopy.stateInput = Mathf.Clamp01(input);
    }
    protected override void OnRelease()
    {
        base.OnRelease();
        cannopy.stateInput = cannopy.state;
    }
    protected override void Animate()
    {
        transform.localPosition = Vector3.Lerp(defaultPos,openedPos,cannopy.state);
        if (bailOut) bailOut.xrGrip.colliders[0].enabled = bailOut.secondXrGrip.colliders[0].enabled = cannopy.state > 0.7f;
    }

    private void Update()
    {
        CockpitInteractableUpdate();
    }
}

