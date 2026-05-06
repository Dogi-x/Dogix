using UnityEngine;
using Fusion;

// Single hidden tunnel: inside castle <-> outside castle.
// Location is fixed per map; revealed to defenders on spawn, to attackers only if they find it.
public class SecretPassage : NetworkBehaviour
{
    [SerializeField] private Transform entrancePoint;  // inside castle
    [SerializeField] private Transform exitPoint;      // outside castle
    [SerializeField] private float     triggerRadius  = 2f;
    [SerializeField] private float     cooldown       = 5f;
    [SerializeField] private bool      bidirectional  = true;

    [Networked] public bool IsRevealed { get; private set; }

    private float _lastUsedTime = -999f;

    public void Reveal()
    {
        if (HasStateAuthority) IsRevealed = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return;
        if (Time.time - _lastUsedTime < cooldown) return;

        var cc = other.GetComponentInParent<CharacterController>();
        if (cc == null) return;
        Transform player = cc.transform;

        if (Vector3.Distance(player.position, entrancePoint.position) < triggerRadius)
        {
            Teleport(cc, exitPoint.position);
        }
        else if (bidirectional &&
                 Vector3.Distance(player.position, exitPoint.position) < triggerRadius)
        {
            Teleport(cc, entrancePoint.position);
        }
    }

    private void Teleport(CharacterController cc, Vector3 destination)
    {
        _lastUsedTime  = Time.time;
        cc.enabled     = false;
        cc.transform.position = destination;
        cc.enabled     = true;
        RPC_TeleportFX(destination);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_TeleportFX(Vector3 dest)
        => AudioManager.Instance?.PlayPassageSound();
}
