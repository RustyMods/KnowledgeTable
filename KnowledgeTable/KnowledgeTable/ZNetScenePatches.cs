using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace KnowledgeTable.KnowledgeTable;

public static class ZNetScenePatches
{
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static class ZNetSceneAwakePatch
    {
        private static void Postfix(ZNetScene __instance)
        {
            if (!__instance) return;

            GameObject KnowledgeTable = __instance.GetPrefab("KnowledgeTable_RS");
            if (!KnowledgeTable) return;
            
            AddKnowledgeTableAssets(__instance, KnowledgeTable);
            
        }
    }
    
    private static void AddKnowledgeTableAssets(ZNetScene instance, GameObject prefab)
    {
        if (!prefab.TryGetComponent(out KnowledgeTable component)) return;
        GameObject VFX_cartographer_table_write = instance.GetPrefab("vfx_cartographertable_write");
        if (!VFX_cartographer_table_write) return;
        component.m_writeEffects = new EffectList()
        {
            m_effectPrefabs = new[]
            {
                new EffectList.EffectData()
                {
                    m_prefab = VFX_cartographer_table_write,
                    m_enabled = true,
                    m_variant = -1,
                    m_attach = false,
                    m_follow = false,
                    m_inheritParentRotation = true,
                    m_inheritParentScale = false,
                    m_multiplyParentVisualScale = false,
                    m_randomRotation = false,
                    m_scale = false
                }
            }
        };
        
        Transform guidePoint = prefab.transform.Find("GuidePoint");
        if (!guidePoint.TryGetComponent(out GuidePoint guideComponent)) return;
        guideComponent.m_ravenPrefab = GetRavens();
    }
    
    private static GameObject? GetRavens()
    {
        List<GameObject> allObjects = Resources.FindObjectsOfTypeAll<GameObject>().ToList();
        GameObject Ravens =
            allObjects.Find(item => item.name == "Ravens" && item.transform.GetChild(0).name == "Hugin");
        if (!Ravens) return null;

        return Ravens;
    }
}