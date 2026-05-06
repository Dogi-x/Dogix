using UnityEngine;
using Fusion;

public enum LootRarity   { Common, Rare, Epic }
public enum LootCategory { Weapon, Ammo, Armor, Heal, Explosive }

[CreateAssetMenu(menuName = "UnderSiege/LootItemData")]
public class LootItemData : ScriptableObject
{
    public int            id;
    public string         itemName;
    public LootRarity     rarity;
    public LootCategory   category;
    public int            weight   = 10;
    public int            quantity = 1;
    public Sprite         icon;
    public NetworkPrefabRef prefab;
}
