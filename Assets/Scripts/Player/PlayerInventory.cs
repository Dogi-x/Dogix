using UnityEngine;
using Fusion;

public class PlayerInventory : NetworkBehaviour
{
    public const int MAX_WEIGHT   = 300;
    public const int PRIMARY_SLOTS = 2;    // + 1 pistol slot

    [Networked, Capacity(3)] public NetworkArray<int> WeaponSlots { get; }
    [Networked] public int ActiveWeaponSlot { get; private set; }
    [Networked] public int CurrentWeight    { get; private set; }
    [Networked] private TickTimer SwitchCooldown { get; set; }

    [SerializeField] private float switchTime = 0.7f;

    private WeaponBase[] _spawnedWeapons = new WeaponBase[3];

    public WeaponBase GetActiveWeapon()
    {
        if (!SwitchCooldown.ExpiredOrNotRunning(Runner)) return null;
        return _spawnedWeapons[ActiveWeaponSlot];
    }

    public void SwitchWeapon()
    {
        if (!HasInputAuthority || !SwitchCooldown.ExpiredOrNotRunning(Runner)) return;
        RPC_SwitchWeapon();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SwitchWeapon()
    {
        int next = (ActiveWeaponSlot + 1) % PRIMARY_SLOTS;
        if (WeaponSlots[next] == 0) return;
        ActiveWeaponSlot = next;
        SwitchCooldown   = TickTimer.CreateFromSeconds(Runner, switchTime);
    }

    public bool CanPickUp(int weight) => CurrentWeight + weight <= MAX_WEIGHT;

    public bool TryPickUp(LootItem item)
    {
        if (!HasStateAuthority || !CanPickUp(item.Weight)) return false;
        CurrentWeight += item.Weight;
        return true;
    }
}
