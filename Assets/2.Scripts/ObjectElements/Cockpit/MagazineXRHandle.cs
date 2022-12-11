using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[RequireComponent(typeof(Magazine))]
public class MagazineXRHandle : CockpitInteractable
{
    private Magazine mag;
    private Gun[] guns;


    public override void Initialize(ObjectData d, bool firstTime)
    {
        base.Initialize(d, firstTime);
        if (firstTime)
        {
            mag = GetComponent<Magazine>();
            guns = data.GetComponentsInChildren<Gun>();
        }
    }

    protected override void VRInteraction(Vector3 gripPos, Quaternion gripRot)
    {
        if (mag.attachedGun)
        {
            //Remove the magazine if it's pulled too far
            if ((mag.attachedGun.MagazinePosition() - gripPos).sqrMagnitude > 0.01f)
                mag.attachedGun.RemoveMagazine();
        } 
        else
        {
            transform.SetPositionAndRotation(gripPos, gripRot);
            //Check every gun to load into
            foreach (Gun gun in guns)
            {
                bool canFit = gun.gunPreset == mag.gunPreset;
                canFit &= gun.magazine == null;
                canFit &= (gun.MagazinePosition() - transform.position).sqrMagnitude < 0.01f;
                if (canFit)
                    gun.LoadMagazine(mag);//Attach Magazine to the gun
            }
        }
    }
    private void Update()
    {
        CockpitInteractableUpdate();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MagazineXRHandle))]
public class MagazineXRHandleInteractable : CockpitInteractableEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        MagazineXRHandle mag = (MagazineXRHandle)target;


        if (GUI.changed)
        {
            EditorUtility.SetDirty(mag);
            EditorSceneManager.MarkSceneDirty(mag.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
