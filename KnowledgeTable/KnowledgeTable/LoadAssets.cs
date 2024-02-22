using KnowledgeTable.Managers;
using PieceManager;

namespace KnowledgeTable.KnowledgeTable;

public static class LoadAssets
{
    public static void InitPieces()
    {
        BuildPiece KnowledgeTable = new("knowledgetablebundle", "KnowledgeTable_RS");
        KnowledgeTable.Name.English("Knowledge Table");
        KnowledgeTable.Description.English("Share your knowledge");
        KnowledgeTable.Category.Set(BuildPieceCategory.Misc);
        KnowledgeTable.Crafting.Set(CraftingTable.Forge);
        KnowledgeTable.RequiredItems.Add("FineWood", 10, true);
        KnowledgeTable.RequiredItems.Add("Copper", 2, true);
        KnowledgeTable.RequiredItems.Add("Thunderstone", 1, true);
        KnowledgeTable.RequiredItems.Add("Coal", 10, true);
        MaterialReplacer.RegisterGameObjectForMatSwap(KnowledgeTable.Prefab.transform.Find("$part_replace_dest").gameObject);
        MaterialReplacer.RegisterGameObjectForMatSwap(KnowledgeTable.Prefab.transform.Find("model/$part_replace").gameObject);
        KnowledgeTable component = KnowledgeTable.Prefab.AddComponent<KnowledgeTable>();
        component.m_readSwitch = KnowledgeTable.Prefab.transform.Find("ReadMap").GetComponent<Switch>();
        component.m_writeSwitch = KnowledgeTable.Prefab.transform.Find("WriteMap").GetComponent<Switch>();
        
        PieceEffectManager.PrefabsToSet.Add(KnowledgeTable.Prefab);
    }
}