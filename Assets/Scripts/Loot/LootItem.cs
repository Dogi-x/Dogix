using UnityEngine;
using Fusion;

public class LootItem : NetworkBehaviour
{
    [SerializeField] private LootItemData itemData;

    public string       ItemName => itemData.itemName;
    public LootRarity   Rarity   => itemData.rarity;
    public LootCategory Category => itemData.category;
    public int          Weight   => itemData.weight;
    public int          Quantity => itemData.quantity;
    public LootItemData Data     => itemData;

    private bool _pickedUp;

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority || _pickedUp) return;

        var inventory = other.GetComponentInParent<PlayerInventory>();
        if (inventory == null) return;

        if (inventory.TryPickUp(this))
        {
            _pickedUp = true;
            Runner.Despawn(Object);
        }
    }
}
