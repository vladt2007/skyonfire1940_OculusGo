﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


public class MapData : MonoBehaviour
{
    public AnimationCurve temperature;
    public float winterOffset = 10f;

    public float maxHeight = 300f;
    public HeightArrayShort[] heightMap;
    
    public string assignedScene = "Map";
    public float upRef = 1000;
    public float downRef = -1000;
    public float rightRef = 1000;
    public float leftRef = -1000;

    [HideInInspector] public Vector2 defaultSize = new Vector2 (1024f,1024f);
    RectTransform rect;

    public Vector2 RealMapPosition(Vector2 relativePosition)
    {
        Vector2 output = Vector3.zero;
        output.x = Mathf.Lerp(leftRef, rightRef, relativePosition.x);
        output.y = Mathf.Lerp(downRef, upRef, relativePosition.y);
        return output;
    }

    private void Start()
    {
        rect = GetComponent<RectTransform>();
        defaultSize = rect.sizeDelta;
    }
}


[System.Serializable]
public class HeightArrayShort
{
    public short[] array;
    public HeightArrayShort(short[] a)
    {
        array = a;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MapData))]
public class MapDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        MapData map = (MapData)target;

        map.assignedScene = EditorGUILayout.TextField("Assigned Scene", map.assignedScene);

        GUILayout.Space(15f);
        GUI.color = Color.white;
        EditorGUILayout.HelpBox("Minimap Alignement", MessageType.None);
        GUI.color = GUI.backgroundColor;
        map.upRef = EditorGUILayout.FloatField("North Reference",map.upRef);
        map.downRef = EditorGUILayout.FloatField("South Reference", map.downRef);
        map.rightRef = EditorGUILayout.FloatField("East Reference", map.rightRef);
        map.leftRef = EditorGUILayout.FloatField("West Reference", map.leftRef);

        GUILayout.Space(15f);
        GUI.color = Color.red;
        EditorGUILayout.HelpBox("Temperature & Weather", MessageType.None);
        GUI.color = GUI.backgroundColor;
        map.temperature = EditorGUILayout.CurveField("Temperature over day", map.temperature);
        map.winterOffset = EditorGUILayout.FloatField("Winter Offset Degree", map.winterOffset);

        GUILayout.Space(15f);
        GUI.color = Color.grey;
        EditorGUILayout.HelpBox("Height map", MessageType.None);
        GUI.color = GUI.backgroundColor;
        map.maxHeight = EditorGUILayout.FloatField("Max Height", map.maxHeight);
        SerializedProperty heightMap = serializedObject.FindProperty("heightMap");
        EditorGUILayout.PropertyField(heightMap, true);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(map);
            EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
