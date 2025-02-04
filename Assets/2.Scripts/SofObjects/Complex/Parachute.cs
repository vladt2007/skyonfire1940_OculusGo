using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class Parachute : SofComplex
{
    public Vector3 dragPoint = new Vector3 (0f,4f,0f);
    public float feetGroundOffset = -1.6f;
    public float radius = 3f;
    public CrewSeat seat;

    private float rotationSpeed;
    private float trueRadius;
    private float dragCoeff = 1.75f;
    private bool landed;

    const float terminalVelocity = 7f;

    private Vector3 startVelocity;
    private string startTag;

    private void Start()
    {
        //Do not delete, or initialize will be called twice
    }
    public void TriggerParachute(SofAircraft aircraft, CrewMember _crew)
    {
        startVelocity = aircraft.data.rb.velocity + aircraft.transform.up * 5f;
        startTag = aircraft.tag;

        _crew.transform.parent = seat.transform.parent;

        Initialize();

        _crew.seats = new List<CrewSeat>(new CrewSeat[] { seat });
        _crew.SwitchSeat(0);

        if (_crew == Player.crew)
        {
            if (GameManager.gm.vr) SofVrRig.instance.ResetView();
            //else PlayerCamera.ResetRotation(false);
            Player.Set(this);
        }
        aircraft.OnPartDetach(this);
    }
    protected override void Initialize()
    {
        base.Initialize();
        rotationSpeed = Random.Range(-2f, 2f);
        dragCoeff = Random.Range(0.9f,1.1f) * 1600f / (terminalVelocity * terminalVelocity * Aerodynamics.seaLvlDensity * radius * radius * Mathf.PI);
        rb.centerOfMass = Vector3.zero;
        rb.angularDrag = 1f;
        rb.inertiaTensor = new Vector3(100f, 400f * Random.Range(0.8f,1.2f), 100f);
        rb.velocity = startVelocity;
        trueRadius = 0f;
        landed = false;
        tag = startTag;
    }
    private void FixedUpdate()
    {
        Vector3 vel = data.rb.GetPointVelocity(transform.TransformPoint(dragPoint));
        trueRadius = Mathf.MoveTowards(trueRadius, radius, Time.fixedDeltaTime * 0.5f);
        float area = trueRadius * trueRadius * Mathf.PI;
        data.rb.AddForceAtPosition(Aerodynamics.Drag(vel,data.tas.Get, data.density.Get, area, dragCoeff, 1f),transform.TransformPoint(dragPoint));
        if (data.relativeAltitude.Get < -feetGroundOffset && !landed) Land();

        transform.Rotate(0f, rotationSpeed * Time.fixedDeltaTime, 0f, Space.Self);
    }
    private void Land()
    {
        landed = true;
        data.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        data.rb.isKinematic = true;

        Vector3 pos = transform.position;
        pos.y = data.altitude.Get - data.relativeAltitude.Get - feetGroundOffset;
        transform.position = pos;
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        if (data.altitude.Get - data.relativeAltitude.Get == 0f) GetComponent<Animator>().SetBool("Swimming", true);
        else GetComponent<Animator>().SetBool("Grounded", true);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Parachute))]
public class ParachuteEditor : SofComplexEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Parachute para = (Parachute)target;
        para.seat = EditorGUILayout.ObjectField("Seat", para.seat, typeof(CrewSeat), true) as CrewSeat;
        para.radius = EditorGUILayout.FloatField("Parachute Radius",para.radius);
        para.dragPoint = EditorGUILayout.Vector3Field("Parachute Drag Point", para.dragPoint);
        para.feetGroundOffset = EditorGUILayout.FloatField("Feet ground offset", para.feetGroundOffset);


        if (GUI.changed)
        {
            EditorUtility.SetDirty(para);
            EditorSceneManager.MarkSceneDirty(para.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
