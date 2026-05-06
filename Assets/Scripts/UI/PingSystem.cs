using UnityEngine;
using Fusion;

public enum PingType { Enemy, Loot, Danger }

public class PingSystem : NetworkBehaviour
{
    [SerializeField] private Camera    mainCamera;
    [SerializeField] private LayerMask pingMask;

    public void SendPing()
    {
        if (!HasInputAuthority || mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(
            new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));

        bool hit = Runner.LagCompensation.Raycast(
            ray.origin, ray.direction, 300f,
            Object.InputAuthority, out LagCompensatedHit lhit, pingMask);

        if (!hit) return;

        PingType type = DeterminePingType(lhit);
        RPC_Ping(lhit.Point, type, Object.InputAuthority);
    }

    private PingType DeterminePingType(LagCompensatedHit hit)
    {
        if (hit.GameObject == null) return PingType.Danger;
        if (hit.GameObject.GetComponentInParent<PlayerHealth>() != null) return PingType.Enemy;
        if (hit.GameObject.GetComponentInParent<LootItem>()     != null) return PingType.Loot;
        return PingType.Danger;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_Ping(Vector3 worldPos, PingType type, PlayerRef sender)
    {
        if (TeamManager.Instance != null && !TeamManager.Instance.SameTeam(sender, Runner.LocalPlayer))
            return;

        FindObjectOfType<MinimapController>()?.ShowPing(worldPos, type);
        AudioManager.Instance?.PlayPingSound(type);
    }
}
