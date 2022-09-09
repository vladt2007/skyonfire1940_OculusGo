﻿using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class Part : ObjectElement
{
    //References
    [HideInInspector] public ParticleSystem burningEffect;
    [HideInInspector] protected AudioSource burningAudio;

    //Damage model
    public float[] burningRatios;
    public PartMaterial material;
    public float emptyMass = 0f;
    public float maxHp;
    public float hp;
    [HideInInspector] public float structureDamage = 1f;
    [HideInInspector] public bool ripped;
    [HideInInspector] public bool burning;

    public override float Mass() { return emptyMass; }
    public override float EmptyMass() { return emptyMass; }


    public override void Initialize(ObjectData d,bool firstTime)
    {
        if (!material) { Debug.LogError(name + " has no material attached", this); return; }
        base.Initialize(d, firstTime);
        if (firstTime) maxHp = hp = material.hp;
    }
    public virtual void Damage(float damage, float caliber,float fireCoeff)
    {
        structureDamage -= damage/hp;
        structureDamage = Mathf.Clamp01(structureDamage);

        if (structureDamage <= 0f && !ripped) Rip(); 
    }
    public virtual void Damage(float damage)
    {
        Damage(damage, 0f, 0f);
    }

    public void TryBurn(float fireCoeff)
    {
        if (material.ignitable && !burning && structureDamage < 0.8f)
        {
            float burnChance = (1f-Mathf.Pow(structureDamage,3)) *  material.burningChance * fireCoeff;
            if (Random.value < burnChance) Burn();
        }
    }

    public virtual void Burn()
    {
        if (!burningEffect)
        {
            burningEffect = Instantiate(material.burningEffect, transform);
            burningAudio = burningEffect.GetComponent<AudioSource>();
        }
        burning = true;
        if (sofObject) sofObject.burning = true;
        burningEffect.Play();
        burningAudio.Play();
    }
    public void Burning()
    {
        Burning(false, 0f);
    }
    public virtual void Burning(bool receive,float coeff)
    {
        if (receive)
        {
            structureDamage -= Time.deltaTime * 0.03f * coeff;
        }
        else if (burning)
        {
            for (int i = 0; i < data.parts.Length; i++)
            {
                data.parts[i].Burning(true, burningRatios[i]);
            }
        }
    }
    public virtual void Rip()
    {
        if (ripped) return;
        ripped = true;
    }
    public void Detach()
    {
        //Destroy all line renderers
        foreach (LineRenderer rope in GetComponentsInChildren<LineRenderer>()) rope.enabled = false;

        //Completely separate this part from the aircraft
        data.mass -= FlightModel.TotalMass(GetComponentsInChildren<Part>(),false);
        GameObject obj = new GameObject(name + " Ripped Off");
        obj.transform.SetPositionAndRotation(transform.position, transform.rotation);
        Rigidbody objRb = obj.AddComponent<Rigidbody>();
        objRb.velocity = rb.velocity;
        transform.parent = obj.transform;
        objRb.ResetCenterOfMass();
        objRb.ResetInertiaTensor();
        ObjectData objData = obj.AddComponent<ObjectData>();
        
        objData.Initialize(false);
        Destroy(obj, 60f);
    }

}
#if UNITY_EDITOR
[CustomEditor(typeof(Part))]
public class PartEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Part part = (Part)target;

        part.emptyMass = EditorGUILayout.FloatField("Empty Mass", part.emptyMass);
        part.hp = EditorGUILayout.FloatField("Health Point", part.hp);
        part.material = EditorGUILayout.ObjectField("Part Material", part.material, typeof(PartMaterial),false) as PartMaterial;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(part);
            EditorSceneManager.MarkAllScenesDirty();
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
