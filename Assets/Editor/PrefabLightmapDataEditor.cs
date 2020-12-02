using CustomFloorPlugin;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabLightmapDataEditor : MonoBehaviour
{

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Bake Prefab Lightmaps")]
    static void GenerateLightmapInfo()
    {
        if (UnityEditor.Lightmapping.giWorkflowMode != UnityEditor.Lightmapping.GIWorkflowMode.OnDemand)
        {
            Debug.LogError("ExtractLightmapData requires that you have baked you lightmaps and Auto mode is disabled.");
            return;
        }
        UnityEditor.Lightmapping.Bake();

        PrefabLightmapData[] prefabs = FindObjectsOfType<PrefabLightmapData>();

        foreach (var instance in prefabs)
        {
            var gameObject = instance.gameObject;
            var renderers = new List<Renderer>();
            var lightmapOffsetScales = new List<Vector4>();
            var lightmaps = new List<Texture2D>();

            GenerateLightmapInfo(gameObject, renderers, lightmapOffsetScales, lightmaps);

            instance.m_Renderers = renderers.ToArray();
            instance.m_LightmapOffsetScales = lightmapOffsetScales.ToArray();
            instance.m_Lightmaps = lightmaps.ToArray();

            var targetPrefab = UnityEditor.PrefabUtility.GetPrefabParent(gameObject) as GameObject;
            if (targetPrefab != null)
            {
                //UnityEditor.Prefab
                UnityEditor.PrefabUtility.ReplacePrefab(gameObject, targetPrefab);
            }
        }
    }

    static void GenerateLightmapInfo(GameObject root, List<Renderer> rendererList, List<Vector4> lightmapOffsetScaleList, List<Texture2D> lightmaps)
    {
        var renderers = root.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer.lightmapIndex != -1)
            {
                Texture2D lightmap = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapColor;
                rendererList.Add(renderer);
                lightmapOffsetScaleList.Add(renderer.lightmapScaleOffset);
                lightmaps.Add(lightmap);
            }
        }
    }
#endif
}
