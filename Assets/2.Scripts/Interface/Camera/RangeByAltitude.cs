﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeByAltitude : MonoBehaviour
{
    private Camera cam;
    float maxRange;
    const float altitudeZeroFactor = 0.7f;

    void Start()
    {
        cam = GetComponent<Camera>();
        maxRange = PlayerPrefs.GetFloat("MaxViewRange", 12000f);
    }

    void Update()
    {
        bool cockpit = GameManager.gm.vr || PlayerCamera.viewMode != 0;
        cam.nearClipPlane = cockpit ? 0.1f : 1f;
        cam.farClipPlane = Mathf.Lerp(altitudeZeroFactor * maxRange, maxRange, cam.transform.position.y / 6000f);
    }
}
