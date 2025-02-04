﻿using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SofObject : MonoBehaviour
{
    public virtual int DefaultLayer() { return 0; }
    //References
    public Transform tr;
    public Rigidbody rb;

    public SofComplex complex;
    public SofAircraft aircraft;
    public SofDebris debris;
    public SofSimple simpleDamage;

    public bool warOnly;

    public bool destroyed = false;
    public bool burning = false;

    private void Start()
    {
        if (warOnly && !GameManager.war) Destroy(gameObject);
        Initialize();
    }
    public virtual void SetReferences()
    {
        tr = transform;

        simpleDamage = GetComponent<SofSimple>();
        complex = GetComponent<SofComplex>();
        debris = GetComponent<SofDebris>();
        aircraft = GetComponent<SofAircraft>();
        if (Application.isPlaying)
        {
            gameObject.layer = complex ? 9 : 0;
            rb = tr.IsChildOf(GameManager.gm.mapmap.transform) ? GameManager.gm.mapmap.rb : this.GetCreateComponent<Rigidbody>();
        }
    }
    protected virtual void Initialize()
    {
        SetReferences();

        GameManager.sofObjects.Add(this);
    }
    public virtual void Explosion(Vector3 center, float tnt)
    {
        if (simpleDamage) simpleDamage.Explosion(center, tnt);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SofObject))]
public class SofObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SofObject sofObj = (SofObject)target;


        if (!sofObj.aircraft) sofObj.warOnly = EditorGUILayout.Toggle("War Only", sofObj.warOnly);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(sofObj);
            EditorSceneManager.MarkSceneDirty(sofObj.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif