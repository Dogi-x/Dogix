using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "UnderSiege/LootTable")]
public class LootTable : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public LootItemData item;
        [Range(0f, 100f)] public float weight;
    }

    [SerializeField] private List<Entry> commonPool;
    [SerializeField] private List<Entry> rarePool;
    [SerializeField] private List<Entry> epicPool;

    // Outside: Common 60%, Rare 30%, Epic 10%
    // Inside castle: +10-15% quality shift
    public LootItemData Roll(bool insideCastle)
    {
        float epicChance   = insideCastle ? 0.17f : 0.10f;
        float rareChance   = insideCastle ? 0.33f : 0.30f;

        float roll = UnityEngine.Random.value;
        List<Entry> pool;

        if (roll < epicChance)                     pool = epicPool;
        else if (roll < epicChance + rareChance)   pool = rarePool;
        else                                       pool = commonPool;

        return SelectFromPool(pool);
    }

    private LootItemData SelectFromPool(List<Entry> pool)
    {
        if (pool == null || pool.Count == 0) return null;

        float total = 0f;
        foreach (var e in pool) total += e.weight;

        float r = UnityEngine.Random.Range(0f, total);
        float cumulative = 0f;
        foreach (var e in pool)
        {
            cumulative += e.weight;
            if (r <= cumulative) return e.item;
        }
        return pool[pool.Count - 1].item;
    }
}
