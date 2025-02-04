using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunsCover : SofComponent
{
    public Gun linkedGun;
    public ParticleSystem destructionEffect;
    public MeshRenderer meshRend;


    private void OnDestruction(float misc)
    {
        meshRend.enabled = false;
        destructionEffect.Play();
        linkedGun.OnFireEvent -= OnDestruction;
    }
    void Start()
    {
        linkedGun.OnFireEvent += OnDestruction;
    }
}
