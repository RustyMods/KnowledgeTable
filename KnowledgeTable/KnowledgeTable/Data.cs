using System;
using System.Collections.Generic;

namespace KnowledgeTable.KnowledgeTable;

[Serializable]
public class KnowledgeTableData
{
    public Dictionary<string, string> m_knownTexts = new();
    public HashSet<string> m_knownRecipes = new();
    public Dictionary<string, int> m_knownStations = new();
    public HashSet<string> m_knownMaterial = new();
}