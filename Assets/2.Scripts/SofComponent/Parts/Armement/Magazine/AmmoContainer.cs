using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class AmmoContainer : SofPart
{
    public GunPreset gunPreset;
    public int capacity = 100;
    public Vector3 ejectVector = new Vector3(0f, 0.2f, 0f);

    [HideInInspector] public int ammo;
    [HideInInspector] public HandGrip grip;
    [HideInInspector] public Gun attachedGun;

    public override float Mass() { return Application.isPlaying ? gunPreset.ammunition.FullMass * ammo + EmptyMass() : LoadedMass(); }
    public float LoadedMass() { return gunPreset.ammunition.FullMass * capacity + EmptyMass(); }

    public override void Rearm()
    {
        base.Rearm();
        ammo = capacity;
    }
    public AmmoContainer Load(Gun gun)
    {
        if (gunPreset.ammunition.caliber != gun.gunPreset.ammunition.caliber) return null;
        attachedGun = gun;
        transform.parent = gun.magazineAttachPoint;
        transform.position = gun.magazineAttachPoint.position;
        transform.rotation = gun.magazineAttachPoint.rotation;
        return this;
    }

    public override void Initialize(SofComplex _complex)
    {
        ammo = capacity;
        base.Initialize(_complex);
        grip = GetComponentInChildren<HandGrip>();
    }
    public virtual bool EjectRound()
    {
        if (ammo <= 0) return false;
        ammo--;
        complex.ShiftMass(-gunPreset.ammunition.FullMass);
        return true;
    }
    public static AmmoContainer CreateAmmoBelt(Gun gun, int capacity, SofObject sofObject)
    {
        AmmoContainer belt = new GameObject(gun.name + " Ammo Belt").AddComponent<AmmoContainer>();
        belt.capacity = belt.ammo = capacity;
        belt.gunPreset = gun.gunPreset;
        belt.InitializeComponent(gun.complex);
        gun.LoadMagazine(belt);

        return belt;
    }
}
