using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrewHand : MonoBehaviour
{
    public Animator animator;
    public AvatarIKGoal ikGoal = AvatarIKGoal.RightHand;
    private Vector3 handPos = Vector3.zero;
    private Quaternion handRot = Quaternion.identity;
    private float grip;
    private float trigger;
    private float thumbDown;
    private float thumbIn;

    private const float handSpeed = 1.6f;
    private const float handRotSpeed = 420f;

    private void SetHandAnim(string name, ref float current, float target)
    {
        current = Mathf.MoveTowards(current, target, Time.deltaTime * 5f);
        animator.SetFloat(name, current);
        animator.Update(0f);
    }
    public void SetHandPose(Animator crew, HandGrip handGrip)
    {
        SetHandAnim("ThumbDown", ref thumbDown, handGrip.thumbDown);
        SetHandAnim("ThumbIn", ref thumbIn, handGrip.thumbIn);
        SetHandAnim("Grip", ref grip, handGrip.grip);
        SetHandAnim("Trigger", ref trigger, handGrip.trigger);


        //Position
        Vector3 localGoal = ikGoal == AvatarIKGoal.RightHand ? handGrip.rightPosOffset : handGrip.leftPosOffset;

        Vector3 point = handGrip.transform.position;
        if (handGrip.fixedRotation) point += handGrip.transform.root.TransformDirection(localGoal);
        else point += handGrip.transform.TransformDirection(localGoal);

        handPos = Vector3.MoveTowards(handPos, transform.InverseTransformPoint(point), Time.deltaTime * handSpeed);
        crew.SetIKPositionWeight(ikGoal, 1f);
        crew.SetIKPosition(ikGoal, transform.TransformPoint(handPos));

        //Rotation
        Quaternion offset = Quaternion.Euler(ikGoal == AvatarIKGoal.RightHand ? handGrip.rightEulerOffset : handGrip.leftEulerOffset);
        Transform reference = handGrip.fixedRotation ? handGrip.transform.root : handGrip.transform;
        Quaternion rot = reference.rotation * offset;
        handRot = Quaternion.RotateTowards(handRot, rot, Time.deltaTime * handRotSpeed);
        crew.SetIKRotationWeight(ikGoal, 1f);
        crew.SetIKRotation(ikGoal, handRot);
    }
}
