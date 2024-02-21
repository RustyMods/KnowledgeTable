using System.Collections.Generic;
using System.Linq;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;

namespace KnowledgeTable.KnowledgeTable;

public class KnowledgeTable : MonoBehaviour
{
    private static readonly int KnowledgeHash = "KnowledgeHash".GetStableHashCode();
    
    public string m_name = "$piece_knowledge_table";
    public Switch m_readSwitch = null!;
    public Switch m_writeSwitch = null!;
    public EffectList m_writeEffects = new();
    public ZNetView _znv = null!;

    public void Awake()
    {
        GetTableWriteEffects();
        m_readSwitch = transform.Find("ReadMap").GetComponent<Switch>();
        m_writeSwitch = transform.Find("WriteMap").GetComponent<Switch>();
        Transform guidePoint = transform.Find("GuidePoint");
        if (!guidePoint.TryGetComponent(out GuidePoint component)) return;
        component.m_ravenPrefab = GetRavens();
    }

    public void Start()
    {
        _znv = GetComponent<ZNetView>();
        _znv.Register<string>(nameof(RPC_KnowledgeData),RPC_KnowledgeData);
        m_readSwitch.m_onUse += OnRead;
        m_readSwitch.m_onHover += GetReadHoverText;
        m_writeSwitch.m_onUse += OnWrite;
        m_writeSwitch.m_onHover += GetWriteHoverText;
    }

    private void GetTableWriteEffects()
    {
        if (!ZNetScene.instance) return;
        GameObject VFX_cartographer_table_write = ZNetScene.instance.GetPrefab("vfx_cartographertable_write");
        if (!VFX_cartographer_table_write) return;
        m_writeEffects = new EffectList()
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
    }
    
    private static GameObject? GetRavens()
    {
        List<GameObject> allObjects = Resources.FindObjectsOfTypeAll<GameObject>().ToList();
        GameObject Ravens =
            allObjects.Find(item => item.name == "Ravens" && item.transform.GetChild(0).name == "Hugin");
        if (!Ravens) return null;

        return Ravens;
    }

    private string GetReadHoverText()
    {
        return !PrivateArea.CheckAccess(transform.position, flash: false)
            ? Localization.instance.Localize(m_name + "\n$piece_noaccess")
            : Localization.instance.Localize(m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_read_knowledge ");
    }

    private string GetWriteHoverText()
    {
        return !PrivateArea.CheckAccess(transform.position, flash: false)
            ? Localization.instance.Localize(m_name + "\n$piece_noaccess")
            : Localization.instance.Localize(m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_write_knowledge ");
    }

    private bool OnRead(Switch caller, Humanoid user, ItemDrop.ItemData? item)
    {
        if (item != null) return false;
        string data = _znv.GetZDO().GetString(KnowledgeHash);
        if (!data.IsNullOrWhiteSpace())
        {
            Player? player = user as Player;
            if (player == null) return false;
            IDeserializer deserializer = new DeserializerBuilder().Build();
            KnowledgeTableData info = deserializer.Deserialize<KnowledgeTableData>(data);
            int count = 0;
            foreach (KeyValuePair<string, string> text in info.m_knownTexts)
            {
                if (player.m_knownTexts.ContainsKey(text.Key)) continue;
                player.m_knownTexts[text.Key] = text.Value;
                ++count;
            }

            foreach (string recipe in info.m_knownRecipes)
            {
                if (player.m_knownRecipes.Contains(recipe)) continue;
                player.m_knownRecipes.Add(recipe);
                ++count;
            }

            foreach (KeyValuePair<string, int> station in info.m_knownStations)
            {
                if (player.m_knownStations.ContainsKey(station.Key)) continue;
                player.m_knownStations[station.Key] = station.Value;
                ++count;
            }

            foreach (string material in info.m_knownMaterial)
            {
                if (player.m_knownMaterial.Contains(material)) continue;
                player.m_knownMaterial.Add(material);
                ++count;
            }

            user.Message(MessageHud.MessageType.Center,
                count == 0
                    ? "$msg_no_new_knowledge"
                    : Localization.instance.Localize("$msg_learned") + $" {count} " + Localization.instance.Localize("$msg_new_things"));
        }

        return false;
    }

    private bool OnWrite(Switch caller, Humanoid user, ItemDrop.ItemData? item)
    {
        if (item != null || !_znv.IsValid()) return false;
        if (!PrivateArea.CheckAccess(transform.position)) return true;
        string data = _znv.GetZDO().GetString(KnowledgeHash);
        if (data != null)
        {
            Player? player = user as Player;
            if (player == null) return false;
            ISerializer serializer = new SerializerBuilder().Build();
            KnowledgeTableData info = new()
            {
                m_knownTexts = player.m_knownTexts,
                m_knownRecipes = player.m_knownRecipes,
                m_knownStations = player.m_knownStations,
                m_knownMaterial = player.m_knownMaterial
            };
            string serialized = serializer.Serialize(info);
            _znv.InvokeRPC(nameof(RPC_KnowledgeData), serialized);
            user.Message(MessageHud.MessageType.Center, "$msg_knowledge_saved");
            Transform transform1 = transform;
            m_writeEffects.Create(transform1.position, transform1.rotation);
        }

        return true;
    }

    public void RPC_KnowledgeData(long sender, string data)
    {
        if (!_znv.IsOwner()) return;
        _znv.GetZDO().Set(KnowledgeHash, data);
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item) => false;
}