using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class LootSpawner : NetworkBehaviour
{
    [SerializeField] private LootTable       lootTable;
    [SerializeField] private List<Transform> outsidePoints;   // ~60% of all loot
    [SerializeField] private List<Transform> insidePoints;    // ~40%, higher quality
    [SerializeField] private int             outsideCount = 60;
    [SerializeField] private int             insideCount  = 40;

    public override void Spawned()
    {
        if (!HasStateAuthority) return;
        SpawnLoot(outsidePoints, outsideCount, false);
        SpawnLoot(insidePoints,  insideCount,  true);
    }

    private void SpawnLoot(List<Transform> points, int count, bool insideCastle)
    {
        var available = new List<Transform>(points);
        int spawned   = 0;

        while (spawned < count && available.Count > 0)
        {
            int idx = UnityEngine.Random.Range(0, available.Count);
            Transform pt = available[idx];
            available.RemoveAt(idx);

            LootItemData data = lootTable.Roll(insideCastle);
            if (data?.prefab.IsValid == true)
            {
                Runner.Spawn(data.prefab, pt.position, Quaternion.identity);
                spawned++;
            }
        }
    }
}
