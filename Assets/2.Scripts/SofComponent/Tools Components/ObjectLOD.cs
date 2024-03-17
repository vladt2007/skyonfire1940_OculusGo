using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class ObjectLOD : SofComponent
{
    public List<Renderer> renderers { get; private set; }
    public Transform[] mobileFiltersExceptions = new Transform[0];
    public Renderer lod2;
    public Renderer lod3;

    private SofComplexMerger merger;
    private LODGroup lodGroup;

    private bool brokenMode;
    private int lod = -1;
    public int LOD() { return lod; }

    public delegate void SwitchEvent(int lod);
    public SwitchEvent OnSwitchEvent;
    private void Awake()
    {
        brokenMode = false;
        if (lod2) lod2.gameObject.SetActive(true);
        if (lod3) lod3.gameObject.SetActive(true);
    }
    public override void Initialize(SofComplex _complex)
    {
        base.Initialize(_complex);
        renderers = new List<Renderer>(complex.GetComponentsInChildren<Renderer>());
        lodGroup = complex.GetCreateComponent<LODGroup>();
        GetComponentInParent<Animator>().Update(0f);
        merger = new SofComplexMerger(this, mobileFiltersExceptions);
        ResetLODGroup();

        complex.onPartDetached += OnPartDetached;
    }
    void Update()
    {
        if (LODLevel() != lod)
        {
            lod = LODLevel();
            if (OnSwitchEvent != null) OnSwitchEvent(lod);
        }
    }
    private void OnPartDetached(SofComplex debris)
    {
        Renderer[] detachedRenderers = debris.GetComponentsInChildren<Renderer>();
        merger.OnPartDetached(detachedRenderers);

        if (!brokenMode)
        {
            foreach (Renderer renderer in detachedRenderers)
                if (!merger.mobileFiltRends.Contains(new FilterRenderer(renderer)))
                    ToggleBrokenMode();
        }
        ResetLODGroup();
    }
    private void ToggleBrokenMode()
    {
        if (brokenMode) return;
        brokenMode = true;
        lod2.gameObject.DestroyAllChildren();
        lod2.materials = new Material[0];
        lod3.gameObject.DestroyAllChildren();
        lod3.materials = new Material[0];
    }

    const float lod0Limit = 0.5f;
    const float lod1Limit = 0.15f;
    const float lod2Limit = 0.05f;
    const float lod3Limit = 0f;
    public void ResetLODGroup()
    {
        LOD[] lods = lodGroup.GetLODs();

        if (lods.Length != 4) lods = new LOD[] { new LOD(lod0Limit, null), new LOD(lod1Limit, null), new LOD(lod2Limit, null), new LOD(lod3Limit, null) };
        lods[0].renderers = LOD0Renderers();
        lods[1].renderers = LOD1Renderers();
        lods[2].renderers = brokenMode ? BrokenModeLOD2orLOD3(false) : LOD2Renderers();
        lods[3].renderers = brokenMode ? BrokenModeLOD2orLOD3(true) : LOD3Renderers();
        lodGroup.SetLODs(lods);
    }
    private Renderer[] LOD0Renderers()
    {
        List<Renderer> lod0Renderers = new List<Renderer>();
        foreach (FilterRenderer filtRend in merger.unMergedFiltRends)
            lod0Renderers.Add(filtRend.rend);
        foreach (FilterRenderer filtRend in merger.mobileFiltRends)
            lod0Renderers.Add(filtRend.rend);
        lod0Renderers.Add(merger.fixedMerged.renderer);

        return lod0Renderers.ToArray();
    }
    private Renderer[] LOD1Renderers()
    {
        List<Renderer> lod1Renderers = new List<Renderer>();
        foreach (FilterRenderer filtRend in merger.unMergedFiltRends)
            lod1Renderers.Add(filtRend.rend);
        lod1Renderers.Add(merger.fullMerged.renderer);

        return lod1Renderers.ToArray();
    }
    private Renderer[] LOD2Renderers()
    {
        if (!lod2)
        {
            lod2 = Instantiate(merger.fullMerged.renderer, tr.position, tr.rotation, tr);
            lod2.name = "LOD 2";
            foreach (Cockpit cockpit in sofObject.GetComponentsInChildren<Cockpit>())
            {
                GameObject cockpitModel = lod2.transform.CreateChild("Internal").gameObject;
                cockpitModel.AddComponent<MeshRenderer>().sharedMaterials = cockpit.GetComponent<MeshRenderer>().sharedMaterials;
                cockpitModel.AddComponent<MeshFilter>().sharedMesh = cockpit.GetComponent<MeshFilter>().sharedMesh;
            }

        }
        return lod2.GetComponentsInChildren<Renderer>();
    }
    private Renderer[] LOD3Renderers()
    {
        if (!lod3)
        {
            lod3 = Instantiate(lod2, tr.position, tr.rotation, tr);
            lod3.name = "LOD 3";
        }
        return lod3.GetComponentsInChildren<Renderer>();
    }
    private Renderer[] BrokenModeLOD2orLOD3(bool trueIsLod3)
    {
        List<Renderer> renderers = new List<Renderer>(LOD1Renderers());
        renderers.Add(trueIsLod3 ? lod3 : lod2);
        return renderers.ToArray();
    }
    private int LODLevel()
    {
        //TODO OnPlayerChange instead
        //if (PlayerManager.complex == complex) return 0;
        int newLod = 0;
        if (merger.fullMerged.renderer.isVisible) newLod = 1;
        if (lod2.isVisible) newLod = 2;
        if (lod3.isVisible) newLod = 3;

        return newLod;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ObjectLOD))]
public class LodManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();

        ObjectLOD lodManager = (ObjectLOD)target;
        if (lodManager.lod2 && !lodManager.lod2.transform.IsChildOf(lodManager.transform))
            EditorGUILayout.HelpBox("LOD's must be children of this gameObject",MessageType.Error);
        if (lodManager.lod3 && !lodManager.lod3.transform.IsChildOf(lodManager.transform))
            EditorGUILayout.HelpBox("LOD's must be children of this gameObject", MessageType.Error);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(lodManager);
            EditorSceneManager.MarkSceneDirty(lodManager.gameObject.scene);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif