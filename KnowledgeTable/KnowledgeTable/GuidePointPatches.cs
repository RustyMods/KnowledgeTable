using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace KnowledgeTable.KnowledgeTable;

public static class GuidePointPatches
{
    [HarmonyPatch(typeof(GuidePoint), nameof(GuidePoint.Start))]
    private static class GuidePointStartPatch
    {
        private static bool Prefix(GuidePoint __instance)
        {
            if (!__instance) return false;
            if (__instance.m_ravenPrefab == null)
            {
                __instance.m_ravenPrefab = GetRavens();
                if (__instance.m_ravenPrefab == null) return false;
            };
            return true;
        }
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