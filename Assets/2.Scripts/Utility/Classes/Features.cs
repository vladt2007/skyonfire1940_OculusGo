using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static LinkJoint;

public class Features
{
#if UNITY_EDITOR
    public static void DrawControlHandles(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Color face, Color outline)
    {
        Vector3[] v = new Vector3[4];
        v[0] = A;
        v[1] = B;
        v[2] = C;
        v[3] = D;
        Handles.color = Color.white;
        Handles.DrawSolidRectangleWithOutline(v, face, outline);
    }
#endif
    public static void DrawCircle(Vector3 center, Vector3 axis, float radius ,Color color, int segments)
    {
        Gizmos.color = color;

        Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation(Vector3.Cross(axis,Vector3.up)), Vector3.one);;

        Vector3 startPoint = center;

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * 2.0f * Mathf.PI / segments;

            Vector3 localEndPoint = new Vector3(0.0f, Mathf.Sin(angle), Mathf.Cos(angle)) * radius;

            Vector3 endPoint = rotationMatrix.MultiplyPoint3x4(localEndPoint) + center;

            Gizmos.DrawLine(startPoint, endPoint);
            startPoint = endPoint;
        }
    }
    public static void PlayerPrefsSetVector3(string key, Vector3 value)
    {
        PlayerPrefs.SetFloat(key + "X", value.x);
        PlayerPrefs.SetFloat(key + "Y", value.y);
        PlayerPrefs.SetFloat(key + "Z", value.z);
    }

    public static Vector3 PlayerPrefsGetVector3(string key, Vector3 defaultValue)
    {
        Vector3 value = defaultValue;
        value.x = PlayerPrefs.GetFloat(key + "X", defaultValue.x);
        value.y = PlayerPrefs.GetFloat(key + "Y", defaultValue.y);
        value.z = PlayerPrefs.GetFloat(key + "Z", defaultValue.z);
        return value;
    }
    public static Vector2 ScreenSize()
    {
        return new Vector2(Screen.width, Screen.height);
    }
}
