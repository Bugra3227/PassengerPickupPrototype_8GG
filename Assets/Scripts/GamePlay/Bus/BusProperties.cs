using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class BusProperties : MonoBehaviour
{
    public enum BusColor
    {
        Red,
        Blue,
        Purple,
        Orange
    }

    [Serializable]
    public class ColorSet
    {
        public BusColor color;
        public Material headMaterial;
        public Material tailMaterial;
    }

    [Header("Config")]
    [SerializeField] private BusColor color = BusColor.Red;
    [SerializeField, Min(0)] private int tailCount = 3;
    [SerializeField] private GameObject tailPrefab;
    [SerializeField] private Transform tailRoot;
    [SerializeField] private float segmentSpacing = 1f;

    [Header("Renderers")]
    [SerializeField] private MeshRenderer headRenderer;

    [Header("Color Sets")]
    [SerializeField] private ColorSet[] colorSets;

    private readonly List<Transform> _tails = new List<Transform>();

    private void OnValidate()
    {
        if (!isActiveAndEnabled)
            return;

        if (tailRoot == null)
            tailRoot = transform;

        RebuildTails();
        ApplyColor();
    }

    private void RebuildTails()
    {
#if UNITY_EDITOR
       
        for (int i = _tails.Count - 1; i >= 0; i--)
        {
            if (_tails[i] == null) continue;
            if (Application.isPlaying)
                Destroy(_tails[i].gameObject);
            else
                DestroyImmediate(_tails[i].gameObject);
        }
        _tails.Clear();

        if (tailPrefab == null || tailRoot == null)
            return;

       
        Vector3 basePos = headRenderer != null ? headRenderer.transform.position : transform.position;
        Vector3 backDir = -transform.forward;

        for (int i = 0; i < tailCount; i++)
        {
            Vector3 pos = basePos + backDir * (segmentSpacing * (i + 1));
            Quaternion rot = transform.rotation;

            GameObject inst = Application.isPlaying
                ? Instantiate(tailPrefab, pos, rot, tailRoot)
                : (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(tailPrefab, tailRoot);

            inst.transform.position = pos;
            inst.transform.rotation = rot;

            _tails.Add(inst.transform);
        }
#endif
    }

    private void ApplyColor()
    {
        if (colorSets == null || colorSets.Length == 0)
            return;

        ColorSet set = null;
        for (int i = 0; i < colorSets.Length; i++)
        {
            if (colorSets[i].color == color)
            {
                set = colorSets[i];
                break;
            }
        }

        if (set == null)
            return;

        if (headRenderer != null && set.headMaterial != null)
        {
            headRenderer.sharedMaterial = set.headMaterial;
        }

        if (set.tailMaterial != null)
        {
            for (int i = 0; i < _tails.Count; i++)
            {
                if (_tails[i] == null) continue;
                var r = _tails[i].GetComponentInChildren<MeshRenderer>();
                if (r != null)
                    r.sharedMaterial = set.tailMaterial;
            }
        }
    }
}
