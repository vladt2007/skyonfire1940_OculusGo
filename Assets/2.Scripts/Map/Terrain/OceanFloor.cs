using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanFloor : MonoBehaviour
{
    private Renderer rend;
    private bool active;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        active = false;
        rend.enabled = false;
    }

    private void Update()
    {
        if (!SofCamera.tr) return;
        bool newActive = SofCamera.tr.position.y < 0f;
        if (newActive != active)
        {
            active = newActive;
            rend.enabled = active;
        }
    }
}
